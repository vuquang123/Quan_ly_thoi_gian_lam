using System;
using System.Collections.Generic;
using System.Linq;
using FaceIDHRM.Models;

namespace FaceIDHRM.Managers
{
    public class ExceptionNhanVienDaTonTai : Exception
    {
        public ExceptionNhanVienDaTonTai(string message) : base(message) { }
    }

    public class NhanSuManager : IQuanLy<NhanVien>
    {
        private List<NhanVien> _danhSachNhanVien;
        private const string FILE_NAME = "nhanvien.json";

        public NhanSuManager()
        {
            _danhSachNhanVien = DataStorage.LoadData<NhanVien>(FILE_NAME);
            if (_danhSachNhanVien == null)
            {
                _danhSachNhanVien = new List<NhanVien>();
            }
        }

        public void Them(NhanVien nv)
        {
            if (_danhSachNhanVien.Any(n => n.MaNV == nv.MaNV))
            {
                throw new ExceptionNhanVienDaTonTai($"Nhân viên có mã {nv.MaNV} đã tồn tại trong hệ thống.");
            }
            _danhSachNhanVien.Add(nv);
            LuuDuLieu();
        }

        public void Sua(NhanVien nv)
        {
            var existingNV = _danhSachNhanVien.FirstOrDefault(n => n.MaNV == nv.MaNV);
            if (existingNV != null)
            {
                existingNV.HoTen = nv.HoTen;
                existingNV.NgaySinh = nv.NgaySinh;
                existingNV.SoDienThoai = nv.SoDienThoai;
                existingNV.Email = nv.Email;
                existingNV.ChucVu = nv.ChucVu;
                existingNV.LuongCoBan = nv.LuongCoBan;
                existingNV.FaceDataPath = nv.FaceDataPath;
                // Có thể cập nhật các thuộc tính riêng cho FullTime/PartTime nếu cần qua ép kiểu
                LuuDuLieu();
            }
        }

        public void Xoa(string maNV)
        {
            var nv = _danhSachNhanVien.FirstOrDefault(n => n.MaNV == maNV);
            if (nv != null)
            {
                _danhSachNhanVien.Remove(nv);
                LuuDuLieu();
            }
        }

        // Đa hình: Tìm kiếm theo mã NV (chính xác)
        public NhanVien TimKiem(string keyword)
        {
            return _danhSachNhanVien.FirstOrDefault(n => n.MaNV == keyword || n.HoTen.Contains(keyword));
        }

        // Đa hình (Overload): Tìm danh sách nhân viên theo Tên
        public List<NhanVien> TimKiemTheoTen(string name)
        {
            return _danhSachNhanVien.Where(n => n.HoTen.ToLower().Contains(name.ToLower())).ToList();
        }

        public List<NhanVien> LayDanhSach()
        {
            return _danhSachNhanVien;
        }

        private void LuuDuLieu()
        {
            DataStorage.SaveData(_danhSachNhanVien, FILE_NAME);
        }
    }
}
