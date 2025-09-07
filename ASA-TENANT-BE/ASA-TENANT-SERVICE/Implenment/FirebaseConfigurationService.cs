using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace ASA_TENANT_SERVICE.Implement
{
    public class FirebaseConfigurationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<FirebaseConfigurationService> _logger;

        public FirebaseConfigurationService(IConfiguration configuration, ILogger<FirebaseConfigurationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }
        public void InitializeFirebase()
        {
            // Safe check: try to read DefaultInstance. If not initialized, FirebaseApp.DefaultInstance throws.
            try
            {
                var dummy = FirebaseApp.DefaultInstance;
                _logger.LogInformation("Firebase already initialized (DefaultInstance present).");
                return;
            }
            catch (InvalidOperationException)
            {
                // Not initialized yet -> continue to initialize
            }

            try
            {
                GoogleCredential credential = null;

                // 1) Env var base64 (recommended for CI/CD)
                var envBase64 = Environment.GetEnvironmentVariable("FIREBASE_SERVICE_ACCOUNT_BASE64");
                if (!string.IsNullOrEmpty(envBase64))
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(envBase64));
                    credential = GoogleCredential.FromJson(json);
                    _logger.LogInformation("Loaded Firebase credentials from FIREBASE_SERVICE_ACCOUNT_BASE64 env var.");
                }
                else
                {
                    // 2) JSON string in config (appsettings) - often used in Render/Heroku secrets
                    var serviceAccountKey = _configuration["Firebase:ServiceAccountKey"];
                    var serviceAccountKeyPath = _configuration["Firebase:ServiceAccountKeyPath"];

                    if (!string.IsNullOrEmpty(serviceAccountKey))
                    {
                        // fix escaped newlines if needed
                        if (serviceAccountKey.Contains("\\n")) serviceAccountKey = serviceAccountKey.Replace("\\n", "\n");
                        credential = GoogleCredential.FromJson(serviceAccountKey);
                        _logger.LogInformation("Loaded Firebase credentials from configuration ServiceAccountKey.");
                    }
                    else if (!string.IsNullOrEmpty(serviceAccountKeyPath))
                    {
                        if (!File.Exists(serviceAccountKeyPath))
                            throw new FileNotFoundException($"Firebase service account file not found: {serviceAccountKeyPath}");

                        credential = GoogleCredential.FromFile(serviceAccountKeyPath);
                        _logger.LogInformation($"Loaded Firebase credentials from file: {serviceAccountKeyPath}");
                    }
                    else
                    {
                        // 3) Fallback to Application Default Credentials (useful if running on GCP)
                        credential = GoogleCredential.GetApplicationDefault();
                        _logger.LogInformation("Using Google Application Default Credentials (ADC) fallback.");
                    }
                }

                var projectId = _configuration["Firebase:ProjectId"];
                var options = new AppOptions()
                {
                    Credential = credential,
                    ProjectId = projectId
                };

                FirebaseApp.Create(options);
                _logger.LogInformation("Firebase Admin SDK initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase Admin SDK.");
                throw;
            }
        }
    }
}
