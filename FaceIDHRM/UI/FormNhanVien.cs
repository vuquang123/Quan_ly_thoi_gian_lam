using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceIDHRM.Integration;
using FaceIDHRM.Managers;
using FaceIDHRM.Models;
using OpenCvSharp.Extensions;

namespace FaceIDHRM.UI
{
    public class FormNhanVien : Form
    {
        private FaceIDManager _faceManager;
        private NhanSuManager _nhanSuManager;
        private ChamCongManager _chamCongManager;
        
        private PictureBox _cameraBox;
        private Label _lblStatus;
        private Label _lblClock;
        private ListBox _lstHistory;
        private System.Windows.Forms.Timer _videoTimer;
        private System.Windows.Forms.Timer _clockTimer;
        
        private DateTime _latestScanTime = DateTime.MinValue;
        private DateTime _cooldownUntil = DateTime.MinValue;

        private enum KioskState { ChoKhuonMat, DangKyKhuonMat, ChoDuyetCheckoutSom }
        private KioskState _trangThai = KioskState.ChoKhuonMat;
        private string _idTamThoi = null;
        private DateTime _thoiGianDemNguocDangKy;
        private string _pendingApprovalRequestId = string.Empty;

        private readonly IEarlyCheckoutGateway _approvalGateway;

        public FormNhanVien()
        {
            this.Text = "Chấm công";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
            this.FormClosing += FormNhanVien_FormClosing;

            _faceManager = new FaceIDManager();
            _nhanSuManager = new NhanSuManager();
            _chamCongManager = new ChamCongManager();
            _approvalGateway = new EarlyCheckoutGateway(ServerConfig.ApprovalServerUrl);
            _approvalGateway.RequestUpdated += OnApprovalRequestUpdated;

            SetupUI();
            
            this.Load += (s, e) => {
                _faceManager.BatCamera();
                _videoTimer.Start();
                _clockTimer.Start();
                ShowWaitingState();
                _ = _approvalGateway.ConnectAsync();
            };
        }

        private void SetupUI()
        {
            TableLayoutPanel tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            this.Controls.Add(tlp);

            // Cột bên Trái: Camera
            Panel pnlLeft = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            _cameraBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            pnlLeft.Controls.Add(_cameraBox);
            tlp.Controls.Add(pnlLeft, 0, 0);

            // Cột bên Phải: Đồng hồ, Result, History
            Panel pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            _lblClock = new Label
            {
                Text = "00:00:00",
                Font = new Font("Arial", 48, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 100,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkSlateBlue
            };
            pnlRight.Controls.Add(_lblClock);

            Panel sep1 = new Panel { Dock = DockStyle.Top, Height = 20 };
            pnlRight.Controls.Add(sep1);

            _lblStatus = new Label
            {
                Text = "Đang chờ nhận diện...",
                Font = new Font("Arial", 18, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 120,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(10)
            };
            pnlRight.Controls.Add(_lblStatus);

            Panel sep2 = new Panel { Dock = DockStyle.Top, Height = 30 };
            pnlRight.Controls.Add(sep2);

            Label lblHistoryTitle = new Label
            {
                Text = "Lịch sử chấm công",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlRight.Controls.Add(lblHistoryTitle);

            _lstHistory = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 16),
                ItemHeight = 25,
                IntegralHeight = false
            };
            pnlRight.Controls.Add(_lstHistory);

            Button btnExit = new Button
            {
                Text = "Thoát",
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.IndianRed,
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnExit.Click += (s, e) => this.Close();
            pnlRight.Controls.Add(btnExit);

            Panel sepBtn = new Panel { Dock = DockStyle.Bottom, Height = 10 };
            pnlRight.Controls.Add(sepBtn);

            Button btnDangKy = new Button
            {
                Text = "📷 Bổ sung FaceID (Người mới)",
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.DarkBlue,
                ForeColor = Color.White,
                Font = new Font("Arial", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnDangKy.Click += BtnDangKy_Click;
            pnlRight.Controls.Add(btnDangKy);

            tlp.Controls.Add(pnlRight, 1, 0);

            _videoTimer = new System.Windows.Forms.Timer { Interval = 30 };
            _videoTimer.Tick += VideoTimer_Tick;

            _clockTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _clockTimer.Tick += ClockTimer_Tick;
        }

        private void ClockTimer_Tick(object? sender, EventArgs e)
        {
            _lblClock.Text = DateTime.Now.ToString("HH:mm:ss");

            // Reset UI nếu đã hết Cooldown
            if (_trangThai != KioskState.ChoDuyetCheckoutSom && DateTime.Now >= _cooldownUntil && _lblStatus.ForeColor != Color.Gray)
            {
                ShowWaitingState();
            }
        }

        private void ShowWaitingState()
        {
            _lblStatus.Text = "Đang chờ nhận diện...";
            _lblStatus.ForeColor = Color.Gray;
            _lblStatus.BackColor = Color.WhiteSmoke;
        }

        private void VideoTimer_Tick(object? sender, EventArgs e)
        {
            using var frame = _faceManager.GetFrame();
            if (frame != null && !frame.Empty())
            {
                var faces = _faceManager.PhatHienKhuonMat(frame);
                foreach (var rect in faces)
                {
                    var color = DateTime.Now < _cooldownUntil ? OpenCvSharp.Scalar.Orange : OpenCvSharp.Scalar.Green;
                    OpenCvSharp.Cv2.Rectangle(frame, rect, color, 3);
                }

                var oldImg = _cameraBox.Image;
                _cameraBox.Image = BitmapConverter.ToBitmap(frame);
                oldImg?.Dispose();

                if (DateTime.Now < _cooldownUntil) return;

                if (_trangThai == KioskState.DangKyKhuonMat)
                {
                    double timeLeft = (_thoiGianDemNguocDangKy - DateTime.Now).TotalSeconds;
                    
                    if (timeLeft > 0)
                    {
                        ShowResult($"📷 Giữ thẳng mặt! Đang bắt nét trong {Math.Ceiling(timeLeft)}s...", Color.DarkBlue, Color.Yellow);
                    }
                    else
                    {
                        if (faces.Length == 1) // Chỉ lưu nếu có duy nhất 1 mặt rõ nét để tránh dính nhiều người
                        {
                            using var croppedFace = new OpenCvSharp.Mat(frame, faces[0]);
                            try
                            {
                                string newPath = _faceManager.Enrollment(_idTamThoi, croppedFace);
                                
                                var nv = _nhanSuManager.TimKiem(_idTamThoi);
                                nv.FaceDataPath = newPath;
                                _nhanSuManager.Sua(nv); // Lưu vào DB

                                Console.Beep(1000, 500); // Kéo tiếng bip dài báo ok
                                ShowResult("✅ Lấy FaceID thành công! Giờ bạn có thể chấm công.", Color.White, Color.Green);
                                _cooldownUntil = DateTime.Now.AddSeconds(4);
                            }
                            catch (Exception ex)
                            {
                                ShowResult("❌ Lỗi khi lấy ảnh: " + ex.Message, Color.Red, Color.MistyRose);
                                _cooldownUntil = DateTime.Now.AddSeconds(2);
                            }
                        }
                        else if (faces.Length > 1)
                        {
                            ShowResult("❌ Quá nhiều khuôn mặt trong khung hình! Vui lòng đứng 1 mình.", Color.Red, Color.MistyRose);
                            _cooldownUntil = DateTime.Now.AddSeconds(3);
                        }
                        else
                        {
                            ShowResult("❌ Không tìm thấy khuôn mặt rõ diện! Hãy thử lại sau.", Color.Red, Color.MistyRose);
                            _cooldownUntil = DateTime.Now.AddSeconds(3);
                        }
                        _trangThai = KioskState.ChoKhuonMat; // Trả về trạng thái quét bình thường
                    }
                    return; // Ngắt không chạy các logic Check-in hay AntiSpoofing bên dưới nữa
                }

                if (_trangThai == KioskState.ChoDuyetCheckoutSom)
                {
                    return;
                }

                // Logic Auto-Scan: Scan 1 giây 1 lần nếu hết cooldown và có mặt
                if (faces.Length > 0 && DateTime.Now >= _cooldownUntil)
                {
                    if ((DateTime.Now - _latestScanTime).TotalMilliseconds >= 1000)
                    {
                        _latestScanTime = DateTime.Now;
                        ProcessFaceCheckIn(frame, faces[0]);
                    }
                }
            }
        }

        private void ProcessFaceCheckIn(OpenCvSharp.Mat frame, OpenCvSharp.Rect faceRect)
        {
            using var croppedFace = new OpenCvSharp.Mat(frame, faceRect);
            string recognizedID = _faceManager.Verification(croppedFace, _nhanSuManager.LayDanhSach());

            if (recognizedID != null)
            {
                var nhanVien = _nhanSuManager.TimKiem(recognizedID);
                if (nhanVien != null)
                {
                    ExecuteChamCongLogic(recognizedID);
                }
            }
            else
            {
                ShowResult("❌ Không nhận diện được khuôn mặt. Vui lòng đứng gần lại!", Color.Red, Color.MistyRose);
                // Tìm thấy mặt nhưng không match => cooldown 2s tránh giật UI liên tục
                _cooldownUntil = DateTime.Now.AddSeconds(2);
            }
        }

        private void ExecuteChamCongLogic(string recognizedID)
        {
            if (!string.IsNullOrEmpty(_pendingApprovalRequestId))
            {
                return;
            }

            try
            {
                var nhanVien = _nhanSuManager.TimKiem(recognizedID);
                
                // Sử dụng logic mới XuLyQuetMatTuDong cho nhiều ca
                var recordResult = _chamCongManager.XuLyQuetMatTuDong(recognizedID, DateTime.Now);
                string processText = "";
                string timeStr = "";

                if (recordResult.GioCheckOut.HasValue) // Hành động Check-out
                {
                    timeStr = recordResult.GioCheckOut.Value.ToString(@"hh\:mm");
                    processText = $"✅ Check-out {recordResult.TenCa} thành công: {nhanVien.HoTen} lúc {timeStr}";
                    ShowResult(processText, Color.DarkBlue, Color.LightSkyBlue);
                }
                else // Hành động Check-in
                {
                    timeStr = recordResult.GioCheckIn.Value.ToString(@"hh\:mm");
                    processText = $"✅ Check-in {recordResult.TenCa} thành công: {nhanVien.HoTen} lúc {timeStr}";
                    ShowResult(processText, Color.Green, Color.LightGreen);
                }
                AddHistory(processText);
                _cooldownUntil = DateTime.Now.AddSeconds(5);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Vui lòng Check-out sau"))
                {
                    var request = TaoYeuCauCheckoutSom(recognizedID);
                    if (request != null)
                    {
                        _pendingApprovalRequestId = request.Id;
                        _trangThai = KioskState.ChoDuyetCheckoutSom;
                        ShowResult("🕒 Đã gửi yêu cầu checkout sớm. Đang chờ admin duyệt...", Color.DarkBlue, Color.Khaki);
                        _cooldownUntil = DateTime.Now.AddMinutes(10);
                        return;
                    }

                    ShowResult("❌ Không kết nối được máy duyệt admin. Vui lòng báo quản trị viên.", Color.Red, Color.MistyRose);
                    _cooldownUntil = DateTime.Now.AddSeconds(5);
                    return;
                }

                ShowResult("❌ Lỗi: " + ex.Message, Color.Red, Color.MistyRose);
                _cooldownUntil = DateTime.Now.AddSeconds(4);
            }
        }

        private EarlyCheckoutRequestDto? TaoYeuCauCheckoutSom(string maNV)
        {
            try
            {
                return Task.Run(() => _approvalGateway.CreateRequestAsync(new CreateEarlyCheckoutRequestDto
                {
                    MaNV = maNV,
                    LyDo = "Nhân viên cần checkout sớm tại kiosk",
                    RequestedFromMachine = Environment.MachineName
                })).GetAwaiter().GetResult();
            }
            catch
            {
                return null;
            }
        }

        private void OnApprovalRequestUpdated(EarlyCheckoutRequestDto request)
        {
            if (string.IsNullOrEmpty(_pendingApprovalRequestId) || request.Id != _pendingApprovalRequestId)
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => XuLyKetQuaDuyet(request)));
                return;
            }

            XuLyKetQuaDuyet(request);
        }

        private void XuLyKetQuaDuyet(EarlyCheckoutRequestDto request)
        {
            var nhanVien = _nhanSuManager.TimKiem(request.MaNV);
            var tenNV = nhanVien != null ? nhanVien.HoTen : request.MaNV;

            if (request.Status == EarlyCheckoutRequestStatus.Approved)
            {
                try
                {
                    var record = _chamCongManager.CheckOut(request.MaNV, request.CheckoutTime ?? DateTime.Now);
                    var timeStr = record.GioCheckOut.HasValue ? record.GioCheckOut.Value.ToString(@"hh\:mm") : DateTime.Now.ToString("HH:mm");
                    var log = $"✅ Admin đã duyệt checkout sớm: {tenNV} lúc {timeStr}";
                    ShowResult(log, Color.DarkBlue, Color.LightSkyBlue);
                    AddHistory(log);
                    _ = _approvalGateway.MarkProcessedAsync(request.Id);
                }
                catch (Exception ex)
                {
                    ShowResult("❌ Duyệt thành công nhưng checkout lỗi: " + ex.Message, Color.Red, Color.MistyRose);
                }
            }
            else if (request.Status == EarlyCheckoutRequestStatus.Rejected)
            {
                var note = string.IsNullOrWhiteSpace(request.AdminNote) ? "Không có ghi chú." : request.AdminNote;
                ShowResult("❌ Admin từ chối checkout sớm: " + note, Color.Red, Color.MistyRose);
            }

            _pendingApprovalRequestId = string.Empty;
            _trangThai = KioskState.ChoKhuonMat;
            _cooldownUntil = DateTime.Now.AddSeconds(5);
        }

        private void ShowResult(string message, Color fore, Color back)
        {
            _lblStatus.Text = message;
            _lblStatus.ForeColor = fore;
            _lblStatus.BackColor = back;
        }

        private void BtnDangKy_Click(object? sender, EventArgs e)
        {
            // Tạm dừng check-in để nhập liệu
            _cooldownUntil = DateTime.Now.AddMinutes(5); 

            using (Form prompt = new Form()
            {
                Width = 400, Height = 180, FormBorderStyle = FormBorderStyle.FixedDialog, Text = "Đăng ký FaceID mới", StartPosition = FormStartPosition.CenterParent
            })
            {
                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Vui lòng nhập Mã Nhân Viên của bạn:", Width = 300 };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340 };
                Button confirmation = new Button() { Text = "Xác nhận", Left = 260, Width = 100, Top = 90, DialogResult = DialogResult.OK };
                confirmation.Click += (s, ev) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    string maNV = textBox.Text.Trim();
                    var nhanVien = _nhanSuManager.TimKiem(maNV);
                    if (nhanVien == null)
                    {
                        MessageBox.Show("Mã nhân viên không tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        _cooldownUntil = DateTime.Now; // Khôi phục scan
                        return;
                    }
                    if (!string.IsNullOrEmpty(nhanVien.FaceDataPath))
                    {
                        MessageBox.Show("Nhân viên này ĐÃ CÓ dữ liệu khuôn mặt. Bạn không thể tự cập nhật đè! Vui lòng nhờ Quản trị viên xử lý nếu muốn thay đổi.", "Cảnh báo bảo mật", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        _cooldownUntil = DateTime.Now; // Khôi phục scan
                        return;
                    }

                    // Chuyển sang trạng thái đếm ngược chụp ảnh tự động
                    _idTamThoi = maNV;
                    _trangThai = KioskState.DangKyKhuonMat;
                    _thoiGianDemNguocDangKy = DateTime.Now.AddSeconds(4); // 4s đếm lùi vì 1s đầu là để ngước nhìn
                    Console.Beep(600, 200);
                }
                else
                {
                    _cooldownUntil = DateTime.Now; // Khôi phục scan nếu huỷ bỏ
                }
            }
        }

        private void AddHistory(string log)
        {
            _lstHistory.Items.Insert(0, log);
            if (_lstHistory.Items.Count > 5)
            {
                _lstHistory.Items.RemoveAt(5);
            }
        }

        private void FormNhanVien_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _videoTimer?.Stop();
            _clockTimer?.Stop();
            _approvalGateway.RequestUpdated -= OnApprovalRequestUpdated;
            _faceManager?.TatCamera();
        }
    }
}
