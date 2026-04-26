using System;

namespace FaceIDHRM.Server.Dtos.Workforce
{
    public class ScanAttendanceDto
    {
        public string MaNV { get; set; } = string.Empty;
        public DateTime ScanTime { get; set; } = DateTime.Now;
    }
}
