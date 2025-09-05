using ASA_TENANT_REPO.Models;
using ASA_TENANT_SERVICE.DTOs.Request;
using ASA_TENANT_SERVICE.DTOs.Response;
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
            CreateMap<Customer, CustomerResponse>().ReverseMap();
            CreateMap<CustomerRequest, Customer>().ReverseMap();
            CreateMap<CustomerGetRequest, Customer>().ReverseMap();

            //Product Mappings
            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.CategoryName))
                .ReverseMap();
            CreateMap<ProductRequest, Product>().ReverseMap();
            CreateMap<ProductGetRequest, Product>().ReverseMap();

            // User Mappings
            CreateMap<User, UserResponse>().ReverseMap();
            CreateMap<UserRequest, User>().ReverseMap();
            CreateMap<UserGetRequest, User>().ReverseMap();
            CreateMap<LoginResponse, User>().ReverseMap();

            // Shop Mappings
            CreateMap<Shop, ShopResponse>().ReverseMap();
            CreateMap<ShopRequest, Shop>().ReverseMap();
            CreateMap<ShopGetRequest, Shop>().ReverseMap();

            // InventoryTransaction Mappings
            CreateMap<InventoryTransaction, InventoryTransactionResponse>().ReverseMap();
            CreateMap<InventoryTransactionRequest, InventoryTransaction>().ReverseMap();
            CreateMap<InventoryTransactionGetRequest, InventoryTransaction>().ReverseMap();

            // Promotion Mappings
            CreateMap<Promotion, PromotionResponse>().ReverseMap();
            CreateMap<PromotionRequest, Promotion>().ReverseMap();
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
            CreateMap<Order, OrderResponse>().ReverseMap();
            CreateMap<OrderRequest, Order>().ReverseMap();
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
        }
    }
}
