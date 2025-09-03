using ASA_TENANT_REPO.DBContext;
using ASA_TENANT_REPO.Repository;
using ASA_TENANT_SERVICE.Implenment;
using ASA_TENANT_SERVICE.Interface;
using ASA_TENANT_SERVICE.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
builder.Services.AddSwaggerGen();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
