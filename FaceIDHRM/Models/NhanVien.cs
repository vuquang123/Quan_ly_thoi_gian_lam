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
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public double[] FaceEncoding { get; set; } // Vector khuôn mặt

        public string FaceEncodingBase64
        {
            get
            {
                if (FaceEncoding == null) return null;
                var bytes = new byte[FaceEncoding.Length * 8];
                Buffer.BlockCopy(FaceEncoding, 0, bytes, 0, bytes.Length);
                return Convert.ToBase64String(bytes);
            }
            set
            {
                if (string.IsNullOrEmpty(value)) { FaceEncoding = null; return; }
                var bytes = Convert.FromBase64String(value);
                FaceEncoding = new double[bytes.Length / 8];
                Buffer.BlockCopy(bytes, 0, FaceEncoding, 0, bytes.Length);
            }
        }

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
            FaceEncoding = null;
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
