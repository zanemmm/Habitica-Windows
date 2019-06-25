using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Habitica.Models;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using AppTask = Habitica.Models.Task;

namespace Habitica.Utils
{
    class HttpApi
    {
        private static readonly HttpClient Client = new HttpClient();

        private static HttpApi Instance;

        private const string BaseUrl = "https://habitica.com/api/v3/";

        private const string ClientToken = "88e6cb45-5700-4643-b866-f333aa3539b5-WindowsHabitica";

        public Setting Setting;

        public static HttpApi New(Setting setting)
        {
            if (Instance == null)
            {
                Instance = new HttpApi
                {
                    Setting = setting
                };
            }
            return Instance;
        }

        public async Task<List<AppTask>> GetAllTasks()
        {
            // 发起请求
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "tasks/user");
            HttpResponseMessage response = await Client.SendAsync(AddHeaderToRequest(request));
            // 获取响应
            string json = await response.Content.ReadAsStringAsync();
            // 格式化响应
            List<AppTask> tasks = GetListResponseData<AppTask>(json);

            return tasks;
        }

        public List<AppTask> FilterTodayTargetOutOfTasks(List<AppTask> tasks, Tag todayTargetTag)
        {
            List<AppTask> todayTargetTasks = new List<AppTask>();
            foreach (AppTask task in tasks)
            {
                if (task.Tags.Contains(todayTargetTag.Id))
                {
                    todayTargetTasks.Add(task);
                }
            }
            return todayTargetTasks;
        }

        public async Task<List<Tag>> GetAllTags()
        {
            // 发起请求
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "tags");
            HttpResponseMessage response = await Client.SendAsync(AddHeaderToRequest(request));
            // 获取响应
            string json = await response.Content.ReadAsStringAsync();
            // 格式化响应
            List<Tag> tags = GetListResponseData<Tag>(json);

            return tags;
        }

        public async Task<Tag> CreateTag(string name)
        {
            // 构造请求
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "tags");
            request = AddHeaderToRequest(request);
            request.Content = new StringContent($"{{\"name\": \"{name}\"}}");
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json"); // 必须设置请求内容为 JSON 格式，否则服务器无法识别
            // 发起请求
            HttpResponseMessage response = await Client.SendAsync(request);
            // 获取响应
            string json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(json);
            Tag tag = GetObjectResponseData<Tag>(json);

            return tag;
        }

        private List<Model> GetListResponseData<Model>(string json)
        {
            JObject jObject = JObject.Parse(json);
            IList<JToken> data = jObject["data"].Children().ToList();
            List<Model> result = new List<Model>();
            foreach (JToken item in data)
            {
                Model model = item.ToObject<Model>();
                result.Add(model);
            }
            return result;
        }

        private Model GetObjectResponseData<Model>(string json)
        {
            JObject result = JObject.Parse(json);
            Model data = result["data"].ToObject<Model>();
            return data;
        }

        private HttpRequestMessage AddHeaderToRequest(HttpRequestMessage request)
        {
            request.Headers.Add("x-api-user", Setting.UserId);
            request.Headers.Add("x-api-key", Setting.ApiToken);
            request.Headers.Add("x-client", ClientToken);
            return request;
        }

        private async Task<Tag> GetTodayTargetTag()
        {
            List<Tag> tags = await GetAllTags();
            foreach (Tag tag in tags)
            {
                if (tag.Name == "TodayTarget")
                {
                    return tag;
                }
            }
            return null;
        }
    }
}
