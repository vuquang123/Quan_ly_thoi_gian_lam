using System.Collections.Generic;
using FaceIDHRM.Server.Domain.Workforce;

namespace FaceIDHRM.Server.Repositories.Workforce
{
    public interface IAttendanceRepository
    {
        List<AttendanceRecord> GetAll();
        void Save(AttendanceRecord record);
        void Delete(string maNLV);
    }
}
