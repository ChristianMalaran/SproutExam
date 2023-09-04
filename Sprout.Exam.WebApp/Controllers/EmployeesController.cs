using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Sprout.Exam.Business.DataTransferObjects;
using Sprout.Exam.Common.Enums;
using Sprout.Exam.WebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly SproutExamDbContext _context;

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        /// 

        public EmployeesController(SproutExamDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            
            var result = await _context.Employees.Where(e => e.IsDeleted == false).ToListAsync();
            return Ok(result);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
          

            var result = await _context.Employees
                    .Where(e => e.Id == id)
                    .FirstOrDefaultAsync();

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Refactor this method to go through proper layers and update changes to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] Employee employee)
        {
      
            if (employee == null)
            {
                return BadRequest();
            }

            var existingEmployee = await _context.Employees.FindAsync(id);

            if (existingEmployee == null)
            {
                return NotFound(); 
            }

            try
            {

                existingEmployee.FullName = employee.FullName; 
                existingEmployee.Birthdate = employee.Birthdate; 
                existingEmployee.Tin = employee.Tin;
                existingEmployee.EmployeeTypeId = employee.EmployeeTypeId; 


                await _context.SaveChangesAsync();

                return Ok(existingEmployee); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }

        /// <summary>
        /// Refactor this method to go through proper layers and insert employees to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee employee)
        {


            if (employee == null)
            {
                return BadRequest(); 
            }
            try
            {
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return Created($"/api/employees/{employee.Id}", employee.Id);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }

        }


        /// <summary>
        /// Refactor this method to go through proper layers and perform soft deletion of an employee to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
      
            var existingEmployee = await _context.Employees.FindAsync(id);

            if (existingEmployee == null)
            {
                return NotFound();
            }

            try
            {

                existingEmployee.IsDeleted = true;



                await _context.SaveChangesAsync();

                return Ok(existingEmployee.Id); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }



        /// <summary>
        /// Refactor this method to go through proper layers and use Factory pattern
        /// </summary>
        /// <param name="id"></param>
        /// <param name="absentDays"></param>
        /// <param name="workedDays"></param>
        /// <returns></returns>
        [HttpPost("{id}/calculate")]
        public async Task<IActionResult> Calculate(int id, [FromBody] SalaryCalculation salaryCalculation)
        {
           
            decimal res = 0;
            var result = await _context.Employees.FindAsync(id);
            if (result == null) return NotFound();
            var type = result.EmployeeTypeId;
            switch (type)
            {
                case 1:
                 
                    res = CalculateRegular(salaryCalculation.AbsentDays);
                    break;
                case 2:
                    
                    res = CalculateContractual(salaryCalculation.WorkedDays);

                    break;
                default:
                    return NotFound("Employee Type not found");
            }

            return Ok(res);
        }

        public decimal CalculateRegular(decimal absent)
        {
            decimal basic = 20000M;
            decimal tax = 0.12M;
            decimal result =  (basic - ((basic/22M)*absent) - (basic * tax));
            result = Math.Round(result, 2);

            return result;
        }

        public decimal CalculateContractual(decimal work)
        {

            decimal result = work*500;
            result = Math.Round(result, 2);

            return result;
        }

    }
}
