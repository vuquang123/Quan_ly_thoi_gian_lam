using System.Collections.Generic;

namespace FaceIDHRM.Models
{
    public class PhongBan
    {
        public string MaPhong { get; set; }
        public string TenPhong { get; set; }
        public string MaTruongPhong { get; set; }
        
        // Tính đóng gói: ds Nhan Vien private
        private List<string> _danhSachMaNV;

        public PhongBan(string maPhong, string tenPhong)
        {
            MaPhong = maPhong;
            TenPhong = tenPhong;
            _danhSachMaNV = new List<string>();
        }

        public List<string> DanhSachMaNV 
        { 
            get { return _danhSachMaNV; }
            set { _danhSachMaNV = value; }
        }

        public void ThemNhanVienVaoPhong(string maNV)
        {
            if (!_danhSachMaNV.Contains(maNV))
            {
                _danhSachMaNV.Add(maNV);
            }
        }

        public void XoaNhanVienKhoiPhong(string maNV)
        {
            if (_danhSachMaNV.Contains(maNV))
            {
                _danhSachMaNV.Remove(maNV);
            }
        }

        public int ThongKeSoLuong()
        {
            return _danhSachMaNV.Count;
        }
    }
}
