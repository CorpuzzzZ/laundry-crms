namespace CRM.WinForms.Config
{
    public static class AppConfig
    {
        public const string ApiBaseUrl = "https://localhost:7001";
        public const string ApiVersion = "v1";
        public const string AppName = "Laundry CRM System";
        public const string AppVersion = "1.0.0";

        public static string ApiUrl(string endpoint)
        {
            return $"{ApiBaseUrl}/{endpoint.TrimStart('/')}";
        }

        public static string ApiV1Url(string endpoint)
        {
            return $"{ApiBaseUrl}/api/{ApiVersion}/{endpoint.TrimStart('/')}";
        }
    }
}