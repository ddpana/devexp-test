namespace Lseg.Devexp.Test;

// Define a config that can be used at a later time easily trougout the application
public class Config
{
    // Folder that contains stock exchange data (this folder should contain folders, for each exchange, and inside those folders we should find csv files with data for that particular stock exchange)
    public string StockExchangeDataFolder { get; set; } = string.Empty;

    // Output folder
    public string OutputFolder {  get; set; } = string.Empty;

    // Number of files to be processed, assuming a default of 2
    public int NumberOfFilesToProcess { get; set; } = 2;

    // Number of records from file that should be processed
    public int NumberOfRecordsToProcess { get; set; } = 30;
}
