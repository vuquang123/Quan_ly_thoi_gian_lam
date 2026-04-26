using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaceIDHRM.Server.Domain.Workforce;
using Newtonsoft.Json;

namespace FaceIDHRM.Server.Repositories.Workforce
{
    public class JsonAttendanceRepository : IAttendanceRepository
    {
        private readonly object _sync = new object();
        private readonly string _path;

        public JsonAttendanceRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _path = Path.Combine(dataDir, "attendance.json");
            if (!File.Exists(_path))
            {
                File.WriteAllText(_path, "[]");
            }
        }

        public List<AttendanceRecord> GetAll()
        {
            lock (_sync)
            {
                return ReadAll();
            }
        }

        public void Save(AttendanceRecord record)
        {
            lock (_sync)
            {
                var all = ReadAll();
                var idx = all.FindIndex(x => x.MaNLV == record.MaNLV);
                if (idx >= 0)
                {
                    all[idx] = record;
                }
                else
                {
                    all.Add(record);
                }

                WriteAll(all);
            }
        }

        public void Delete(string maNLV)
        {
            lock (_sync)
            {
                var all = ReadAll().Where(x => x.MaNLV != maNLV).ToList();
                WriteAll(all);
            }
        }

        private List<AttendanceRecord> ReadAll()
        {
            var raw = File.ReadAllText(_path);
            return JsonConvert.DeserializeObject<List<AttendanceRecord>>(raw) ?? new List<AttendanceRecord>();
        }

        private void WriteAll(List<AttendanceRecord> data)
        {
            File.WriteAllText(_path, JsonConvert.SerializeObject(data, Formatting.Indented));
        }
    }
}
