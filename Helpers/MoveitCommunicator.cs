using MoveitLocalFolderScanner.Models;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

namespace MoveitLocalFolderScanner.Helpers
{
    public class MoveitCommunicator
    {
        private static MoveitCommunicator _instance;
        private HttpClient httpClient;
        private string serverUrl = "https://testserver.moveitcloud.com/api/v1";

        public static MoveitCommunicator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MoveitCommunicator();
                }
                return _instance;
            }
        }

        private MoveitCommunicator()
        {
            httpClient = new HttpClient();
        }

        public async Task<string> RetrieveAPIToken(User user)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, serverUrl + "/token");
            request.Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", user.Username),
                new KeyValuePair<string, string>("password", user.Password)
            ]);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                JObject contentJson = JObject.Parse(await response.Content.ReadAsStringAsync());
                return contentJson["access_token"].ToString();
            }
            else
            {
                string message = await response.Content.ReadAsStringAsync();
                JObject errorJson = JObject.Parse(message);
                throw new HttpRequestException($"Failed to retrieve access token. {(string)errorJson["detail"]}");
            }
        }

        public async Task<string> GetHomeUserFolderId(User user)
        {
            HttpRequestMessage request = CreateRequest(user, "/folders", "get");
            HttpResponseMessage response = await httpClient.SendAsync(request);

            string content = await response.Content.ReadAsStringAsync();
            JObject contentJson = JObject.Parse(content);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Can't reach server! {contentJson["detail"]}");
            }
            JToken userHomeFolder = contentJson["items"]
                .FirstOrDefault(i => (string)i["name"] == user.Username.ToLower());

            if (userHomeFolder == null)
            {
                throw new FileNotFoundException($"Home user folder was not found!");
            }

            string userHomeFolderId = (string)userHomeFolder["id"];

            return userHomeFolderId;
        }

        public async Task<List<string>> ScanForNewFiles(User user, string folderId, string folderPath)
        {
            List<string> missingFiles = new List<string>();

            HttpRequestMessage request = CreateRequest(user, $"/folders/{folderId}/files", "get");
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string content = await response.Content.ReadAsStringAsync();
            JObject contentJson = JObject.Parse(content);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Can't get files from folder! {(string)contentJson["detail"]}");
            }

            string[] filesInHome = contentJson["items"]
                .Select(i => (string)i["name"])
                .ToArray();

            string[] localFilesPaths = Directory.GetFiles(folderPath);

            foreach(string localFilePath in localFilesPaths)
            {
                string fileName = Path.GetFileName(localFilePath);
                if(!filesInHome.Contains(fileName))
                {
                    missingFiles.Add(localFilePath);
                }
            }

            return missingFiles;
        }

        public async Task UploadFile(User user, string folderId, string filePath)
        {
            HttpRequestMessage request = CreateRequest(user, $"/folders/{folderId}/files", "post");
            MultipartFormDataContent form = new MultipartFormDataContent();

            ByteArrayContent fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file", Path.GetFileName(filePath));

            request.Content = form;

            HttpResponseMessage response = await httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string message = await response.Content.ReadAsStringAsync();
                JObject errorJson = JObject.Parse(message);
                throw new HttpRequestException($"File couldn't be uploaded! {(string)errorJson["detail"]}");
            }
        }

        private HttpRequestMessage CreateRequest(User user, string endPoint, string method)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method.ToUpper()), serverUrl + endPoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.AccessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return request;
        }
    }
}
