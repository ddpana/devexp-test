using Lseg.Devexp.Test.Models;
using Lseg.Devexp.Test.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lseg.Devexp.Test.Controllers;

[ApiController]
[Route("[controller]")]
public class StockDataController : Controller
{
    private ILogger<StockDataController> _logger;
    private StockDataProcessingService _stockDataProcessingService;

    public StockDataController(ILogger<StockDataController> logger, StockDataProcessingService stockDataProcessingService) { 
        _logger = logger;
        _stockDataProcessingService = stockDataProcessingService;
    }

    [HttpGet]
    [Route("datapoints/{numberOfFilesToProcess}")]
    public List<StockExchangeDataPoint> ProcessDataPoints(int numberOfFilesToProcess)
    {
        return _stockDataProcessingService.ProcessStockData(numberOfFilesToProcess);
    }

    [HttpGet]
    [Route("outliers/{numberOfFilesToProcess}")]
    public List<StockExchangeOutlier> ProcessOutliers(int numberOfFilesToProcess)
    {
        var dataPoints = _stockDataProcessingService.ProcessStockData(numberOfFilesToProcess);
        return _stockDataProcessingService.GetOutliers(dataPoints);
    }
}
