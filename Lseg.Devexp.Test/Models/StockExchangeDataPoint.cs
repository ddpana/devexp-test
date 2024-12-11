namespace Lseg.Devexp.Test.Models;

public class StockExchangeDataPoint
{
    public string ExchangeName { get; set; } = string.Empty;
    public List<StockIndexDataPoint> StockIndexes { get; set; } = new List<StockIndexDataPoint>();
}

public class StockIndexDataPoint
{
    public string IndexName { get; set; } = string.Empty;
    public List<DataPoint> DataPoints { get; set; } = new List<DataPoint>();
}