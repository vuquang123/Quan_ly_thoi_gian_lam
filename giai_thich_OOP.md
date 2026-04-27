# Các Nguyên Lý Lập Trình Hướng Đối Tượng (OOP) Áp Dụng Trong FaceIDHRM

Tài liệu này giải thích chi tiết cách 4 tính chất cốt lõi của Lập trình hướng đối tượng (OOP) được áp dụng vào kiến trúc phần mềm FaceIDHRM, kèm theo dẫn chứng cụ thể từ mã nguồn. Đây là tài liệu phục vụ cho việc thuyết trình bảo vệ đồ án.

---

## 1. Tính Trừu Tượng (Abstraction)
**Khái niệm:** Trừu tượng hóa là việc ẩn đi các chi tiết triển khai phức tạp bên trong, chỉ bộc lộ ra các phương thức hoặc giao diện (interface) cần thiết cho đối tượng khác sử dụng.

**Áp dụng trong dự án:**
Hệ thống sử dụng rất nhiều `Interface` để thiết kế kiến trúc phần mềm (Design Pattern: Dependency Injection, Repository Pattern).
*   **Ví dụ 1: `IQuanLy<T>`**: Đây là một Interface chung định nghĩa các hành vi quản lý dữ liệu (Thêm, Sửa, Xóa, Tìm kiếm, Lấy danh sách). Cả `NhanSuManager` và `ChamCongManager` đều triển khai (implement) Interface này. Người gọi (Giao diện UI) chỉ cần biết nó có hàm `TimKiem()`, chứ không cần quan tâm nó tìm kiếm bằng mảng, bằng List, hay đệ quy như thế nào.
*   **Ví dụ 2: Lớp Gateway (`IWorkforceGateway`, `IEarlyCheckoutGateway`)**: Tầng giao diện của Kiosk và Admin chỉ cần gọi `await _workforceGateway.GetEmployeesAsync()`. Việc nó tạo kết nối HTTP, cấu hình Timeout, gắn Header để né hệ thống chặn mạng (Ngrok), hay Parse JSON... đều được ẩn đi hoàn toàn phía sau lớp vỏ Gateway.

---

## 2. Tính Đóng Gói (Encapsulation)
**Khái niệm:** Đóng gói là việc nhóm các dữ liệu (thuộc tính) và các hành vi (phương thức) liên quan chặt chẽ với nhau vào trong một lớp (Class). Đồng thời, che giấu dữ liệu bằng các Access Modifier (`private`, `protected`) để ngăn chặn việc thay đổi dữ liệu tùy tiện từ bên ngoài.

**Áp dụng trong dự án:**
*   **Che giấu trạng thái:** Trong lớp `NhanVien`, các thuộc tính cơ bản được đóng gói. Trong `ChamCongManager`, danh sách `_danhSachChamCong` được khai báo là `private`. Không có bất kỳ lớp nào từ bên ngoài có thể trực tiếp làm thay đổi hoặc xóa phần tử của danh sách này. Mọi thay đổi đều phải đi qua các hàm `CheckIn()`, `CheckOut()` hoặc `XuLyQuetMatTuDong()`.
*   **Gom nhóm logic:** Thay vì tính toán trạng thái "Đi trễ", "Về sớm", "Đúng giờ" ở bên ngoài giao diện, phương thức `XacDinhTrangThai()` được đóng gói ngay bên trong Class `NgayLamViec` (và `AttendanceRecord`). Khi có dữ liệu `GioCheckIn` và `GioCheckOut` thay đổi, bản thân đối tượng đó tự biết cách tính toán trạng thái của chính nó.

---

## 3. Tính Kế Thừa (Inheritance)
**Khái niệm:** Kế thừa cho phép một lớp mới (lớp con) thừa hưởng các thuộc tính và phương thức từ một lớp đã có sẵn (lớp cha). Điều này giúp tái sử dụng mã nguồn và dễ dàng bảo trì.

**Áp dụng trong dự án:**
Hệ thống sử dụng Kế thừa để phân chia mô hình Nhân sự.
*   **Lớp cha:** `abstract class NhanVien` chứa các thông tin chung bắt buộc mà nhân viên nào cũng phải có: `MaNV`, `HoTen`, `NgaySinh`, `SoDienThoai`, `PhongBan`.
*   **Lớp con 1:** `class NhanVienFullTime : NhanVien` kế thừa toàn bộ thuộc tính của cha, bổ sung thêm các thuộc tính riêng như `LuongCoBan`, `HeSoLuong`, `TienPhuCap`.
*   **Lớp con 2:** `class NhanVienPartTime : NhanVien` kế thừa từ cha, bổ sung thêm `MucLuongTheoGio`, `SoGioLamToiDa`.

**Lợi ích:** Tránh việc phải lặp lại việc khai báo các trường như `HoTen`, `MaNV` nhiều lần ở các lớp khác nhau. Khi cần thêm một loại nhân viên mới (VD: `NhanVienThoiVu`), chỉ cần tạo class mới kế thừa từ `NhanVien` là xong.

---

## 4. Tính Đa Hình (Polymorphism)
**Khái niệm:** Đa hình cho phép các đối tượng thuộc các lớp con khác nhau (nhưng cùng chung một lớp cha) thực hiện cùng một hành động theo những cách khác nhau.

**Áp dụng trong dự án:**
Đây là tính chất được áp dụng mạnh mẽ và rõ ràng nhất trong tính năng **Tính Lương**.
*   Trong lớp cha `NhanVien`, có khai báo một phương thức trừu tượng: `public abstract double TinhLuong();`
*   Lớp `NhanVienFullTime` ghi đè (Override) phương thức này:
    `return (LuongCoBan * HeSoLuong) + TienPhuCap;`
*   Lớp `NhanVienPartTime` ghi đè phương thức này theo công thức khác:
    `return MucLuongTheoGio * SoGioDaLamTrongThang;`

**Sức mạnh của Đa Hình lúc chạy (Dynamic Binding):**
Ở màn hình Admin, khi duyệt qua danh sách hàng chục nhân viên để tính quỹ lương:
```csharp
double tongQuyLuong = 0;
foreach (NhanVien nv in dsNV)
{
    // ĐA HÌNH Ở ĐÂY: Hệ thống tự động biết nv đang xét là FullTime hay PartTime 
    // để gọi đúng công thức TinhLuong() tương ứng lúc Runtime.
    double tongLuong = nv.TinhLuong(); 
    tongQuyLuong += tongLuong;
}
```

