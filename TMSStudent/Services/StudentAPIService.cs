using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using TMS_SharedLibrary.Models;
using TMSStudent.Models;

namespace TMSStudent.Services
{
    public class StudentAPIService : IStudentAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StudentAPIService> _logger;
        
        public StudentAPIService(HttpClient httpClient, ILogger<StudentAPIService> logger) 
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<List<Student>> GetAll()
        {
            try
            {
                var response = await _httpClient.GetAsync("Students");
                response.EnsureSuccessStatusCode();
                
                var studentDtos = await response.Content.ReadFromJsonAsync<List<StudentDto>>() 
                    ?? new List<StudentDto>();
                
                return studentDtos.Select(dto => new Student
                {
                    StudentId = dto.StudentId,
                    UserId = dto.UserId,
                    Placement = dto.Placement,
                    Year = dto.Year,
                    User = new User 
                    { 
                        UserId = dto.UserId,
                        Oid = dto.OID,
                        UserType = "Student"
                    }
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving students from API");
                throw new Exception("Error retrieving students from API.", ex);
            }
        }

        public async Task<Student?> Get(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Students/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var dto = await response.Content.ReadFromJsonAsync<StudentDto>();
                    if (dto == null) return null;
                    
                    return new Student
                    {
                        StudentId = dto.StudentId,
                        UserId = dto.UserId,
                        Placement = dto.Placement,
                        Year = dto.Year,
                        User = new User 
                        { 
                            UserId = dto.UserId,
                            Oid = dto.OID,
                            UserType = "Student"
                        }
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id} from API");
                throw new Exception($"Error retrieving student with ID {id} from API.", ex);
            }
        }
        public async Task<int?> GetStudentID(string oid)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Students/ByOID/{oid}");
                if (response.IsSuccessStatusCode)
                {
                    var id = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(id)) return null;

                    if (int.TryParse(id, out var sid))
                    {
                        return sid;
                    }
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student ID with OID {oid} from API");
                throw new Exception($"Error retrieving student ID with OID {oid} from API.", ex);
            }
        }

        public async Task Create(Student student)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("Students", student);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error creating student via API.", ex);
            }
        }

        public async Task Update(Student student)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"Students/{student.StudentId}", student);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error updating student with ID {student.StudentId} via API.", ex);
            }
        }

        public async Task Delete(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"Students/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error deleting student with ID {id} via API.", ex);
            }
        }

        public async Task<List<User>> GetUsers()
        {
            try
            {
                var response = await _httpClient.GetAsync("Users");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<User>>() ?? new List<User>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error retrieving users from API.", ex);
            }
        }

        public async Task<List<ToyLoan>> GetBorrowHistory(int studentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Students/{studentId}/BorrowHistory");
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ToyLoan>>() ?? new List<ToyLoan>();
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Error retrieving borrow history for student ID {studentId} from API.", ex);
            }
        }

        //public async Task<List<ToyLoan>> GetActiveLoans(int studentId)
        //{
        //    try
        //    {
        //        var response = await _httpClient.GetAsync($"/Students/{studentId}/ActiveLoans");

        //        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        //        {
        //            return new List<ToyLoan>();
        //        }

        //        response.EnsureSuccessStatusCode();
        //        return await response.Content.ReadFromJsonAsync<List<ToyLoan>>() ?? new List<ToyLoan>();
        //    }
        //    catch (HttpRequestException ex)
        //    {
        //        Console.WriteLine($"Error retrieving active loans: {ex.Message}");
        //        return new List<ToyLoan>();
        //    }
        //}

        public async Task<List<ToyLoan>> GetCurrentLoans(int studentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Students/{studentId}/CurrentLoans");

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<ToyLoan>();
                }

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<List<ToyLoan>>() ?? new List<ToyLoan>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error retrieving current loans: {ex.Message}");
                return new List<ToyLoan>();
            }
        }
    }
}
