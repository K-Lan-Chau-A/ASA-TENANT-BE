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
        }
    }
}
