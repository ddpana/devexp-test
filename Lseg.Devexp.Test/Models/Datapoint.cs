namespace Lseg.Devexp.Test.Models;

public class DataPoint
{
    public string StockID { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    // ASSUMPTION: we need precision, as we are working with stock data and precision matters and also we do NOT need to round-up
    // therefore, using double in detriment of float or decimal due to precision (float is 32 bits, double is 64 bits, decimal is 128 bits)
    // decimal is more precise, but it does not round up very well and also it's harder to perform math calculation on it.
    // We can use Float if we want.
    public double Price { get; set; }
}
