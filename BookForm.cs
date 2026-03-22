using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using DigitalLibrary.Models;

namespace DigitalLibrary
{
    public class BookForm : Form
    {
        private TextBox txtTitle, txtAuthor, txtCategory;
        private NumericUpDown numCopies;
        private ModernButton btnSave, btnCancel;
        private Book editBook;

        public BookForm(Book book = null)
        {
            this.editBook = book;
            this.Text = book == null ? "Add New Book" : "Update Book";
            this.Size = new Size(450, 420);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            UITheme.StyleForm(this);

            InitializeComponents();

            if (book != null)
            {
                txtTitle.Text = book.Title;
                txtAuthor.Text = book.Author;
                txtCategory.Text = book.Category;
                numCopies.Value = book.AvailableCopies;
            }
        }

        private void InitializeComponents()
        {
            Label lblHeader = new Label { Text = editBook == null ? "Add Book Details" : "Edit Book Details", Font = UITheme.TitleFont, ForeColor = UITheme.PrimaryColor, Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblHeader);

            int y = 80;
            int hGap = 55;
            Font boldFont = new Font("Segoe UI", 10F, FontStyle.Bold);

            Label lblTitle = new Label { Text = "Title:", Location = new Point(30, y + 3), AutoSize = true, Font = boldFont };
            txtTitle = new TextBox { Location = new Point(130, y), Width = 260 };
            this.Controls.Add(lblTitle); this.Controls.Add(txtTitle);
            y += hGap;

            Label lblAuthor = new Label { Text = "Author:", Location = new Point(30, y + 3), AutoSize = true, Font = boldFont };
            txtAuthor = new TextBox { Location = new Point(130, y), Width = 260 };
            this.Controls.Add(lblAuthor); this.Controls.Add(txtAuthor);
            y += hGap;

            Label lblCategory = new Label { Text = "Category:", Location = new Point(30, y + 3), AutoSize = true, Font = boldFont };
            txtCategory = new TextBox { Location = new Point(130, y), Width = 260 };
            this.Controls.Add(lblCategory); this.Controls.Add(txtCategory);
            y += hGap;

            Label lblCopies = new Label { Text = "Copies:", Location = new Point(30, y + 3), AutoSize = true, Font = boldFont };
            numCopies = new NumericUpDown { Location = new Point(130, y), Width = 120, Minimum = 0, Maximum = 10000 };
            this.Controls.Add(lblCopies); this.Controls.Add(numCopies);
            y += hGap + 20;

            btnSave = new ModernButton { Text = "Save", Location = new Point(130, y), Width = 120, Height = 40, BackColor = UITheme.SuccessColor };
            btnCancel = new ModernButton { Text = "Cancel", Location = new Point(270, y), Width = 120, Height = 40, BackColor = Color.Gray };

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.Add(btnSave);
            this.Controls.Add(btnCancel);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtAuthor.Text))
            {
                MessageBox.Show("Title and Author are required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (editBook == null)
            {
                DatabaseHelper.ExecuteNonQuery(
                    "INSERT INTO Books (Title, Author, Category, AvailableCopies) VALUES (@Title, @Author, @Category, @Copies)",
                    new SQLiteParameter("@Title", txtTitle.Text.Trim()),
                    new SQLiteParameter("@Author", txtAuthor.Text.Trim()),
                    new SQLiteParameter("@Category", txtCategory.Text.Trim()),
                    new SQLiteParameter("@Copies", (int)numCopies.Value));
            }
            else
            {
                DatabaseHelper.ExecuteNonQuery(
                    "UPDATE Books SET Title=@Title, Author=@Author, Category=@Category, AvailableCopies=@Copies WHERE BookId=@BookId",
                    new SQLiteParameter("@Title", txtTitle.Text.Trim()),
                    new SQLiteParameter("@Author", txtAuthor.Text.Trim()),
                    new SQLiteParameter("@Category", txtCategory.Text.Trim()),
                    new SQLiteParameter("@Copies", (int)numCopies.Value),
                    new SQLiteParameter("@BookId", editBook.BookId));
            }

            this.DialogResult = DialogResult.OK;
        }
    }
}
