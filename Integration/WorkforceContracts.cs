using System;
using System.Collections.Generic;

namespace FaceIDHRM.Integration
{
    public enum EmployeeType
    {
        FullTime = 0,
        PartTime = 1
    }

    public class EmployeeRecordDto
    {
        public string MaNV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public DateTime NgaySinh { get; set; } = new DateTime(2000, 1, 1);
        public string CanCuoc { get; set; } = string.Empty;
        public string SoDienThoai { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ChucVu { get; set; } = string.Empty;
        public string PhongBan { get; set; } = string.Empty;
        public string FaceDataPath { get; set; } = string.Empty;

        public EmployeeType EmployeeType { get; set; }
        public double LuongCoBan { get; set; }
        public double TienPhuCap { get; set; }
        public double MucLuongTheoGio { get; set; }
        public int SoGioLamToiDa { get; set; }
        public int SoGioDaLamTrongThang { get; set; }
    }

    public class AttendanceRecordDto
    {
        public string MaNLV { get; set; } = string.Empty;
        public string MaNV { get; set; } = string.Empty;
        public DateTime NgayChamCong { get; set; }
        public TimeSpan? GioCheckIn { get; set; }
        public TimeSpan? GioCheckOut { get; set; }
        public string TrangThai { get; set; } = "Vắng mặt";
        public string TenCa { get; set; } = "Hành chính";
    }

    public class ManualCheckDto
    {
        public string MaNV { get; set; } = string.Empty;
        public DateTime Time { get; set; } = DateTime.Now;
    }

    public class ScanAttendanceDto
    {
        public string MaNV { get; set; } = string.Empty;
        public DateTime ScanTime { get; set; } = DateTime.Now;
    }
}
