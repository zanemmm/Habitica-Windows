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

        private const string BaseUrl = "https://habitica.com/api/v3/";

        private const string ClientToken = "88e6cb45-5700-4643-b866-f333aa3539b5-WindowsHabitica";

        public Setting Setting;

        public HttpApi(Setting setting)
        {
            Setting = setting;
        }

        public async Task<List<AppTask>> GetAllTasks()
        {
            // 发起请求
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + "tasks/user");
            HttpResponseMessage response = await Client.SendAsync(AddHeaderToRequest(request));
            // 获取响应
            string json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(json);
            // 格式化响应
            List<AppTask> tasks = GetListResponseData<AppTask>(json);

            return tasks;
        }

        public async Task<AppTask> CreateTask(string text, string type, string[] tags = null, string date = null)
        {
            // 构造请求
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "tasks/user");
            request = AddHeaderToRequest(request);
            JObject c = new JObject
            {
                { "text", text },
                { "type", type },
            };
            if (tags != null)
            {
                c.Add("tags", new JArray(tags));
            }
            if (date != null)
            {
                c.Add("date", date);
            }
            Debug.WriteLine(c.ToString());
            request.Content = new StringContent(c.ToString());
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json"); // 必须设置请求内容为 JSON 格式，否则服务器无法识别
            // 发起请求
            HttpResponseMessage response = await Client.SendAsync(request);
            // 获取响应
            string json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(json);
            AppTask newTask = GetObjectResponseData<AppTask>(json);

            return newTask;
        }

        public async Task<bool> DeleteTask(string id)
        {
            // 构造请求
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Delete, BaseUrl + $"tasks/{id}");
            request = AddHeaderToRequest(request);
            // 发起请求
            HttpResponseMessage response = await Client.SendAsync(request);
            // 获取响应
            string json = await response.Content.ReadAsStringAsync();

            return GetResponseStatus(json);
        }

        public async Task<bool> ScoreTask(string id, string direction)
        {
            // 构造请求
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + $"tasks/{id}/score/{direction}");
            request = AddHeaderToRequest(request);
            // 发起请求
            HttpResponseMessage response = await Client.SendAsync(request);
            // 获取响应
            string json = await response.Content.ReadAsStringAsync();

            return GetResponseStatus(json);
        }

        public List<AppTask> TodayTargetTaskFilter(List<AppTask> tasks, Tag todayTargetTag)
        {
            List<AppTask> todayTargetTasks = new List<AppTask>();
            foreach (AppTask task in tasks)
            {
                if (task.Tags.Contains(todayTargetTag.Id) && task.Type == AppTask.TypeToString(TaskType.Todo))
                {
                    todayTargetTasks.Add(task);
                }
            }

            return todayTargetTasks;
        }

        public List<AppTask> DailyTargetTaskFilter(List<AppTask> tasks)
        {
            List<AppTask> dailyTargetTasks = new List<AppTask>();
            foreach (AppTask task in tasks)
            {
                if (task.Type == AppTask.TypeToString(TaskType.Daily))
                {
                    dailyTargetTasks.Add(task);
                }
            }

            return dailyTargetTasks;
        }

        public List<AppTask> PlanTargetTaskFilter(List<AppTask> tasks, Tag todayTargetTag)
        {
            List<AppTask> planTargetTasks = new List<AppTask>();
            foreach (AppTask task in tasks)
            {
                if (!task.Tags.Contains(todayTargetTag.Id) && task.Type == AppTask.TypeToString(TaskType.Todo))
                {
                    planTargetTasks.Add(task);
                }
            }

            return planTargetTasks;
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

        public Tag TodayTargetTagFilter(List<Tag> tags)
        {
            foreach (Tag tag in tags)
            {
                if (tag.Name == "TodayTarget")
                {
                    return tag;
                }
            }
            return null;
        }

        private bool GetResponseStatus(string json)
        {
            JObject result = JObject.Parse(json);
            return result["success"].ToObject<bool>();
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
    }
}
