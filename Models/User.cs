namespace UserNamespace
{
    public class UserModel 
    {
        public required string UserType { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public required string Department { get; set; }
        public int? TeamSize { get; set; } // Make it nullable
    }
}
