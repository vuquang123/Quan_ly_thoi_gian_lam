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

---

## 5. Luồng thực thi (Flow) của đồ án gắn liền với các tính chất OOP

Để có cái nhìn tổng quan về cách các nguyên lý OOP vận hành trong thực tế đồ án, dưới đây là luồng thực thi (flow) từ lúc bật phần mềm đến lúc tính lương cuối tháng:

### Flow 1: Khởi động hệ thống & Tải dữ liệu (Tính Trừu Tượng + Đóng Gói)
*   **Quá trình:** Khi phần mềm bật lên, thay vì giao diện màn hình (UI) trực tiếp tự mở các file `.json` hoặc tự kết nối Database để lấy dữ liệu, thì giao diện sẽ **ủy quyền** việc đó cho các "Người quản lý" (`NhanSuManager`, `ChamCongManager`).
*   **Áp dụng OOP:**
    *   **Đóng gói:** Các danh sách chứa hàng trăm nhân viên hay hàng nghìn lượt chấm công được cất giấu bằng từ khóa `private _danhSachNhanVien` bên trong Manager. Form không thể tự ý sửa xóa trực tiếp.
    *   **Trừu tượng:** UI giao tiếp với Manager thông qua một Interface `IQuanLy<T>`. UI chỉ biết gọi lệnh `"Này Manager, lấy danh sách ra đây"`, hoàn toàn không biết cấu trúc lưu trữ bên dưới của Manager là gì.

### Flow 2: Thêm mới Nhân sự (Tính Kế Thừa)
*   **Quá trình:** Khi Admin nhập thông tin và chọn phân loại nhân viên là *Full-time* hay *Part-time* rồi bấm Lưu.
*   **Áp dụng OOP:**
    *   **Kế thừa:** Dưới code, hệ thống khởi tạo `new NhanVienFullTime()` hoặc `new NhanVienPartTime()`. Nhờ kế thừa từ lớp `NhanVien`, lập trình viên không phải viết code lại các trường như `MaNV`, `HoTen`. Đặc biệt, hệ thống có thể nhét tất cả các thể loại nhân viên này vào chung một danh sách duy nhất `List<NhanVien>` để dễ dàng duyệt và quản lý tập trung.

### Flow 3: Luồng Chấm công Kiosk (Tính Đóng Gói Hành Vi)
*   **Quá trình:** Khi camera nhận diện được khuôn mặt, nó gửi `MaNV` và thời gian hiện tại vào hệ thống chấm công.
*   **Áp dụng OOP:**
    *   **Đóng gói hành vi:** Thay vì Kiosk phải viết các lệnh `if/else` để tính toán xem người này đi làm đúng giờ hay đi trễ, nó chỉ việc khởi tạo đối tượng `NgayLamViec` và gọi phương thức `openRecord.XacDinhTrangThai()`. Bản thân đối tượng `NgayLamViec` đã "đóng gói" sẵn quy tắc các ca làm việc bên trong nó. Nó tự lấy thời gian check-in để đối chiếu và tự dán nhãn "Đúng giờ" hay "Đi trễ".

### Flow 4: Chạy Báo Cáo Tính Lương (Tính Đa Hình)
*   **Quá trình:** Cuối tháng, Admin bấm nút **"Tính Lương"**. Lương Full-time = [Lương cơ bản + Phụ cấp], lương Part-time = [Số giờ làm x Tiền lương/giờ].
*   **Áp dụng OOP:**
    *   **Đa hình:** Vòng lặp tính lương duyệt qua danh sách hàng trăm người chỉ cần dùng đúng 1 dòng code duy nhất: `double tien = nv.TinhLuong();` mà không cần lệnh rẽ nhánh `if-else` để kiểm tra loại nhân viên.
    * Tại thời điểm ứng dụng chạy, hệ thống sẽ tự động bắt mạch xem cái biến `nv` kia đang chứa đối tượng Full-time hay Part-time để gọi đúng công thức tính lương của riêng người đó.

---

## 6. Các Thư Viện Được Sử Dụng Trong Dự Án

Để kiến trúc OOP và hệ thống vận hành mượt mà, dự án tích hợp các thư viện sau:

### A. Phía Client / Kiosk (`FaceIDHRM`)
*   **`OpenCvSharp4` / `OpenCvSharp4.Extensions` / `OpenCvSharp4.runtime.win`**: 
    *   *Vai trò:* Cung cấp các Wrapper C# cho OpenCV gốc.
    *   *Tính năng:* Điều phối luồng Camera trực tiếp (`OpenCvCamera`), phát hiện biên độ khuôn mặt bằng Cascade Classifier, hỗ trợ chuyển đổi định dạng Ma trận hình ảnh (`Mat`) sang `System.Drawing.Bitmap` hiển thị trên giao diện Windows Forms.
*   **`Microsoft.AspNetCore.SignalR.Client`**:
    *   *Vai trò:* Trình kết nối Socket Client.
    *   *Tính năng:* Nhận diện các luồng dữ liệu Real-time đồng bộ từ server phát xuống, ví dụ: Cập nhật tức thì các yêu cầu Checkout sớm khi được Admin phê chuẩn.
*   **`Newtonsoft.Json`**:
    *   *Vai trò:* Chuyển đổi dữ liệu.

### B. Phía Server (`FaceIDHRM.Server`)
*   **`Microsoft.AspNetCore.OpenApi`**:
    *   *Vai trò:* Tự động hóa xuất tài liệu API (Swagger).
*   **Tích hợp sẵn SignalR Server Core**: Xử lý phân phối gói tin tức thì.
