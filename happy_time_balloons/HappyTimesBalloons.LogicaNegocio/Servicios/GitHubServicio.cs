using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HappyTimesBalloons.Abstraccion.Interfaces.Servicios;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HappyTimesBalloons.LogicaNegocio.Servicios
{
    public class GitHubServicio : IGitHubServicio
    {
        private static readonly HttpClient _http = new HttpClient();

        private static string Token => ConfigurationManager.AppSettings["GitHub:Token"] ?? "";
        private static string Owner => ConfigurationManager.AppSettings["GitHub:Owner"] ?? "";
        private static string Repo => ConfigurationManager.AppSettings["GitHub:Repo"] ?? "";

        public async Task<string> CrearIssueAsync(string titulo, string cuerpo, string[] labels)
        {
            var url = $"https://api.github.com/repos/{Owner}/{Repo}/issues";

            var payload = new
            {
                title = titulo,
                body = cuerpo,
                labels = labels
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("token", Token);
                request.Headers.UserAgent.Add(new ProductInfoHeaderValue("HappyTimesBalloons", "1.0"));
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
                request.Content = content;

                using (var response = await _http.SendAsync(request))
                {
                    var body = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new InvalidOperationException(
                            $"GitHub API respondió {(int)response.StatusCode}: {response.ReasonPhrase}");
                    }

                    var obj = JObject.Parse(body);
                    return obj["html_url"]?.ToString() ?? url;
                }
            }
        }
    }
}
