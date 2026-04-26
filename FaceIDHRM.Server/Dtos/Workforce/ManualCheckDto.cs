using System;

namespace FaceIDHRM.Server.Dtos.Workforce
{
    public class ManualCheckDto
    {
        public string MaNV { get; set; } = string.Empty;
        public DateTime Time { get; set; } = DateTime.Now;
    }
}
