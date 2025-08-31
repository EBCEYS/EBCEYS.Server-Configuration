using System.Reflection;
using EBCEYS.ContainersEnvironment.HealthChecks.Extensions;
using EBCEYS.Server_configuration.ConfigDatabase;
using EBCEYS.Server_configuration.Middle;
using EBCEYS.Server_configuration.Middle.Archives;
using EBCEYS.Server_configuration.ServiceEnvironment;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

namespace EBCEYS.Server_configuration;

/// <summary>
///     A <see cref="Program" /> class.
/// </summary>
public class Program
{
    /// <summary>
    ///     The db connection string.
    /// </summary>
    public static string DBConnectionString { get; } =
        $"Data source={SupportedEnvironmentVariables.ServiceDatabasePath.Value}";

    /// <summary>
    ///     The main.
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    {
        var firstArg = args.FirstOrDefault();
        if (firstArg != null && (firstArg == "--help" || firstArg == "-h"))
        {
            Console.WriteLine("Help:");
            Console.WriteLine(SupportedEnvironmentVariables.GetHelp());
            return;
        }

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        ConfigureServices(builder);
        ConfigureLogging(builder);
        ConfigureConfigurating(builder);

        var app = builder.Build();

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.ConfigureHealthChecks();

        // Configure the HTTP request pipeline.
        if (SupportedEnvironmentVariables.ServiceEnableSwagger.Value!.Value)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();
        app.UseAuthentication();

        app.UseRouting();

        app.MapControllers();

        app.Run();
    }

    private static void ConfigureConfigurating(WebApplicationBuilder builder)
    {
        builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
        builder.Configuration.AddJsonFile("appsettings.json", false);
        builder.Configuration.AddEnvironmentVariables();
    }

    private static void ConfigureLogging(WebApplicationBuilder builder)
    {
        LogManager.Setup().LoadConfigurationFromAppSettings();
        builder.Logging.ClearProviders();
        builder.Logging.AddNLogWeb();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.ConfigureHealthChecks();
        builder.Services.AddDbContext<ConfigurationDatabaseContext>(opts => { opts.UseSqlite(DBConnectionString); },
            ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        builder.Services.AddHostedService<MigrationService>();

        builder.Services.AddSingleton<DockerController>();

        builder.Services.AddSingleton<KeysStorageService>();
        builder.Services.AddHostedService(sp => { return sp.GetService<KeysStorageService>()!; });
        builder.Services.AddSingleton<ConfigurationProcessingService>();
        builder.Services.AddHostedService(sp => { return sp.GetService<ConfigurationProcessingService>()!; });

        builder.Services.AddSingleton<IArchiveHelper, TarArchiveHelper>();

        builder.Services.AddMemoryCache();

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(opt => { opt.IncludeXmlComments(Assembly.GetEntryAssembly(), true); });

        builder.Services.AddRouting(r =>
        {
            r.LowercaseUrls = true;
            r.LowercaseQueryStrings = true;
        });
    }
}