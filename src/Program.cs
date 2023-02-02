using DotnetActionsToolkit;
using System;
using System.Collections;
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
                
                var ms = _core.GetInput("milliseconds");
                _core.Debug($"Waiting {ms} milliseconds..."); // debug is only output if you set teh secret ACTIONS_RUNNER_DEBUG to true

                _core.Debug(DateTime.Now.ToLongTimeString());
                await Task.Delay(int.Parse(ms));
                // await Task.Delay(5000);

                CredentialSeeds.ClearCredentials(OcpiService.GetMongoDb()).Wait();
                OcpiService.StopAsync().Wait();
                
                _core.Debug(DateTime.Now.ToLongTimeString());

                _core.SetOutput("time", DateTime.Now.ToLongTimeString());
            }
            catch (Exception ex)
            {
                _core.SetFailed(ex.Message);
            }
        }
    }
}
