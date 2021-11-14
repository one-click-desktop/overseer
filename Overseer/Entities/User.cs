namespace OneClickDesktop.Overseer.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public Role Role { get; set; }
        public string Password { get; set; }
    }
}
