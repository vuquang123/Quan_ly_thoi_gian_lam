using System;
using System.Collections.Generic;
using System.Linq;
using FaceIDHRM.Integration;
using FaceIDHRM.Models;

namespace FaceIDHRM.Managers
{
    public class ChamCongManager : IQuanLy<NgayLamViec>
    {
        private List<NgayLamViec> _danhSachChamCong;
        private readonly IWorkforceGateway _workforceGateway;

        public ChamCongManager()
        {
            _workforceGateway = new WorkforceGateway(ServerConfig.ApprovalServerUrl);
            _danhSachChamCong = TaiDuLieuTuServer();
        }

        public void Them(NgayLamViec nlv)
        {
            throw new InvalidOperationException("Thêm trực tiếp không được hỗ trợ ở chế độ đồng bộ server. Hãy dùng CheckIn hoặc XuLyQuetMatTuDong.");
        }

        public void Sua(NgayLamViec nlv)
        {
            throw new InvalidOperationException("Sửa trực tiếp không được hỗ trợ ở chế độ đồng bộ server.");
        }

        public void Xoa(string maNLV)
        {
            throw new InvalidOperationException("Xóa trực tiếp không được hỗ trợ ở chế độ đồng bộ server.");
        }

        public NgayLamViec TimKiem(string keyword)
        {
            return _danhSachChamCong.FirstOrDefault(n => n.MaNLV == keyword || n.MaNV == keyword);
        }

        public List<NgayLamViec> LayDanhSach()
        {
            return _danhSachChamCong;
        }

        public void LamMoiDuLieu()
        {
            _danhSachChamCong = TaiDuLieuTuServer();
        }

        public NgayLamViec CheckIn(string maNV, DateTime? customTime = null)
        {
            var result = Task.Run(() => _workforceGateway.CheckInAsync(new ManualCheckDto
            {
                MaNV = maNV,
                Time = customTime ?? DateTime.Now
            })).GetAwaiter().GetResult();

            if (result == null)
            {
                throw new Exception("Không thể check-in qua server.");
            }

            _danhSachChamCong = TaiDuLieuTuServer();
            return ToModel(result);
        }

        public NgayLamViec CheckOut(string maNV, DateTime? customTime = null)
        {
            var result = Task.Run(() => _workforceGateway.CheckOutAsync(new ManualCheckDto
            {
                MaNV = maNV,
                Time = customTime ?? DateTime.Now
            })).GetAwaiter().GetResult();

            if (result == null)
            {
                throw new Exception("Không thể check-out qua server.");
            }

            _danhSachChamCong = TaiDuLieuTuServer();
            return ToModel(result);
        }

        public NgayLamViec XuLyQuetMatTuDong(string maNV, DateTime scanTime)
        {
            var result = Task.Run(() => _workforceGateway.ScanAutoAsync(new ScanAttendanceDto
            {
                MaNV = maNV,
                ScanTime = scanTime
            })).GetAwaiter().GetResult();

            if (result == null)
            {
                throw new Exception("Không thể xử lý quét mặt tự động qua server.");
            }

            _danhSachChamCong = TaiDuLieuTuServer();
            return ToModel(result);
        }

        private List<NgayLamViec> TaiDuLieuTuServer()
        {
            var data = Task.Run(() => _workforceGateway.GetAttendanceAsync()).GetAwaiter().GetResult();
            return data.Select(ToModel).ToList();
        }

        private static NgayLamViec ToModel(AttendanceRecordDto dto)
        {
            var nlv = new NgayLamViec(dto.MaNLV, dto.MaNV, dto.NgayChamCong)
            {
                GioCheckIn = dto.GioCheckIn,
                GioCheckOut = dto.GioCheckOut,
                TrangThai = dto.TrangThai,
                TenCa = dto.TenCa
            };
            return nlv;
        }
    }
}
