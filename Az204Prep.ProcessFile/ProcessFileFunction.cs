using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Identity;
using Azure.Storage.Blobs;
using System.Text;
using Azure;

namespace Az204Prep.ProcessFile
{
    public static class ProcessFileFunction
    {
        [FunctionName("ProcessFile")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            var creds = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = Environment.GetEnvironmentVariable("ClientId") });
            var blobClient = new BlobContainerClient(new Uri(Environment.GetEnvironmentVariable("ContainerName")), creds);

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject(requestBody);
                name = name ?? data?.name;

                string content = $"Blob {name}";
                byte[] byteArray = Encoding.ASCII.GetBytes(content);
                using MemoryStream memoryStream = new MemoryStream(byteArray);
                await blobClient.UploadBlobAsync(name, memoryStream);
            }
            catch(RequestFailedException ex)
            {
                log.LogError(ex.Message);
                throw;
            }

            return new OkObjectResult("File creataed");
        }
    }
}
