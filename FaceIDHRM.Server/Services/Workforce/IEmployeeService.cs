using System.Collections.Generic;
using FaceIDHRM.Server.Domain.Workforce;

namespace FaceIDHRM.Server.Services.Workforce
{
    public interface IEmployeeService
    {
        List<EmployeeRecord> GetAll();
        EmployeeRecord? GetById(string maNV);
        EmployeeRecord Save(EmployeeRecord employee);
        void Delete(string maNV);
    }
}
