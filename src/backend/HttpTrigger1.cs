using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace lesson05
{
    public static class restapi
    {
        [FunctionName("HttpExample")]
        public static async Task<IActionResult> GetAllDishes(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "recipes")] HttpRequest req,
        [CosmosDB(
        databaseName: "mydb",
        collectionName: "myfirstcontainer",
        ConnectionStringSetting = "CosmosDbConnectionString",
        SqlQuery = "SELECT FROM c")]IEnumerable<Recipe> recipes,
        ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            if(recipes != null) {
                return new OkObjectResult(recipes);
            }

            return new NotFoundResult();
        }

        [FunctionName("Testing")]
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
