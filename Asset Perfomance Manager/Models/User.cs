namespace AssetPerformanceManager.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string FullName { get; set; }
    }

    // Статический класс для хранения текущего пользователя после входа
    public static class CurrentSession
    {
        public static User CurrentUser { get; set; }
    }
}