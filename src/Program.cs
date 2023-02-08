using DotnetActionsToolkit;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using dotnet_sample_action.Containers;
using dotnet_sample_action.Seeds;

namespace dotnet_sample_action
{
    public class Program
    {
        static readonly Core _core = new Core();

        static async Task Main(string[] args)
        {
            try
            {
                OcpiService.StartAsync().Wait();
                CredentialSeeds.SeedCredentials(OcpiService.GetMongoDb()).Wait();
                
                Console.WriteLine("This is the end");
                var ms = _core.GetInput("milliseconds");
                _core.Debug($"Waiting {ms} milliseconds...");

                _core.Debug(DateTime.Now.ToLongTimeString());
                await Task.Delay(int.Parse(ms));
                // await Task.Delay(5000);
                using var ocpiStdoutReader = new StreamReader(OcpiService.ocpiConsumer.Stdout);
                var ocpiStdout = ocpiStdoutReader.ReadToEnd();
                _core.Debug(ocpiStdout);
                
                using var stdoutReader = new StreamReader(OcpiService.mongoConsumer.Stdout);
                var stdout = stdoutReader.ReadToEnd();
                _core.Debug(stdout);

                CredentialSeeds.ClearCredentials(OcpiService.GetMongoDb()).Wait();
                OcpiService.StopAsync().Wait();
                
                _core.Debug(DateTime.Now.ToLongTimeString());

                _core.SetOutput("time", DateTime.Now.ToLongTimeString());
                Console.WriteLine("This is the end");
            }
            catch (Exception ex)
            {
                _core.SetFailed(ex.Message);
            }
        }
    }
}
