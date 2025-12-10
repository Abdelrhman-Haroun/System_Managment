using AutoMapper;
using BLL.ViewModels.Customer;
using BLL.ViewModels.Product;
using BLL.ViewModels.ProductCategory;
using BLL.ViewModels.Store;
using BLL.ViewModels.Supplier;
using DAL.Models;
namespace BLL.Mapper
{
    public class DomainProfile : Profile
    {
        public DomainProfile()
        {    
            // Create
            CreateMap<CreateCustomerVM, Customer>();
            // Edit
            CreateMap<Customer, EditCustomerVM>();
            CreateMap<EditCustomerVM, Customer>();

            // Create
            CreateMap<CreateSupplierVM, Supplier>();
            // Edit
            CreateMap<Supplier, EditSupplierVM>();
            CreateMap<EditSupplierVM, Supplier>();
            
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
