using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FaceIDHRM.Integration
{
    public class WorkforceGateway : IWorkforceGateway
    {
        private readonly HttpClient _httpClient;

        public WorkforceGateway(string baseUrl)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/')),
                Timeout = TimeSpan.FromSeconds(4)
            };
        }

        public async Task<List<EmployeeRecordDto>> GetEmployeesAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<EmployeeRecordDto>>("/api/employees");
            return data ?? new List<EmployeeRecordDto>();
        }

        public async Task<EmployeeRecordDto?> SaveEmployeeAsync(EmployeeRecordDto employee)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/employees", employee);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EmployeeRecordDto>();
        }

        public async Task DeleteEmployeeAsync(string maNV)
        {
            await _httpClient.DeleteAsync($"/api/employees/{maNV}");
        }

        public async Task<List<AttendanceRecordDto>> GetAttendanceAsync(DateTime? from = null, DateTime? to = null)
        {
            var query = string.Empty;
            if (from.HasValue || to.HasValue)
            {
                var q = new List<string>();
                if (from.HasValue) q.Add($"from={Uri.EscapeDataString(from.Value.ToString("o"))}");
                if (to.HasValue) q.Add($"to={Uri.EscapeDataString(to.Value.ToString("o"))}");
                query = "?" + string.Join("&", q);
            }

            var data = await _httpClient.GetFromJsonAsync<List<AttendanceRecordDto>>($"/api/attendance{query}");
            return data ?? new List<AttendanceRecordDto>();
        }

        public async Task<AttendanceRecordDto?> CheckInAsync(ManualCheckDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/attendance/checkin", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AttendanceRecordDto>();
        }

        public async Task<AttendanceRecordDto?> CheckOutAsync(ManualCheckDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/attendance/checkout", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<AttendanceRecordDto>();
        }

        public async Task<AttendanceRecordDto?> ScanAutoAsync(ScanAttendanceDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/attendance/scan-auto", dto);
            if (!response.IsSuccessStatusCode)
            {
                var message = await response.Content.ReadAsStringAsync();
                throw new Exception(message.Trim('"'));
            }

            return await response.Content.ReadFromJsonAsync<AttendanceRecordDto>();
        }
    }
}
