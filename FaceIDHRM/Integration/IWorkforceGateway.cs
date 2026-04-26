using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FaceIDHRM.Integration
{
    public interface IWorkforceGateway
    {
        Task<List<EmployeeRecordDto>> GetEmployeesAsync();
        Task<EmployeeRecordDto?> SaveEmployeeAsync(EmployeeRecordDto employee);
        Task DeleteEmployeeAsync(string maNV);

        Task<List<AttendanceRecordDto>> GetAttendanceAsync(DateTime? from = null, DateTime? to = null);
        Task<AttendanceRecordDto?> CheckInAsync(ManualCheckDto dto);
        Task<AttendanceRecordDto?> CheckOutAsync(ManualCheckDto dto);
        Task<AttendanceRecordDto?> ScanAutoAsync(ScanAttendanceDto dto);
    }
}
