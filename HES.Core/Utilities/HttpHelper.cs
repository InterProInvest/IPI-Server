using System.Text;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Core.Utilities
{
    public class HttpHelper
    {
        public StringContent GetJsonContentFromObject<T>(T obj)
        {
            var data = JsonConvert.SerializeObject(obj);
            return new StringContent(data, Encoding.UTF8, "application/json");
        }

        public async Task<T> GetObjectFromResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}
