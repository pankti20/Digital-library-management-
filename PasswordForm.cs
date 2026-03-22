using System;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalLibrary
{
    public class PasswordForm : Form
    {
        private TextBox txtPassword;
        private ModernButton btnSubmit, btnCancel;
        public bool IsAuthenticated { get; private set; } = false;

        public PasswordForm()
        {
            this.Text = "Admin Authentication";
            this.Size = new Size(350, 200);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            UITheme.StyleForm(this);

            var lblHeader = new Label { Text = "Enter Admin Password", Font = UITheme.TitleFont, ForeColor = UITheme.PrimaryColor, Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            var lblPassword = new Label { Text = "Password:", Location = new Point(30, 75), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtPassword = new TextBox { Location = new Point(120, 72), Width = 180, PasswordChar = '*' };
            this.Controls.Add(lblPassword); this.Controls.Add(txtPassword);

            btnSubmit = new ModernButton { Text = "Submit", Location = new Point(80, 115), Width = 100, Height = 35, BackColor = UITheme.PrimaryColor };
            btnCancel = new ModernButton { Text = "Cancel", Location = new Point(190, 115), Width = 100, Height = 35, BackColor = Color.Gray };

            btnSubmit.Click += BtnSubmit_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnSubmit); this.Controls.Add(btnCancel);
        }

        private void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (txtPassword.Text == "123")
            {
                IsAuthenticated = true;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Incorrect Password!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
