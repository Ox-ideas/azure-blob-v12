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
            Settings settings = config.GetRequiredSection("Settings").Get<Settings>();

            // Write the values to the console.
            Console.WriteLine($"KeyOne = {settings.KeyOne}");
            Console.WriteLine($"KeyTwo = {settings.KeyTwo}");
            Console.WriteLine($"KeyThree:Message = {settings.KeyThree.Message}");
            Console.WriteLine($"KeyThree:MessageB = {settings.KeyThree.MessageB}");
        }

        public class Settings
        {
            public int KeyOne { get; set; }
            public bool KeyTwo { get; set; }
            public NestedSettings KeyThree { get; set; } = null!;
        }

        public class NestedSettings
        {
            public string Message { get; set; } = null!;
            public string MessageB { get; set; } = null!;
        }
    }
}