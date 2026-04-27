# Sơ Đồ Luồng (Flow Chart) Dự Án FaceIDHRM

Bạn có thể sử dụng các sơ đồ này để dán vào Slide PowerPoint hoặc báo cáo Word. Các sơ đồ này được vẽ bằng cú pháp Mermaid, bạn có thể copy đoạn code bên dưới paste vào trang web [Mermaid Live Editor](https://mermaid.live/) để xuất ra hình ảnh độ nét cao.

---

## 1. Sơ đồ Kiến trúc Hệ thống Tổng thể (System Architecture)
Mô tả cách Kiosk và Admin giao tiếp với Server thông qua Internet (Ngrok) và SignalR.

```mermaid
graph TD
    subgraph Client [Tầng Client - Máy Trạm]
        K[Máy Kiosk Nhân viên]
        A[Máy tính Admin]
    end

    subgraph Internet [Tầng Mạng]
        N((Ngrok Tunnel))
    end

    subgraph Server [Tầng Server - FaceIDHRM.Server]
        API[RESTful API Controllers]
        Hub[SignalR Hub Real-time]
        Repo[JSON Repositories]
    end

    subgraph Database [Tầng Dữ Liệu]
        DB[(File JSON: NhanVien, ChamCong...)]
    end

    K -- Gửi HTTP Request --> N
    A -- Gửi HTTP Request --> N
    N -- Forward Port 5055 --> API
    K <== Kênh kết nối 2 chiều ==> Hub
    A <== Kênh kết nối 2 chiều ==> Hub
    
    API --> Repo
    Hub --> Repo
    Repo <--> DB
```

---

## 2. Sơ đồ Luồng Chấm Công Nhận Diện Khuôn Mặt (Attendance Flow)
Mô tả logic thông minh của Kiosk khi một nhân viên đứng trước Camera.

```mermaid
flowchart TD
    Start([Nhân viên đứng trước Camera]) --> FaceDetect{Camera nhận diện <br>thành công?}
    FaceDetect -- Không --> Start
    FaceDetect -- Có --> GetNV[Trích xuất Mã NV]
    
    GetNV --> CheckComplete{Hôm nay đã <br>hoàn thành ca làm <br>chưa?}
    
    CheckComplete -- Rồi --> Error1[Báo lỗi: Đã hoàn thành ca làm việc] --> End([Kết thúc])
    
    CheckComplete -- Chưa --> CheckOpen{Có ca nào đang mở <br>(đã Check-in, chưa Check-out) <br>không?}
    
    CheckOpen -- Không --> CheckIn[Khởi tạo Check-in mới]
    CheckIn --> PhanCa{Giờ hiện tại?}
    PhanCa -- Trước 12h30 --> Ca1[Xếp vào Ca 1]
    PhanCa -- Sau 12h30 --> Ca2[Xếp vào Ca 2]
    Ca1 --> SuccessCheckIn[Báo: Check-in thành công] --> End
    Ca2 --> SuccessCheckIn
    
    CheckOpen -- Có --> CheckTime{Đã hết giờ quy định <br>của ca làm chưa?}
    
    CheckTime -- Rồi --> CheckOut[Thực hiện Check-out] --> SuccessCheckOut[Báo: Check-out thành công] --> End
    
    CheckTime -- Chưa --> AskEarly{Hỏi: Bạn muốn xin<br>về sớm không?}
    AskEarly -- Không --> End
    AskEarly -- Có --> SendReq[Gửi Yêu cầu qua SignalR lên Admin]
    
    SendReq --> Waiting[Kiosk đóng băng, <br>Chờ Admin duyệt]
    Waiting --> AdminAction{Admin Quyết định?}
    AdminAction -- Từ chối --> Reject[Báo lỗi: Bị từ chối] --> End
    AdminAction -- Chấp nhận --> CheckOut
```

---

## 3. Sơ đồ OOP - Đa Hình Tính Lương (Polymorphism Flow)
Mô tả cách nguyên lý Đa Hình (Polymorphism) giải quyết bài toán tính lương.

```mermaid
sequenceDiagram
    participant Admin as Màn hình Admin
    participant Manager as ChamCongManager
    participant NV as Danh sách List<NhanVien>
    participant FT as NhanVienFullTime (Lớp con)
    participant PT as NhanVienPartTime (Lớp con)

    Admin->>Manager: Bấm nút "Tính Lương"
    Manager->>NV: Lấy danh sách toàn bộ nhân viên
    
    loop Duyệt từng nhân viên (nv)
        Manager->>NV: Gọi hàm nv.TinhLuong()
        
        alt Nếu nv là Full-time
            NV->>FT: Kích hoạt hàm TinhLuong() của Full-time
            FT-->>Manager: Trả về: Lương CB * Hệ Số + Phụ Cấp
        else Nếu nv là Part-time
            NV->>PT: Kích hoạt hàm TinhLuong() của Part-time
            PT-->>Manager: Trả về: Mức lương/giờ * Số giờ làm
        end
    end
    
    Manager-->>Admin: Hiển thị Tổng quỹ lương lên bảng
```
