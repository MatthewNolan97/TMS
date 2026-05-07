using TMS_SharedLibrary.Models;

namespace TMSTeacher.Services {
    public class TeacherAPIService : ITeacherAPIService {
        private readonly HttpClient _httpClient;
        public TeacherAPIService(HttpClient httpClient) {
            _httpClient = httpClient;
        }
        public async Task<Teacher?> Get(int id) {
            try {
                var response = await _httpClient.GetAsync($"Teachers/{id}");
                if (response.IsSuccessStatusCode) {
                    var teacher = await response.Content.ReadFromJsonAsync<Teacher>();
                    return teacher;
                } else {
                    return null;
                }
            } catch (HttpRequestException ex) {
                throw new Exception("Error retrieving Teacher from API.", ex);
            }
        }

        public async Task<List<Teacher>> GetAll() {
            try {
                var response = await _httpClient.GetAsync("Teachers");  
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<Teacher>>() ?? new List<Teacher>();
            } catch (HttpRequestException ex) {
                throw new Exception("Error retrieving Teacher from API.", ex);
            }
        }

        public async Task<Teacher> Create(Teacher teacher)
        {
            try 
            {
                var response = await _httpClient.PostAsJsonAsync("Teachers", teacher);
                response.EnsureSuccessStatusCode();
                
                var createdTeacher = await response.Content.ReadFromJsonAsync<Teacher>();
                if (createdTeacher == null)
                {
                    throw new Exception("Failed to deserialize the created teacher.");
                }
                
                return createdTeacher;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error creating teacher in the API.", ex);
            }
        }

    }
}
