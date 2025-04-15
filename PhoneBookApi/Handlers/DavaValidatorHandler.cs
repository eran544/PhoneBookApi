using System.Text.RegularExpressions;

namespace PhoneBookApi.Handlers
{
    public static class DavaValidatorHandler
    {
        public static bool IsValidEmail(string email)
        {
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return emailRegex.IsMatch(email);
        }
    }
}
