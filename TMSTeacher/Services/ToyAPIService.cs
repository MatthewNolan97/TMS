using System.Net.Http.Headers;
using System.Text.Json;
using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services {
    public class ToyAPIService : IToyAPIService {
        private readonly HttpClient _httpClient;
        public ToyAPIService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<bool> Create(Toy toy) {
            try {
                toy.IsActive = true;
                var response = await _httpClient.PostAsJsonAsync("Toys", toy);
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException ex) {
                throw new Exception("Error Creating Toy", ex);
            }
        }

        public async Task<bool> Delete(int id) {
            try {
                var response = await _httpClient.DeleteAsync($"Toys/{id}");
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException ex) {
                throw new Exception("Error Deleting Toy.", ex);
            }
        }

        public async Task<Toy?> Get(int id) {
            try {
                var response = await _httpClient.GetAsync($"Toys/{id}");
                if (response.IsSuccessStatusCode) {
                    var toy = await response.Content.ReadFromJsonAsync<Toy>();
                    return toy;
                } else {
                    return null;
                }
            } catch (HttpRequestException ex) {
                throw new Exception("Error retrieving Toys from API.", ex);
            }
        }

        public async Task<IEnumerable<Toy>> GetAll() {
            try {
                var response = await _httpClient.GetAsync("Toys");
                response.EnsureSuccessStatusCode();
                var toys = await response.Content.ReadFromJsonAsync<IEnumerable<Toy>>() ?? Enumerable.Empty<Toy>();
                return toys.Where(t => t.IsActive);
            } catch (HttpRequestException ex) {
                throw new Exception("Error retrieving Toys from API.", ex);
            }
        }

        public async Task<IEnumerable<Toy>> GetFiltered(string[]? materials, string[]? categories) {
            var queryParams = new List<string>();

            if (materials != null && materials.Length > 0) {
                queryParams.AddRange(materials
                    .Where(m => !string.IsNullOrEmpty(m))
                    .Select(m => $"materials={Uri.EscapeDataString(m.Trim())}"));
            }

            if (categories != null && categories.Length > 0) {
                queryParams.AddRange(categories
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Select(c => $"categories={Uri.EscapeDataString(c.Trim())}"));
            }

            var queryString = queryParams.Count > 0 ? $"?{string.Join("&", queryParams)}" : "";
            var response = await _httpClient.GetAsync($"/Toys/filter{queryString}");
            response.EnsureSuccessStatusCode();

            var toys = await response.Content.ReadFromJsonAsync<IEnumerable<Toy>>() ?? Enumerable.Empty<Toy>();
            return toys.Where(t => t.IsActive);
        }

        public async Task<bool> UpdateToy(int id, Toy t) {
            try {
                var response = await _httpClient.PutAsJsonAsync($"Toys/{id}", t);
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException ex) {
                throw new Exception("Error Updating Toy.", ex);
            }
        }

        public async Task<string?> UploadToyImageAsync(IFormFile file) {
            using var form = new MultipartFormDataContent();

            await using var fileStream = file.OpenReadStream();
            using var streamContent = new StreamContent(fileStream);

            streamContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType
            );

            form.Add(streamContent, "File", file.FileName);

            var response = await _httpClient.PostAsync("Uploads/toy-image", form);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.TryGetProperty("fileName", out var p)
                ? p.GetString()
                : null;
        }
    }
}
