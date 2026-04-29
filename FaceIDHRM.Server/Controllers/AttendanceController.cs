using System;
using System.Threading.Tasks;
using FaceIDHRM.Server.Dtos.Workforce;
using FaceIDHRM.Server.Services.Workforce;
using FaceIDHRM.Server.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FaceIDHRM.Server.Controllers
{
    [ApiController]
    [Route("api/attendance")]
    public class AttendanceController : ControllerBase
    {
        private readonly IAttendanceService _service;
        private readonly IHubContext<EarlyCheckoutHub> _hubContext;

        public AttendanceController(IAttendanceService service, IHubContext<EarlyCheckoutHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null)
        {
            return Ok(_service.GetAll(from, to));
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> CheckIn([FromBody] ManualCheckDto dto)
        {
            try
            {
                var res = _service.CheckIn(dto.MaNV, dto.Time);
                await _hubContext.Clients.All.SendAsync("AttendanceUpdated");
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> CheckOut([FromBody] ManualCheckDto dto)
        {
            try
            {
                var res = _service.CheckOut(dto.MaNV, dto.Time);
                await _hubContext.Clients.All.SendAsync("AttendanceUpdated");
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("scan-auto")]
        public async Task<IActionResult> ScanAuto([FromBody] ScanAttendanceDto dto)
        {
            try
            {
                var res = _service.ScanAuto(dto.MaNV, dto.ScanTime);
                await _hubContext.Clients.All.SendAsync("AttendanceUpdated");
                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
