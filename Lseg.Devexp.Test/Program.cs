using Lseg.Devexp.Test.Services;
using Microsoft.Extensions.Options;

namespace Lseg.Devexp.Test;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var config = builder.Configuration.Get<Config>();
        builder.Services.Configure<Config>(builder.Configuration);
        builder.Services.AddSingleton(s => s.GetRequiredService<IOptions<Config>>().Value);

        builder.Services.AddSingleton<StockDataProcessingService>();

        builder.Services.AddHealthChecks();

        builder.Services.AddControllers();

        builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(o =>
        {
            o.EnableTryItOutByDefault();
            o.EnableValidator();
            o.EnableFilter();
            o.DisplayRequestDuration();
        });


        app.UseHttpsRedirection();
        app.UseRouting();

        app.MapHealthChecks("/api/health").AllowAnonymous();

        app.MapControllers();

        app.Run();

    }
}