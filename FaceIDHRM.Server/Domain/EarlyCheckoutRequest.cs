using System;

namespace FaceIDHRM.Server.Domain
{
    public class EarlyCheckoutRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MaNV { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; } = DateTime.Now;
        public string LyDo { get; set; } = string.Empty;
        public string RequestedFromMachine { get; set; } = string.Empty;
        public EarlyCheckoutRequestStatus Status { get; set; } = EarlyCheckoutRequestStatus.Pending;
        public string? AdminName { get; set; }
        public string? AdminNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public DateTime? CheckoutTime { get; set; }
    }
}
