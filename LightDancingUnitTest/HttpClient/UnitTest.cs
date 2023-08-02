using System.Threading.Tasks;
using LightDancing.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace LightDancingUnitTest.HttpClient
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public async Task TestUseHttpClientManagerAsync()
        {
            var httpClientManager = new HttpClientManager("http://192.168.0.88:16021/api/v1/SRwR5MAcz0lEIJJLmNZ6j0f5eqPILEnM");

            // GET request
            Rootobject getResponse = httpClientManager.GetAsync<Rootobject>("/panelLayout/layout");

            // POST request
            //string postContent = "{\"key\": \"value\"}";
            //ApiResponse postResponse = await httpClientManager.PostAsync<ApiResponse>("/endpoint", postContent);

            // PUT request
            //string putContent = "{\"key\": \"updatedValue\"}";
            //ApiResponse putResponse = await httpClientManager.PutAsync<ApiResponse>("/endpoint", putContent);

            // Change domain
            httpClientManager.ChangeDomain("https://api.newexample.com");
        }
    }
    public class ApiResponse
    {
        public string Message { get; set; }
    }
    public class Rootobject
    {
        [JsonProperty("numPanels")]
        public int NumPanels { get; set; }
        [JsonProperty("sideLength")]
        public int SideLength { get; set; }
        [JsonProperty("positionData")]
        public Positiondata[] PositionData { get; set; }
    }

    public class Positiondata
    {
        [JsonProperty("panelId")]
        public int PanelId { get; set; }
        [JsonProperty("x")]
        public int X { get; set; }
        [JsonProperty("y")]
        public int Y { get; set; }
        [JsonProperty("o")]
        public int O { get; set; }
        [JsonProperty("shapeType")]
        public int ShapeType { get; set; }
    }

}
