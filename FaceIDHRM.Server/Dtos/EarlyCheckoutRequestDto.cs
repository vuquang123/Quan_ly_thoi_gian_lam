using System;
using FaceIDHRM.Server.Domain;

namespace FaceIDHRM.Server.Dtos
{
    public class EarlyCheckoutRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public string MaNV { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public string RequestedFromMachine { get; set; } = string.Empty;
        public EarlyCheckoutRequestStatus Status { get; set; }
        public string? AdminName { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? CheckoutTime { get; set; }
    }
}
