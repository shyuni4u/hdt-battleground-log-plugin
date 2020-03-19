using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BGLogPlugin
{
    public static class Api
    {
        public static async Task Post(string ApiServer, string payload)
        {
            Uri url = new Uri(ApiServer);
            HttpContent content = new StringContent(payload, Encoding.UTF8, "application/json");
            using (var client = new HttpClient())
            {
                HttpResponseMessage result = await client.PostAsync(url, content);
                if (result.StatusCode == HttpStatusCode.OK)
                {
                    //string resultContent = await result.Content.ReadAsStringAsync();
                    //Console.WriteLine(resultContent);
                    //  do something if you want to control response
                }
                else
                {
                    //  do something if you want to control response
                }
            }
        }
    }
}
