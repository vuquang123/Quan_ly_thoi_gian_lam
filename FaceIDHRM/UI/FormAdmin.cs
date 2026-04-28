using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceIDHRM.Integration;
using FaceIDHRM.Core;
using FaceIDHRM.Core.Implementations;
using FaceIDHRM.Managers;
using FaceIDHRM.Models;
using OpenCvSharp.Extensions;

namespace FaceIDHRM.UI
{
    public class FormAdmin : Form
    {
        private FaceIDSystem _faceManager;
        private NhanSuManager _nhanSuManager;
        private ChamCongManager _chamCongManager;
        private readonly IEarlyCheckoutGateway _approvalGateway;
        
        private PictureBox _cameraBox;
        private System.Windows.Forms.Timer _timer;
        // Custom: Giữ ảnh tạm để thêm
        
        // Multi-step enrollment logic
        private int _enrollmentStep = -1; // -1: không phải chế độ đăng ký
        private double[] _avgEncoding = new double[10000];
        private DateTime _thoiGianDemNguocDangKy;
        private double[] _tempFaceEncoding = null;

        // UI Controls cho Tab 1
        private TextBox txtMaNV, txtHoTen, txtLuongCB, txtSoDienThoai, txtSoKhac;
        private ComboBox cbLoaiNV, cbPhongBan;
        private DataGridView dgvNhanVien;
        private Label lblLuong1, lblLuong2;
        private TextBox txtSearch;
        private Label lblStatus;

        // UI Controls cho Tab 2
        private DataGridView dgvChamCong;
        private TextBox txtManualCC, txtSearchChamCong;
        private Label lblManualNVName;
        private DateTimePicker dtpManualTime, dtpFilterFrom, dtpFilterTo;
        private DataGridView dgvPendingApprovals;
        private TextBox txtApprovalNote;
        private Label lblApprovalStatus;

        // UI Controls cho Tab 3
        private DataGridView dgvBaoCao;
        private ComboBox cbFilterBaoCao;
        private Label lblSummary;

        public FormAdmin()
        {
            this.Text = "Hệ thống Quản trị (Admin) - Dashboard";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormClosing += FormAdmin_FormClosing;

            _faceManager = new FaceIDSystem(new OpenCvCamera(), new FaceRecognitionDotNetDetector(), new FaceRecognitionDotNetRecognizer());
            _nhanSuManager = new NhanSuManager();
            _chamCongManager = new ChamCongManager();
            _approvalGateway = new EarlyCheckoutGateway(ServerConfig.ApprovalServerUrl);
            _approvalGateway.RequestUpdated += OnApprovalRequestUpdated;

            SetupUI();

            this.Load += async (s, e) =>
            {
                await _approvalGateway.ConnectAsync();
                await ReloadPendingApprovalsAsync();
            };
        }

        private void SetupUI()
        {
            TabControl tabControl = new TabControl { Dock = DockStyle.Fill, Font = new Font("Arial", 10) };

            TabPage tabNhanSu = new TabPage("Quản Lý Nhân Sự & Face ID");
            TabPage tabChamCong = new TabPage("Quản Lý Chấm Công");
            TabPage tabBaoCao = new TabPage("Báo Cáo & Tính Lương");

            tabControl.TabPages.Add(tabNhanSu);
            tabControl.TabPages.Add(tabChamCong);
            tabControl.TabPages.Add(tabBaoCao);

            this.Controls.Add(tabControl);

            BuildTabNhanSu(tabNhanSu);
            BuildTabChamCong(tabChamCong);
            BuildTabBaoCao(tabBaoCao);

            UpdateNhanVienList();
            UpdateChamCongList();

            _timer = new System.Windows.Forms.Timer { Interval = 30 };
            _timer.Tick += Timer_Tick;
        }

        private void BuildTabNhanSu(TabPage tab)
        {
            // --- Cột trái: Form nhập Info ---
            GroupBox grpInfo = new GroupBox { Text = "Thông tin chi tiết", Location = new Point(10, 10), Size = new Size(350, 600) };
            
            grpInfo.Controls.Add(new Label { Text = "Mã NV (Tự tạo/Nhập):", Location = new Point(10, 30), Size = new Size(150, 20) });
            txtMaNV = new TextBox { Location = new Point(160, 30), Size = new Size(180, 25) };
            grpInfo.Controls.Add(txtMaNV);

            grpInfo.Controls.Add(new Label { Text = "Họ và Tên:", Location = new Point(10, 65), Size = new Size(150, 20) });
            txtHoTen = new TextBox { Location = new Point(160, 65), Size = new Size(180, 25) };
            grpInfo.Controls.Add(txtHoTen);

            grpInfo.Controls.Add(new Label { Text = "Loại NV:", Location = new Point(10, 100), Size = new Size(150, 20) });
            cbLoaiNV = new ComboBox { Location = new Point(160, 100), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbLoaiNV.Items.AddRange(new string[] { "Full-Time", "Part-Time" });
            
            grpInfo.Controls.Add(new Label { Text = "SĐT:", Location = new Point(10, 135), Size = new Size(150, 20) });
            txtSoDienThoai = new TextBox { Location = new Point(160, 135), Size = new Size(180, 25) };
            grpInfo.Controls.Add(txtSoDienThoai);

            grpInfo.Controls.Add(new Label { Text = "Phòng Ban:", Location = new Point(10, 170), Size = new Size(150, 20) });
            cbPhongBan = new ComboBox { Location = new Point(160, 170), Size = new Size(180, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbPhongBan.Items.AddRange(new string[] { "Ban Giám Đốc", "Nhân sự", "Kế toán", "IT", "Kinh doanh" });
            grpInfo.Controls.Add(cbPhongBan);

            lblLuong1 = new Label { Text = "Lương CB:", Location = new Point(10, 205), Size = new Size(150, 20) };
            grpInfo.Controls.Add(lblLuong1);
            txtLuongCB = new TextBox { Location = new Point(160, 205), Size = new Size(180, 25) };
            txtLuongCB.TextChanged += TxtCurrency_TextChanged;
            grpInfo.Controls.Add(txtLuongCB);

            lblLuong2 = new Label { Text = "Phụ Cấp:", Location = new Point(10, 240), Size = new Size(150, 20) };
            grpInfo.Controls.Add(lblLuong2);
            txtSoKhac = new TextBox { Location = new Point(160, 240), Size = new Size(180, 25) };
            txtSoKhac.TextChanged += TxtCurrency_TextChanged;
            grpInfo.Controls.Add(txtSoKhac);

            cbLoaiNV.SelectedIndexChanged += (s, e) => {
                if (cbLoaiNV.SelectedIndex == 0) // Fulltime
                {
                    lblLuong1.Text = "Lương Cơ Bản:";
                    lblLuong2.Text = "Hệ Số / Phụ Cấp:";
                    lblLuong2.Visible = true;
                    txtSoKhac.Visible = true;
                    txtSoKhac.PlaceholderText = "VD: 500000";
                }
                else // Parttime
                {
                    lblLuong1.Text = "Mức Lương/Giờ:";
                    lblLuong2.Visible = false;      // Ẩn hệ số theo chuẩn Polymorphism Model
                    txtSoKhac.Visible = false;
                }
            };
            cbLoaiNV.SelectedIndex = 0;
            grpInfo.Controls.Add(cbLoaiNV);

            // Camera Area
            _cameraBox = new PictureBox { Location = new Point(35, 275), Size = new Size(280, 150), BackColor = Color.Black, SizeMode = PictureBoxSizeMode.Zoom };
            grpInfo.Controls.Add(_cameraBox);

            Button btnToggleCam = new Button { Text = "Bật/Tắt Cam", Location = new Point(45, 435), Size = new Size(100, 30) };
            btnToggleCam.Click += (s, e) => ToggleCamera();
            grpInfo.Controls.Add(btnToggleCam);

            Button btnCapture = new Button { Text = "📷 Chụp FaceID", Location = new Point(155, 435), Size = new Size(150, 30), BackColor = Color.LightSkyBlue, Font=new Font("Arial", 9, FontStyle.Bold) };
            btnCapture.Click += BtnCaptureFace_Click;
            grpInfo.Controls.Add(btnCapture);

            GroupBox grpAction = new GroupBox { Text = "Thao tác dữ liệu", Location = new Point(10, 480), Size = new Size(330, 110) };
            Button btnAdd = new Button { Text = "➕ Thêm Mới", Location = new Point(10, 25), Size = new Size(150, 35), BackColor = Color.LightGreen, Font=new Font("Arial", 9, FontStyle.Bold) };
            btnAdd.Click += BtnAdd_Click;
            grpAction.Controls.Add(btnAdd);

            Button btnUpdate = new Button { Text = "💾 Cập Nhật", Location = new Point(170, 25), Size = new Size(150, 35), BackColor = Color.LightYellow };
            btnUpdate.Click += BtnUpdateInfo_Click;
            grpAction.Controls.Add(btnUpdate);

            Button btnDelete = new Button { Text = "🗑️ Xóa", Location = new Point(10, 65), Size = new Size(310, 35), BackColor = Color.LightCoral };
            btnDelete.Click += BtnDeleteNV_Click;
            grpAction.Controls.Add(btnDelete);
            
            grpInfo.Controls.Add(grpAction);
            tab.Controls.Add(grpInfo);

            // --- Cột phải: Danh sách DataGridView ---
            GroupBox grpList = new GroupBox { Text = "Danh sách Nhân Sự", Location = new Point(380, 10), Size = new Size(700, 600) };
            
            txtSearch = new TextBox { Location = new Point(20, 30), Size = new Size(300, 25), PlaceholderText = "Nhập Tên hoặc ID cần tìm..." };
            txtSearch.TextChanged += (s, e) => SearchNhanVien();
            grpList.Controls.Add(txtSearch);

            Button btnSearch = new Button { Text = "Tìm Kiếm", Location = new Point(330, 28), Size = new Size(100, 30) };
            btnSearch.Click += (s, e) => SearchNhanVien();
            grpList.Controls.Add(btnSearch);

            Button btnImportCSV = new Button { Text = "📥 Nhập CSV", Location = new Point(440, 28), Size = new Size(100, 30), BackColor = Color.LightSkyBlue };
            btnImportCSV.Click += BtnImportCSV_Click;
            grpList.Controls.Add(btnImportCSV);

            Button btnExportCSVNV = new Button { Text = "📤 Xuất CSV", Location = new Point(550, 28), Size = new Size(100, 30), BackColor = Color.LightPink };
            btnExportCSVNV.Click += BtnExportCSVNV_Click;
            grpList.Controls.Add(btnExportCSVNV);

            dgvNhanVien = new DataGridView { Location = new Point(20, 70), Size = new Size(660, 480), AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White };
            dgvNhanVien.Columns.Add("MaNV", "Mã NV");
            dgvNhanVien.Columns.Add("HoTen", "Họ Tên");
            dgvNhanVien.Columns.Add("PhongBan", "Phòng Ban");
            dgvNhanVien.Columns.Add("LoaiNV", "Loại NV");
            dgvNhanVien.Columns.Add("SDT", "SĐT");
            dgvNhanVien.Columns.Add("LuongCB", "Lương CB/Giờ");
            
            dgvNhanVien.Columns["HoTen"].Width = 150;
            dgvNhanVien.Columns["LuongCB"].DefaultCellStyle.FormatProvider = new System.Globalization.CultureInfo("vi-VN");
            dgvNhanVien.Columns["LuongCB"].DefaultCellStyle.Format = "#,##0 VNĐ";
            dgvNhanVien.SelectionChanged += DgvNhanVien_SelectionChanged;
            grpList.Controls.Add(dgvNhanVien);

            lblStatus = new Label { Text = "Sẵn sàng", Location = new Point(20, 560), Size = new Size(600, 30), ForeColor = Color.Blue };
            grpList.Controls.Add(lblStatus);

            tab.Controls.Add(grpList);
        }

        private void BuildTabChamCong(TabPage tab)
        {
            // Toolbar Filter
            Label lblFilter = new Label { Text = "Từ ngày:", Location = new Point(20, 23), Size = new Size(60, 20) };
            tab.Controls.Add(lblFilter);
            
            dtpFilterFrom = new DateTimePicker { Location = new Point(80, 20), Size = new Size(110, 25), Format = DateTimePickerFormat.Short };
            dtpFilterFrom.Value = DateTime.Now.AddDays(-7);
            dtpFilterFrom.ValueChanged += (s, e) => { if (dgvChamCong != null) UpdateChamCongList(); };
            tab.Controls.Add(dtpFilterFrom);

            Label lblFilterTo = new Label { Text = "Đến ngày:", Location = new Point(205, 23), Size = new Size(65, 20) };
            tab.Controls.Add(lblFilterTo);

            dtpFilterTo = new DateTimePicker { Location = new Point(275, 20), Size = new Size(110, 25), Format = DateTimePickerFormat.Short };
            dtpFilterTo.ValueChanged += (s, e) => { if (dgvChamCong != null) UpdateChamCongList(); };
            tab.Controls.Add(dtpFilterTo);

            Label lblSearch = new Label { Text = "Tìm Mã/Tên:", Location = new Point(410, 23), Size = new Size(80, 20) };
            tab.Controls.Add(lblSearch);

            txtSearchChamCong = new TextBox { Location = new Point(495, 20), Size = new Size(140, 25) };
            txtSearchChamCong.TextChanged += (s, e) => { if (dgvChamCong != null) UpdateChamCongList(); };
            tab.Controls.Add(txtSearchChamCong);

            dgvChamCong = new DataGridView { Location = new Point(20, 60), Size = new Size(700, 480), AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White };
            dgvChamCong.Columns.Add("Ngay", "Ngày");
            dgvChamCong.Columns.Add("MaNV", "Mã NV");
            dgvChamCong.Columns.Add("HoTen", "Họ Tên");
            dgvChamCong.Columns.Add("GioVao", "Giờ Vào");
            dgvChamCong.Columns.Add("GioRa", "Giờ Ra");
            dgvChamCong.Columns.Add("TrangThai", "Trạng Thái");
            dgvChamCong.Columns["Ngay"].Width = 90;
            dgvChamCong.Columns["HoTen"].Width = 160;
            tab.Controls.Add(dgvChamCong);

            Button btnExportMatrix = new Button { Text = "Xuất Ma trận Chấm Công (Lưới)", Location = new Point(20, 560), Size = new Size(700, 40), BackColor = Color.LightSeaGreen, ForeColor = Color.White, Font = new Font("Arial", 11, FontStyle.Bold) };
            btnExportMatrix.Click += BtnExportMatrix_Click;
            tab.Controls.Add(btnExportMatrix);

            GroupBox grpManual = new GroupBox { Text = "Chấm Công Thủ Công & Sửa Log", Location = new Point(740, 60), Size = new Size(330, 250) };
            
            grpManual.Controls.Add(new Label { Text = "Mã Nhân Viên:", Location = new Point(20, 40), Size = new Size(100, 20) });
            txtManualCC = new TextBox { Location = new Point(130, 37), Size = new Size(150, 25) };
            txtManualCC.TextChanged += TxtManualCC_TextChanged;
            grpManual.Controls.Add(txtManualCC);

            lblManualNVName = new Label { Text = "Chưa có thông tin", Location = new Point(130, 65), Size = new Size(190, 20), ForeColor = Color.Gray, Font = new Font("Arial", 9, FontStyle.Italic) };
            grpManual.Controls.Add(lblManualNVName);

            grpManual.Controls.Add(new Label { Text = "Thời gian quẹt:", Location = new Point(20, 100), Size = new Size(100, 20) });
            dtpManualTime = new DateTimePicker { Location = new Point(130, 97), Size = new Size(180, 25), Format = DateTimePickerFormat.Custom, CustomFormat = "dd/MM/yyyy HH:mm" };
            grpManual.Controls.Add(dtpManualTime);

            Button btnManualCheckin = new Button { Text = "Ép Check-In", Location = new Point(20, 150), Size = new Size(120, 40), BackColor = Color.LightGreen };
            btnManualCheckin.Click += (s, e) => ManualCheckIn();
            grpManual.Controls.Add(btnManualCheckin);

            Button btnManualCheckout = new Button { Text = "Ép Check-Out", Location = new Point(160, 150), Size = new Size(120, 40), BackColor = Color.LightCoral };
            btnManualCheckout.Click += (s, e) => ManualCheckOut();
            grpManual.Controls.Add(btnManualCheckout);

            tab.Controls.Add(grpManual);

            GroupBox grpApproval = new GroupBox { Text = "Duyệt Checkout Sớm (Realtime)", Location = new Point(740, 325), Size = new Size(330, 275) };

            dgvPendingApprovals = new DataGridView
            {
                Location = new Point(12, 25),
                Size = new Size(305, 140),
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White
            };
            dgvPendingApprovals.Columns.Add("RequestId", "RequestId");
            dgvPendingApprovals.Columns.Add("MaNV", "Mã NV");
            dgvPendingApprovals.Columns.Add("RequestedAt", "Yêu cầu lúc");
            dgvPendingApprovals.Columns.Add("LyDo", "Lý do");
            dgvPendingApprovals.Columns["RequestId"].Visible = false;
            dgvPendingApprovals.Columns["LyDo"].Width = 130;
            grpApproval.Controls.Add(dgvPendingApprovals);

            txtApprovalNote = new TextBox { Location = new Point(12, 173), Size = new Size(305, 25), PlaceholderText = "Ghi chú duyệt/từ chối..." };
            grpApproval.Controls.Add(txtApprovalNote);

            Button btnApprove = new Button { Text = "Duyệt", Location = new Point(12, 206), Size = new Size(145, 35), BackColor = Color.LightGreen };
            btnApprove.Click += async (s, e) => await ApproveSelectedRequestAsync();
            grpApproval.Controls.Add(btnApprove);

            Button btnReject = new Button { Text = "Từ Chối", Location = new Point(172, 206), Size = new Size(145, 35), BackColor = Color.LightCoral };
            btnReject.Click += async (s, e) => await RejectSelectedRequestAsync();
            grpApproval.Controls.Add(btnReject);

            lblApprovalStatus = new Label { Text = "Sẵn sàng nhận yêu cầu.", Location = new Point(12, 247), Size = new Size(305, 20), ForeColor = Color.DarkBlue };
            grpApproval.Controls.Add(lblApprovalStatus);

            tab.Controls.Add(grpApproval);
        }

        private void BuildTabBaoCao(TabPage tab)
        {
            Label lblFilter = new Label { Text = "Kỳ tính lương:", Location = new Point(20, 25), Size = new Size(110, 20), Font = new Font("Arial", 10, FontStyle.Bold) };
            tab.Controls.Add(lblFilter);

            cbFilterBaoCao = new ComboBox { Location = new Point(135, 22), Size = new Size(130, 25), DropDownStyle = ComboBoxStyle.DropDownList };
            cbFilterBaoCao.Items.Add("Cả Năm Nay");
            for(int i = 1; i <= 12; i++) cbFilterBaoCao.Items.Add("Tháng " + i);
            cbFilterBaoCao.SelectedIndex = DateTime.Now.Month;
            tab.Controls.Add(cbFilterBaoCao);

            Button btnTinhLuong = new Button { Text = "⚡ Tính Lương (OOP Đa Hình)", Location = new Point(280, 20), Size = new Size(300, 32), Font = new Font("Arial", 11, FontStyle.Bold), BackColor = Color.Gold };
            btnTinhLuong.Click += BtnTinhLuong_Click;
            tab.Controls.Add(btnTinhLuong);

            Button btnExportBaoCao = new Button { Text = "Xuất CSV Lương", Location = new Point(600, 20), Size = new Size(160, 32), Font = new Font("Arial", 11, FontStyle.Bold), BackColor = Color.LightGreen };
            btnExportBaoCao.Click += BtnExportBaoCao_Click;
            tab.Controls.Add(btnExportBaoCao);

            lblSummary = new Label { Text = "Tổng quan: Chưa có dữ liệu", Location = new Point(20, 70), Size = new Size(1000, 25), Font = new Font("Arial", 11, FontStyle.Italic), ForeColor = Color.DarkBlue };
            tab.Controls.Add(lblSummary);

            dgvBaoCao = new DataGridView { Location = new Point(20, 100), Size = new Size(1040, 500), AllowUserToAddRows = false, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, BackgroundColor = Color.White };
            dgvBaoCao.Columns.Add("MaNV", "Mã NV");
            dgvBaoCao.Columns.Add("HoTen", "Họ Tên");
            dgvBaoCao.Columns.Add("LoaiNV", "Loại NV");
            dgvBaoCao.Columns.Add("ThoiGianLam", "Số Ngày/Giờ Làm");
            dgvBaoCao.Columns.Add("DiTre", "Đi Trễ");
            dgvBaoCao.Columns.Add("LuongCB", "Lương CB/Giờ");
            dgvBaoCao.Columns.Add("PhuCap", "Phụ Cấp");
            dgvBaoCao.Columns.Add("TongLuong", "Tổng Lương");
            
            dgvBaoCao.Columns["HoTen"].Width = 180;
            dgvBaoCao.Columns["LuongCB"].DefaultCellStyle.FormatProvider = new System.Globalization.CultureInfo("vi-VN");
            dgvBaoCao.Columns["LuongCB"].DefaultCellStyle.Format = "#,##0 VNĐ";
            dgvBaoCao.Columns["PhuCap"].DefaultCellStyle.FormatProvider = new System.Globalization.CultureInfo("vi-VN");
            dgvBaoCao.Columns["PhuCap"].DefaultCellStyle.Format = "#,##0 VNĐ";
            dgvBaoCao.Columns["TongLuong"].DefaultCellStyle.FormatProvider = new System.Globalization.CultureInfo("vi-VN");
            dgvBaoCao.Columns["TongLuong"].DefaultCellStyle.Format = "#,##0 VNĐ";
            
            tab.Controls.Add(dgvBaoCao);
        }

        // --- Logic Events ---

        private double ParseCurrency(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            string digits = new string(text.Where(char.IsDigit).ToArray());
            return string.IsNullOrEmpty(digits) ? 0 : double.Parse(digits);
        }

        private void TxtCurrency_TextChanged(object? sender, EventArgs e)
        {
            TextBox? txt = sender as TextBox;
            if (txt == null) return;
            
            string raw = new string(txt.Text.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(raw)) 
            {
                txt.TextChanged -= TxtCurrency_TextChanged;
                txt.Text = "";
                txt.TextChanged += TxtCurrency_TextChanged;
                return;
            }

            if (double.TryParse(raw, out double value))
            {
                txt.TextChanged -= TxtCurrency_TextChanged;
                string fm = value.ToString("#,##0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
                txt.Text = fm + " VNĐ";
                txt.SelectionStart = Math.Max(0, txt.Text.Length - 4);
                txt.TextChanged += TxtCurrency_TextChanged;
            }
        }

        private void ToggleCamera()
        {
            if (_timer.Enabled)
            {
                _timer.Stop();
                _faceManager.TatCamera();
                _cameraBox.Image?.Dispose();
                _cameraBox.Image = null;
                lblStatus.Text = "Camera tắt.";
                lblStatus.ForeColor = Color.Orange;
            }
            else
            {
                _faceManager.BatCamera();
                _timer.Start();
                lblStatus.Text = "Camera hoạt động.";
                lblStatus.ForeColor = Color.Green;
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            using var frame = _faceManager.GetFrame();
            if (frame != null && !frame.Empty())
            {
                var faces = _faceManager.PhatHienKhuonMat(frame);
                foreach (var rect in faces)
                {
                    OpenCvSharp.Cv2.Rectangle(frame, rect, OpenCvSharp.Scalar.Red, 2);
                }

                if (_enrollmentStep >= 0 && _enrollmentStep < 3)
                {
                    double timeLeft = (_thoiGianDemNguocDangKy - DateTime.Now).TotalSeconds;
                    string stepMsg = _enrollmentStep == 0 ? "Nhin Thang" : (_enrollmentStep == 1 ? "Nghieng TRAI" : "Nghieng PHAI");
                    
                    OpenCvSharp.Cv2.PutText(frame, $"Buoc {_enrollmentStep + 1}/3: {stepMsg}", new OpenCvSharp.Point(10, 40), OpenCvSharp.HersheyFonts.HersheySimplex, 1.0, OpenCvSharp.Scalar.Yellow, 2);
                    OpenCvSharp.Cv2.PutText(frame, $"Tien trinh: {_enrollmentStep * 33}%", new OpenCvSharp.Point(10, 80), OpenCvSharp.HersheyFonts.HersheySimplex, 1.0, OpenCvSharp.Scalar.LimeGreen, 2);

                    if (timeLeft > 0)
                    {
                        lblStatus.Text = $"📷 Vui lòng: {stepMsg}! Chụp sau {Math.Ceiling(timeLeft)}s...";
                        lblStatus.ForeColor = Color.DarkBlue;
                    }
                    else
                    {
                        if (faces.Length == 1)
                        {
                            using var croppedFace = new OpenCvSharp.Mat(frame, faces[0]);
                            try
                            {
                                // Kiem tra gian lan ngay lap tuc
                                string duplicatedID = _faceManager.Verification(croppedFace, _nhanSuManager.LayDanhSach());
                                string targetMaNV = txtMaNV.Text;
                                if (duplicatedID != null && duplicatedID != targetMaNV && !string.IsNullOrEmpty(targetMaNV))
                                {
                                    MessageBox.Show($"Khuôn mặt này đã được gắn cho nhân viên [{duplicatedID}]. Bạn không thể dùng mặt người khác!", "Cảnh báo Bảo Mật", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    _enrollmentStep = -1; // Huy
                                }
                                else
                                {
                                    double[] vec = _faceManager.GetEncoding(croppedFace);
                                    for (int i = 0; i < 10000; i++) _avgEncoding[i] += vec[i] / 3.0;

                                    _enrollmentStep++;
                                    if (_enrollmentStep < 3)
                                    {
                                        Console.Beep(800, 200);
                                        _thoiGianDemNguocDangKy = DateTime.Now.AddSeconds(3);
                                    }
                                    else
                                    {
                                        Console.Beep(1000, 500);
                                        _tempFaceEncoding = _avgEncoding;
                                        lblStatus.Text = "Đã lấy FaceID 3 góc thành công. Nhấn Cập Nhật hoặc Thêm Mới để Lưu.";
                                        lblStatus.ForeColor = Color.Orange;
                                        MessageBox.Show("Hoàn tất thu thập FaceID 3 góc!");
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                lblStatus.Text = "Lỗi khi lấy ảnh: " + ex.Message;
                                _thoiGianDemNguocDangKy = DateTime.Now.AddSeconds(3);
                            }
                        }
                        else
                        {
                            lblStatus.Text = "❌ Không phát hiện khuôn mặt rõ diện. Thử lại sau 3s...";
                            _thoiGianDemNguocDangKy = DateTime.Now.AddSeconds(3);
                        }
                    }
                }

                var oldImg = _cameraBox.Image;
                _cameraBox.Image = BitmapConverter.ToBitmap(frame);
                oldImg?.Dispose();
            }
        }

        private void BtnCaptureFace_Click(object? sender, EventArgs e)
        {
            if (!_timer.Enabled) { MessageBox.Show("Phải BẬT Camera để tạo mã nhận diện khuôn mặt mới!"); return; }
            
            _enrollmentStep = 0;
            _avgEncoding = new double[10000];
            _thoiGianDemNguocDangKy = DateTime.Now.AddSeconds(4); // Start 4s countdown
            lblStatus.Text = "Bắt đầu đăng ký khuôn mặt 3 góc độ...";
            lblStatus.ForeColor = Color.Blue;
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtHoTen.Text)) { MessageBox.Show("Họ tên trống!"); return; }
            string targetMaNV = string.IsNullOrEmpty(txtMaNV.Text) ? "NV" + (_nhanSuManager.LayDanhSach().Count + 1).ToString("D2") : txtMaNV.Text;
            if (_nhanSuManager.TimKiem(targetMaNV) != null) 
            { 
                MessageBox.Show("Mã NV đã tồn tại! Vui lòng dùng nút Cập Nhật."); 
                return; 
            }

            double luong = ParseCurrency(txtLuongCB.Text);
            double sK = txtSoKhac.Visible ? ParseCurrency(txtSoKhac.Text) : 0;

            NhanVien nvMoi;
            if (cbLoaiNV.SelectedIndex == 0) // Fulltime
            {
                nvMoi = new NhanVienFullTime(targetMaNV, txtHoTen.Text, new DateTime(2000, 1, 1), "0001", txtSoDienThoai.Text, "email", "Nhân sự", luong, 1.0, sK);
            }
            else // Parttime
            {
                nvMoi = new NhanVienPartTime(targetMaNV, txtHoTen.Text, new DateTime(2000, 1, 1), "0001", txtSoDienThoai.Text, "email", "Nhân sự", 0, luong, 300); // hardcode max 300h theo logic
            }
            nvMoi.PhongBan = cbPhongBan.SelectedItem?.ToString() ?? "Chưa phân bổ";
            // Nếu có FaceID thì gán, không có thì thôi, không bắt buộc
            if (_tempFaceEncoding != null)
            {
                nvMoi.FaceEncoding = _tempFaceEncoding;
                _tempFaceEncoding = null;
            }
            try
            {
                _nhanSuManager.Them(nvMoi);
                lblStatus.Text = $"Đã tạo tài khoản {targetMaNV} thành công!";
                lblStatus.ForeColor = Color.Green;
                UpdateNhanVienList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnUpdateInfo_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtMaNV.Text)) return;
            var target = _nhanSuManager.TimKiem(txtMaNV.Text);
            if (target != null)
            {
                target.HoTen = txtHoTen.Text;
                target.SoDienThoai = txtSoDienThoai.Text;
                target.PhongBan = cbPhongBan.SelectedItem?.ToString() ?? "Chưa phân bổ";
                double luong = ParseCurrency(txtLuongCB.Text);
                
                if (target is NhanVienFullTime f)
                {
                    f.LuongCoBan = luong;
                    f.TienPhuCap = ParseCurrency(txtSoKhac.Text);
                }
                else if (target is NhanVienPartTime p)
                {
                    p.MucLuongTheoGio = luong;
                }

                if (_tempFaceEncoding != null)
                {
                    target.FaceEncoding = _tempFaceEncoding;
                    _tempFaceEncoding = null;
                }

                _nhanSuManager.Sua(target);
                lblStatus.Text = $"Cập nhật thành công ID {txtMaNV.Text}!";
                UpdateNhanVienList();
            }
        }

        private void BtnDeleteNV_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtMaNV.Text))
            {
                var dialog = MessageBox.Show($"Bạn có chắc muốn đuổi việc NV {txtMaNV.Text}?", "Warning", MessageBoxButtons.YesNo);
                if (dialog == DialogResult.Yes)
                {
                    _nhanSuManager.Xoa(txtMaNV.Text);
                    UpdateNhanVienList();
                    lblStatus.Text = "Đã Xóa KHỏi Hệ Thống.";
                }
            }
        }

        private void DgvNhanVien_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvNhanVien.SelectedRows.Count == 0) return;
            string id = dgvNhanVien.SelectedRows[0].Cells["MaNV"].Value?.ToString() ?? "";
            var target = _nhanSuManager.TimKiem(id);
            if (target != null)
            {
                txtMaNV.Text = target.MaNV;
                txtHoTen.Text = target.HoTen;
                txtSoDienThoai.Text = target.SoDienThoai;
                if (!string.IsNullOrEmpty(target.PhongBan) && cbPhongBan.Items.Contains(target.PhongBan))
                    cbPhongBan.SelectedItem = target.PhongBan;
                else
                    cbPhongBan.SelectedIndex = -1;
                
                if (target is NhanVienFullTime f)
                {
                    cbLoaiNV.SelectedIndex = 0;
                    txtLuongCB.Text = f.LuongCoBan.ToString();
                    txtSoKhac.Text = f.TienPhuCap.ToString();
                }
                else if (target is NhanVienPartTime p)
                {
                    cbLoaiNV.SelectedIndex = 1;
                    txtLuongCB.Text = p.MucLuongTheoGio.ToString();
                    // txtSoKhac is hidden, so no need to populate
                }
            }
        }

        private void SearchNhanVien()
        {
            UpdateNhanVienList();
        }

        private void TxtManualCC_TextChanged(object? sender, EventArgs e)
        {
            var target = _nhanSuManager.TimKiem(txtManualCC.Text);
            if (target != null)
            {
                lblManualNVName.Text = $"{target.HoTen} ({target.ChucVu})";
                lblManualNVName.ForeColor = Color.Green;
            }
            else
            {
                lblManualNVName.Text = "Mã NV không tồn tại";
                lblManualNVName.ForeColor = Color.Red;
            }
        }

        private void ManualCheckIn()
        {
            try
            {
                var n = _chamCongManager.CheckIn(txtManualCC.Text, dtpManualTime.Value);
                UpdateChamCongList();
                MessageBox.Show($"Check-in thủ công cho {txtManualCC.Text} vào lúc {dtpManualTime.Value:dd/MM HH:mm} thành công!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void ManualCheckOut()
        {
             try
            {
                var n = _chamCongManager.CheckOut(txtManualCC.Text, dtpManualTime.Value);
                UpdateChamCongList();
                MessageBox.Show($"Check-out thủ công cho {txtManualCC.Text} vào lúc {dtpManualTime.Value:dd/MM HH:mm} thành công!");
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private async Task ReloadPendingApprovalsAsync()
        {
            try
            {
                var pending = await _approvalGateway.GetPendingAsync();
                dgvPendingApprovals.Rows.Clear();

                foreach (var request in pending)
                {
                    dgvPendingApprovals.Rows.Add(
                        request.Id,
                        request.MaNV,
                        request.RequestedAt.ToString("dd/MM HH:mm"),
                        request.LyDo);
                }

                lblApprovalStatus.Text = $"Đang chờ duyệt: {pending.Count} yêu cầu.";
            }
            catch (Exception ex)
            {
                lblApprovalStatus.Text = "Không tải được yêu cầu: " + ex.Message;
                lblApprovalStatus.ForeColor = Color.Red;
            }
        }

        private async Task ApproveSelectedRequestAsync()
        {
            if (dgvPendingApprovals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một yêu cầu cần duyệt.");
                return;
            }

            var requestId = dgvPendingApprovals.SelectedRows[0].Cells["RequestId"].Value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(requestId))
            {
                return;
            }

            var response = await _approvalGateway.ApproveAsync(requestId, new ResolveEarlyCheckoutRequestDto
            {
                AdminName = "Admin",
                AdminNote = txtApprovalNote.Text,
                CheckoutTime = DateTime.Now
            });

            if (response == null)
            {
                MessageBox.Show("Duyệt thất bại. Vui lòng thử lại.");
                return;
            }

            lblApprovalStatus.Text = $"Đã duyệt yêu cầu của {response.MaNV}.";
            lblApprovalStatus.ForeColor = Color.Green;
            txtApprovalNote.Text = string.Empty;
            await ReloadPendingApprovalsAsync();
        }

        private async Task RejectSelectedRequestAsync()
        {
            if (dgvPendingApprovals.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một yêu cầu cần từ chối.");
                return;
            }

            var requestId = dgvPendingApprovals.SelectedRows[0].Cells["RequestId"].Value?.ToString() ?? string.Empty;
            if (string.IsNullOrEmpty(requestId))
            {
                return;
            }

            var response = await _approvalGateway.RejectAsync(requestId, new ResolveEarlyCheckoutRequestDto
            {
                AdminName = "Admin",
                AdminNote = string.IsNullOrWhiteSpace(txtApprovalNote.Text) ? "Không đủ điều kiện checkout sớm." : txtApprovalNote.Text
            });

            if (response == null)
            {
                MessageBox.Show("Từ chối thất bại. Vui lòng thử lại.");
                return;
            }

            lblApprovalStatus.Text = $"Đã từ chối yêu cầu của {response.MaNV}.";
            lblApprovalStatus.ForeColor = Color.OrangeRed;
            txtApprovalNote.Text = string.Empty;
            await ReloadPendingApprovalsAsync();
        }

        private void OnApprovalRequestUpdated(EarlyCheckoutRequestDto request)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(async () => await ReloadPendingApprovalsAsync()));
                return;
            }

            _ = ReloadPendingApprovalsAsync();
        }

        private void BtnExportMatrix_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = "BangChamCong_Matrix.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            int year = DateTime.Now.Year;
                            int month = DateTime.Now.Month;
                            int daysInMonth = DateTime.DaysInMonth(year, month);

                            // Build Header
                            var headers = new System.Collections.Generic.List<string> { "MaNV", "HoTen" };
                            for (int i = 1; i <= daysInMonth; i++)
                            {
                                headers.Add($"{i:D2}/{month:D2}");
                            }
                            headers.Add("TongNgayLam");
                            sw.WriteLine(string.Join(",", headers));

                            // Build Data Rows
                            var danhSachNV = _nhanSuManager.LayDanhSach();
                            var danhSachCC = _chamCongManager.LayDanhSach();

                            foreach (var nv in danhSachNV)
                            {
                                var row = new System.Collections.Generic.List<string> { nv.MaNV, nv.HoTen };
                                int tongNgayLam = 0;

                                for (int i = 1; i <= daysInMonth; i++)
                                {
                                    var log = danhSachCC.FirstOrDefault(c => c.MaNV == nv.MaNV && c.NgayChamCong.Year == year && c.NgayChamCong.Month == month && c.NgayChamCong.Day == i);
                                    if (log != null)
                                    {
                                        string gioThucTe = "";
                                        if (log.GioCheckIn.HasValue && log.GioCheckOut.HasValue)
                                        {
                                            gioThucTe = $"{log.GioCheckIn.Value.ToString(@"hh\:mm")}-{log.GioCheckOut.Value.ToString(@"hh\:mm")}";
                                        }
                                        else if (log.GioCheckIn.HasValue && !log.GioCheckOut.HasValue)
                                        {
                                            gioThucTe = $"{log.GioCheckIn.Value.ToString(@"hh\:mm")}-Không QMT";
                                        }

                                        if (log.TrangThai == "Đi trễ") gioThucTe += " (Tre)";
                                        else if (log.TrangThai == "Về sớm") gioThucTe += " (Som)";
                                        
                                        row.Add(gioThucTe);
                                        tongNgayLam++;
                                    }
                                    else
                                    {
                                        row.Add("Vang");
                                    }
                                }
                                row.Add(tongNgayLam.ToString());
                                sw.WriteLine(string.Join(",", row));
                            }
                        }
                        MessageBox.Show($"Đã xuất Bảng ma trận chấm công tháng {DateTime.Now.Month} thành công!");
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            }
        }

        private void BtnTinhLuong_Click(object? sender, EventArgs e)
        {
            dgvBaoCao.Rows.Clear(); // Xóa dữ liệu cũ
            
            int selectedIndex = cbFilterBaoCao.SelectedIndex;
            int selMonth = selectedIndex == 0 ? 0 : selectedIndex;
            int selYear = DateTime.Now.Year;

            Task.Run(() => _chamCongManager.LamMoiDuLieu()).GetAwaiter().GetResult();

            var dsCC = _chamCongManager.LayDanhSach();
            var dsNV = _nhanSuManager.LayDanhSach();

            double tongQuyLuong = 0;
            int tongNVFullTime = 0;
            int tongNVPartTime = 0;

            foreach (var nv in dsNV)
            {
                var myCC = dsCC.Where(c => c.MaNV == nv.MaNV && c.NgayChamCong.Year == selYear);
                if (selMonth > 0) myCC = myCC.Where(c => c.NgayChamCong.Month == selMonth);

                int soNgayLam = myCC.Count(c => c.GioCheckIn.HasValue && c.GioCheckOut.HasValue);
                int soLanTre = myCC.Count(c => c.TrangThai == "Đi trễ" || c.TrangThai == "Về sớm");
                double soGioLam = myCC.Sum(c => c.TinhTongSoGioLam());

                double luongCoBan_MucGio = 0;
                double phuCap = 0;
                string loai = "";
                string fThoiGianLam = "";

                if (nv is NhanVienFullTime f)
                {
                    luongCoBan_MucGio = f.LuongCoBan;
                    phuCap = f.TienPhuCap;
                    loai = "Full-time";
                    fThoiGianLam = $"{soNgayLam} ngày";
                    tongNVFullTime++;
                }
                else if (nv is NhanVienPartTime p)
                {
                    luongCoBan_MucGio = p.MucLuongTheoGio;
                    phuCap = 0; 
                    p.SoGioDaLamTrongThang = (int)Math.Round(soGioLam); 
                    loai = "Part-time";
                    fThoiGianLam = $"{Math.Round(soGioLam, 1)} giờ";
                    tongNVPartTime++;
                }

                double tongLuong = nv.TinhLuong(); // Phương thức chạy Đa Hình chuẩn xác
                tongQuyLuong += tongLuong;

                // Insert row
                dgvBaoCao.Rows.Add(nv.MaNV, nv.HoTen, loai, fThoiGianLam, soLanTre, luongCoBan_MucGio, phuCap, tongLuong);
            }

            string fmtQuyLuong = tongQuyLuong.ToString("#,##0", System.Globalization.CultureInfo.InvariantCulture).Replace(",", ".");
            lblSummary.Text = $"Tổng quan: {dsNV.Count} nhân viên ({tongNVFullTime} Full-time, {tongNVPartTime} Part-time)  |  Tổng quỹ lương tháng {selMonth}: {fmtQuyLuong} VNĐ";
        }

        private void UpdateNhanVienList()
        {
            if (dgvNhanVien == null) return;
            dgvNhanVien.Rows.Clear();
            string keyword = txtSearch?.Text.ToLower() ?? "";

            foreach (var nv in _nhanSuManager.LayDanhSach())
            {
                if (!string.IsNullOrEmpty(keyword) && !nv.MaNV.ToLower().Contains(keyword) && !nv.HoTen.ToLower().Contains(keyword))
                    continue;

                string loai = nv is NhanVienFullTime ? "Full-Time" : "Part-Time";
                double luongTieuBieu = nv is NhanVienFullTime f ? f.LuongCoBan : ((NhanVienPartTime)nv).MucLuongTheoGio;
                string phongBan = nv.PhongBan ?? "Chưa phân bổ";
                
                dgvNhanVien.Rows.Add(nv.MaNV, nv.HoTen, phongBan, loai, nv.SoDienThoai, luongTieuBieu);
            }
        }

        private void UpdateChamCongList()
        {
            if (dgvChamCong == null) return;
            dgvChamCong.Rows.Clear();
            var keyword = txtSearchChamCong?.Text.ToLower() ?? "";
            var from = dtpFilterFrom?.Value.Date ?? DateTime.MinValue;
            var to = dtpFilterTo?.Value.Date.AddDays(1).AddSeconds(-1) ?? DateTime.MaxValue;

            // Kéo dữ liệu mới nhất từ Server về trước khi hiển thị
            Task.Run(() => _chamCongManager.LamMoiDuLieu()).GetAwaiter().GetResult();
            
            var dsNV = _nhanSuManager.LayDanhSach();

            var logs = _chamCongManager.LayDanhSach()
                .Where(c => c.NgayChamCong >= from && c.NgayChamCong <= to)
                .OrderByDescending(c => c.NgayChamCong);

            foreach (var cc in logs)
            {
                var nv = dsNV.FirstOrDefault(x => x.MaNV == cc.MaNV);
                string hoTen = nv != null ? nv.HoTen : "Không xác định";
                
                if (!string.IsNullOrEmpty(keyword))
                {
                    if (!cc.MaNV.ToLower().Contains(keyword) && !hoTen.ToLower().Contains(keyword))
                        continue;
                }

                string gioVao = cc.GioCheckIn.HasValue ? cc.GioCheckIn.Value.ToString(@"hh\:mm") : "--:--";
                string gioRa = cc.GioCheckOut.HasValue ? cc.GioCheckOut.Value.ToString(@"hh\:mm") : "--:--";
                string trangThaiCC = string.IsNullOrEmpty(cc.TenCa) ? cc.TrangThai : $"[{cc.TenCa}] {cc.TrangThai}";
                dgvChamCong.Rows.Add(cc.NgayChamCong.ToString("dd/MM/yyyy"), cc.MaNV, hoTen, gioVao, gioRa, trangThaiCC);
            }
        }

        private void BtnExportBaoCao_Click(object? sender, EventArgs e)
        {
            int selectedIndex = cbFilterBaoCao.SelectedIndex;
            int selMonth = selectedIndex == 0 ? 0 : selectedIndex;
            int selYear = DateTime.Now.Year;
            string fileName = selMonth == 0 ? $"BaoCaoLuong_Nam{selYear}.csv" : $"BaoCao_TinhLuong_Thang{selMonth}.csv";

            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = fileName })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("MaNV,HoTen,LoaiNV,SoNgayLam,SoGioLam,SoLanTre,LuongCoBan_MucGio,PhuCap,TongLuong");
                            var dsCC = _chamCongManager.LayDanhSach();

                            foreach (var nv in _nhanSuManager.LayDanhSach())
                            {
                                var myCC = dsCC.Where(c => c.MaNV == nv.MaNV && c.NgayChamCong.Year == selYear);
                                if (selMonth > 0) myCC = myCC.Where(c => c.NgayChamCong.Month == selMonth);

                                int soNgayLam = myCC.Count(c => c.GioCheckIn.HasValue && c.GioCheckOut.HasValue);
                                int soLanTre = myCC.Count(c => c.TrangThai == "Đi trễ" || c.TrangThai == "Về sớm");
                                double soGioLam = myCC.Sum(c => c.TinhTongSoGioLam());

                                double luongCoBan_MucGio = 0;
                                double phuCap = 0;

                                if (nv is NhanVienFullTime f)
                                {
                                    luongCoBan_MucGio = f.LuongCoBan;
                                    phuCap = f.TienPhuCap;
                                }
                                else if (nv is NhanVienPartTime p)
                                {
                                    luongCoBan_MucGio = p.MucLuongTheoGio;
                                    phuCap = 0;
                                    p.SoGioDaLamTrongThang = (int)Math.Round(soGioLam); // Cập nhật để TinhLuong() chạy đúng Đa Hình
                                }

                                double tongLuong = nv.TinhLuong();
                                
                                string loai = nv is NhanVienFullTime ? "Full-time" : "Part-time";
                                string strNgay = nv is NhanVienFullTime ? soNgayLam.ToString() : "";
                                string strGio = nv is NhanVienPartTime ? Math.Round(soGioLam, 1).ToString() : "";

                                sw.WriteLine($"{nv.MaNV},{nv.HoTen},{loai},{strNgay},{strGio},{soLanTre},{luongCoBan_MucGio},{phuCap},{tongLuong}");
                            }
                        }
                        MessageBox.Show("Xuất báo cáo theo " + (selMonth == 0 ? "năm" : $"tháng {selMonth}") + " thành công!");
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            }
        }

        private void BtnExportCSVNV_Click(object? sender, EventArgs e)
        {
            using (SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV Files|*.csv", FileName = "DanhSachNhanVien.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var sw = new System.IO.StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                        {
                            sw.WriteLine("MaNV,HoTen,LoaiNV,LuongCBMucGio,HeSoPhuCapSoGio");
                            foreach (var nv in _nhanSuManager.LayDanhSach())
                            {
                                if (nv is NhanVienFullTime f)
                                    sw.WriteLine($"{nv.MaNV},{nv.HoTen},Full,{f.LuongCoBan},{f.TienPhuCap}");
                                else if (nv is NhanVienPartTime p)
                                    sw.WriteLine($"{nv.MaNV},{nv.HoTen},Part,{p.MucLuongTheoGio},{p.SoGioLamToiDa}");
                            }
                        }
                        MessageBox.Show("Xuất danh sách Nhân viên thành công!");
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
                }
            }
        }

        private void BtnImportCSV_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog { Filter = "CSV Files|*.csv" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        List<string> linesList = new List<string>();
                        using (var fs = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
                        using (var sr = new System.IO.StreamReader(fs, System.Text.Encoding.UTF8))
                        {
                            string? line;
                            while ((line = sr.ReadLine()) != null)
                            {
                                linesList.Add(line);
                            }
                        }
                        string[] lines = linesList.ToArray();
                        int count = 0;
                        for (int i = 1; i < lines.Length; i++)
                        {
                            string[] cols = lines[i].Split(',');
                            if (cols.Length >= 5)
                            {
                                string maNV = cols[0].Trim();
                                if (_nhanSuManager.TimKiem(maNV) != null) continue;
                                
                                string hoTen = cols[1].Trim();
                                string loai = cols[2].Trim();
                                double luongCB = double.Parse(cols[3].Trim());
                                double heSo = double.Parse(cols[4].Trim());

                                NhanVien nv;
                                if (loai.ToLower() == "full")
                                    nv = new NhanVienFullTime(maNV, hoTen, DateTime.Now, "001", "090", "a@a.a", "Nhân sự", luongCB, 1.0, heSo);
                                else
                                    nv = new NhanVienPartTime(maNV, hoTen, DateTime.Now, "001", "090", "a@a.a", "Nhân sự", 0, luongCB, (int)heSo);

                                _nhanSuManager.Them(nv);
                                count++;
                            }
                        }
                        MessageBox.Show($"Nhập thành công {count} nhân viên mới từ CSV!");
                        UpdateNhanVienList();
                    }
                    catch (Exception ex) { MessageBox.Show("Lỗi định dạng file: " + ex.Message); }
                }
            }
        }

        private void FormAdmin_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _timer?.Stop();
            _approvalGateway.RequestUpdated -= OnApprovalRequestUpdated;
            _faceManager?.TatCamera();
        }
    }
}
