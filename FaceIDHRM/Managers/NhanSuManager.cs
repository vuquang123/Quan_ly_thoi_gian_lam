using System;
using System.Collections.Generic;
using System.Linq;
using FaceIDHRM.Integration;
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
        private bool _useServerSync;
        private readonly IWorkforceGateway _workforceGateway;
        private const string FILE_NAME = "nhanvien.json";

        public NhanSuManager()
        {
            _useServerSync = ServerConfig.UseServerSync;
            _workforceGateway = new WorkforceGateway(ServerConfig.ApprovalServerUrl);

            if (_useServerSync)
            {
                try
                {
                    _danhSachNhanVien = TaiDuLieuTuServer();
                }
                catch
                {
                    // Lỗi mạng tạm thời, vẫn nạp local nhưng không tắt ServerSync để lần sau còn lấy được
                    _danhSachNhanVien = TaiDuLieuLocal();
                }
            }
            else
            {
                _danhSachNhanVien = TaiDuLieuLocal();
            }
        }

        public void Them(NhanVien nv)
        {
            if (_useServerSync)
            {
                var all = TaiDuLieuTuServer();
                if (all.Any(n => n.MaNV == nv.MaNV))
                {
                    throw new ExceptionNhanVienDaTonTai($"Nhân viên có mã {nv.MaNV} đã tồn tại trong hệ thống.");
                }

                var saved = Task.Run(() => _workforceGateway.SaveEmployeeAsync(ToDto(nv))).GetAwaiter().GetResult();
                if (saved == null)
                {
                    throw new Exception("Không thể lưu nhân viên lên server.");
                }

                _danhSachNhanVien = TaiDuLieuTuServer();
                return;
            }

            if (_danhSachNhanVien.Any(n => n.MaNV == nv.MaNV))
            {
                throw new ExceptionNhanVienDaTonTai($"Nhân viên có mã {nv.MaNV} đã tồn tại trong hệ thống.");
            }
            _danhSachNhanVien.Add(nv);
            LuuDuLieu();
        }

        public void Sua(NhanVien nv)
        {
            if (_useServerSync)
            {
                var saved = Task.Run(() => _workforceGateway.SaveEmployeeAsync(ToDto(nv))).GetAwaiter().GetResult();
                if (saved == null)
                {
                    throw new Exception("Không thể cập nhật nhân viên trên server.");
                }

                _danhSachNhanVien = TaiDuLieuTuServer();
                return;
            }

            var existingNV = _danhSachNhanVien.FirstOrDefault(n => n.MaNV == nv.MaNV);
            if (existingNV != null)
            {
                existingNV.HoTen = nv.HoTen;
                existingNV.NgaySinh = nv.NgaySinh;
                existingNV.SoDienThoai = nv.SoDienThoai;
                existingNV.Email = nv.Email;
                existingNV.ChucVu = nv.ChucVu;
                existingNV.LuongCoBan = nv.LuongCoBan;
                existingNV.FaceEncoding = nv.FaceEncoding;
                // Có thể cập nhật các thuộc tính riêng cho FullTime/PartTime nếu cần qua ép kiểu
                LuuDuLieu();
            }
        }

        public void Xoa(string maNV)
        {
            if (_useServerSync)
            {
                Task.Run(() => _workforceGateway.DeleteEmployeeAsync(maNV)).GetAwaiter().GetResult();
                _danhSachNhanVien = TaiDuLieuTuServer();
                return;
            }

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

        public void LamMoiDuLieu()
        {
            if (_useServerSync)
            {
                try
                {
                    _danhSachNhanVien = TaiDuLieuTuServer();
                }
                catch { }
            }
        }

        private void LuuDuLieu()
        {
            DataStorage.SaveData(_danhSachNhanVien, FILE_NAME);
        }

        private static List<NhanVien> TaiDuLieuLocal()
        {
            var data = DataStorage.LoadData<NhanVien>(FILE_NAME);
            return data ?? new List<NhanVien>();
        }

        private List<NhanVien> TaiDuLieuTuServer()
        {
            var data = Task.Run(() => _workforceGateway.GetEmployeesAsync()).GetAwaiter().GetResult();
            return data.Select(ToModel).ToList();
        }

        private static EmployeeRecordDto ToDto(NhanVien nv)
        {
            var dto = new EmployeeRecordDto
            {
                MaNV = nv.MaNV,
                HoTen = nv.HoTen,
                NgaySinh = nv.NgaySinh,
                CanCuoc = nv.CanCuoc,
                SoDienThoai = nv.SoDienThoai,
                Email = nv.Email,
                ChucVu = nv.ChucVu,
                PhongBan = nv.PhongBan,
                FaceEncoding = nv.FaceEncoding,
                EmployeeType = nv is NhanVienPartTime ? EmployeeType.PartTime : EmployeeType.FullTime,
                LuongCoBan = nv.LuongCoBan
            };

            if (nv is NhanVienFullTime f)
            {
                dto.TienPhuCap = f.TienPhuCap;
            }

            if (nv is NhanVienPartTime p)
            {
                dto.MucLuongTheoGio = p.MucLuongTheoGio;
                dto.SoGioLamToiDa = p.SoGioLamToiDa;
                dto.SoGioDaLamTrongThang = p.SoGioDaLamTrongThang;
            }

            return dto;
        }

        private static NhanVien ToModel(EmployeeRecordDto dto)
        {
            NhanVien nv;
            if (dto.EmployeeType == EmployeeType.PartTime)
            {
                nv = new NhanVienPartTime(
                    dto.MaNV,
                    dto.HoTen,
                    dto.NgaySinh,
                    dto.CanCuoc,
                    dto.SoDienThoai,
                    dto.Email,
                    dto.ChucVu,
                    0,
                    dto.MucLuongTheoGio,
                    dto.SoGioLamToiDa);

                ((NhanVienPartTime)nv).SoGioDaLamTrongThang = dto.SoGioDaLamTrongThang;
            }
            else
            {
                nv = new NhanVienFullTime(
                    dto.MaNV,
                    dto.HoTen,
                    dto.NgaySinh,
                    dto.CanCuoc,
                    dto.SoDienThoai,
                    dto.Email,
                    dto.ChucVu,
                    dto.LuongCoBan,
                    1.0,
                    dto.TienPhuCap);
            }

            nv.PhongBan = dto.PhongBan;
            nv.FaceEncoding = dto.FaceEncoding;
            return nv;
        }
    }
}
