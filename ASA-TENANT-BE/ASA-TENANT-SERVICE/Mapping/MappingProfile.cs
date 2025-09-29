using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
using ASA_TENANT_SERVICE.Enums;
using ASA_TENANT_SERVICE.Helper;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASA_TENANT_SERVICE.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            //Category Mappings
            CreateMap<Category, CategoryResponse>().ReverseMap();
            CreateMap<CategoryRequest, Category>().ReverseMap();
            CreateMap<CategoryGetRequest, Category>().ReverseMap();

            //Customer Mappings
            CreateMap<Customer, CustomerResponse>()
                                .ForMember(dest => dest.RankName, opt => opt.MapFrom(src => src.Rank.RankName))
                                .ReverseMap();
            CreateMap<CustomerRequest, Customer>().ReverseMap();
            CreateMap<CustomerGetRequest, Customer>().ReverseMap();

            //Product Mappings
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.ProductImageURL, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ReverseMap();
            CreateMap<ProductRequest, Product>().ReverseMap();
            CreateMap<ProductGetRequest, Product>().ReverseMap();
            // For update: don't overwrite existing values with nulls; never map ImageUrl directly
            CreateMap<ProductUpdateRequest, Product>()
                .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Product, ProductUpdateRequest>();

            // User Mappings
            CreateMap<User, UserResponse>().ReverseMap();
            CreateMap<UserUpdateRequest, User>()
     .          ForMember(dest => dest.Role, opt => opt.MapFrom(src => (short)src.Role))
                .ReverseMap()
     .          ForMember(dest => dest.Role, opt => opt.MapFrom(src => (UserRole)src.Role));
            CreateMap<UserCreateRequest, User>().ReverseMap();
            CreateMap<UserGetRequest, User>().ReverseMap();
            CreateMap<LoginResponse, User>().ReverseMap();

            // Shop Mappings
            CreateMap<Shop, ShopResponse>().ReverseMap();
            CreateMap<ShopRequest, Shop>().ReverseMap();
            CreateMap<ShopGetRequest, Shop>().ReverseMap();

            // InventoryTransaction Mappings
            CreateMap<InventoryTransaction, InventoryTransactionResponse>()
                    .ForMember(dest => dest.InventoryTransImageURL, opt => opt.MapFrom(src => src.ImageUrl)).ReverseMap();
            CreateMap<InventoryTransactionRequest, InventoryTransaction>().ReverseMap();
            CreateMap<InventoryTransactionGetRequest, InventoryTransaction>().ReverseMap();

            // Promotion Mappings
            CreateMap<Promotion, PromotionResponse>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (short?)src.Type))
                 .ForMember(dest => dest.Products,
        opt => opt.MapFrom(src =>
            src.PromotionProducts != null
                ? src.PromotionProducts.Select(pp => pp.ProductId.Value).ToHashSet()
                : new HashSet<long>()))
                .ReverseMap()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (PromotionType)src.Type));
            CreateMap<PromotionRequest, Promotion>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (short)src.Type))
                .ReverseMap()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (PromotionType)src.Type));
            CreateMap<PromotionGetRequest, Promotion>().ReverseMap();

            // LogActivity Mappings
            CreateMap<LogActivity, LogActivityResponse>().ReverseMap();
            CreateMap<LogActivityRequest, LogActivity>().ReverseMap();
            CreateMap<LogActivityGetRequest, LogActivity>().ReverseMap();

            // Unit Mappings
            CreateMap<Unit, UnitResponse>().ReverseMap();
            CreateMap<UnitRequest, Unit>().ReverseMap();
            CreateMap<UnitGetRequest, Unit>().ReverseMap();

            // Order Mappings
            CreateMap<PaymentMethodEnum, string>()
                .ConvertUsing(src => src.ToString());

            CreateMap<string, PaymentMethodEnum>()
                .ConvertUsing(src => EnumHelper.ParsePaymentMethod(src));

            CreateMap<string, PaymentMethodEnum?>()
                .ConvertUsing(src => EnumHelper.ParseNullablePaymentMethod(src));

            CreateMap<Order, OrderResponse>().ReverseMap();
            CreateMap<OrderRequest, Order>()
                .ForMember(dest => dest.OrderDetails, opt => opt.Ignore())
                .ReverseMap();
            CreateMap<OrderGetRequest, Order>().ReverseMap();


            // OrderDetail Mappings
            CreateMap<OrderDetail, OrderDetailResponse>().ReverseMap();
            CreateMap<OrderDetailRequest, OrderDetail>().ReverseMap();
            CreateMap<OrderDetailGetRequest, OrderDetail>().ReverseMap();

            // Transaction Mappings
            CreateMap<Transaction, TransactionResponse>().ReverseMap();
            CreateMap<TransactionRequest, Transaction>().ReverseMap();
            CreateMap<TransactionGetRequest, Transaction>().ReverseMap();

            // Voucher Mappings
            CreateMap<Voucher, VoucherResponse>().ReverseMap();
            CreateMap<VoucherRequest, Voucher>().ReverseMap();
            CreateMap<VoucherGetRequest, Voucher>().ReverseMap();

            // ChatMessage Mappings
            CreateMap<ChatMessage, ChatMessageResponse>().ReverseMap();
            CreateMap<ChatMessageRequest, ChatMessage>().ReverseMap();
            CreateMap<ChatMessageGetRequest, ChatMessage>().ReverseMap();

            //Fcm Mappings
            CreateMap<Fcm, FcmResponse>().ReverseMap();
            CreateMap<FcmRequest, Fcm>().ReverseMap();
            CreateMap<FcmGetRequest, Fcm>().ReverseMap();

            //Notification Mappings
            CreateMap<Notification, NotificationResponse>().ReverseMap();
            CreateMap<NotificationRequest, Notification>().ReverseMap();
            CreateMap<NotificationGetRequest, Notification>().ReverseMap();

            //Shift Mappings
            CreateMap<Shift, ShiftResponse>().ReverseMap();
            CreateMap<ShiftRequest, Shift>().ReverseMap();
            CreateMap<ShiftGetRequest, Shift>().ReverseMap();
            CreateMap<ShiftOpenRequest, Shift>().ReverseMap();

            //ProductUnit Mappings
            CreateMap<ProductUnit, ProductUnitResponse>()
     .          ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.UnitName, opt => opt.MapFrom(src => src.Unit.Name));
            CreateMap<ProductUnitRequest, ProductUnit>().ReverseMap();
            CreateMap<ProductUnitGetRequest, ProductUnit>().ReverseMap();

            // Nfc Mappings
            CreateMap<Nfc, NfcResponse>()
                .ForMember(dest => dest.CustomerFullName, opt => opt.MapFrom(src => src.Customer.FullName))
                .ForMember(dest => dest.CustomerEmail, opt => opt.MapFrom(src => src.Customer.Email))
                .ForMember(dest => dest.CustomerPhone, opt => opt.MapFrom(src => src.Customer.Phone))
                .ForMember(dest => dest.CustomerRank, opt => opt.MapFrom(src => src.Customer.Rank));
            CreateMap<NfcRequest, Nfc>().ReverseMap();
            CreateMap<NfcGetRequest, Nfc>().ReverseMap();

            // Promotion Product Mappings
            CreateMap<PromotionProduct, PromotionProductResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.PromotionName, opt => opt.MapFrom(src => src.Promotion.Name))
                .ForMember(dest => dest.PromotionValue, opt => opt.MapFrom(src => src.Promotion.Value))
                .ForMember(dest => dest.PromotionType, opt => opt.MapFrom(src => src.Promotion.Type))
                .ForMember(dest => dest.PromotionStartDate, opt => opt.MapFrom(src => src.Promotion.StartDate))
                .ForMember(dest => dest.PromotionEndDate, opt => opt.MapFrom(src => src.Promotion.EndDate))
                .ForMember(dest => dest.PromotionStartTime, opt => opt.MapFrom(src => src.Promotion.StartTime))
                .ForMember(dest => dest.PromotionEndTime, opt => opt.MapFrom(src => src.Promotion.EndTime));
            CreateMap<PromotionProductRequest, PromotionProduct>().ReverseMap();
            CreateMap<PromotionProductGetRequest, PromotionProduct>().ReverseMap();

            // Prompt Mappings
            CreateMap<Prompt, PromptResponse>().ReverseMap();
            CreateMap<PromptRequest, Prompt>().ReverseMap();
            CreateMap<PromptGetRequest, Prompt>().ReverseMap();

            // Report Mappings
            CreateMap<Report, ReportResponse>()
                .ForMember(dest => dest.ReportDetails, opt => opt.MapFrom(src => src.ReportDetails));
            CreateMap<ReportGetRequest, Report>().ReverseMap();

            CreateMap<ReportDetail, ReportDetailResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.ProductName))
                .ForMember(dest => dest.ProductCategory, opt => opt.MapFrom(src => src.Product.Category.CategoryName))
                .ForMember(dest => dest.ProductPrice, opt => opt.MapFrom(src => src.Product.Price));

            //UserFeature Mappings
            CreateMap<UserFeature, UserFeatureResponse>()
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User.Username))
                .ForMember(dest => dest.ShopId, opt => opt.MapFrom(src => src.User.ShopId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.User.Role));
            CreateMap<UserFeatureRequest, UserFeature>().ReverseMap();
            CreateMap<UserFeatureGetRequest, UserFeature>().ReverseMap();

            // ShopSubscription Mappings
            CreateMap<ShopSubscription, ShopSubscriptionResponse>()
                .ForMember(dest => dest.shopSubscriptionId, opt => opt.MapFrom(src => src.ShopSubscriptionId))
                .ForMember(dest => dest.shopId, opt => opt.MapFrom(src => src.ShopId))
                .ForMember(dest => dest.platformProductId, opt => opt.MapFrom(src => src.PlatformProductId))
                .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.createdAt, opt => opt.MapFrom(src => src.CreatedAt));
            CreateMap<ShopSubscriptionRequest, ShopSubscription>()
                .ForMember(dest => dest.ShopId, opt => opt.MapFrom(src => src.shopId))
                .ForMember(dest => dest.PlatformProductId, opt => opt.MapFrom(src => src.platformProductId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.startDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.endDate))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status));
            CreateMap<ShopSubscriptionGetRequest, ShopSubscription>()
                .ForMember(dest => dest.ShopSubscriptionId, opt => opt.MapFrom(src => src.shopSubscriptionId ?? 0))
                .ForMember(dest => dest.ShopId, opt => opt.MapFrom(src => src.shopId))
                .ForMember(dest => dest.PlatformProductId, opt => opt.MapFrom(src => src.platformProductId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.startDate ?? default(DateTime)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.endDate ?? default(DateTime)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.status));
        }
    }
}
