using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore;
using PracticeWithC1stApproach.Models;
using System.Linq;

namespace PracticeWithD1stApproach.Controllers
{

    public class EmployeeController : Controller
    {
        private readonly EmployeeDBContext employeeDBC;

        public EmployeeController(EmployeeDBContext employeeDBC)
        {
            this.employeeDBC = employeeDBC;
        }

        public IActionResult Index()
        {
            return View();
        }

        /************View Operation************/

        public IActionResult AllEmployees()
        {
            var employeeData = employeeDBC.Employees.ToList();
            return View(employeeData); // Pass the data to the view
        }

        public async Task<IActionResult> EmployeeDetail(int? id)
        {
            if (id == 0)
            {
                NotFound();
            }

            var employeeData = await employeeDBC.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);

            if (employeeData == null)
            {
                NotFound();
            }

            return View(employeeData); // Pass the data to the view
        }

        /************Create Operation************/

        public IActionResult CreateEmployee()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(EmployeeModel employee)
        {
            if (ModelState.IsValid)
            {
                await employeeDBC.Employees.AddAsync(employee);
                await employeeDBC.SaveChangesAsync();

                TempData["success"] = "Employee Added";
                return RedirectToAction("allEmployees");
            }
            return View(employee);
        }


        /************Delete Operation************/

        public async Task<IActionResult> DeleteEmployee(int? id)
        {
            if (id == 0)
            {
                NotFound();

            }

            var employeeData = await employeeDBC.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
            if (employeeData == null)
            {
                return NotFound();
            }
            return View(employeeData);
        }

        [HttpPost, ActionName("DeleteEmployee")]
        public async Task<IActionResult> DeleteEmployeeConfirm(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the employee data asynchronously
            var Edata = await employeeDBC.Employees.FindAsync(id);

            if (Edata == null)
            {
                return NotFound();
            }

            // Remove the employee from the database
            employeeDBC.Employees.Remove(Edata);

            // Save changes to the database
            await employeeDBC.SaveChangesAsync();

            TempData["success"] = "Employee Deleted.";

            // Redirect to a view or another action (e.g., index page)
            return RedirectToAction("allEmployees");
        }


        /************Update Operation************/

        public async Task<IActionResult> UpdateEmployee(int? id)
        {
            if (id == 0)
            {
                NotFound();

            }

            var employeeData = await employeeDBC.Employees.FirstOrDefaultAsync(x => x.EmployeeId == id);
            if (employeeData == null)
            {
                return NotFound();
            }
            return View(employeeData);
        }

        [HttpPost, ActionName("UpdateEmployee")]
        public async Task<IActionResult> UpdateProcess(int? id, EmployeeModel employee)
        {
            if (id == 0 || employee == null)
            {
                NotFound();

            }

            if (ModelState.IsValid)
            {
                employeeDBC.Employees.Update(employee);
                await employeeDBC.SaveChangesAsync();

                TempData["success"] = "Employee Updated";
                return RedirectToAction("allEmployees");
            }


            return View(employee);
        }


    }
}
