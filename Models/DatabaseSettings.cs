namespace Reflectly.Models
{
    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string Account_Collection { get; set; } = null!;

        public string RefreshToken_Collection { get; set; } = null!;
    }
}
