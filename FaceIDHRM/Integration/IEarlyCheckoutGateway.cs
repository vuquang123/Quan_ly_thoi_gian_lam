using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaceIDHRM.Integration
{
    public interface IEarlyCheckoutGateway
    {
        event Action<EarlyCheckoutRequestDto>? RequestUpdated;
        event Action? AttendanceUpdated;
        Task ConnectAsync();
        Task<List<EarlyCheckoutRequestDto>> GetPendingAsync();
        Task<EarlyCheckoutRequestDto?> GetByIdAsync(string requestId);
        Task<EarlyCheckoutRequestDto?> CreateRequestAsync(CreateEarlyCheckoutRequestDto dto);
        Task<EarlyCheckoutRequestDto?> ApproveAsync(string requestId, ResolveEarlyCheckoutRequestDto dto);
        Task<EarlyCheckoutRequestDto?> RejectAsync(string requestId, ResolveEarlyCheckoutRequestDto dto);
        Task<EarlyCheckoutRequestDto?> MarkProcessedAsync(string requestId);
    }
}
