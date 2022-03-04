using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace BlobV12Net6
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Build a config object, using env vars and JSON providers.
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Get values from the config given their key and their target type.
            int keyOneValue = config.GetValue<int>("Settings:KeyOne");
            bool keyTwoValue = config.GetValue<bool>("Settings:KeyTwo");
            string keyThreeNestedValue = config.GetValue<string>("Settings:KeyThree:Message");
            var keyThreeNestedValueB = config.GetValue<string>("Settings:KeyThree:MessageB");

            // Write the values to the console.
            Console.WriteLine($"KeyOne = {keyOneValue}");
            Console.WriteLine($"KeyTwo = {keyTwoValue}");
            Console.WriteLine($"KeyThree:Message = {keyThreeNestedValue}");
            Console.WriteLine($"KeyThree:MessageB = {keyThreeNestedValueB}");
        }
    }
}