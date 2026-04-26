using System;
using FaceIDHRM.Server.Domain.Workforce;
using FaceIDHRM.Server.Services.Workforce;
using Microsoft.AspNetCore.Mvc;

namespace FaceIDHRM.Server.Controllers
{
    [ApiController]
    [Route("api/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _service;

        public EmployeesController(IEmployeeService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(_service.GetAll());
        }

        [HttpGet("{maNV}")]
        public IActionResult GetById(string maNV)
        {
            var employee = _service.GetById(maNV);
            if (employee == null)
            {
                return NotFound();
            }

            return Ok(employee);
        }

        [HttpPost]
        public IActionResult Save([FromBody] EmployeeRecord employee)
        {
            try
            {
                var saved = _service.Save(employee);
                return Ok(saved);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{maNV}")]
        public IActionResult Delete(string maNV)
        {
            _service.Delete(maNV);
            return NoContent();
        }
    }
}
