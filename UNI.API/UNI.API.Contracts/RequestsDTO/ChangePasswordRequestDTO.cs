namespace UNI.API.Contracts.RequestsDTO
{
    public class ChangePasswordRequestDTO
    {
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}