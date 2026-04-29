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
        private readonly IWorkforceGateway _workforceGateway;

        public NhanSuManager()
        {
            _workforceGateway = new WorkforceGateway(ServerConfig.ApprovalServerUrl);
            _danhSachNhanVien = TaiDuLieuTuServer();
        }

        public void Them(NhanVien nv)
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
        }

        public void Sua(NhanVien nv)
        {
            var saved = Task.Run(() => _workforceGateway.SaveEmployeeAsync(ToDto(nv))).GetAwaiter().GetResult();
            if (saved == null)
            {
                throw new Exception("Không thể cập nhật nhân viên trên server.");
            }

            _danhSachNhanVien = TaiDuLieuTuServer();
        }

        public void Xoa(string maNV)
        {
            Task.Run(() => _workforceGateway.DeleteEmployeeAsync(maNV)).GetAwaiter().GetResult();
            _danhSachNhanVien = TaiDuLieuTuServer();
        }

        public NhanVien TimKiem(string keyword)
        {
            return _danhSachNhanVien.FirstOrDefault(n => n.MaNV == keyword || n.HoTen.Contains(keyword));
        }

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
            _danhSachNhanVien = TaiDuLieuTuServer();
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
            if (nv.FaceEncoding == null && !string.IsNullOrEmpty(dto.FaceEncodingBase64))
            {
                try
                {
                    var bytes = Convert.FromBase64String(dto.FaceEncodingBase64);
                    nv.FaceEncoding = new double[bytes.Length / 8];
                    Buffer.BlockCopy(bytes, 0, nv.FaceEncoding, 0, bytes.Length);
                }
                catch { }
            }
            return nv;
        }
    }
}
