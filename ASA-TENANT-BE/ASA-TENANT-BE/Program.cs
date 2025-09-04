using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.Mapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// Cấu hình để chạy trên Docker/Render
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080); // Render yêu cầu chạy ở port 8080
});

Console.WriteLine("🌍 ENVIRONMENT = " + builder.Environment.EnvironmentName);
Console.WriteLine("🔌 CONNECTION = " + builder.Configuration.GetConnectionString("DefaultConnection"));

// Add services to the container.
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IChatMessageService, ChatMessageService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IFcmService, FcmService>();
builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
builder.Services.AddScoped<ILogActivityService, LogActivityService>();
builder.Services.AddScoped<INfcService, NfcService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductUnitService, ProductUnitService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IPromotionProductService, PromotionProductService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();
builder.Services.AddScoped<IPromptService, PromptService>();
builder.Services.AddScoped<IReportDetailService, ReportDetailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IShopService, ShopService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IZalopayService, ZalopayService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Register repositories
builder.Services.AddScoped<CategoryRepo>();
builder.Services.AddScoped<ChatMessageRepo>();
builder.Services.AddScoped<CustomerRepo>();
builder.Services.AddScoped<FcmRepo>();
builder.Services.AddScoped<InventoryTransactionRepo>();
builder.Services.AddScoped<LogActivityRepo>();
builder.Services.AddScoped<NfcRepo>();
builder.Services.AddScoped<NotificationRepo>();
builder.Services.AddScoped<OrderDetailRepo>();
builder.Services.AddScoped<OrderRepo>();
builder.Services.AddScoped<ProductRepo>();
builder.Services.AddScoped<ProductUnitRepo>();
builder.Services.AddScoped<PromotionRepo>();
builder.Services.AddScoped<PromotionProductRepo>();
builder.Services.AddScoped<PromptRepo>();
builder.Services.AddScoped<ReportDetailRepo>();
builder.Services.AddScoped<ReportRepo>();
builder.Services.AddScoped<ShiftRepo>();
builder.Services.AddScoped<ShopRepo>();
builder.Services.AddScoped<TransactionRepo>();
builder.Services.AddScoped<UnitRepo>();
builder.Services.AddScoped<UserRepo>();
builder.Services.AddScoped<VoucherRepo>();
builder.Services.AddScoped<ZalopayRepo>();

// Add HttpClient for BePlatform
builder.Services.AddHttpClient("BePlatform", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["BePlatformURL:Url"]);
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Đăng ký AutoMapper
builder.Services.AddAutoMapper(cfg => {}, typeof(MappingProfile).Assembly);
// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        builder =>
        {
            builder.WithOrigins(
                    "http://localhost:5173",
                    "http://localhost:3000",
                    "https://asa-web-app-tawny.vercel.app",
                    "https://asa-fe-three.vercel.app",
                    "https://asa-admin-mu.vercel.app"
                 )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// ==================== Controllers & Swagger ====================
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "JWT Authorization header using the access token",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };
    options.SwaggerDoc("v1", new() { Title = "ASA-TENANT-BE API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        jwtSecurityScheme, Array.Empty<string>()
                    }
                });
});

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtConfig");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    // Custom response when the token is invalid or missing
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // Skip default behavior
            context.HandleResponse();

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                Success = false,
                Status = 401,
                Message = "Unauthorized: Token is missing or invalid"
            }));
        },
        OnForbidden = async context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                Success = false,
                Status = 403,
                Message = "Forbidden: You do not have permission to access this resource"
            }));
        }
    };
});

// Add DbContext
builder.Services.AddDbContext<ASATENANTDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();


// ==================== Middleware Pipeline ====================
// Luôn bật swagger (kể cả Production như EDUConnect)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ASA Tenant API v1");
});

// CORS phải đặt trước Authorization
app.UseCors("AllowFrontend");

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
