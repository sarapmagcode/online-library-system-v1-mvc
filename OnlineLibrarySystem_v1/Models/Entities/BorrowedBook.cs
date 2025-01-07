namespace OnlineLibrarySystem_v1.Models.Entities
{
    public class BorrowedBook
    {
        public int Id { get; set; }
        
        public int BookId { get; set; }
        public Book Book { get; set; }
        
        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime BorrowDate { get; set; }
        
        public DateTime DueDate { get; set; }
        
        public DateTime? ReturnDate { get; set; }
    }
}
