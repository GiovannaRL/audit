using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net.Http;

namespace OfflineXPlanner.Extensions
{
    public static class HttpClientExtension
    {
        public static HttpResponseMessage Post(this HttpClient client, string requestUri, HttpContent content)
        {
            var task = client.PostAsync(requestUri, content);
            task.Wait();
            return task.Result;
        }
        public static HttpResponseMessage Put(this HttpClient client, string requestUri, HttpContent content)
        {
            var task = client.PutAsync(requestUri, content);
            task.Wait();
            return task.Result;
        }
        public static HttpResponseMessage Get(this HttpClient client, string requestUri, HttpCompletionOption completionOption)
        {
            var task = client.GetAsync(requestUri, completionOption);
            task.Wait();
            return task.Result;
        }
        public static HttpResponseMessage Get(this HttpClient client, string requestUri)
        {
            var task = client.GetAsync(requestUri);
            task.Wait();
            return task.Result;
        }

        public static HttpResponseMessage PutAsJson<T>(this HttpClient client, string requestUri, T objValue)
        {
            var task = client.PutAsJsonAsync(requestUri, objValue);
            task.Wait();
            return task.Result;
        }
        public static HttpResponseMessage PostAsJson<T>(this HttpClient client, string requestUri, T objValue)
        {
            var task = client.PostAsJsonAsync(requestUri, objValue);
            task.Wait();
            return task.Result;
        }
        public static HttpResponseMessage Send(this HttpClient client, HttpRequestMessage request)
        {
            var task = client.SendAsync(request);
            task.Wait();
            return task.Result;
        }
    }
}
