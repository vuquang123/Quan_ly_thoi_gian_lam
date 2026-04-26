using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FaceIDHRM.UI
{
    public class FormSetupDevice : Form
    {
        public FormSetupDevice()
        {
            this.Text = "Hệ thống FaceID HRM - Cấu hình thiết bị lần đầu";
            this.Size = new Size(500, 350);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            SetupUI();
        }

        private void SetupUI()
        {
            Label lblTitle = new Label
            {
                Text = "CÀI ĐẶT VAI TRÒ THIẾT BỊ",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(50, 30),
                Size = new Size(400, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            Label lblSubTitle = new Label
            {
                Text = "Vui lòng chọn chức năng cho máy tính này. Thiết lập sẽ được lưu lại cho các lần mở sau.",
                Font = new Font("Arial", 10),
                Location = new Point(50, 65),
                Size = new Size(400, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            this.Controls.Add(lblSubTitle);

            // Nút Nhân Viên (Kiosk)
            Button btnNhanVien = new Button
            {
                Text = "Máy Chấm Công (Kiosk)",
                Location = new Point(100, 120),
                Size = new Size(300, 50),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnNhanVien.Click += (s, e) => SaveModeAndRestart("KIOSK");
            this.Controls.Add(btnNhanVien);

            // Nút Admin
            Button btnAdmin = new Button
            {
                Text = "Máy Quản Trị (Admin)",
                Location = new Point(100, 190),
                Size = new Size(300, 50),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnAdmin.Click += (s, e) => SaveModeAndRestart("ADMIN");
            this.Controls.Add(btnAdmin);
        }

        private void SaveModeAndRestart(string mode)
        {
            try
            {
                File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "device_mode.txt"), mode);
                MessageBox.Show("Đã lưu cấu hình thiết bị thành công! Ứng dụng sẽ tự động khởi động lại vào luồng tương ứng.\n\n(Để cài đặt lại sau này, hãy xóa file device_mode.txt trong thư mục ứng dụng)", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.Restart();
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lưu cấu hình: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
