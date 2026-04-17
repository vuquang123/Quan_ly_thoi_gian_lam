using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
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

        private enum KioskState { ChoKhuonMat, ChoChongGiaMao }
        private KioskState _trangThai = KioskState.ChoKhuonMat;
        private string _idTamThoi = null;
        private DateTime _thoiGianHetHanChongGiaMao;

        public FormNhanVien()
        {
            this.Text = "Kiosk Chấm Công Bằng FaceID";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
            this.FormClosing += FormNhanVien_FormClosing;

            _faceManager = new FaceIDManager();
            _nhanSuManager = new NhanSuManager();
            _chamCongManager = new ChamCongManager();

            SetupUI();
            
            this.Load += (s, e) => {
                _faceManager.BatCamera();
                _videoTimer.Start();
                _clockTimer.Start();
                ShowWaitingState();
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
            if (DateTime.Now >= _cooldownUntil && _lblStatus.ForeColor != Color.Gray)
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

                if (_trangThai == KioskState.ChoChongGiaMao)
                {
                    if (DateTime.Now > _thoiGianHetHanChongGiaMao)
                    {
                        // Hết thời gian chống giả mạo
                        _trangThai = KioskState.ChoKhuonMat;
                        ShowResult("❌ Hết thời gian xác thực! Đã huỷ phiên quét.", Color.Red, Color.MistyRose);
                        _cooldownUntil = DateTime.Now.AddSeconds(4);
                        return;
                    }

                    // Chống False Positive cực mạnh: 
                    // Chỉ khi khuôn mặt thẳng BIẾN MẤT (do nghiêng đi) thì mới xét profile
                    if (faces.Length == 0 && _faceManager.PhatHienGocNghieng(frame))
                    {
                        Console.Beep(800, 200); // Kêu bíp báo hiệu thành công
                        ExecuteChamCongLogic(_idTamThoi);
                        _trangThai = KioskState.ChoKhuonMat;
                    }
                    return; // Nếu đang chờ góc lệch thì ngắt luồng quét mặt thẳng tiếp
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
                    // Chuyển sang State Thử thách chờ Liveness
                    _trangThai = KioskState.ChoChongGiaMao;
                    _idTamThoi = recognizedID;
                    _thoiGianHetHanChongGiaMao = DateTime.Now.AddSeconds(10);
                    
                    Console.Beep(500, 100);
                    ShowResult($"⚠️ {nhanVien.HoTen}, xin vặn HƠI NGHIÊNG mặt để xác minh!", Color.DarkOrange, Color.WhiteSmoke);
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
                ShowResult("❌ Lỗi: " + ex.Message, Color.Red, Color.MistyRose);
                _cooldownUntil = DateTime.Now.AddSeconds(4);
            }
        }

        private void ShowResult(string message, Color fore, Color back)
        {
            _lblStatus.Text = message;
            _lblStatus.ForeColor = fore;
            _lblStatus.BackColor = back;
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
            _faceManager?.TatCamera();
        }
    }
}
