using System;

namespace FaceIDHRM.Server.Domain.Workforce
{
    public class EmployeeRecord
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
}
