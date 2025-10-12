using ASA_TENANT_SERVICE.Configuration;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ASA_TENANT_SERVICE.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChatbotServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register configuration
            services.Configure<GeminiConfiguration>(configuration.GetSection("GeminiSettings"));
            services.Configure<ChatbotConfiguration>(configuration.GetSection("ChatbotSettings"));

            // Register services
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IChatbotService, ChatbotService>();

            // Add HTTP client for Gemini API
            services.AddHttpClient<GeminiService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("GeminiSettings:TimeoutSeconds", 30));
                client.DefaultRequestHeaders.Add("User-Agent", "ASA-Tenant-Chatbot/1.0");
            });

            return services;
        }

        public static IServiceCollection AddChatbotWithFallback(this IServiceCollection services, IConfiguration configuration)
        {
            var enableGemini = configuration.GetValue<bool>("ChatbotSettings:EnableGeminiAI", true);
            
            if (enableGemini)
            {
                // Register with Gemini AI
                services.AddChatbotServices(configuration);
            }
            else
            {
                // Register only hardcoded chatbot
                services.AddScoped<IChatbotService, ChatbotService>();
            }

            return services;
        }
    }
}
