using CRUDApi.Data;
using CRUDApi.Models;
using CRUDApi.Models.Enitites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CRUDApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly AppDBContext dbContext;

        public EmployeesController(AppDBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult getAllEmployees()
        {
            var employees = dbContext.Employees.ToList();
            return Ok(employees);

        }

        [HttpPost]
        public IActionResult addEmployee(AddEmployeesDTO empData)
        {
            Employee empObj = new Employee()
            {
                Name = empData.Name,
                Email = empData.Email,
                Phone = empData.Phone,
                Salary = empData.Salary
            };

            dbContext.Employees.Add(empObj);

            dbContext.SaveChanges();

            return Ok(empObj);
        }

        [HttpGet]
        [Route("{id:guid}")]
        public IActionResult getEmployee(Guid  id)
        {
            var employee = dbContext.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }


        [HttpPut]
        [Route("{id:guid}")]
        public IActionResult updateEmployee(UpdateEmployeeDTO updateEmp, Guid id)
        {

            var emp = dbContext.Employees.Find(id);
            if (emp == null)
            {
                return NotFound();
            }
           

            emp.Name = updateEmp.Name;
            emp.Email = updateEmp.Email;
            emp.Phone = updateEmp.Phone;
            emp.Salary = updateEmp.Salary;

            dbContext.SaveChanges();

            

            return Ok(emp);

        }

        [HttpDelete]
        [Route("{id:guid}")]
        public IActionResult deleteEmployee(Guid id)
        {
            var emp = dbContext.Employees.Find(id);
            if (emp == null)
            {
                return NotFound();
            }

            dbContext.Employees.Remove(emp);
            dbContext.SaveChanges();

            return Ok(emp);
        }

    }
}
