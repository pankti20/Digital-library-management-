using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalLibrary
{
    public class IssueBookForm : Form
    {
        private ComboBox cmbBooks, cmbUsers;
        private ModernButton btnIssue, btnCancel;

        public IssueBookForm()
        {
            this.Text = "Issue Book";
            this.Size = new Size(500, 320);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            UITheme.StyleForm(this);

            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            Label lblHeader = new Label { Text = "Issue a Book", Font = UITheme.TitleFont, ForeColor = UITheme.PrimaryColor, Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            int y = 80;
            int hGap = 60;

            Label lblUser = new Label { Text = "User Name/ID:", Location = new Point(30, y + 5), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            cmbUsers = new ComboBox 
            { 
                Location = new Point(160, y), 
                Width = 280, 
                DropDownStyle = ComboBoxStyle.DropDown, 
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.ListItems
            };
            this.Controls.Add(lblUser); this.Controls.Add(cmbUsers);
            y += hGap;

            Label lblBook = new Label { Text = "Select Book:", Location = new Point(30, y + 5), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            cmbBooks = new ComboBox 
            { 
                Location = new Point(160, y), 
                Width = 280, 
                DropDownStyle = ComboBoxStyle.DropDownList 
            };
            this.Controls.Add(lblBook); this.Controls.Add(cmbBooks);
            y += hGap + 10;

            btnIssue = new ModernButton { Text = "Issue Book", Location = new Point(160, y), Width = 130, Height = 40, BackColor = UITheme.PrimaryColor };
            btnCancel = new ModernButton { Text = "Cancel", Location = new Point(310, y), Width = 130, Height = 40, BackColor = Color.Gray };

            btnIssue.Click += BtnIssue_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnIssue);
            this.Controls.Add(btnCancel);
        }

        private void LoadData()
        {
            var dtBooks = DatabaseHelper.ExecuteQuery("SELECT BookId, Title || ' (' || Author || ')' AS DisplayName FROM Books WHERE AvailableCopies > 0");
            cmbBooks.DataSource = dtBooks;
            cmbBooks.DisplayMember = "DisplayName";
            cmbBooks.ValueMember = "BookId";

            var dtUsers = DatabaseHelper.ExecuteQuery("SELECT UserId, UserId || ' - ' || Name AS DisplayName FROM Users");
            cmbUsers.DataSource = dtUsers;
            cmbUsers.DisplayMember = "DisplayName";
            cmbUsers.ValueMember = "UserId";
        }

        private void BtnIssue_Click(object sender, EventArgs e)
        {
            if (cmbBooks.SelectedValue == null)
            {
                MessageBox.Show("Please select a valid Book.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int userId = -1;
            if (cmbUsers.SelectedValue != null && cmbUsers.SelectedIndex != -1)
            {
                userId = Convert.ToInt32(cmbUsers.SelectedValue);
            }
            else
            {
                if (!int.TryParse(cmbUsers.Text, out userId))
                {
                    var dtUsersSource = (DataTable)cmbUsers.DataSource;
                    if (dtUsersSource != null)
                    {
                        foreach(DataRow r in dtUsersSource.Rows)
                        {
                            string display = r["DisplayName"].ToString();
                            string uIdStr = r["UserId"].ToString();
                            if (display.StartsWith(cmbUsers.Text, StringComparison.OrdinalIgnoreCase) || uIdStr == cmbUsers.Text)
                            {
                                userId = Convert.ToInt32(r["UserId"]);
                                break;
                            }
                        }
                    }
                }
            }

            if (userId <= 0)
            {
                 MessageBox.Show("Please select or enter a valid User ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return;
            }

            var userCheckDt = DatabaseHelper.ExecuteQuery("SELECT COUNT(*) FROM Users WHERE UserId = @Id", new SQLiteParameter("@Id", userId));
            if (Convert.ToInt32(userCheckDt.Rows[0][0]) == 0)
            {
                 MessageBox.Show($"User ID {userId} does not exist in the system.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 return;
            }

            int bookId = Convert.ToInt32(cmbBooks.SelectedValue);
            DateTime issueDate = DateTime.Now;

            var checkDt = DatabaseHelper.ExecuteQuery("SELECT AvailableCopies FROM Books WHERE BookId = @Id", new SQLiteParameter("@Id", bookId));
            int available = Convert.ToInt32(checkDt.Rows[0][0]);
            
            if (available <= 0)
            {
                MessageBox.Show("This book is currently out of stock!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO IssuedBooks (BookId, UserId, IssueDate) VALUES (@BookId, @UserId, @IssueDate)",
                new SQLiteParameter("@BookId", bookId),
                new SQLiteParameter("@UserId", userId),
                new SQLiteParameter("@IssueDate", issueDate));

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE BookId = @BookId",
                new SQLiteParameter("@BookId", bookId));

            MessageBox.Show("Book issued successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
