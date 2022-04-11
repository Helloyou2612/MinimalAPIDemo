namespace MinimalAPIDemo.Models
{
    public class Student
    {
        public int StudentId { get; set; }
        public string? Fullname { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public DateTime DateOfBirth { get; set; }
    }
}
