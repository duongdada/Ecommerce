using Microsoft.Extensions.Options;

namespace E_Commerce.Areas.Admin.Helper
{
    public class MyClass
    {
        //trả về chuỗi ConnectionString
        public static string GetConnectionString()
        {
            //tạo đối tượng để kết nối được vào file appsettings.json
            var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
            //lấy tag MyConnectionString trong file appsettings.json
            var strConnectionString = config.GetConnectionString("MyConnectionString");
            return strConnectionString;
        }
    }
}
