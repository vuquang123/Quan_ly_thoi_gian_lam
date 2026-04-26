using System;
using FaceIDHRM.Server.Dtos.Workforce;
using FaceIDHRM.Server.Services.Workforce;
using Microsoft.AspNetCore.Mvc;

namespace FaceIDHRM.Server.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;

        public AttendanceController(IAttendanceService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            return Ok(_service.GetAll(from, to));
        }

        [HttpPost("checkin")]
        public IActionResult CheckIn([FromBody] ManualCheckDto dto)
        {
            try
            {
                return Ok(_service.CheckIn(dto.MaNV, dto.Time));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("checkout")]
        public IActionResult CheckOut([FromBody] ManualCheckDto dto)
        {
            try
            {
                return Ok(_service.CheckOut(dto.MaNV, dto.Time));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("scan-auto")]
        public IActionResult ScanAuto([FromBody] ScanAttendanceDto dto)
        {
            try
            {
                return Ok(_service.ScanAuto(dto.MaNV, dto.ScanTime));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
