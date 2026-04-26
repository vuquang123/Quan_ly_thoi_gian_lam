using System;
using System.Linq;
using System.Threading.Tasks;
using FaceIDHRM.Server.Domain;
using FaceIDHRM.Server.Dtos;
using FaceIDHRM.Server.Hubs;
using FaceIDHRM.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FaceIDHRM.Server.Controllers
{
    [ApiController]
    [Route("api/early-checkout-requests")]
    public class EarlyCheckoutRequestsController : ControllerBase
    {
        private readonly IEarlyCheckoutApprovalService _service;
        private readonly IHubContext<EarlyCheckoutHub> _hubContext;

        public EarlyCheckoutRequestsController(
            IEarlyCheckoutApprovalService service,
            IHubContext<EarlyCheckoutHub> hubContext)
        {
            _service = service;
            _hubContext = hubContext;
        }

        [HttpGet("pending")]
        public IActionResult GetPending()
        {
            var result = _service.GetPending().Select(ToDto).ToList();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            var request = _service.GetById(id);
            if (request == null)
            {
                return NotFound();
            }

            return Ok(ToDto(request));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEarlyCheckoutRequestDto dto)
        {
            try
            {
                var request = _service.CreateRequest(dto.MaNV, dto.LyDo, dto.RequestedFromMachine);
                var payload = ToDto(request);
                await _hubContext.Clients.All.SendAsync("RequestUpdated", payload);
                return Ok(payload);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(string id, [FromBody] ResolveEarlyCheckoutRequestDto dto)
        {
            try
            {
                var request = _service.Approve(id, dto.AdminName, dto.AdminNote, dto.CheckoutTime);
                var payload = ToDto(request);
                await _hubContext.Clients.All.SendAsync("RequestUpdated", payload);
                return Ok(payload);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(string id, [FromBody] ResolveEarlyCheckoutRequestDto dto)
        {
            try
            {
                var request = _service.Reject(id, dto.AdminName, dto.AdminNote);
                var payload = ToDto(request);
                await _hubContext.Clients.All.SendAsync("RequestUpdated", payload);
                return Ok(payload);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("{id}/processed")]
        public async Task<IActionResult> MarkProcessed(string id)
        {
            try
            {
                var request = _service.MarkProcessed(id);
                var payload = ToDto(request);
                await _hubContext.Clients.All.SendAsync("RequestUpdated", payload);
                return Ok(payload);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private static EarlyCheckoutRequestDto ToDto(EarlyCheckoutRequest x)
        {
            return new EarlyCheckoutRequestDto
            {
                Id = x.Id,
                MaNV = x.MaNV,
                RequestedAt = x.RequestedAt,
                LyDo = x.LyDo,
                RequestedFromMachine = x.RequestedFromMachine,
                Status = x.Status,
                AdminName = x.AdminName,
                AdminNote = x.AdminNote,
                ResolvedAt = x.ResolvedAt,
                CheckoutTime = x.CheckoutTime
            };
        }
    }
}
