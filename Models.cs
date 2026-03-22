using System;

namespace DigitalLibrary.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Category { get; set; }
        public int AvailableCopies { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
    }

    public class IssuedBook
    {
        public int IssueId { get; set; }
        public int BookId { get; set; }
        public int UserId { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public decimal LateFee { get; set; }

        public string BookTitle { get; set; }
        public string UserName { get; set; }
    }

    public class BorrowRecord
    {
        public int RecordId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; }
        public string BorrowerName { get; set; }
        public int Copies { get; set; }
        public DateTime BorrowTime { get; set; }
        public DateTime ReturnTime { get; set; }
        public string Status { get; set; }
        public decimal ChargedAmount { get; set; }
    }
}
