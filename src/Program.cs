
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using dotnet_sample_action.Containers;
using DotnetActionsToolkit;

namespace dotnet_sample_action
{
    public class Program
    {
        static readonly Core _core = new();

        static async Task Main(string[] args)
        {
            try
            {
                Service.StartAsync().Wait();
                
                var ms = _core.GetInput("milliseconds");

                _core.Debug(DateTime.Now.ToLongTimeString());
                await Task.Delay(int.Parse(ms));

                using var ocpiStdoutReader = new StreamReader(Service.Consumer.Stdout);
                var ocpiStdout = ocpiStdoutReader.ReadToEnd();
                _core.Debug(ocpiStdout);
                
                using var stdoutReader = new StreamReader(Service.mongoConsumer.Stdout);
                var stdout = stdoutReader.ReadToEnd();
                _core.Debug(stdout);

                Service.StopAsync().Wait();
                
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
