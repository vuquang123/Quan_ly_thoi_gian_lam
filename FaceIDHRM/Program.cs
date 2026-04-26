using System;
using System.IO;
using System.Windows.Forms;
using FaceIDHRM.UI;

namespace FaceIDHRM;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        string modeFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "device_mode.txt");
        string mode = "";

        if (File.Exists(modeFile))
        {
            mode = File.ReadAllText(modeFile).Trim().ToUpper();
        }

        if (mode == "ADMIN")
        {
            using (Form prompt = new Form()
            {
                Width = 400, Height = 180, FormBorderStyle = FormBorderStyle.FixedDialog, Text = "Nhập Mật Khẩu Admin", StartPosition = FormStartPosition.CenterScreen
            })
            {
                Label textLabel = new Label() { Left = 20, Top = 20, Text = "Vui lòng nhập mật khẩu truy cập hệ thống:", Width = 300 };
                TextBox textBox = new TextBox() { Left = 20, Top = 50, Width = 340, UseSystemPasswordChar = true };
                Button confirmation = new Button() { Text = "Xác nhận", Left = 260, Width = 100, Top = 90, DialogResult = DialogResult.OK };
                confirmation.Click += (s, ev) => { prompt.Close(); };
                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(textBox);
                prompt.Controls.Add(confirmation);
                prompt.AcceptButton = confirmation;

                if (prompt.ShowDialog() == DialogResult.OK)
                {
                    if (textBox.Text == "admin123")
                    {
                        Application.Run(new FormAdmin());
                    }
                    else
                    {
                        MessageBox.Show("Sai mật khẩu! Ứng dụng sẽ thoát.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        else if (mode == "KIOSK")
        {
            Application.Run(new FormNhanVien());
        }
        else
        {
            // Fallback for first time setup
            Application.Run(new FormSetupDevice());
        }
    }    
}