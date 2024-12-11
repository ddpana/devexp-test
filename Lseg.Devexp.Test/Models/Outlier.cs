namespace Lseg.Devexp.Test.Models;

public class Outlier
{
    public string StockID { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public double ActualPrice { get; set; }
    public double Mean { get; set; }
    public double Deviation { get; set; }
    public double PercentageDeviation { get; set; }
}