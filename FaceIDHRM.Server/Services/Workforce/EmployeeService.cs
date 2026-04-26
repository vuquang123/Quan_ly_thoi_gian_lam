using System;
using System.Collections.Generic;
using System.Linq;
using FaceIDHRM.Server.Domain.Workforce;
using FaceIDHRM.Server.Repositories.Workforce;

namespace FaceIDHRM.Server.Services.Workforce
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;

        public EmployeeService(IEmployeeRepository repository)
        {
            _repository = repository;
        }

        public List<EmployeeRecord> GetAll()
        {
            return _repository.GetAll();
        }

        public EmployeeRecord? GetById(string maNV)
        {
            return _repository.GetById(maNV);
        }

        public EmployeeRecord Save(EmployeeRecord employee)
        {
            if (string.IsNullOrWhiteSpace(employee.MaNV))
            {
                throw new ArgumentException("Mã nhân viên không hợp lệ.");
            }

            employee.MaNV = employee.MaNV.Trim();
            _repository.Save(employee);
            return employee;
        }

        public void Delete(string maNV)
        {
            _repository.Delete(maNV);
        }
    }
}
