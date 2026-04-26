using System;

namespace FaceIDHRM.Models
{
    public class NgayLamViec
    {
        public string MaNLV { get; set; }
        public string MaNV { get; set; }
        public DateTime NgayChamCong { get; set; }
        public TimeSpan? GioCheckIn { get; set; }
        public TimeSpan? GioCheckOut { get; set; }
        public string TrangThai { get; set; } // Đúng giờ / Đi trễ / Về sớm / Vắng mặt
        public string TenCa { get; set; } // Ca 1, Ca 2, Ca 3, v.v.

        public NgayLamViec(string maNLV, string maNV, DateTime ngayChamCong)
        {
            MaNLV = maNLV;
            MaNV = maNV;
            NgayChamCong = ngayChamCong;
            TrangThai = "Vắng mặt"; // default
            TenCa = "Hành chính";
        }

        public double TinhTongSoGioLam()
        {
            if (GioCheckIn.HasValue && GioCheckOut.HasValue)
            {
                TimeSpan diff = GioCheckOut.Value - GioCheckIn.Value;
                return diff.TotalHours;
            }
            return 0;
        }

        public void XacDinhTrangThai()
        {
            if (!GioCheckIn.HasValue)
            {
                TrangThai = "Vắng mặt";
                return;
            }

            TimeSpan gioVaoChuan = new TimeSpan(8, 0, 0);
            TimeSpan gioRaChuan = new TimeSpan(17, 0, 0);

            if (TenCa.Contains("Ca 1")) gioVaoChuan = new TimeSpan(8, 0, 0);
            else if (TenCa.Contains("Ca 2")) gioVaoChuan = new TimeSpan(13, 0, 0);
            else if (TenCa.Contains("Ca 3")) gioVaoChuan = new TimeSpan(17, 0, 0);

            if (TenCa.Contains("Ca 3")) gioRaChuan = new TimeSpan(21, 30, 0);
            else if (TenCa.Contains("Ca 2")) gioRaChuan = new TimeSpan(17, 0, 0);
            else if (TenCa.Contains("Ca 1")) gioRaChuan = new TimeSpan(13, 0, 0);

            if (GioCheckIn.Value > gioVaoChuan)
                TrangThai = "Đi trễ";
            else if (GioCheckOut.HasValue && GioCheckOut.Value < gioRaChuan)
                TrangThai = "Về sớm";
            else
                TrangThai = "Đúng giờ";
        }
    }
}
