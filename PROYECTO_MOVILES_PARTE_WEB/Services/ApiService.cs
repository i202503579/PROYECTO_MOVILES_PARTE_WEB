using Newtonsoft.Json;
using System.Text;

namespace PROYECTO_MOVILES_PARTE_WEB.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
            DateFormatString = "yyyy-MM-ddTHH:mm:ss"
        };

        public ApiService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _http = httpClientFactory.CreateClient();
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://proyectomovilespartewebapi-cpaafpdddafvgybe.chilecentral-01.azurewebsites.net/api/";
            if (!_baseUrl.EndsWith("/"))
            {
                _baseUrl += "/";
            }
        }

        public async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}{endpoint}");
                if (!response.IsSuccessStatusCode) return new List<T>();
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<T>>(json, _jsonSettings) ?? new List<T>();
            }
            catch
            {
                return new List<T>();
            }
        }

        public async Task<T?> GetByIdAsync<T>(string endpoint)
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}{endpoint}");
                if (!response.IsSuccessStatusCode) return default;
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(json, _jsonSettings);
            }
            catch
            {
                return default;
            }
        }

        public async Task<(bool ok, string mensaje)> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PostAsync($"{_baseUrl}{endpoint}", content);
                var body = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool ok, string mensaje)> PutAsync<T>(string endpoint, T data)
        {
            try
            {
                var json = JsonConvert.SerializeObject(data, _jsonSettings);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _http.PutAsync($"{_baseUrl}{endpoint}", content);
                var body = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool ok, string mensaje)> DeleteAsync(string endpoint)
        {
            try
            {
                var response = await _http.DeleteAsync($"{_baseUrl}{endpoint}");
                var body = await response.Content.ReadAsStringAsync();
                return (response.IsSuccessStatusCode, body);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
}
