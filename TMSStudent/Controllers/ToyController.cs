using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using TMS_SharedLibrary.Models;
using TMSStudent.Services;

namespace TMSStudent.Controllers
{
    public class ToyController : Controller
    {
        private readonly IToyAPIService _toyAPIService;
        private readonly IStudentAPIService _studentAPIService;
        private readonly ITokenAcquisition _tokenAcquisition;

        public ToyController(IToyAPIService toyAPIService, IStudentAPIService studentAPIService, ITokenAcquisition tokenAcquisition)
        {
            _toyAPIService = toyAPIService;
            _studentAPIService = studentAPIService;
            _tokenAcquisition = tokenAcquisition;
        }

        public async Task<IActionResult> Index(string categories = null, string materials = null)
        {
            try
            {
                var toys = await _toyAPIService.GetAll();

                if (!string.IsNullOrEmpty(categories))
                {
                    toys = toys.Where(t => t.Category == categories).ToList();
                    ViewBag.SelectedCategory = categories;
                }

                if (!string.IsNullOrEmpty(materials))
                {
                    toys = toys.Where(t => t.Material == materials).ToList();
                    ViewBag.SelectedMaterial = materials;
                }

                var oid = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                var studentID = await _studentAPIService.GetStudentID(oid!);

                if (studentID == null)
                {
                    TempData["ErrorMessage"] = "Failed to find the student ID.";
                    return View(new List<ToyViewModel>());
                }

                var currentLoans = await _studentAPIService.GetCurrentLoans((int)studentID);
                var borrowedToyIds = currentLoans.Select(cl => cl.ToyId).ToHashSet();

                var toyViewModels = toys.Select(t => new ToyViewModel
                {
                    Toy = t,
                    BorrowedByCurrentStudent = borrowedToyIds.Contains(t.ToyId)
                })
                .OrderByDescending(vm => vm.BorrowedByCurrentStudent)
                .ThenByDescending(vm => vm.Toy.IsAvailable)
                .ThenBy(vm => vm.Toy.Name)
                .ToList();

                return View(toyViewModels);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(new List<ToyViewModel>());
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var toy = await _toyAPIService.Get(id);

                if (toy == null)
                    return NotFound("Toy not found");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return PartialView("_ToyDetailsPartial", toy);

                return View(toy);
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                throw;
            }
        }

        public async Task<IActionResult> Borrow(int id)
        {
            try
            {
                var oid = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                var studentID = await _studentAPIService.GetStudentID(oid!);

                if (studentID == null)
                {
                    TempData["ErrorMessage"] = "Failed to find the student ID.";
                    return RedirectToAction("Index");
                }

                bool success = await _toyAPIService.BorrowToy((int)studentID, id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Toy successfully reserved!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to borrow the toy. It may no longer be available.";
                }

                return RedirectToAction("Index");
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Return(int id)
        {
            try
            {
                var oid = User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
                var studentID = await _studentAPIService.GetStudentID(oid!);

                if (studentID == null)
                {
                    TempData["ErrorMessage"] = "Failed to find the student ID.";
                    return RedirectToAction("Index");
                }

                bool success = await _toyAPIService.ReturnToy((int)studentID, id);

                if (success)
                {
                    TempData["SuccessMessage"] = "Toy successfully returned!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to return the toy.";
                }

                return RedirectToAction("Index");
            }
            catch (MicrosoftIdentityWebChallengeUserException)
            {
                throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}