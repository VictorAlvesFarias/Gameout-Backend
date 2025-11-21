namespace Application.Dtos.User
{
    public class GetUserResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime CreateDate { get; set; }
    }
}

