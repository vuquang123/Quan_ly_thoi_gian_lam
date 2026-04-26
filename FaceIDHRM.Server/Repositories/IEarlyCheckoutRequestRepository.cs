using System.Collections.Generic;
using FaceIDHRM.Server.Domain;

namespace FaceIDHRM.Server.Repositories
{
    public interface IEarlyCheckoutRequestRepository
    {
        List<EarlyCheckoutRequest> GetAll();
        List<EarlyCheckoutRequest> GetPending();
        EarlyCheckoutRequest? GetById(string id);
        EarlyCheckoutRequest? GetPendingByMaNV(string maNV);
        void Save(EarlyCheckoutRequest request);
    }
}
