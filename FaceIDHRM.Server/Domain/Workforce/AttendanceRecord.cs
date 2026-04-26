using System;

namespace FaceIDHRM.Server.Domain.Workforce
{
    public class AttendanceRecord
    {
        public string MaNLV { get; set; } = string.Empty;
        public string MaNV { get; set; } = string.Empty;
        public DateTime NgayChamCong { get; set; }
        public TimeSpan? GioCheckIn { get; set; }
        public TimeSpan? GioCheckOut { get; set; }
        public string TrangThai { get; set; } = "Vắng mặt";
        public string TenCa { get; set; } = "Hành chính";
    }
}
