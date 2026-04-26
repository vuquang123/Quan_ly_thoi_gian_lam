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
        private bool _useServerSync;
        private readonly IWorkforceGateway _workforceGateway;
        private const string FILE_NAME = "chamcong.json";

        public ChamCongManager()
        {
            _useServerSync = ServerConfig.UseServerSync;
            _workforceGateway = new WorkforceGateway(ServerConfig.ApprovalServerUrl);

            if (_useServerSync)
            {
                try
                {
                    _danhSachChamCong = TaiDuLieuTuServer();
                }
                catch
                {
                    // Lỗi mạng tạm thời, vẫn nạp local nhưng không tắt ServerSync để lần sau còn lấy được
                    _danhSachChamCong = TaiDuLieuLocal();
                }
            }
            else
            {
                _danhSachChamCong = TaiDuLieuLocal();
            }
        }

        public void Them(NgayLamViec nlv)
        {
            if (_useServerSync)
            {
                throw new InvalidOperationException("Thêm trực tiếp không được hỗ trợ ở chế độ đồng bộ server. Hãy dùng CheckIn hoặc XuLyQuetMatTuDong.");
            }

            // Kiểm tra trùng mã NLV theo GUID là đủ
            if (_danhSachChamCong.Any(n => n.MaNLV == nlv.MaNLV))
            {
                throw new Exception("Bản ghi này đã tồn tại.");
            }
            _danhSachChamCong.Add(nlv);
            LuuDuLieu();
        }

        public void Sua(NgayLamViec nlv)
        {
            if (_useServerSync)
            {
                throw new InvalidOperationException("Sửa trực tiếp không được hỗ trợ ở chế độ đồng bộ server.");
            }

            var existing = _danhSachChamCong.FirstOrDefault(n => n.MaNLV == nlv.MaNLV);
            if (existing != null)
            {
                existing.GioCheckIn = nlv.GioCheckIn;
                existing.GioCheckOut = nlv.GioCheckOut;
                existing.TrangThai = nlv.TrangThai;
                LuuDuLieu();
            }
        }

        public void Xoa(string maNLV)
        {
            if (_useServerSync)
            {
                throw new InvalidOperationException("Xóa trực tiếp không được hỗ trợ ở chế độ đồng bộ server.");
            }

            var nlv = _danhSachChamCong.FirstOrDefault(n => n.MaNLV == maNLV);
            if (nlv != null)
            {
                _danhSachChamCong.Remove(nlv);
                LuuDuLieu();
            }
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
            if (_useServerSync)
            {
                try
                {
                    _danhSachChamCong = TaiDuLieuTuServer();
                }
                catch { }
            }
        }

        public NgayLamViec CheckIn(string maNV, DateTime? customTime = null)
        {
            if (_useServerSync)
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

            var timeToUse = customTime ?? DateTime.Now;
            var toDay = timeToUse.Date;
            var record = _danhSachChamCong.FirstOrDefault(n => n.MaNV == maNV && n.NgayChamCong.Date == toDay);

            if (record != null)
            {
                if (!record.GioCheckIn.HasValue)
                {
                    record.GioCheckIn = timeToUse.TimeOfDay;
                    record.XacDinhTrangThai();
                    LuuDuLieu();
                    return record;
                }
                throw new Exception($"Đã Check-in ngày {toDay:dd/MM/yyyy} rồi.");
            }

            var newRecord = new NgayLamViec(Guid.NewGuid().ToString(), maNV, toDay)
            {
                GioCheckIn = timeToUse.TimeOfDay
            };
            newRecord.XacDinhTrangThai();
            Them(newRecord);
            return newRecord;
        }

        public NgayLamViec CheckOut(string maNV, DateTime? customTime = null)
        {
            if (_useServerSync)
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

            var timeToUse = customTime ?? DateTime.Now;
            var toDay = timeToUse.Date;
            var record = _danhSachChamCong.FirstOrDefault(n => n.MaNV == maNV && n.NgayChamCong.Date == toDay);

            if (record != null)
            {
                if (!record.GioCheckIn.HasValue)
                {
                    throw new Exception("Chưa Check-in, không thể Check-out.");
                }
                record.GioCheckOut = timeToUse.TimeOfDay;
                record.XacDinhTrangThai();
                LuuDuLieu();
                return record;
            }
            throw new Exception($"Không tìm thấy bản ghi Check-in ngày {toDay:dd/MM/yyyy}.");
        }

        public NgayLamViec XuLyQuetMatTuDong(string maNV, DateTime scanTime)
        {
            if (_useServerSync)
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

            var toDay = scanTime.Date;
            var t = scanTime.TimeOfDay;

            // Truy vấn DB: Tìm bản ghi ĐANG MỞ trong ngày (có Giờ Vào, chưa có Giờ Ra)
            var openRecord = _danhSachChamCong.FirstOrDefault(n => n.MaNV == maNV && n.NgayChamCong.Date == toDay && n.GioCheckIn.HasValue && !n.GioCheckOut.HasValue);

            if (openRecord != null)
            {
                string startCa = openRecord.TenCa;

                // Ràng buộc: Ngăn chặn Check-out sớm trước giờ quy định của ca
                if (startCa.Contains("Ca 3") && t < new TimeSpan(21, 0, 0))
                    throw new Exception("Vui lòng Check-out sau 21h00!");
                if (startCa.Contains("Ca 2") && t < new TimeSpan(17, 0, 0))
                    throw new Exception("Vui lòng Check-out sau 17h00!");
                if (startCa.Contains("Ca 1") && t < new TimeSpan(13, 0, 0))
                    throw new Exception("Vui lòng Check-out sau 13h00!");

                // Hành động: Check-out ca hiện tại
                openRecord.GioCheckOut = t;

                // Nối tên ca dựa vào thời điểm checkout theo đúng mốc chuẩn
                string currentCa = openRecord.TenCa;
                
                // Trọng số từ cao xuống thấp để lấy ca cuối cùng nhân viên chạm tới
                if (t >= new TimeSpan(21, 0, 0)) currentCa = "Ca 3";
                else if (t >= new TimeSpan(15, 0, 0)) currentCa = "Ca 2"; // Mốc sau 15h cho Ca 2 như bạn chỉ định
                else if (t >= new TimeSpan(13, 0, 0)) currentCa = "Ca 1";

                if (startCa == "Ca 1" && currentCa == "Ca 2") openRecord.TenCa = "Ca 1, 2";
                else if (startCa == "Ca 1" && currentCa == "Ca 3") openRecord.TenCa = "Ca 1, 2, 3";
                else if (startCa == "Ca 2" && currentCa == "Ca 3") openRecord.TenCa = "Ca 2, 3";

                openRecord.XacDinhTrangThai();
                LuuDuLieu();
                return openRecord;
            }

            // Hành động: Khởi tạo Check-in 1 ca mới theo phân định ranh giới
            string tenCa = "Ca Mặc Định";
            if (t < new TimeSpan(12, 30, 0)) tenCa = "Ca 1";
            else if (t >= new TimeSpan(12, 30, 0) && t < new TimeSpan(16, 30, 0)) tenCa = "Ca 2";
            else tenCa = "Ca 3";

            var newRecord = new NgayLamViec(Guid.NewGuid().ToString(), maNV, toDay)
            {
                GioCheckIn = t,
                TenCa = tenCa
            };
            newRecord.XacDinhTrangThai();
            Them(newRecord);
            return newRecord;
        }

        private void LuuDuLieu()
        {
            DataStorage.SaveData(_danhSachChamCong, FILE_NAME);
        }

        private static List<NgayLamViec> TaiDuLieuLocal()
        {
            var data = DataStorage.LoadData<NgayLamViec>(FILE_NAME);
            return data ?? new List<NgayLamViec>();
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
