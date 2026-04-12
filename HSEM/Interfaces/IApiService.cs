using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSEM.Interfaces
{
    public interface IApiService
    {
        Task<HttpResponseMessage> GetWithTokenAsync(string url, string token);
        Task<HttpResponseMessage> PostWithTokenAsync(string url, HttpContent content, string token);

        // Wrappers جديدة
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, object body);
    }

}
