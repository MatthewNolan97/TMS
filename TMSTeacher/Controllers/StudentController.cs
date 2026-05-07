using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using TMS_SharedLibrary.Models;
using TMS_SharedLibrary.ViewModels;
using TMSTeacher.Services;

namespace TMSTeacher.Controllers
{
    [Authorize(Policy = "RequireTeacherRole")]
    public class StudentController : Controller
    {
        private readonly ReportsPDFService _pdfService;
        private readonly GraphServiceClient _graphServiceClient;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly IStudentAPIService _studentAPIService;
        private readonly IUserAPIService _userService;
        private readonly ILogger<StudentController> _logger;

        public StudentController(
            GraphServiceClient graphServiceClient,
            ITokenAcquisition tokenAcquisition,
            IStudentAPIService studentAPIService,
            ILogger<StudentController> logger,
            IUserAPIService userAPIService,
            ReportsPDFService pdfService)
        {
            _pdfService = pdfService;
            _graphServiceClient = graphServiceClient;
            _tokenAcquisition = tokenAcquisition;
            _studentAPIService = studentAPIService;
            _userService = userAPIService;
            _logger = logger;
        }

        [Authorize]
        public async Task<IActionResult> BorrowHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var student = await _studentAPIService.Get(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                var borrowHistory = await _studentAPIService.GetBorrowHistory(id.Value);

                try
                {
                    var user = await _graphServiceClient.Users[student.User?.Oid]
                        .GetAsync(requestConfiguration =>
                        {
                            requestConfiguration.QueryParameters.Select = new[] { "givenName", "surname" };
                        });

                    ViewBag.StudentName = $"{user?.GivenName} {user?.Surname}";
                    ViewBag.StudentId = id.Value;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching user details from Microsoft Graph");
                    ViewBag.StudentName = "Student";
                }

                return View(borrowHistory);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving student borrow history");
                return StatusCode(500, "An error occurred while retrieving the borrow history.");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Download(int? studentId)
        {
            if (studentId == null)
            {
                return NotFound();
            }

            try
            {
                var student = await _studentAPIService.Get(studentId.Value);
                if (student == null)
                {
                    return NotFound();
                }

                var loans = await _studentAPIService.GetBorrowHistory(studentId.Value);

                string studentName = "Student";

                try
                {
                    var user = await _graphServiceClient.Users[student.User?.Oid]
                        .GetAsync(requestConfiguration =>
                        {
                            requestConfiguration.QueryParameters.Select = new[] { "givenName", "surname" };
                        });
                    studentName = $"{user?.GivenName} {user?.Surname}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching user details from Microsoft Graph");
                }

                var pdfBytes = _pdfService.GenerateBorrowHistoryPdf(loans, studentName);
                var safeStudentName = string.Join("_", studentName.Split(Path.GetInvalidFileNameChars()));
                var fileName = $"BorrowHistory_{safeStudentName}_{DateTime.Now:yyyyMMdd}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating or downloading PDF");
                return StatusCode(500, "An error occurred while generating the PDF.");
            }
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                var studentViewModels = new List<StudentViewModel>();
                var students = await _studentAPIService.GetAll();

                foreach (var student in students)
                {
                    if (student.User == null || string.IsNullOrEmpty(student.User.Oid))
                    {
                        studentViewModels.Add(new StudentViewModel
                        {
                            StudentId = student.StudentId,
                            FirstName = "User",
                            LastName = "Not Found",
                            Email = "N/A",
                            Oid = "Missing OID"
                        });
                        continue;
                    }

                    try
                    {
                        var user = await _graphServiceClient.Users[student.User.Oid]
                            .GetAsync(requestConfig =>
                            {
                                requestConfig.QueryParameters.Select = new[]
                                {
                                    "givenName",
                                    "surname",
                                    "userPrincipalName",
                                    "displayName"
                                };
                            });

                        studentViewModels.Add(new StudentViewModel
                        {
                            StudentId = student.StudentId,
                            FirstName = user?.GivenName ?? "No",
                            LastName = user?.Surname ?? "Name",
                            Email = user?.UserPrincipalName ?? "No email",
                            Oid = student.User.Oid
                        });
                    }
                    catch (ServiceException ex) when (ex.ResponseStatusCode == (int)System.Net.HttpStatusCode.NotFound)
                    {
                        studentViewModels.Add(new StudentViewModel
                        {
                            StudentId = student.StudentId,
                            FirstName = "User",
                            LastName = "Not Found in AD",
                            Email = "N/A",
                            Oid = student.User.Oid
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading student data");
                        studentViewModels.Add(new StudentViewModel
                        {
                            StudentId = student.StudentId,
                            FirstName = "Error",
                            LastName = "Loading Data",
                            Email = "Check logs",
                            Oid = student.User.Oid
                        });
                    }
                }

                return View(studentViewModels);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading students");
                return RedirectToAction("Error", "Home", new { message = ex.Message });
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var student = await _studentAPIService.Get(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                return View(student);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id}");
                return StatusCode(500, "An error occurred while retrieving the student details.");
            }
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var student = await _studentAPIService.Get(id.Value);
                if (student == null)
                {
                    return NotFound();
                }
                return View(student);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id} for editing");
                return StatusCode(500, "An error occurred while retrieving the student for editing.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StudentId,UserId,Placement,Year")] Student student)
        {
            if (id != student.StudentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _studentAPIService.Update(student);
                    return RedirectToAction(nameof(Index));
                }
                catch (MicrosoftIdentityWebChallengeUserException)
                {
                    _logger.LogInformation("Triggering consent flow for API access");
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating student with ID {id}");
                    ModelState.AddModelError("", "An error occurred while updating the student.");
                }
            }
            return View(student);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var student = await _studentAPIService.Get(id.Value);
                if (student == null)
                {
                    return NotFound();
                }

                return View(student);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving student with ID {id} for deletion");
                return StatusCode(500, "An error occurred while retrieving the student for deletion.");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _studentAPIService.Delete(id);
                return RedirectToAction(nameof(Index));
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                _logger.LogInformation("Triggering consent flow for API access");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting student with ID {id}");
                return StatusCode(500, "An error occurred while deleting the student.");
            }
        }
    }
}