using System.Collections.Generic;
using FaceIDHRM.Server.Domain.Workforce;

namespace FaceIDHRM.Server.Repositories.Workforce
{
    public interface IEmployeeRepository
    {
        List<EmployeeRecord> GetAll();
        EmployeeRecord? GetById(string maNV);
        void Save(EmployeeRecord employee);
        void Delete(string maNV);
    }
}
