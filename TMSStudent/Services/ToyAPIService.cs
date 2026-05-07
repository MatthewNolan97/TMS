using Newtonsoft.Json.Linq;
using TMS_SharedLibrary.Models;

namespace TMSStudent.Services {
    public class ToyAPIService : IToyAPIService {
        private readonly HttpClient _httpClient;
        public ToyAPIService(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<bool> BorrowToy(int studentid, int toyid) {
            try {
                var response = await _httpClient.PutAsync($"Students/BorrowToy/{studentid}?toyId={toyid}", null);
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException ex) {
                throw new Exception("Error Borrowing Toy.", ex);
            }
        }
        public async Task<bool> ReturnToy(int studentid, int toyid) {
            try {
                var response = await _httpClient.PutAsJsonAsync($"Students/ReturnToy/{studentid}?toyId={toyid}", "");
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException ex) {
                throw new Exception("Error Borrowing Toy.", ex);
            }
        }

        public async Task<bool> Create(Toy toy) {
            try {
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
            var response = await _httpClient.GetAsync($"Toys/filter{queryString}");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<Toy>>() ?? new List<Toy>();
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
                return await response.Content.ReadFromJsonAsync<IEnumerable<Toy>>();
            } catch (HttpRequestException ex) {
                throw new Exception("Error retrieving Toys from API.", ex);
            }
        }

        public async Task<bool> UpdateToy(int id, Toy t) {
            try {
                var response = await _httpClient.PutAsJsonAsync($"Toys/{id}", t);
                return response.IsSuccessStatusCode;
            } catch (HttpRequestException ex) {
                throw new Exception("Error Updating Toy.", ex);
            }
        }
    }
}
