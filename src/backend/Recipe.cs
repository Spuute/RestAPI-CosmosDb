using Newtonsoft.Json;

namespace lesson05
{
    public class Recipe
    {
        //[JsonProperty(PropertyName = "isComplete")]
        public string Name { get; set; }

        public int test { get; set; }
    }
}