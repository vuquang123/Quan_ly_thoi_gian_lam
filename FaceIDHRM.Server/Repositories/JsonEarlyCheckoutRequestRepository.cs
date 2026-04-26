using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FaceIDHRM.Server.Domain;
using Newtonsoft.Json;

namespace FaceIDHRM.Server.Repositories
{
    public class JsonEarlyCheckoutRequestRepository : IEarlyCheckoutRequestRepository
    {
        private readonly object _sync = new object();
        private readonly string _filePath;

        public JsonEarlyCheckoutRequestRepository()
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "early_checkout_requests.json");

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public List<EarlyCheckoutRequest> GetAll()
        {
            lock (_sync)
            {
                return ReadAll();
            }
        }

        public List<EarlyCheckoutRequest> GetPending()
        {
            lock (_sync)
            {
                return ReadAll()
                    .Where(x => x.Status == EarlyCheckoutRequestStatus.Pending)
                    .OrderByDescending(x => x.RequestedAt)
                    .ToList();
            }
        }

        public EarlyCheckoutRequest? GetById(string id)
        {
            lock (_sync)
            {
                return ReadAll().FirstOrDefault(x => x.Id == id);
            }
        }

        public EarlyCheckoutRequest? GetPendingByMaNV(string maNV)
        {
            lock (_sync)
            {
                return ReadAll()
                    .Where(x => x.MaNV == maNV && x.Status == EarlyCheckoutRequestStatus.Pending)
                    .OrderByDescending(x => x.RequestedAt)
                    .FirstOrDefault();
            }
        }

        public void Save(EarlyCheckoutRequest request)
        {
            lock (_sync)
            {
                var all = ReadAll();
                var existingIndex = all.FindIndex(x => x.Id == request.Id);
                if (existingIndex >= 0)
                {
                    all[existingIndex] = request;
                }
                else
                {
                    all.Add(request);
                }

                WriteAll(all);
            }
        }

        private List<EarlyCheckoutRequest> ReadAll()
        {
            var raw = File.ReadAllText(_filePath);
            return JsonConvert.DeserializeObject<List<EarlyCheckoutRequest>>(raw) ?? new List<EarlyCheckoutRequest>();
        }

        private void WriteAll(List<EarlyCheckoutRequest> requests)
        {
            var json = JsonConvert.SerializeObject(requests, Formatting.Indented);
            File.WriteAllText(_filePath, json);
        }
    }
}
