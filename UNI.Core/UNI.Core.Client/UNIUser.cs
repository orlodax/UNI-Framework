using UNI.Core.Library;

namespace UNI.Core.Client
{
    public class UNIUser : BaseModel
    {
        public static string Username { get; set; }

        public static string Password { get; set; }

        public static UNIToken Token { get; set; }

    }
}
