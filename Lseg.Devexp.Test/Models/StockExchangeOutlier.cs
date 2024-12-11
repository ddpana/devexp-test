namespace Lseg.Devexp.Test.Models;

public class StockExchangeOutlier
{
    public string ExchangeName { get; set; } = string.Empty;
    public List<StockIndexOutlier> StockIndexes { get; set; } = new List<StockIndexOutlier>();
}

public class StockIndexOutlier
{
    public string IndexName { get; set; } = string.Empty;
    public List<Outlier> Outliers { get; set; } = new List<Outlier>();
}