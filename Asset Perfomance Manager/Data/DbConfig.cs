namespace AssetPerformanceManager.Data
{
    public static class DbConfig
    {
        // Строка подключения. Проверь имя сервера (обычно (localdb)\mssqllocaldb)
        public static string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=AssetPerformanceDB;Trusted_Connection=True;TrustServerCertificate=True;";
    }
}