namespace ASA_TENANT_SERVICE.Configuration
{
    public class GeminiConfiguration
    {
        public string ApiKey { get; set; } = string.Empty;
        public string Model { get; set; } = "gemini-1.5-flash-latest";
        public int MaxTokens { get; set; } = 2048;
        public double Temperature { get; set; } = 0.7;
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class ChatbotConfiguration
    {
        public bool EnableGeminiAI { get; set; } = true;
        public bool FallbackToHardcoded { get; set; } = true;
        public bool CacheResponses { get; set; } = true;
        public int CacheExpiryMinutes { get; set; } = 5;
        public int MaxRetries { get; set; } = 3;
        public int RetryDelaySeconds { get; set; } = 2;
    }
}
