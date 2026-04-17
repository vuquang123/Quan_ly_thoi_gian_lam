using System;

namespace FaceIDHRM.Models
{
    public class NhanVienPartTime : NhanVien
    {
        public double MucLuongTheoGio { get; set; }
        public int SoGioLamToiDa { get; set; }
        public int SoGioDaLamTrongThang { get; set; } // Giả sử được tính từ các NgayLamViec

        public NhanVienPartTime(string maNV, string hoTen, DateTime ngaySinh, string canCuoc, string sdt, string email, string chucVu, double luongCoBan, double mucLuongGio, int soGioToiDa) 
            : base(maNV, hoTen, ngaySinh, canCuoc, sdt, email, chucVu, luongCoBan)
        {
            MucLuongTheoGio = mucLuongGio;
            SoGioLamToiDa = soGioToiDa;
            SoGioDaLamTrongThang = 0;
        }

        // Tính đa hình: Ghi đè phương thức TinhLuong
        public override double TinhLuong()
        {
            // PartTime lấy số giờ làm * mức lương giờ
            // Lương cơ bản để tượng trưng hoặc có thể là mức cố định, tuỳ logic. Ở đây ta theo đề: Số giờ * Mức lương
            int gioLamThucTe = Math.Min(SoGioDaLamTrongThang, SoGioLamToiDa);
            return gioLamThucTe * MucLuongTheoGio;
        }

        public override string HienThiThongTin()
        {
            return base.HienThiThongTin() + $" - Loại: Part-Time ({SoGioDaLamTrongThang}/{SoGioLamToiDa}h)";
        }
    }
}
