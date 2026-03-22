using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalLibrary
{
    public class ReturnBookForm : Form
    {
        private int issueId;
        private Label lblDetails, lblLateDays, lblLateFeeText;
        private ModernButton btnConfirmReturn, btnCancel;

        private decimal calculateLateFee = 0;
        private int returnBookId = 0;

        public ReturnBookForm(int issueId)
        {
            this.issueId = issueId;
            this.Text = "Return Book";
            this.Size = new Size(450, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            UITheme.StyleForm(this);

            InitializeComponents();
            LoadIssueDetails();
        }

        private void InitializeComponents()
        {
            Label lblHeader = new Label { Text = "Return Book", Font = UITheme.TitleFont, ForeColor = UITheme.PrimaryColor, Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            int y = 70;

            Panel pnlDetails = new Panel { Location = new Point(20, y), Size = new Size(390, 80), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            lblDetails = new Label { Location = new Point(10, 10), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Regular) };
            pnlDetails.Controls.Add(lblDetails);
            this.Controls.Add(pnlDetails);
            y += 100;

            lblLateDays = new Label { Location = new Point(25, y), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            this.Controls.Add(lblLateDays);
            y += 30;

            lblLateFeeText = new Label { Location = new Point(25, y), AutoSize = true, ForeColor = UITheme.DangerColor, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            this.Controls.Add(lblLateFeeText);
            y += 50;

            btnConfirmReturn = new ModernButton { Text = "Confirm Return", Location = new Point(120, y), Width = 140, Height = 40, BackColor = Color.FromArgb(142, 68, 173) };
            btnCancel = new ModernButton { Text = "Cancel", Location = new Point(280, y), Width = 100, Height = 40, BackColor = Color.Gray };

            btnConfirmReturn.Click += BtnConfirmReturn_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnConfirmReturn);
            this.Controls.Add(btnCancel);
        }

        private void LoadIssueDetails()
        {
            var dt = DatabaseHelper.ExecuteQuery(@"
                SELECT ib.BookId, b.Title, u.Name, ib.IssueDate 
                FROM IssuedBooks ib
                JOIN Books b ON ib.BookId = b.BookId
                JOIN Users u ON ib.UserId = u.UserId
                WHERE ib.IssueId = @IssueId", new SQLiteParameter("@IssueId", issueId));

            if (dt.Rows.Count > 0)
            {
                var row = dt.Rows[0];
                returnBookId = Convert.ToInt32(row["BookId"]);
                string title = row["Title"].ToString();
                string userName = row["Name"].ToString();
                DateTime issueDate = Convert.ToDateTime(row["IssueDate"]);

                lblDetails.Text = $"User:\t {userName}\nBook:\t {title}\nIssued:\t {issueDate.ToShortDateString()}";

                int lateDays = (int)(DateTime.Now - issueDate).TotalDays - 14;   // Assuming 14 days allowed
                if (lateDays < 0) lateDays = 0;

                calculateLateFee = lateDays * 10m; // Rs/$/units 10 per day late fee

                lblLateDays.Text = $"Late days (after 14 days period): {lateDays} days";
                lblLateFeeText.Text = $"Calculated Late Fee: {calculateLateFee:C}";
            }
        }

        private void BtnConfirmReturn_Click(object sender, EventArgs e)
        {
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE IssuedBooks 
                SET ReturnDate = @RetDate, LateFee = @Fee 
                WHERE IssueId = @IssueId",
                new SQLiteParameter("@RetDate", DateTime.Now),
                new SQLiteParameter("@Fee", calculateLateFee),
                new SQLiteParameter("@IssueId", issueId));

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Books SET AvailableCopies = AvailableCopies + 1 WHERE BookId = @BookId",
                new SQLiteParameter("@BookId", returnBookId));

            MessageBox.Show("Book returned successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
