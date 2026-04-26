using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace FaceIDHRM.Integration
{
    public class EarlyCheckoutGateway : IEarlyCheckoutGateway
    {
        private readonly HttpClient _httpClient;
        private readonly HubConnection _hubConnection;
        private readonly string _baseUrl;

        public event Action<EarlyCheckoutRequestDto>? RequestUpdated;

        public EarlyCheckoutGateway(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(4)
            };
            _httpClient.DefaultRequestHeaders.Add("ngrok-skip-browser-warning", "true");

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{_baseUrl}/hubs/early-checkout", options => 
                {
                    options.Headers["ngrok-skip-browser-warning"] = "true";
                })
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<EarlyCheckoutRequestDto>("RequestUpdated", payload =>
            {
                RequestUpdated?.Invoke(payload);
            });
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Connected || _hubConnection.State == HubConnectionState.Connecting)
            {
                return;
            }

            try
            {
                await _hubConnection.StartAsync();
            }
            catch
            {
                // Network may be unavailable temporarily; UI should continue to operate with manual refresh.
            }
        }

        public async Task<List<EarlyCheckoutRequestDto>> GetPendingAsync()
        {
            var data = await _httpClient.GetFromJsonAsync<List<EarlyCheckoutRequestDto>>("/api/early-checkout-requests/pending");
            return data ?? new List<EarlyCheckoutRequestDto>();
        }

        public Task<EarlyCheckoutRequestDto?> GetByIdAsync(string requestId)
        {
            return _httpClient.GetFromJsonAsync<EarlyCheckoutRequestDto>($"/api/early-checkout-requests/{requestId}");
        }

        public async Task<EarlyCheckoutRequestDto?> CreateRequestAsync(CreateEarlyCheckoutRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync("/api/early-checkout-requests", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EarlyCheckoutRequestDto>();
        }

        public async Task<EarlyCheckoutRequestDto?> ApproveAsync(string requestId, ResolveEarlyCheckoutRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/early-checkout-requests/{requestId}/approve", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EarlyCheckoutRequestDto>();
        }

        public async Task<EarlyCheckoutRequestDto?> RejectAsync(string requestId, ResolveEarlyCheckoutRequestDto dto)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/early-checkout-requests/{requestId}/reject", dto);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EarlyCheckoutRequestDto>();
        }

        public async Task<EarlyCheckoutRequestDto?> MarkProcessedAsync(string requestId)
        {
            var response = await _httpClient.PostAsync($"/api/early-checkout-requests/{requestId}/processed", null);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EarlyCheckoutRequestDto>();
        }
    }
}
