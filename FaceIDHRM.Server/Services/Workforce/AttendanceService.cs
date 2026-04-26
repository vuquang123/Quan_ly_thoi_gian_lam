using System;
using System.Collections.Generic;
using System.Linq;
using FaceIDHRM.Server.Domain.Workforce;
using FaceIDHRM.Server.Repositories.Workforce;

namespace FaceIDHRM.Server.Services.Workforce
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IAttendanceRepository _attendanceRepository;

        public AttendanceService(IAttendanceRepository attendanceRepository)
        {
            _attendanceRepository = attendanceRepository;
        }

        public List<AttendanceRecord> GetAll(DateTime? from = null, DateTime? to = null)
        {
            var all = _attendanceRepository.GetAll();
            if (from.HasValue)
            {
                all = all.Where(x => x.NgayChamCong >= from.Value).ToList();
            }

            if (to.HasValue)
            {
                all = all.Where(x => x.NgayChamCong <= to.Value).ToList();
            }

            return all;
        }

        public AttendanceRecord CheckIn(string maNV, DateTime time)
        {
            var date = time.Date;
            var all = _attendanceRepository.GetAll();
            var existing = all.FirstOrDefault(x => x.MaNV == maNV && x.NgayChamCong.Date == date);
            if (existing != null)
            {
                if (!existing.GioCheckIn.HasValue)
                {
                    existing.GioCheckIn = time.TimeOfDay;
                    XacDinhTrangThai(existing);
                    _attendanceRepository.Save(existing);
                    return existing;
                }

                throw new Exception($"Đã Check-in ngày {date:dd/MM/yyyy} rồi.");
            }

            var newRecord = new AttendanceRecord
            {
                MaNLV = Guid.NewGuid().ToString(),
                MaNV = maNV,
                NgayChamCong = date,
                GioCheckIn = time.TimeOfDay,
                TenCa = "Hành chính"
            };
            XacDinhTrangThai(newRecord);
            _attendanceRepository.Save(newRecord);
            return newRecord;
        }

        public AttendanceRecord CheckOut(string maNV, DateTime time)
        {
            var date = time.Date;
            var all = _attendanceRepository.GetAll();
            var existing = all.FirstOrDefault(x => x.MaNV == maNV && x.NgayChamCong.Date == date);
            if (existing == null)
            {
                throw new Exception($"Không tìm thấy bản ghi Check-in ngày {date:dd/MM/yyyy}.");
            }

            if (!existing.GioCheckIn.HasValue)
            {
                throw new Exception("Chưa Check-in, không thể Check-out.");
            }

            existing.GioCheckOut = time.TimeOfDay;
            XacDinhTrangThai(existing);
            _attendanceRepository.Save(existing);
            return existing;
        }

        public AttendanceRecord ScanAuto(string maNV, DateTime scanTime)
        {
            var date = scanTime.Date;
            var t = scanTime.TimeOfDay;
            var all = _attendanceRepository.GetAll();

            var openRecord = all.FirstOrDefault(n =>
                n.MaNV == maNV &&
                n.NgayChamCong.Date == date &&
                n.GioCheckIn.HasValue &&
                !n.GioCheckOut.HasValue);

            if (openRecord != null)
            {
                var startCa = openRecord.TenCa;

                if (startCa.Contains("Ca 3") && t < new TimeSpan(21, 0, 0))
                {
                    throw new Exception("Vui lòng Check-out sau 21h00!");
                }

                if (startCa.Contains("Ca 2") && t < new TimeSpan(17, 0, 0))
                {
                    throw new Exception("Vui lòng Check-out sau 17h00!");
                }

                if (startCa.Contains("Ca 1") && t < new TimeSpan(13, 0, 0))
                {
                    throw new Exception("Vui lòng Check-out sau 13h00!");
                }

                openRecord.GioCheckOut = t;

                var currentCa = openRecord.TenCa;
                if (t >= new TimeSpan(21, 0, 0))
                {
                    currentCa = "Ca 3";
                }
                else if (t >= new TimeSpan(15, 0, 0))
                {
                    currentCa = "Ca 2";
                }
                else if (t >= new TimeSpan(13, 0, 0))
                {
                    currentCa = "Ca 1";
                }

                if (startCa == "Ca 1" && currentCa == "Ca 2") openRecord.TenCa = "Ca 1, 2";
                else if (startCa == "Ca 1" && currentCa == "Ca 3") openRecord.TenCa = "Ca 1, 2, 3";
                else if (startCa == "Ca 2" && currentCa == "Ca 3") openRecord.TenCa = "Ca 2, 3";

                XacDinhTrangThai(openRecord);
                _attendanceRepository.Save(openRecord);
                return openRecord;
            }

            var tenCa = "Ca Mặc Định";
            if (t < new TimeSpan(12, 30, 0)) tenCa = "Ca 1";
            else if (t < new TimeSpan(16, 30, 0)) tenCa = "Ca 2";
            else tenCa = "Ca 3";

            var newRecord = new AttendanceRecord
            {
                MaNLV = Guid.NewGuid().ToString(),
                MaNV = maNV,
                NgayChamCong = date,
                GioCheckIn = t,
                TenCa = tenCa
            };

            XacDinhTrangThai(newRecord);
            _attendanceRepository.Save(newRecord);
            return newRecord;
        }

        private void XacDinhTrangThai(AttendanceRecord record)
        {
            if (!record.GioCheckIn.HasValue)
            {
                record.TrangThai = "Vắng mặt";
                return;
            }

            var gioVaoChuan = new TimeSpan(8, 0, 0);
            var gioRaChuan = new TimeSpan(17, 0, 0);

            if (record.TenCa.Contains("Ca 1")) gioVaoChuan = new TimeSpan(8, 0, 0);
            else if (record.TenCa.Contains("Ca 2")) gioVaoChuan = new TimeSpan(13, 0, 0);
            else if (record.TenCa.Contains("Ca 3")) gioVaoChuan = new TimeSpan(17, 0, 0);

            if (record.TenCa.Contains("Ca 3")) gioRaChuan = new TimeSpan(21, 30, 0);
            else if (record.TenCa.Contains("Ca 2")) gioRaChuan = new TimeSpan(17, 0, 0);
            else if (record.TenCa.Contains("Ca 1")) gioRaChuan = new TimeSpan(13, 0, 0);

            if (record.GioCheckIn.Value > gioVaoChuan)
            {
                record.TrangThai = "Đi trễ";
            }
            else if (record.GioCheckOut.HasValue && record.GioCheckOut.Value < gioRaChuan)
            {
                record.TrangThai = "Về sớm";
            }
            else
            {
                record.TrangThai = "Đúng giờ";
            }
        }
    }
}
