using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Net;
using System.Text;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]
namespace TestPutProject
{
    [TestClass]
    public class PostMethod
    {
        private static HttpClient _httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string UserEndpoint = "pet";

        private static string GetURL(string endpoint) => $"{BaseURL}{endpoint}";
        private static Uri GetUri(string endpoint) => new Uri(GetURL(endpoint));
        
        private readonly List<UserModel> cleanUpList = new List<UserModel>();

        [TestInitialize]
        public void TestInitialize()
        {
            _httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = await _httpClient.DeleteAsync(GetURL($"{UserEndpoint}/{data.Id}"));
            }
        }

        [TestMethod]
        public async Task PutMethod()
        {
            UserModel petData = new UserModel()
            {
                Id = 1,
                Category = new Category()
                {
                    Id = 1,
                    Name = "Snoopy"
                },
                Name = "Snoopy",
                PhotoUrls = new List<string>
                {
                    "Pogi"
                },
                Tags = new List<Category>()
                {
                    new Category()
                {
                    Id = 1,
                    Name = "Snoopy"
                }
                },
                Status = "available"
            };

            // Serialize
            var request = JsonConvert.SerializeObject(petData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post
            var postResponse = await _httpClient.PostAsync(GetURL(UserEndpoint), postRequest);
            // Status Code
            var postStatusCode = postResponse.StatusCode;

            // Get Request
            var getResponse = await _httpClient.GetAsync(GetUri($"{UserEndpoint}/{petData.Id}"));
            // Status Code
            var getstatusCode = getResponse.StatusCode;

            // Deserialize Content
            var listOfPet = JsonConvert.DeserializeObject<UserModel>(getResponse.Content.ReadAsStringAsync().Result);

            // Filter created data
            var petUserData = listOfPet.Name;

            // Update Value of petData
            petData.Name = "Garfield";
                
            // Serialize
            request = JsonConvert.SerializeObject(petData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var putResponse = await _httpClient.PutAsync(GetURL(UserEndpoint), postRequest);
            // Get Status Code
            var statusCode = putResponse.StatusCode;

            // Get Request
            getResponse = await _httpClient.GetAsync(GetUri($"{UserEndpoint}/{petData.Id}"));

            var getPetData = JsonConvert.DeserializeObject<UserModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            petUserData = getPetData.Name;

            #region Cleanup Data
            // Add data to Cleanup List
            cleanUpList.Add(petData);
            #endregion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "statusCode code is not equals to 200");
            Assert.AreEqual(petData.Id, getPetData.Id, "Id not matching");
        }
    }
}
