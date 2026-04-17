using System;
using System.Collections.Generic;

namespace FaceIDHRM.Models
{
    public abstract class NhanVien
    {
        // Tính đóng gói: Dùng các Properties để access
        public string MaNV { get; set; }
        public string HoTen { get; set; }
        public DateTime NgaySinh { get; set; }
        public string CanCuoc { get; set; }
        public string SoDienThoai { get; set; }
        public string Email { get; set; }
        public string ChucVu { get; set; }
        public string PhongBan { get; set; } // Bổ sung trường Phòng Ban
        public double LuongCoBan { get; set; }
        public string FaceDataPath { get; set; } // Đường dẫn lưu Face Encoding

        protected NhanVien(string maNV, string hoTen, DateTime ngaySinh, string canCuoc, string sdt, string email, string chucVu, double luongCoBan)
        {
            MaNV = maNV;
            HoTen = hoTen;
            NgaySinh = ngaySinh;
            CanCuoc = canCuoc;
            SoDienThoai = sdt;
            Email = email;
            ChucVu = chucVu;
            LuongCoBan = luongCoBan;
            FaceDataPath = string.Empty;
        }

        // Phương thức hiển thị
        public virtual string HienThiThongTin()
        {
            return $"[{MaNV}] {HoTen} - {ChucVu} ({Email})";
        }

        // Tính trừu tượng: Phương thức trừu tượng tính lương
        public abstract double TinhLuong();
    }
}
