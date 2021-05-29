using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Newtonsoft.Json;

namespace AzureComputerVision
{
    internal class DocumentProcessor
    {
        private readonly IComputerVisionClient _computerVisionClient;

        public DocumentProcessor(IComputerVisionClient computerVisionClient)
        {
            _computerVisionClient = computerVisionClient;
        }

        public async ValueTask<IEnumerable<string>> ProcessDocument(string path)
        {
            // Read text from URL
            var textHeaders = await _computerVisionClient.ReadInStreamAsync(File.OpenRead(path), language: "ja");
            // After the request, get the operation location (operation ID)
            string operationLocation = textHeaders.OperationLocation;

            // Retrieve the URI where the recognized text will be stored from the Operation-Location header.
            // We only need the ID and not the full URL
            const int numberOfCharsInOperationId = 36;
            string operationId = operationLocation.Substring(operationLocation.Length - numberOfCharsInOperationId);

            // Extract the text
            ReadOperationResult results;
            do
            {
                results = await _computerVisionClient.GetReadResultAsync(Guid.Parse(operationId));
            }
            while ((results.Status == OperationStatusCodes.Running ||
                results.Status == OperationStatusCodes.NotStarted));

            File.WriteAllText("output.json", JsonConvert.SerializeObject(results.AnalyzeResult.ReadResults, Formatting.Indented));
            

            return results.AnalyzeResult.ReadResults.SelectMany(r => r.Lines.Select(l => l.Text).Reverse());
        }

        public async ValueTask<IEnumerable<string>> ProcessDocuments(IEnumerable<string> paths)
        {
            return await ProcessDocument(paths.First());
            
            // var tasks = paths.Select(p => ProcessDocument(p).AsTask());
            //var result = await Task.WhenAll(tasks);
            //return result.SelectMany(r => r);
        }
    }
}