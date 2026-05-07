using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.ApplicationsWithAppId;
using TMS_SharedLibrary.Models;
using Microsoft.Extensions.Logging;
using TMSTeacher.Services;

namespace TMSTeacher.Controllers {
    [Authorize(Policy = "RequireTeacherRole")]
    public class ToyController : Controller
    {
        private readonly IStudentAPIService _studentAPIService;
        private readonly IToyAPIService _toyAPIService;
        private readonly ITeacherAPIService _teacherAPIService;
        private readonly IWebHostEnvironment _imgs;
        private readonly IConfiguration _config;

        public ToyController(IStudentAPIService studentAPIService, IToyAPIService toyAPIService, ITeacherAPIService teacherAPIService, IWebHostEnvironment imgs, IConfiguration config) {
            _studentAPIService = studentAPIService;
            _toyAPIService = toyAPIService;
            _teacherAPIService = teacherAPIService;
            _imgs = imgs;
            _config = config;
        }

        public async Task<IActionResult> Index(string? materials, string? categories) {
            try {
                ViewBag.SelectedMaterial = materials;
                ViewBag.SelectedCategory = categories;

                string[]? materialArray = !string.IsNullOrEmpty(materials) ? new[] { materials } : null;
                string[]? categoryArray = !string.IsNullOrEmpty(categories) ? new[] { categories } : null;

                IEnumerable<Toy> toys = !string.IsNullOrEmpty(materials) || !string.IsNullOrEmpty(categories)
                    ? await _toyAPIService.GetFiltered(materialArray, categoryArray)
                    : await _toyAPIService.GetAll();

                toys = (toys ?? Enumerable.Empty<Toy>()).Where(t => t.IsActive);

                var sortedToys = (toys ?? new List<Toy>())
                    .OrderByDescending(t => t.IsAvailable)
                    .ThenBy(t => t.Name);

                return View(sortedToys);
            } catch (Exception) {
                TempData["Error"] = "An error occurred while loading toys. Please try again later.";
                return View(new List<Toy>());
            }
        }

        // GET: Toy/Details/5
        public async Task<IActionResult> Details(int id) {

            if (id <= 0)
                return NotFound();

            try {
                var toy = await _toyAPIService.Get(id);

                if (toy == null)
                    return NotFound();

                return View(toy);
            } catch {
                return NotFound();
            }
        }

        // GET: Toy/Create
        public async Task<IActionResult> Create() {
            var teachers = await _teacherAPIService.GetAll();
            ViewData["ManagedBy"] = new SelectList(teachers, "TeacherId", "TeacherId");
            return View();
        }

        // POST: Toy/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ToyId,Name,Description,Category,Material,LocationCode,ImagePath,IsAvailable,ManagedBy,AdditionalInformation")] Toy toy, IFormFile? imageFile) {

            if (ModelState.IsValid) {
                try {
                    if (imageFile != null && imageFile.Length > 0) {
                        var fileName = await _toyAPIService.UploadToyImageAsync(imageFile);

                        if (string.IsNullOrWhiteSpace(fileName)) {
                            ModelState.AddModelError(string.Empty, "Image upload failed.");
                            var teachersFail = await _teacherAPIService.GetAll();
                            ViewData["ManagedBy"] = new SelectList(teachersFail, "TeacherId", "FirstName", toy.ManagedBy);
                            return View(toy);
                        }

                        toy.ImagePath = fileName;
                    }

                    toy.IsActive = true;
                    var success = await _toyAPIService.Create(toy);

                    if (success) {
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "Unable to save toy to API.");
                } catch (Exception) {
                    ModelState.AddModelError(string.Empty,
                        "An error occurred while creating the toy. Please try again later.");
                }
            }

            try {
                var teachers = await _teacherAPIService.GetAll();
                ViewData["ManagedBy"] = new SelectList(teachers, "TeacherId", "FirstName", toy.ManagedBy);
            } catch {
                ViewData["ManagedBy"] = new SelectList(new List<Teacher>(), "TeacherId", "FirstName");
            }
            return View(toy);
        }

        // GET: Toy/Edit/5
        public async Task<IActionResult> Edit(int id) {
            if (id <= 0) {
                return NotFound();
            }
            var toy = await _toyAPIService.Get(id);
            if (toy == null) {
                return NotFound();
            }

            try {
                var teachers = await _teacherAPIService.GetAll();
                ViewData["ManagedBy"] = new SelectList(teachers, "TeacherId", "TeacherId", toy.ManagedBy);
               
            } catch {
                ViewData["ManagedBy"] = new SelectList(new List<Teacher>(), "TeacherId", "TeacherId");
            }

            return View(toy);
        }


        // POST: Toy/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ToyId,Name,Description,Category,Material,LocationCode,ImagePath,IsAvailable,ManagedBy,AdditionalInformation")] Toy toy, IFormFile? imageFile) {

            if (id != toy.ToyId) {
                return NotFound();
            }

            var existing = await _toyAPIService.Get(id);
            if (existing == null) {
                return NotFound();
            }

            if (ModelState.IsValid) {
                try {
                    if (imageFile != null && imageFile.Length > 0) {
                        var fileName = await _toyAPIService.UploadToyImageAsync(imageFile);

                        if (string.IsNullOrWhiteSpace(fileName)) {
                            ModelState.AddModelError(string.Empty, "Image upload failed.");
                            var teachersFail = await _teacherAPIService.GetAll();
                            ViewData["ManagedBy"] = new SelectList(teachersFail, "TeacherId", "TeacherId", toy.ManagedBy);
                            return View(toy);
                        }

                        toy.ImagePath = fileName;
                    } else {
                        toy.ImagePath = existing.ImagePath;
                    }

                    toy.IsActive = existing.IsActive;
                    var success = await _toyAPIService.UpdateToy(id, toy);

                    if (success) {
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError(string.Empty, "Unable to update toy in API.");
                } catch (Exception) {
                    ModelState.AddModelError(string.Empty,
                        "An error occurred while updating the toy. Please try again later.");
                }
            }
            try {
                var teachers = await _teacherAPIService.GetAll();
                ViewData["ManagedBy"] = new SelectList(teachers, "TeacherId", "TeacherId", toy.ManagedBy);
            } catch {
                ViewData["ManagedBy"] = new SelectList(new List<Teacher>(), "TeacherId", "TeacherId");
            }

            return View(toy);
        }

        // GET: Toy/Delete/5
        public async Task<IActionResult> Delete(int id) {
            if (id == null) {
                return NotFound();
            }

            var toy = await _toyAPIService.Get(id);
            if (toy == null) {
                return NotFound();
            }

            return View(toy);
        }

        // POST: Toy/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            try {
                var success = await _toyAPIService.Delete(id);

                if (!success) {
                    TempData["ErrorMessage"] = "Cannot delete this toy because it is currently borrowed.";
                } else {
                    TempData["SuccessMessage"] = "Toy deleted successfully.";
                }
            } catch (Exception) {
                TempData["ErrorMessage"] = "An error occurred while deleting the toy.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
