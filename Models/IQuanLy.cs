using System.Collections.Generic;

namespace FaceIDHRM.Models
{
    // Tính trừu tượng: Định nghĩa các hành vi chuẩn cho bộ quản lý
    public interface IQuanLy<T>
    {
        void Them(T entity);
        void Sua(T entity);
        void Xoa(string id);
        T TimKiem(string keyword); // Đa hình bằng cách có thể chia thành nhiều parameter
        List<T> LayDanhSach();
    }
}
