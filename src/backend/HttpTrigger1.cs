using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace lesson05
{
    public static class restapi
    {
        [FunctionName("dish")]
        public static async Task<IActionResult> GetDish(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes")] HttpRequest req,
        [CosmosDB(
        databaseName: "mydb",
        collectionName: "myfirstcontainer",
        ConnectionStringSetting = "CosmosDbConnectionString")] DocumentClient client,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var searchterm = req.Query["searchterm"];
            if (string.IsNullOrWhiteSpace(searchterm))
            {
                return (ActionResult)new NotFoundResult();
            }

            Uri collectionUri = UriFactory.CreateDocumentCollectionUri("mydb", "myfirstcontainer");

            log.LogInformation($"Searching for: {searchterm}");

            IDocumentQuery<Recipe> query = client.CreateDocumentQuery<Recipe>(collectionUri, new FeedOptions { EnableCrossPartitionQuery = true })
                .Where(p => p.Name.Contains(searchterm))
                .AsDocumentQuery();

            while (query.HasMoreResults)
            {
                foreach (Recipe result in await query.ExecuteNextAsync())
                {
                    log.LogInformation(result.Name);
                }
            }
            return new OkObjectResult(query);

        }



        [FunctionName("AddDish")]
        public static async Task<IActionResult> AddDish(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "recipe")] HttpRequest req,
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
