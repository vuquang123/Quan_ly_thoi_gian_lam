using System;

namespace FaceIDHRM.Integration
{
    public enum EarlyCheckoutRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Processed = 3
    }

    public class EarlyCheckoutRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public string MaNV { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public string LyDo { get; set; } = string.Empty;
        public string RequestedFromMachine { get; set; } = string.Empty;
        public EarlyCheckoutRequestStatus Status { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string AdminNote { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public DateTime? CheckoutTime { get; set; }
    }

    public class CreateEarlyCheckoutRequestDto
    {
        public string MaNV { get; set; } = string.Empty;
        public string LyDo { get; set; } = string.Empty;
        public string RequestedFromMachine { get; set; } = string.Empty;
    }

    public class ResolveEarlyCheckoutRequestDto
    {
        public string AdminName { get; set; } = string.Empty;
        public string AdminNote { get; set; } = string.Empty;
        public DateTime? CheckoutTime { get; set; }
    }
}
