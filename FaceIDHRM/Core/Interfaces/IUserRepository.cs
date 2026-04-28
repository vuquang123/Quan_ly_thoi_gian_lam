using System.Collections.Generic;
using FaceIDHRM.Models;

namespace FaceIDHRM.Core.Interfaces
{
    public interface IUserRepository
    {
        void SaveUser(NhanVien user);
        IEnumerable<NhanVien> GetAllUsers();
    }
}
