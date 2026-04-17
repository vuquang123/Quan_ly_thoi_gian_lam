using System;

namespace FaceIDHRM.Models
{
    public class NhanVienFullTime : NhanVien
    {
        public double HeSoLuong { get; set; }
        public double TienPhuCap { get; set; }

        public NhanVienFullTime(string maNV, string hoTen, DateTime ngaySinh, string canCuoc, string sdt, string email, string chucVu, double luongCoBan, double heSoLuong, double tienPhuCap) 
            : base(maNV, hoTen, ngaySinh, canCuoc, sdt, email, chucVu, luongCoBan)
        {
            HeSoLuong = heSoLuong;
            TienPhuCap = tienPhuCap;
        }

        // Tính đa hình: Ghi đè phương thức TinhLuong
        public override double TinhLuong()
        {
            return (LuongCoBan * HeSoLuong) + TienPhuCap;
        }

        public override string HienThiThongTin()
        {
            return base.HienThiThongTin() + " - Loại: Full-Time";
        }
    }
}
