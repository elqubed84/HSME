using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Services
{
    public static class HttpHelper
    {
        public static async Task<HttpResponseMessage> GetWithAuthAsync(string url, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return await HttpClientFactory.Instance.SendAsync(request);
        }

        public static async Task<HttpResponseMessage> PostWithAuthAsync(
            string url, HttpContent content, string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = content };
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return await HttpClientFactory.Instance.SendAsync(request);
        }
    }

}
