using AutoMapper;
using BLL.ViewModels.Product;
using BLL.ViewModels.ProductCategory;
using BLL.ViewModels.Supplier;
using DAL.Models;
namespace BLL.Mapper
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {    
            // Create
            CreateMap<CreateVM, Supplier>();
            // Edit
            CreateMap<Supplier, EditVM>();
            CreateMap<EditVM, Supplier>();


            // Create
            CreateMap<CreateVM, Customer>();
            // Edit
            CreateMap<Customer, EditVM>();
            CreateMap<EditVM, Customer>();
            
            // Create
            CreateMap<CreateProductCategoryVM, ProductCategory>();
            // Edit
            CreateMap<ProductCategory, EditProductCategoryVM>();
            CreateMap<EditProductCategoryVM, ProductCategory>(); 
            
            // Create
            CreateMap<CreateProductVM, Product>();
            // Edit
            CreateMap<Product, EditProductVM>();
            CreateMap<EditProductVM, Product>();    
            
            // Create
            CreateMap<CreateStoreVM, Store>();
            // Edit
            CreateMap<Store, EditStoreVM>();
            CreateMap<EditStoreVM, Store>();

        }
    }
}
