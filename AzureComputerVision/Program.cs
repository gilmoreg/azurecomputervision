using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;

namespace AzureComputerVision
{
    internal static class Program
    {
        private const string subscriptionKeyArg = "/subscriptionKey=";
        private const string endpointArg = "/endpoint=";
        private const string folderArg = "/folder=";

        // Example arguments.
        // /subscriptionKey=xyz /endpoint=https://contoso.cognitiveservices.azure.com/ /directory=d:\pageImages
        private static async Task Main(string[] args)
        {
            string subscriptionKey = null;
            string endpoint = null;
            string folder = null;

            foreach (string arg in args)
            {
                if (arg.StartsWith(subscriptionKeyArg, StringComparison.OrdinalIgnoreCase))
                {
                    subscriptionKey = arg.Remove(0, subscriptionKeyArg.Length);
                }

                if (arg.StartsWith(endpointArg, StringComparison.OrdinalIgnoreCase))
                {
                    endpoint = arg.Remove(0, endpointArg.Length);
                }

                if (arg.StartsWith(folderArg, StringComparison.OrdinalIgnoreCase))
                {
                    folder = arg.Remove(0, folderArg.Length);
                }
            }

            if (string.IsNullOrWhiteSpace(subscriptionKey))
            {
                throw new ArgumentNullException(nameof(subscriptionKey));
            }

            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentNullException(nameof(endpoint));
            }

            if (!Directory.Exists(folder))
            {
                throw new ArgumentException("folder does not exist.");
            }

            var files = Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories);

            if (!files.Any())
            {
                throw new ArgumentException($"Unable to find any files in {folder}");
            }

            // Create a client
            var client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(subscriptionKey)) { Endpoint = endpoint };
            var processor = new DocumentProcessor(client);

            var results = await processor.ProcessDocuments(files);
            await File.WriteAllLinesAsync("output.txt", results);
        }
    }
}