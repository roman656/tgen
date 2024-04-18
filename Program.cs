﻿using CommandLine;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Tsyrkov.Tgen;

public static class Program
{
    public static void Main(string[] args)
    {
        var parser = new Parser(config => config.HelpWriter = Console.Out);
        
        parser.ParseArguments<CommandLineOptions>(args).WithParsed(Run);
    }
    
    private static void Run(CommandLineOptions options)
    {
        try
        {
            PrepareLogger();
            
            var core = new Core();

            core.LoadTemplateFromFile(options.TemplateFilePath);
            core.LoadTableValuesFromCsv(options.ValuesFilePath);

            if (string.IsNullOrEmpty(options.OutputFileName))
            {
                Console.WriteLine(core.FillTemplate());
            }
            else
            {
                // Осторожно - файл перезаписывается
                File.WriteAllText(options.OutputFileName, core.FillTemplate());
                Log.Information("Results were saved in {OutputFileName}", options.OutputFileName);
            }
        }
        catch (Exception exception)
        {
            Log.Fatal("{Message}", exception.Message);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
    
    private static void PrepareLogger()
    {
        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console(
                        outputTemplate: "[{Timestamp:HH:mm:ss.fff}] ({Level:u3}) {Message:lj}{NewLine}{Exception}",
                        theme: AnsiConsoleTheme.Sixteen)
                .CreateLogger();
    }
}
