using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace OfflineXPlanner.Extensions
{
    public static class HttpContentExtension
    {
        public static T ReadAs<T>(this HttpContent response)
        {
            var task = response.ReadAsAsync<T>();
            task.Wait();
            return task.Result;
        }
    }
}
