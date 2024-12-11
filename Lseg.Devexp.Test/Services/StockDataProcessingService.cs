using CsvHelper;
using CsvHelper.Configuration;
using Lseg.Devexp.Test.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using MathNet.Numerics.Statistics;

namespace Lseg.Devexp.Test.Services;

public class StockDataProcessingService
{
    private readonly ILogger<StockDataProcessingService> _logger;
    private readonly IOptions<Config> _configuration;
    private readonly string _timestampFolder;
    private readonly CsvConfiguration _csvConfig;

    public StockDataProcessingService(ILogger<StockDataProcessingService> logger, IOptions<Config> configuration) {
        _logger = logger;
        _configuration = configuration;
        _timestampFolder = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        // Configuring CSV helper to specify that the files can be in any culture and it does not have any headers
        _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            MissingFieldFound = null,
            BadDataFound = null,
        };
    }

    public List<StockExchangeDataPoint> ProcessStockData(int numberOfFilesToProcess)
    {
        if (numberOfFilesToProcess < 1)
        {
            numberOfFilesToProcess = _configuration.Value.NumberOfFilesToProcess;
        }

        // This will be the output variable
        var processedDataPoints = new List<StockExchangeDataPoint>();

        Console.WriteLine($"Configuration stock exchange data folder: {_configuration.Value.StockExchangeDataFolder}");
        // Get the list of all folders (stock exchanges) inside the main folder where the exchange data is located
        var inputDirectories = Directory.GetDirectories(_configuration.Value.StockExchangeDataFolder);
        // We need this variable to output data based on the run, to maintain historical data.

        // Check if we have any folder to process
        if (inputDirectories.Count() == 0)
        {
            throw new ArgumentNullException($" Could not find any {nameof(inputDirectories)}.");
        }

        foreach (var inputDirectory in inputDirectories)
        {
            Console.WriteLine($"Processing folder: {inputDirectory}");

            // Check if we have the current Stock Exchange in the output list of datapoints, if not, add it
            if (!processedDataPoints.Any(e => e.ExchangeName == inputDirectory))
            {
                var exchange = new StockExchangeDataPoint() { ExchangeName = inputDirectory };
                processedDataPoints.Add(exchange);
            }

            // here we take the number of files from appsettings or environment variables. We also select random {NumberOfFilesToProcess} number of files.
            var randomFiles = new Random();
            var files = Directory.GetFiles(inputDirectory, "*.csv").OrderBy(x => randomFiles.Next()).Take(numberOfFilesToProcess).ToList();

            // Check if we have any file to process
            if (files.Count() == 0)
            {
                Console.WriteLine($" Could not find any {nameof(files)} in folder {inputDirectory}.");
                continue;
            }

            foreach (var file in files)
            {
                Console.WriteLine($"Processing file: {file}");

                // Check if we have the current Stock Index in the output list of datapoints, if not, add it
                var existingIndex = processedDataPoints.First(e => e.ExchangeName == inputDirectory).StockIndexes.FirstOrDefault(e => e.IndexName == file);
                if (existingIndex == null)
                {
                    var index = new StockIndexDataPoint() { IndexName = file };
                    processedDataPoints.First(e => e.ExchangeName == inputDirectory).StockIndexes.Add(index);
                }


                // Use CSV helper to open the files and convert records to our data Model (DataPoint)
                try
                {
                    StreamReader? reader = new StreamReader(file);

                    string headerLine = reader.ReadLine();

                    // Check if the file is empty or has no header
                    if (string.IsNullOrEmpty(headerLine))
                    {
                        Console.WriteLine($"File is empty");
                        continue;
                    }

                    try
                    {
                        CsvReader csv = new CsvReader(reader, _csvConfig);
                        // Map columns to specific data types, with specific formatting
                        csv.Context.RegisterClassMap<TransactionLineMap>();
                        
                        var records = csv.GetRecords<DataPoint>().ToList();

                        // Check if we have less than NumberOfRecordsToProcess (default 30)
                        if (records.Count < _configuration.Value.NumberOfRecordsToProcess)
                        {
                            Console.WriteLine($"File {file} cannot be processed as it has less than {_configuration.Value.NumberOfRecordsToProcess} records.");
                            // We can throw error here if we want.
                            continue;
                        }

                        // Get a random row as starting point
                        var random = new Random();
                        // Ensure we have at least 30 entries from the start point
                        int startPoint = random.Next(0, records.Count - 29);
                        var recordsToProcess = records.Skip(startPoint).Take(_configuration.Value.NumberOfRecordsToProcess).ToList();

                        // Return Result

                        var currentIndex = processedDataPoints.First(e => e.ExchangeName == inputDirectory).StockIndexes.First(e => e.IndexName == file);
                        currentIndex.DataPoints = recordsToProcess;
                        continue;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Cannot convert file to DataPoints as it is invalid: {file}: {ex.Message}");
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Invalid data file {file}: {ex.Message}");
                    continue;
                }
            }
        }
        return processedDataPoints;
    }


    public List<StockExchangeOutlier> GetOutliers(List<StockExchangeDataPoint> stockExchangeDataPoints)
    {
        var stockExchangeOutliers = new List<StockExchangeOutlier>();

        foreach (var stockExchangeDataPoint in stockExchangeDataPoints)
        {

            // Check if we have the current Stock Exchange in the output list of outliers, if not, add it
            if (!stockExchangeOutliers.Any(e => e.ExchangeName == stockExchangeDataPoint.ExchangeName))
            {
                var exchange = new StockExchangeOutlier() { ExchangeName = stockExchangeDataPoint.ExchangeName };
                stockExchangeOutliers.Add(exchange);
            }

            foreach (var stockExchangeIndex in stockExchangeDataPoint.StockIndexes)
            {

                // Check if we have the current Stock Index in the output list of datapoints, if not, add it
                var existingIndex = stockExchangeOutliers.First(e => e.ExchangeName == stockExchangeDataPoint.ExchangeName).StockIndexes.FirstOrDefault(e => e.IndexName == stockExchangeIndex.IndexName);
                if (existingIndex == null)
                {
                    var currentIndex = new StockIndexOutlier() { IndexName = stockExchangeIndex.IndexName };
                    stockExchangeOutliers.First(e => e.ExchangeName == stockExchangeDataPoint.ExchangeName).StockIndexes.Add(currentIndex);
                }

                // Show deviation
                var outliers = FindOutliers(stockExchangeIndex.DataPoints);

                // var relativePath = Path.GetRelativePath(_configuration.Value.StockExchangeDataFolder, inputFilePath);

                var outputFolder = Path.Combine(Path.Combine(_configuration.Value.OutputFolder, _timestampFolder), stockExchangeDataPoint.ExchangeName);
                Console.WriteLine($"   Output folder: {outputFolder}");
                // Ensure the output directory exists
                if (!Directory.Exists(outputFolder))
                {
                    try
                    {
                        Directory.CreateDirectory(outputFolder);
                        Console.WriteLine($"Folder {outputFolder} does not exist and has been created");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Folder {outputFolder} cannot be created: {ex.Message}");
                    }
                }

                // Write the output file, in the same structure
                var writer = new StreamWriter(Path.Combine(Path.Combine(_configuration.Value.OutputFolder, _timestampFolder), stockExchangeIndex.IndexName));
                var csvOutput = new CsvWriter(writer, _csvConfig);
                csvOutput.WriteRecords(outliers);
                // Close the file.
                csvOutput.Dispose();

                var index = stockExchangeOutliers.First(e => e.ExchangeName == stockExchangeDataPoint.ExchangeName).StockIndexes.FirstOrDefault(e => e.IndexName == stockExchangeIndex.IndexName);
                index.Outliers = outliers;

                foreach (var outlier in outliers)
                {
                    Console.WriteLine($"Record: {outlier.StockID} {outlier.Timestamp} {outlier.ActualPrice} {outlier.Mean} {outlier.PercentageDeviation}");
                }

            }
        }
        return stockExchangeOutliers;
    }

    public List<Outlier> FindOutliers(List<DataPoint> dataPoints)
    {

        // Select the prices of datapoints
        var prices = dataPoints.Select(dp => dp.Price).ToList();
        // Find the mean of all datapoint prices
        double mean = prices.Mean();
        // Find the std deviation for datapoints
        double stdDev = prices.StandardDeviation();
        // Define the threshold for outlier definition
        double threshold = 2 * stdDev;

        // Return the datapoints in the outlier data model format
        return dataPoints
            .Where(dp => Math.Abs(dp.Price - mean) > threshold)
            .Select(dp => new Outlier
            {
                StockID = dp.StockID,
                Timestamp = dp.Timestamp,
                ActualPrice = dp.Price,
                Mean = mean,
                Deviation = dp.Price - mean,
                PercentageDeviation = ((dp.Price - mean) / mean) * 100
            })
            .ToList();
    }

    // Class for CSV transformation map
    internal class TransactionLineMap : ClassMap<DataPoint>
    {
        public TransactionLineMap()
        {
            try
            {
                Map(m => m.StockID);
                Map(m => m.Timestamp)
                    .TypeConverter<CsvHelper.TypeConversion.DateTimeConverter>()
                    .TypeConverterOption.Format("dd-MM-yyyy");
                Map(m => m.Price)
                    .TypeConverter<CsvHelper.TypeConversion.DoubleConverter>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot convert data as datapoints are invalid: {ex.Message}.");
            }
        }
    }
}
