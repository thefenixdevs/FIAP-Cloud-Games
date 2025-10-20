using Microsoft.AspNetCore.Builder;
using Serilog;

namespace GameStore.CrossCutting.Installers.Libraries;

public static class LoggingModule
{
  public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
  {
    Log.Logger = new LoggerConfiguration()
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/gamestore-.log",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .CreateLogger();

    builder.Host.UseSerilog();

    return builder;
  }
}
