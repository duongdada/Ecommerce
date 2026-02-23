using Microsoft.EntityFrameworkCore;
using E_Commerce.Models;
namespace E_Commerce.Models
{
    public class MyDbContext:DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //Lấy chuỗi kết nối từ appsettings.json (mở file json ra và đọc dữ liệu trong đó
            var config = new ConfigurationBuilder().SetBasePath
                (Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            //Lấy tag MyConnectionString trong file appsettings.json
            var strConnectionString = config.GetConnectionString("MyConnectionString");
            //Kết nối CSDL đến SQL server
            optionsBuilder.UseSqlServer(strConnectionString);
        }
        public DbSet<RowUser> Users { get; set; }
        public DbSet<RowCategory> Categories { get; set; }
        public DbSet<ItemAdv> Adv { get; set; }
        public DbSet<ItemNews> News { get; set; }
        public DbSet<ItemCustomer> Customers { get; set; }
        public DbSet<ItemOrder> Orders { get; set; }
        public DbSet<ItemOrderDetail> OrdersDetail { get; set; }
        public DbSet<ItemProduct> Products { get; set; }
        public DbSet<ItemRating> Rating { get; set; }
        public DbSet<ItemSlide> Slides { get; set; }
        public DbSet<ItemCategoryProduct> CategoriesProducts { get; set; }

    }
}
