namespace FaceIDHRM.Server.Dtos
{
    public class ResolveEarlyCheckoutRequestDto
    {
        public string AdminName { get; set; } = string.Empty;
        public string AdminNote { get; set; } = string.Empty;
        public DateTime? CheckoutTime { get; set; }
    }
}
