namespace FaceIDHRM.Server.Dtos
{
    public class CreateEarlyCheckoutRequestDto
    {
        public string MaNV { get; set; } = string.Empty;
        public string LyDo { get; set; } = string.Empty;
        public string RequestedFromMachine { get; set; } = string.Empty;
    }
}
