using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaceIDHRM.Server.Domain.Workforce;
using Newtonsoft.Json;

namespace FaceIDHRM.Server.Repositories.Workforce
{
    public class JsonEmployeeRepository : IEmployeeRepository
    {
        private readonly object _sync = new object();
        private readonly string _path;

        public JsonEmployeeRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _path = Path.Combine(dataDir, "employees.json");
            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "[]");
            }
        }

        public List<EmployeeRecord> GetAll()
        {
            lock (_sync)
            {
                return ReadAll();
            }
        }

        public EmployeeRecord? GetById(string maNV)
        {
            lock (_sync)
            {
                return ReadAll().FirstOrDefault(x => x.MaNV == maNV);
            }
        }

        public void Save(EmployeeRecord employee)
        {
            lock (_sync)
            {
                var all = ReadAll();
                var idx = all.FindIndex(x => x.MaNV == employee.MaNV);
                if (idx >= 0)
                {
                    all[idx] = employee;
                }
                else
                {
                    all.Add(employee);
                }

                WriteAll(all);
            }
        }

        public void Delete(string maNV)
        {
            lock (_sync)
            {
                var all = ReadAll().Where(x => x.MaNV != maNV).ToList();
                WriteAll(all);
            }
        }

        private List<EmployeeRecord> ReadAll()
        {
            var raw = File.ReadAllText(_path);
            return JsonConvert.DeserializeObject<List<EmployeeRecord>>(raw) ?? new List<EmployeeRecord>();
        }

        private void WriteAll(List<EmployeeRecord> data)
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
}
