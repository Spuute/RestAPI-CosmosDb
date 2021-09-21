using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace lesson05
{
    public static class restapi
    {
        [FunctionName("HttpExample")]
        public static async Task<IActionResult> GetDish(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
        [CosmosDB(
        databaseName: "mydb",
        collectionName: "myfirstcontainer",
        ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> documentsOut,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;

            if (!string.IsNullOrEmpty(name))
            {
                // Add a JSON document to the output container.
                await documentsOut.AddAsync(new
                {
                    // create a random ID
                    id = System.Guid.NewGuid().ToString(),
                    name = name
                });
        }
        

        string responseMessage = string.IsNullOrEmpty(name)
        ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
        : $"Hello, {name}. This HTTP triggered function executed successfully.";

        return new OkObjectResult(responseMessage);
        }

        
        [FunctionName("Testing")]
        public static async Task<IActionResult> PostDish(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
        [CosmosDB(
        databaseName: "mydb",
        collectionName: "myfirstcontainer",
        ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<dynamic> recipe,
        ILogger log)
        {
            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

                var input = JsonConvert.DeserializeObject<Recipe>(requestBody);

                var newRecipe = new Recipe
                {
                    Name = input.Name,
                    test = input.test
                };

                await recipe.AddAsync(newRecipe);

                return new OkObjectResult(newRecipe);
            }
            catch (Exception ex)
            {
                log.LogError($"Couldn't insert item. Exception thrown: {ex.Message}");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }


    }
}
