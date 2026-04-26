using System;
using System.Collections.Generic;
using FaceIDHRM.Server.Domain.Workforce;

namespace FaceIDHRM.Server.Services.Workforce
{
    public interface IAttendanceService
    {
        List<AttendanceRecord> GetAll(DateTime? from = null, DateTime? to = null);
        AttendanceRecord CheckIn(string maNV, DateTime time);
        AttendanceRecord CheckOut(string maNV, DateTime time);
        AttendanceRecord ScanAuto(string maNV, DateTime scanTime);
    }
}
