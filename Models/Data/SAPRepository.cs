using AppCompras.Models;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AppCompras.Models.Data
{
    public class SAPRepository (HttpClient http, IOptions<ApiSettings> _settings)
    {
        private readonly ApiSettings settings = _settings.Value;
        public async Task<string> ConectarApi(object requisicao, string api)
        {
            var url = $"{settings.BaseUrl}/{api}";

            var json = JsonSerializer.Serialize(requisicao);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            http.DefaultRequestHeaders.Authorization =
                AuthenticationHeaderValue.Parse(settings.AuthToken);

            var response = await http.PostAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();
          
            return responseContent;
        }

        // equivalente ao busca_sap_api_query
        public async Task<string> BuscarSapQuery(object query)
        {
            return await ConectarApi(query, "query");
        }
    }
}
