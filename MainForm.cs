using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DigitalLibrary.Models;

namespace DigitalLibrary
{
    public class MainForm : Form
    {
        private TabControl tabControl;
        private DataGridView gridBooks;
        private DataGridView gridBorrowRecords;
        private TextBox txtSearchBook;
        
        private List<Book> allBooks;

        public MainForm()
        {
            this.Text = "Digital Library Management System";
            this.Size = new Size(1100, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            UITheme.StyleForm(this);
            InitializeComponents();
            LoadBooks();
            LoadBorrowRecords();
        }

        private void InitializeComponents()
        {
            Panel pnlHeader = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = UITheme.FormBackColor };
            Label lblAppTitle = new Label { Text = "Digital Library Management", Font = UITheme.TitleFont, ForeColor = UITheme.PrimaryColor, AutoSize = true, Location = new Point(20, 20) };
            pnlHeader.Controls.Add(lblAppTitle);
            this.Controls.Add(pnlHeader);

            tabControl = new TabControl { Dock = DockStyle.Fill, Font = UITheme.MainFont, ItemSize = new Size(180, 40), Padding = new Point(20, 5) };

            // ----- Books Tab -----
            TabPage tabBooks = new TabPage("Books Management") { BackColor = UITheme.FormBackColor };
            Panel pnlBooksTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = UITheme.FormBackColor };
            
            Label lblSearch = new Label { Text = "Search by Title/Author:", AutoSize = true, Location = new Point(20, 30), Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            txtSearchBook = new TextBox { Location = new Point(200, 28), Width = 300, Font = UITheme.MainFont };
            txtSearchBook.TextChanged += (s, e) => SearchBooks(txtSearchBook.Text);

            ModernButton btnAddBook = new ModernButton { Text = "+ Add Book", Location = new Point(550, 24), Width = 120, Height = 35, BackColor = UITheme.SuccessColor };
            ModernButton btnUpdateBook = new ModernButton { Text = "✎ Update", Location = new Point(680, 24), Width = 120, Height = 35, BackColor = UITheme.SecondaryColor };
            ModernButton btnDeleteBook = new ModernButton { Text = "✕ Delete", Location = new Point(810, 24), Width = 120, Height = 35, BackColor = UITheme.DangerColor };

            btnAddBook.Click += BtnAddBook_Click;
            btnUpdateBook.Click += BtnUpdateBook_Click;
            btnDeleteBook.Click += BtnDeleteBook_Click;

            pnlBooksTop.Controls.AddRange(new Control[] { lblSearch, txtSearchBook, btnAddBook, btnUpdateBook, btnDeleteBook });

            gridBooks = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            UITheme.StyleDataGridView(gridBooks);

            tabBooks.Controls.Add(gridBooks);
            tabBooks.Controls.Add(pnlBooksTop);

            // ----- Borrow Records Tab -----
            TabPage tabBorrow = new TabPage("Borrow Operations") { BackColor = UITheme.FormBackColor };
            Panel pnlBorrowTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = UITheme.FormBackColor };
            
            ModernButton btnBorrowBook = new ModernButton { Text = "+ Borrow Book", Location = new Point(20, 24), Width = 150, Height = 35, BackColor = UITheme.PrimaryColor };
            ModernButton btnReturnBook = new ModernButton { Text = "↩ Return Selected", Location = new Point(190, 24), Width = 160, Height = 35, BackColor = Color.FromArgb(142, 68, 173) };

            btnBorrowBook.Click += BtnBorrowBook_Click;
            btnReturnBook.Click += BtnReturnBook_Click;

            pnlBorrowTop.Controls.AddRange(new Control[] { btnBorrowBook, btnReturnBook });

            gridBorrowRecords = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            UITheme.StyleDataGridView(gridBorrowRecords);

            tabBorrow.Controls.Add(gridBorrowRecords);
            tabBorrow.Controls.Add(pnlBorrowTop);

            tabControl.TabPages.Add(tabBooks);
            tabControl.TabPages.Add(tabBorrow);

            Panel pnlMain = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            pnlMain.Controls.Add(tabControl);
            this.Controls.Add(pnlMain);
            pnlHeader.SendToBack();
            pnlMain.BringToFront();
        }

        public void LoadBooks()
        {
            var dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Books");
            allBooks = new List<Book>();
            foreach (DataRow row in dt.Rows)
            {
                allBooks.Add(new Book
                {
                    BookId = Convert.ToInt32(row["BookId"]),
                    Title = row["Title"].ToString(),
                    Author = row["Author"].ToString(),
                    Category = row["Category"].ToString(),
                    AvailableCopies = Convert.ToInt32(row["AvailableCopies"])
                });
            }
            gridBooks.DataSource = null;
            gridBooks.DataSource = allBooks;
        }

        private void SearchBooks(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                gridBooks.DataSource = allBooks;
                return;
            }

            var lowerQuery = query.ToLower();
            var filtered = allBooks.Where(b => 
                b.Title.ToLower().Contains(lowerQuery) || 
                b.Author.ToLower().Contains(lowerQuery)
            ).ToList();

            gridBooks.DataSource = null;
            gridBooks.DataSource = filtered;
        }

        public void LoadBorrowRecords()
        {
            var dt = DatabaseHelper.ExecuteQuery(@"
                SELECT RecordId, BookTitle, BorrowerName, Copies, BorrowTime, ReturnTime, Status, ChargedAmount
                FROM BorrowRecords
                ORDER BY CASE WHEN Status = 'Borrowed' THEN 0 ELSE 1 END, BorrowTime DESC");

            gridBorrowRecords.DataSource = null;
            gridBorrowRecords.DataSource = dt;
        }

        private void BtnAddBook_Click(object sender, EventArgs e)
        {
            using (var form = new BookForm(null))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    LoadBooks();
                }
            }
        }

        private void BtnUpdateBook_Click(object sender, EventArgs e)
        {
            if (gridBooks.SelectedRows.Count > 0)
            {
                var selectedBook = (Book)gridBooks.SelectedRows[0].DataBoundItem;
                using (var form = new BookForm(selectedBook))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        LoadBooks();
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a book to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnDeleteBook_Click(object sender, EventArgs e)
        {
            if (gridBooks.SelectedRows.Count > 0)
            {
                var selectedBook = (Book)gridBooks.SelectedRows[0].DataBoundItem;
                var result = MessageBox.Show($"Are you sure you want to delete '{selectedBook.Title}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    var countDt = DatabaseHelper.ExecuteQuery(
                        "SELECT COUNT(*) FROM BorrowRecords WHERE BookId = @BookId AND Status = 'Borrowed'",
                        new SQLiteParameter("@BookId", selectedBook.BookId));
                    int openIssues = Convert.ToInt32(countDt.Rows[0][0]);
                    
                    if (openIssues > 0)
                    {
                        MessageBox.Show("Cannot delete book because there are copies currently borrowed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    DatabaseHelper.ExecuteNonQuery("DELETE FROM Books WHERE BookId = @BookId", new SQLiteParameter("@BookId", selectedBook.BookId));
                    LoadBooks();
                }
            }
            else
            {
                MessageBox.Show("Please select a book to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void BtnBorrowBook_Click(object sender, EventArgs e)
        {
            using (var authForm = new PasswordForm())
            {
                if (authForm.ShowDialog() == DialogResult.OK && authForm.IsAuthenticated)
                {
                    using (var form = new BorrowBookForm())
                    {
                        if (form.ShowDialog() == DialogResult.OK)
                        {
                            LoadBooks();
                            LoadBorrowRecords();
                        }
                    }
                }
            }
        }

        private void BtnReturnBook_Click(object sender, EventArgs e)
        {
            if (gridBorrowRecords.SelectedRows.Count > 0)
            {
                var row = gridBorrowRecords.SelectedRows[0];
                int recordId = Convert.ToInt32(row.Cells["RecordId"].Value);
                string status = row.Cells["Status"].Value.ToString();
                
                if (status == "Returned")
                {
                    MessageBox.Show("This book has already been returned.", "Already Returned", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (var authForm = new PasswordForm())
                {
                    if (authForm.ShowDialog() == DialogResult.OK && authForm.IsAuthenticated)
                    {
                        var dt = DatabaseHelper.ExecuteQuery("SELECT BookId, Copies, ReturnTime FROM BorrowRecords WHERE RecordId = @Id", new SQLiteParameter("@Id", recordId));
                        if(dt.Rows.Count > 0)
                        {
                            int bookId = Convert.ToInt32(dt.Rows[0]["BookId"]);
                            int copies = Convert.ToInt32(dt.Rows[0]["Copies"]);
                            DateTime expectedReturn = Convert.ToDateTime(dt.Rows[0]["ReturnTime"]);
                            DateTime actualReturn = DateTime.Now;

                            decimal chargedAmount = 0;
                            if (actualReturn > expectedReturn)
                            {
                                chargedAmount = copies * 20m;
                                MessageBox.Show($"Book returned late!\nCharged Amount: {chargedAmount:C}", "Late Return", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            else
                            {
                                MessageBox.Show("Book returned on time. No late fees.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }

                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE BorrowRecords SET Status = 'Returned', ChargedAmount = @Charge WHERE RecordId = @Id",
                                new SQLiteParameter("@Charge", chargedAmount),
                                new SQLiteParameter("@Id", recordId));

                            DatabaseHelper.ExecuteNonQuery(
                                "UPDATE Books SET AvailableCopies = AvailableCopies + @Copies WHERE BookId = @BookId",
                                new SQLiteParameter("@Copies", copies),
                                new SQLiteParameter("@BookId", bookId));

                            LoadBooks();
                            LoadBorrowRecords();
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a borrowed record to return.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
