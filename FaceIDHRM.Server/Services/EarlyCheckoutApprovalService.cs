using System;
using System.Collections.Generic;
using FaceIDHRM.Server.Domain;
using FaceIDHRM.Server.Repositories;

namespace FaceIDHRM.Server.Services
{
    public class EarlyCheckoutApprovalService : IEarlyCheckoutApprovalService
    {
        private readonly IEarlyCheckoutRequestRepository _repository;

        public EarlyCheckoutApprovalService(IEarlyCheckoutRequestRepository repository)
        {
            _repository = repository;
        }

        public List<EarlyCheckoutRequest> GetPending()
        {
            return _repository.GetPending();
        }

        public EarlyCheckoutRequest? GetById(string id)
        {
            return _repository.GetById(id);
        }

        public EarlyCheckoutRequest CreateRequest(string maNV, string lyDo, string requestedFromMachine)
        {
            if (string.IsNullOrWhiteSpace(maNV))
            {
                throw new ArgumentException("Mã nhân viên là bắt buộc.");
            }

            var existingPending = _repository.GetPendingByMaNV(maNV);
            if (existingPending != null)
            {
                return existingPending;
            }

            var request = new EarlyCheckoutRequest
            {
                Id = Guid.NewGuid().ToString(),
                MaNV = maNV.Trim(),
                LyDo = string.IsNullOrWhiteSpace(lyDo) ? "Việc đột xuất" : lyDo.Trim(),
                RequestedFromMachine = string.IsNullOrWhiteSpace(requestedFromMachine) ? "Kiosk" : requestedFromMachine.Trim(),
                RequestedAt = DateTime.Now,
                Status = EarlyCheckoutRequestStatus.Pending
            };

            _repository.Save(request);
            return request;
        }

        public EarlyCheckoutRequest Approve(string id, string adminName, string adminNote, DateTime? checkoutTime = null)
        {
            var request = _repository.GetById(id) ?? throw new ArgumentException("Không tìm thấy yêu cầu.");
            if (request.Status != EarlyCheckoutRequestStatus.Pending)
            {
                throw new InvalidOperationException("Yêu cầu này đã được xử lý trước đó.");
            }

            request.Status = EarlyCheckoutRequestStatus.Approved;
            request.AdminName = string.IsNullOrWhiteSpace(adminName) ? "Admin" : adminName.Trim();
            request.AdminNote = adminNote?.Trim();
            request.ResolvedAt = DateTime.Now;
            request.CheckoutTime = checkoutTime;

            _repository.Save(request);
            return request;
        }

        public EarlyCheckoutRequest Reject(string id, string adminName, string adminNote)
        {
            var request = _repository.GetById(id) ?? throw new ArgumentException("Không tìm thấy yêu cầu.");
            if (request.Status != EarlyCheckoutRequestStatus.Pending)
            {
                throw new InvalidOperationException("Yêu cầu này đã được xử lý trước đó.");
            }

            request.Status = EarlyCheckoutRequestStatus.Rejected;
            request.AdminName = string.IsNullOrWhiteSpace(adminName) ? "Admin" : adminName.Trim();
            request.AdminNote = string.IsNullOrWhiteSpace(adminNote) ? "Từ chối checkout sớm." : adminNote.Trim();
            request.ResolvedAt = DateTime.Now;

            _repository.Save(request);
            return request;
        }

        public EarlyCheckoutRequest MarkProcessed(string id)
        {
            var request = _repository.GetById(id) ?? throw new ArgumentException("Không tìm thấy yêu cầu.");
            request.Status = EarlyCheckoutRequestStatus.Processed;
            _repository.Save(request);
            return request;
        }
    }
}
