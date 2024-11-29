namespace TeachingAssignmentApp.Model
{
    public class SignInResponse
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
    }
}
