using CRUDDatabaseFirst.Model;
using CRUDDatabaseFirst.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;

namespace CRUDDatabaseFirst.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly StudentsContext stdContext;

        public HomeController(ILogger<HomeController> logger, StudentsContext stdContext)
        {
            _logger = logger;
            this.stdContext = stdContext;
        }

        public IActionResult Index()
        {
            var studentData = stdContext.StudentInfos.ToList();

            if (!studentData.Any())
            {
                TempData["StudentNotFound"] = "No student found";
            }

            return View(studentData);
        }

        /*=========================Details=========================*/
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || stdContext.StudentInfos == null)
            {
                return NotFound();
            }

            var studentData = await stdContext.StudentInfos.FirstOrDefaultAsync(x => x.StudentId == id);
            if (studentData == null)
            {
                return NotFound();
            }

            return View(studentData);
        }

        /*=========================Create New=========================*/
        public IActionResult CreateStudent()
        {
            return View();
        }

        [HttpPost, ActionName("CreateStudent")]
        public async Task<IActionResult> Create(StudentInfo std)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await stdContext.StudentInfos.AddAsync(std);
                    await stdContext.SaveChangesAsync();

                    TempData["Message"] = "Student Inserted Successfully";

                    // Redirect to Index or another view after successful creation
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the exception and show an error message if necessary
                    ModelState.AddModelError(string.Empty, "An error occurred while saving data.");
                }
            }

            // If ModelState is invalid, or if an error occurred, return to the form
            return View(std);
        }


        /*=========================Update=========================*/

        public async Task<IActionResult> UpdateStudent(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await stdContext.StudentInfos.FirstOrDefaultAsync(x => x.StudentId == id);

            if (student == null)
            {
                return NotFound();
            }



            return View(student);
        }

        [HttpPost]
        public async Task<IActionResult> Update(StudentInfo std)
        {
            if (std == null)
            {
                return NotFound();
            }

            // Ensure the student exists in the database before updating
            var existingStudent = await stdContext.StudentInfos.FindAsync(std.StudentId);
            if (existingStudent == null)
            {
                return NotFound();
            }

            // Update the existing student record
            stdContext.Entry(existingStudent).CurrentValues.SetValues(std);

            // Validate model before saving
            if (!ModelState.IsValid)
            {
                return View(std);  // Return the view with the invalid model to display errors
            }

            await stdContext.SaveChangesAsync();

            TempData["Message"] = "Updated Successfully";
            return RedirectToAction("Index");
        }

        /*=========================Delete=========================*/
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var student = await stdContext.StudentInfos.FirstOrDefaultAsync(std => std.StudentId == id);
            if (student == null)
            {
                return NotFound();
            }


            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> ConfirmDelete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            // Find the student by id
            var student = await stdContext.StudentInfos.FindAsync(id);

            if (student == null)
            {
                return NotFound(); // If student doesn't exist, return NotFound
            }

            // Remove the student record
            stdContext.StudentInfos.Remove(student);
            await stdContext.SaveChangesAsync();

            // Set a success message for TempData
            TempData["Message"] = "Student deleted successfully!";
            return RedirectToAction("Index"); // Redirect to the index page after successful delete





        }





        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
