# Hướng Dẫn Chi Tiết Sử Dụng, Kết Nối & Kịch Bản Test Dự Án FaceIDHRM

Tài liệu này đóng vai trò như một cuốn cẩm nang toàn tập giúp bạn (và Giảng viên) nắm rõ cách vận hành hệ thống từ A-Z, từ khâu thiết lập kết nối mạng cho đến các kịch bản demo (Test Cases) ăn điểm nhất.

---

## PHẦN 1: HƯỚNG DẪN KẾT NỐI HỆ THỐNG QUA INTERNET

Hệ thống của chúng ta chia làm 2 phần: **Máy chủ (Server)** và **Máy trạm (Client - Kiosk/Admin)**. Để chúng giao tiếp được với nhau khi ở 2 máy tính khác nhau (hoặc 2 mạng Wifi khác nhau), chúng ta sử dụng công cụ **Ngrok**.

### Bước 1: Khởi động máy chủ Server (Thực hiện trên máy tính đóng vai trò Admin)
1. Mở Terminal / PowerShell.
2. Cấp một đường hầm mạng bằng Ngrok: 
   ```powershell
   ngrok http 5055
   ```
3. Màn hình Ngrok sẽ hiện ra. Hãy bôi đen và copy đường link màu trắng ở dòng `Forwarding` (ví dụ: `https://abcd-1234.ngrok-free.dev`). Bỏ qua phần `-> http://localhost:5055`.
4. Mở một Terminal / PowerShell **MỚI** (vẫn giữ nguyên cửa sổ Ngrok chạy ngầm), điều hướng vào thư mục dự án và gõ lệnh để bật Server lên:
   ```powershell
   dotnet run --project FaceIDHRM.Server
   ```
5. Đợi đến khi màn hình hiện chữ xanh lá `Now listening on: http://0.0.0.0:5055` là Server đã sẵn sàng hứng dữ liệu.

### Bước 2: Khởi động phần mềm Client (Thực hiện trên máy tính Kiosk hoặc nhân viên)
1. Mở Terminal / PowerShell trên máy tính thứ 2.
2. Gắn đường link Ngrok lúc nãy vào môi trường để Kiosk biết đường tìm đến Server:
   ```powershell
   $env:FACEID_SERVER_URL="https://abcd-1234.ngrok-free.dev"
   ```
   *(Nhớ thay link của bạn vào và giữ nguyên dấu ngoặc kép)*.
3. Chạy phần mềm:
   ```powershell
   dotnet run --project FaceIDHRM
   ```
   *(Nếu bạn mở Client trên cùng 1 máy tính với Server để test, bạn không cần chạy lệnh gắn biến môi trường, chỉ cần `dotnet run` là xong vì nó mặc định gọi vào localhost:5055).*

---

## PHẦN 2: CÁC KỊCH BẢN TEST (TEST CASES) ĂN ĐIỂM KHI DEMO

Để buổi thuyết trình mượt mà và chứng minh được các tính năng ưu việt, bạn hãy test theo đúng các kịch bản thực tế sau:

### Kịch Bản 1: Thêm nhân viên mới & Nhận diện tức thì (Test cơ chế Đồng bộ ngầm)
**Mục đích:** Chứng minh rằng Kiosk (Máy nhân viên) luôn luôn đồng bộ dữ liệu thời gian thực với Admin mà không cần khởi động lại app.
*   **Bước 1:** Bật máy Admin, vào Tab **Nhân Sự**.
*   **Bước 2:** Nhập tên nhân viên mới (VD: "Nguyen Van A"), chọn loại *Full-time*, điền mức lương.
*   **Bước 3:** Bấm **Bật Camera** -> Đưa mặt người A vào -> Bấm **📷 Chụp FaceID**.
*   **Bước 4:** Một popup xác nhận hiện lên cho xem trước khuôn mặt. Bấm **Chuẩn rồi**. Sau đó bấm nút **Thêm Mới**.
*   **Bước 5:** Lập tức quay sang màn hình máy **Kiosk**, cho người A đứng trước Camera. Máy Kiosk sẽ tự động nhận ra người A và hỏi: *"Nhận diện thành công... Bạn có muốn XÁC NHẬN chấm công không?"*. 
*   **Thành công:** Kiosk nhận diện được ngay lập tức chứng tỏ Kiosk đã tự động kéo dữ liệu mới từ Server về thành công.

### Kịch Bản 2: Phân luồng Ca làm việc tự động
**Mục đích:** Chứng minh tính thông minh trong việc phân loại Ca 1 / Ca 2.
*   **Tình huống:** Cho 1 nhân viên quẹt mặt Check-in thành công.
*   **Kiểm chứng:** Tùy thuộc vào giờ đồng hồ hiện tại trên máy tính:
    *   Nếu đang là sáng (trước 12h30): Màn hình báo "Check-in Ca 1 thành công".
    *   Nếu đang là chiều (sau 12h30): Màn hình báo "Check-in Ca 2 thành công".
*   *Lưu ý:* Bấm sang máy Admin, mở Tab "Quản Lý Chấm Công", bấm tìm kiếm để thấy dòng lịch sử vừa quẹt nhảy lên lập tức.

### Kịch Bản 3: Xin Về Sớm & SignalR Real-time (Tính năng Đinh của dự án)
**Mục đích:** Chứng minh công nghệ giao tiếp 2 chiều SignalR.
*   **Bước 1:** Ngay sau khi nhân viên A vừa Check-in (chưa hết giờ ca làm), bảo họ tiếp tục đứng trước Camera để quẹt mặt lần 2.
*   **Bước 2:** Kiosk lập tức chặn lại và hiện cảnh báo: *"Chưa đến giờ tan làm! Bạn có chắc chắn muốn gửi YÊU CẦU XIN VỀ SỚM cho Admin không?"*.
*   **Bước 3:** Bấm **Yes**. Màn hình Kiosk chuyển sang màu vàng "Đang chờ admin duyệt...".
*   **Bước 4:** Bấm sang màn hình Admin, vào Tab **Duyệt Xin Về Sớm**. Lập tức thấy 1 yêu cầu vừa bay tới.
*   **Bước 5:** Bấm chọn dòng đó, nhập ghi chú (VD: "Duyệt cho đi khám bệnh") rồi bấm nút **Duyệt (Chấp nhận)**.
*   **Bước 6:** Vừa bấm duyệt xong, màn hình Kiosk tự động nảy sang màu xanh: *"✅ Admin đã duyệt checkout sớm..."* và tự động lưu giờ Check-out.

### Kịch Bản 4: Bắt lỗi "Cố tình quẹt lặp" (Bảo vệ tính toàn vẹn dữ liệu)
**Mục đích:** Chứng minh bạn có nghĩ đến các Case hóc búa để vá Bug.
*   **Tình huống:** Sau khi Kịch bản 3 thành công (nhân viên đã được duyệt về sớm = xong việc trong ngày). Cố tình cho nhân viên đó đứng trước Camera Kiosk để quẹt mặt lần thứ 3.
*   **Kiểm chứng:** Kiosk sẽ hiện lên màu đỏ chót và từ chối: *"❌ Bạn đã hoàn thành ca làm việc và check-out trong ngày hôm nay rồi!"*. Ngăn chặn việc tạo ra các bản ghi rác.

### Kịch Bản 5: Tính lương Đa hình (Polymorphism)
**Mục đích:** Chứng minh việc áp dụng OOP xuất sắc.
*   **Bước 1:** Ở máy Admin, thêm 2 nhân viên: 1 người chọn **Full-time** (Lương cơ bản: 10 triệu, Phụ cấp: 1 triệu) và 1 người chọn **Part-time** (Mức lương/giờ: 20k).
*   **Bước 2:** Cho cả 2 người cùng quẹt mặt Check-in và Check-out (Có thể chỉnh tay giờ trong Database cho nhanh nếu cần).
*   **Bước 3:** Vào Tab Quản Lý Chấm Công, bấm nút **Tính Lương**.
*   **Kiểm chứng:** Cột "Tổng lương" sẽ hiện ra 2 cách tính hoàn toàn khác nhau:
    *   Người Full-time: Tiền lương = Lương cứng + Phụ cấp.
    *   Người Part-time: Tiền lương = Số giờ thực làm x 20k.
    *   *Giải thích với Giảng viên:* Dù nằm chung 1 danh sách, hệ thống tự gọi hàm đa hình `TinhLuong()` tương ứng với từng loại nhân viên mà không cần dùng chuỗi lệnh `if-else` dài dòng.

### Kịch Bản 6: Xuất Báo Cáo
**Mục đích:** Tính ứng dụng thực tế.
*   **Bước 1:** Bấm nút **Xuất Báo Cáo Lương (CSV)** ở góc dưới màn hình Admin.
*   **Bước 2:** Chọn nơi lưu file. Sau đó mở file `.csv` bằng Excel.
*   **Kiểm chứng:** Bảng lương được format ngay ngắn thành các cột, sẵn sàng gửi cho phòng kế toán.
