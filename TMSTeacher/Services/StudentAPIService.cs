using Humanizer;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using TMS_SharedLibrary.Models;
using TMSTeacher.Models;

namespace TMSTeacher.Services
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
        public async Task<List<ToyLoan>> GetBorrowHistory(int studentId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"Students/{studentId}/BorrowHistory");
                response.EnsureSuccessStatusCode();
                var loanDTOs = await response.Content.ReadFromJsonAsync<List<ToyLoanDTO>>() ?? new List<ToyLoanDTO>();

                var toyLoanTasks = loanDTOs.Select(async dto =>
                {
                    Toy? toy = null;

                    try
                    {
                        var toyResponse = await _httpClient.GetAsync($"Toys/{dto.ToyId}");

                        if (toyResponse.IsSuccessStatusCode)
                        {
                            toy = await toyResponse.Content.ReadFromJsonAsync<Toy>();
                        }
                        else
                        {
                            _logger.LogWarning($"Toy {dto.ToyId} returned {toyResponse.StatusCode}. Using loan record data.");
                            toy = new Toy
                            {
                                ToyId = dto.ToyId,
                                Name = dto.Name ?? $"Toy #{dto.ToyId}",
                                Description = "This toy is no longer available",
                                IsActive = false,
                                Category = "Unknown",
                                Material = "Unknown"
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error fetching toy {dto.ToyId}. Using loan record data.");
                        toy = new Toy
                        {
                            ToyId = dto.ToyId, 
                            Name = dto.Name ?? $"Toy #{dto.ToyId}",
                            Description = "Unable to load current toy details",
                            IsActive = false,
                            Category = "Unknown",
                            Material = "Unknown"
                        };
                    }

                    return new ToyLoan
                    {
                        Toy = toy,
                        LoanId = dto.LoanId,
                        ToyId = dto.ToyId,
                        StudentId = dto.StudentId,
                        BorrowDate = dto.BorrowDate,
                        DueDate = dto.DueDate,
                        ReturnDate = dto.ReturnDate
                    };
                });

                var toyLoans = await Task.WhenAll(toyLoanTasks);
                return toyLoans.ToList();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"Error retrieving borrow history for student ID {studentId} from API.");
                throw new Exception($"Error retrieving borrow history for student ID {studentId} from API.", ex);
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

        public async Task Create(Student student)
        {
            try
            {
                _logger.LogInformation($"Attempting to create student with UserId: {student.UserId}");
                var response = await _httpClient.PostAsJsonAsync("Students", student);
                response.EnsureSuccessStatusCode();
                _logger.LogInformation($"Successfully created student with UserId: {student.UserId}");
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, $"HTTP error creating student with UserId: {student.UserId}. Status: {httpEx.StatusCode}, Message: {httpEx.Message}");
                throw new Exception($"Error creating student with UserId: {student.UserId} via API. Status: {httpEx.StatusCode}", httpEx);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Unexpected error creating student with UserId: {student.UserId}. Error: {ex.Message}");
                throw;
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
    }
}
