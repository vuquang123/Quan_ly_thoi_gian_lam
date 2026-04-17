using System;
using System.Drawing;
using System.Windows.Forms;

namespace FaceIDHRM.UI
{
    public class FormLogin : Form
    {
        public FormLogin()
        {
            this.Text = "Hệ thống FaceID HRM - Đăng nhập";
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
                Text = "CHỌN LUỒNG ĐĂNG NHẬP",
                Font = new Font("Arial", 16, FontStyle.Bold),
                Location = new Point(100, 30),
                Size = new Size(300, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Nút Nhân Viên
            Button btnNhanVien = new Button
            {
                Text = "Dành Cho Nhân Viên",
                Location = new Point(100, 100),
                Size = new Size(300, 50),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnNhanVien.Click += BtnNhanVien_Click;
            this.Controls.Add(btnNhanVien);

            // Nút Admin
            Button btnAdmin = new Button
            {
                Text = "Dành Cho Admin",
                Location = new Point(100, 180),
                Size = new Size(300, 50),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnAdmin.Click += BtnAdmin_Click;
            this.Controls.Add(btnAdmin);
        }

        private void BtnNhanVien_Click(object? sender, EventArgs e)
        {
            FormNhanVien frmNV = new FormNhanVien();
            this.Hide();
            frmNV.ShowDialog();
            this.Show(); // Hiện lại form login khi tắt form NV
        }

        private void BtnAdmin_Click(object? sender, EventArgs e)
        {
            // Simple Password Prompt bằng InputBox thủ công
            // (Thường C# không có sẵn InputBox kiểu VB.NET, ta tạo Form nhỏ tự chế)
            using (Form prompt = new Form()
            {
                Width = 400, Height = 180, FormBorderStyle = FormBorderStyle.FixedDialog, Text = "Nhập Mật Khẩu Admin", StartPosition = FormStartPosition.CenterParent
            })
            {
                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Vui lòng nhập mật khẩu truy cập hệ thống:", Width=300 };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, UseSystemPasswordChar = true };
                Button confirmation = new Button() { Text = "Xác nhận", Left = 260, Width = 100, Top = 90, DialogResult = DialogResult.OK };
                confirmation.Click += (s, ev) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    if (textBox.Text == "QuangVD@08102005")
                    {
                        FormAdmin frmAdmin = new FormAdmin();
                        this.Hide();
                        frmAdmin.ShowDialog();
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("Sai mật khẩu!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
