using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiversifiedPortfolio.Core.Extensions
{
    public static class HttpResponseMessageHelpers
    {
        private static readonly JsonSerializerOptions options;

        static HttpResponseMessageHelpers()
        {
            options = new JsonSerializerOptions
            {
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public static async Task<T> GetResponseAsync<T>(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.IsSuccessStatusCode == false)
            {
                Console.WriteLine($"HTTP request failed: {httpResponseMessage.ReasonPhrase}");
                var failure = await httpResponseMessage.Content.ReadAsStringAsync();
                throw new HttpRequestException(failure);
            }

            var result = await httpResponseMessage.Content.ReadAsStringAsync();
            if (typeof(T) == typeof(string))
            {
                var @object = (object)result;
                return (T)@object;
            }

            return JsonSerializer.Deserialize<T>(result, options);
        }
    }
}
