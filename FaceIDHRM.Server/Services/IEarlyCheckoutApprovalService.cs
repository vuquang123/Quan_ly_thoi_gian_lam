using System.Collections.Generic;
using FaceIDHRM.Server.Domain;

namespace FaceIDHRM.Server.Services
{
    public interface IEarlyCheckoutApprovalService
    {
        List<EarlyCheckoutRequest> GetPending();
        EarlyCheckoutRequest? GetById(string id);
        EarlyCheckoutRequest CreateRequest(string maNV, string lyDo, string requestedFromMachine);
        EarlyCheckoutRequest Approve(string id, string adminName, string adminNote, DateTime? checkoutTime = null);
        EarlyCheckoutRequest Reject(string id, string adminName, string adminNote);
        EarlyCheckoutRequest MarkProcessed(string id);
    }
}
