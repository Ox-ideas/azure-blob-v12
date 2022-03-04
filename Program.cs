using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlobV12Net6
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args).Build();

            // Ask the service provider for the configuration abstraction.
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();

            // Get values from the config given their key and their target type.
            int keyOneValue = config.GetValue<int>("KeyOne");
            bool keyTwoValue = config.GetValue<bool>("KeyTwo");
            string keyThreeNestedValue = config.GetValue<string>("KeyThree:Message");
            string keyThreeNestedValueB = config.GetValue<string>("KeyThree:MessageB");

            // Write the values to the console.
            Console.WriteLine($"KeyOne = {keyOneValue}");
            Console.WriteLine($"KeyTwo = {keyTwoValue}");
            Console.WriteLine($"KeyThree:Message = {keyThreeNestedValue}");
            Console.WriteLine($"KeyThree:MessageB = {keyThreeNestedValueB}");

            // Application code which might rely on the config could start here.

            await host.RunAsync();
        }
    }
}