using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace DigitalLibrary
{
    public class BorrowBookForm : Form
    {
        private ComboBox cmbBooks;
        private TextBox txtBorrowerName;
        private NumericUpDown numCopies;
        private ModernButton btnBorrow, btnCancel;

        public BorrowBookForm()
        {
            this.Text = "Borrow Book";
            this.Size = new Size(500, 360);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            UITheme.StyleForm(this);

            InitializeComponents();
            LoadData();
        }

        private void InitializeComponents()
        {
            Label lblHeader = new Label { Text = "Borrow a Book", Font = UITheme.TitleFont, ForeColor = UITheme.PrimaryColor, Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            int y = 70;
            int hGap = 55;
            Font boldFont = new Font("Segoe UI", 10F, FontStyle.Bold);

            Label lblBook = new Label { Text = "Select Book:", Location = new Point(30, y + 5), AutoSize = true, Font = boldFont };
            cmbBooks = new ComboBox 
            { 
                Location = new Point(170, y), 
                Width = 270, 
                DropDownStyle = ComboBoxStyle.DropDownList 
            };
            this.Controls.Add(lblBook); this.Controls.Add(cmbBooks);
            y += hGap;

            Label lblBorrower = new Label { Text = "Borrower Name:", Location = new Point(30, y + 3), AutoSize = true, Font = boldFont };
            txtBorrowerName = new TextBox { Location = new Point(170, y), Width = 270 };
            this.Controls.Add(lblBorrower); this.Controls.Add(txtBorrowerName);
            y += hGap;

            Label lblCopies = new Label { Text = "Copies:", Location = new Point(30, y + 3), AutoSize = true, Font = boldFont };
            numCopies = new NumericUpDown { Location = new Point(170, y), Width = 120, Minimum = 1, Maximum = 10000 };
            this.Controls.Add(lblCopies); this.Controls.Add(numCopies);
            y += hGap + 15;

            btnBorrow = new ModernButton { Text = "Borrow Book", Location = new Point(170, y), Width = 130, Height = 40, BackColor = UITheme.PrimaryColor };
            btnCancel = new ModernButton { Text = "Cancel", Location = new Point(310, y), Width = 130, Height = 40, BackColor = Color.Gray };

            btnBorrow.Click += BtnBorrow_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnBorrow);
            this.Controls.Add(btnCancel);
        }

        private void LoadData()
        {
            var dtBooks = DatabaseHelper.ExecuteQuery("SELECT BookId, Title || ' (' || Author || ') - ' || AvailableCopies || ' available' AS DisplayName, Title FROM Books WHERE AvailableCopies > 0");
            cmbBooks.DataSource = dtBooks;
            cmbBooks.DisplayMember = "DisplayName";
            cmbBooks.ValueMember = "BookId";
        }

        private void BtnBorrow_Click(object sender, EventArgs e)
        {
            if (cmbBooks.SelectedValue == null)
            {
                MessageBox.Show("Please select a valid Book.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBorrowerName.Text))
            {
                MessageBox.Show("Please enter a Borrower Name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int bookId = Convert.ToInt32(cmbBooks.SelectedValue);
            string bookTitle = ((DataRowView)cmbBooks.SelectedItem)["Title"].ToString();
            string borrowerName = txtBorrowerName.Text.Trim();
            int copiesObj = (int)numCopies.Value;

            var checkDt = DatabaseHelper.ExecuteQuery("SELECT AvailableCopies FROM Books WHERE BookId = @Id", new SQLiteParameter("@Id", bookId));
            int available = Convert.ToInt32(checkDt.Rows[0][0]);
            
            if (available < copiesObj)
            {
                MessageBox.Show($"Only {available} copies available!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime borrowTime = DateTime.Now;
            DateTime returnTime = borrowTime.AddHours(1);

            DatabaseHelper.ExecuteNonQuery(
                "INSERT INTO BorrowRecords (BookId, BookTitle, BorrowerName, Copies, BorrowTime, ReturnTime, Status) " +
                "VALUES (@BookId, @BookTitle, @BorrowerName, @Copies, @BorrowTime, @ReturnTime, @Status)",
                new SQLiteParameter("@BookId", bookId),
                new SQLiteParameter("@BookTitle", bookTitle),
                new SQLiteParameter("@BorrowerName", borrowerName),
                new SQLiteParameter("@Copies", copiesObj),
                new SQLiteParameter("@BorrowTime", borrowTime),
                new SQLiteParameter("@ReturnTime", returnTime),
                new SQLiteParameter("@Status", "Borrowed"));

            DatabaseHelper.ExecuteNonQuery(
                "UPDATE Books SET AvailableCopies = AvailableCopies - @Copies WHERE BookId = @BookId",
                new SQLiteParameter("@Copies", copiesObj),
                new SQLiteParameter("@BookId", bookId));

            MessageBox.Show("Book borrowed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.DialogResult = DialogResult.OK;
        }
    }
}
