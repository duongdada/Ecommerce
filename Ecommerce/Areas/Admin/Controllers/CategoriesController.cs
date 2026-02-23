using E_Commerce.Areas.Admin.Attributes;
using E_Commerce.Areas.Admin.Helper;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using X.PagedList;

namespace E_Commerce.Areas.Admin.Controllers
{
    [Area("Admin")]
    [CheckLogin]
    public class CategoriesController : Controller
    {
        public MyDbContext db = new MyDbContext();

        public IActionResult Index()
        {
            return RedirectToAction("Read");
        }

        public IActionResult Read(int? page)
        {
            string strConnectionString = MyClass.GetConnectionString();
            DataTable dtCategories = new DataTable();
            List<RowCategory> listCategories = new List<RowCategory>();

            using (SqlConnection conn = new SqlConnection(strConnectionString))
            {
                // Chỉ lấy danh mục cấp 1 (ParentId = 0)
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Categories WHERE ParentId = 0 ORDER BY Id DESC", conn);
                da.Fill(dtCategories);

                if (dtCategories.Rows.Count > 0)
                {
                    foreach (DataRow row in dtCategories.Rows)
                    {
                        listCategories.Add(new RowCategory()
                        {
                            Id = Convert.ToInt32(row["Id"]),
                            ParentId = Convert.ToInt32(row["ParentId"]),
                            Name = row["Name"].ToString(),
                            DisplayHomePage = Convert.ToInt32(row["DisplayHomePage"])
                        });
                    }
                }
            }

            // Phân trang
            int page_number = page ?? 1;
            int page_size = 10;

            return View("Read", listCategories.ToPagedList(page_number, page_size));
        }

        public IActionResult Update(int id)
        {
            RowCategory rowCategory = db.Categories.FirstOrDefault(c => c.Id == id);
            ViewBag.formAction = "/Admin/Categories/UpdatePost/" + id;
            return View("CreateUpdate", rowCategory);
        }

        [HttpPost]
        public IActionResult UpdatePost(int id, IFormCollection fc)
        {
            string _Name = fc["Name"];
            int _DisplayHomePage = !String.IsNullOrEmpty(fc["DisplayHomePage"]) ? 1 : 0;
            int _ParentId = Convert.ToInt32(fc["ParentId"]);

            string strConnectionString = MyClass.GetConnectionString();
            using (SqlConnection conn = new SqlConnection(strConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "UPDATE Categories SET Name=@var_name, ParentId=@var_parent_id, DisplayHomePage=@var_display WHERE Id=@var_id",
                    conn);

                cmd.Parameters.AddWithValue("@var_display", _DisplayHomePage);
                cmd.Parameters.AddWithValue("@var_name", _Name);
                cmd.Parameters.AddWithValue("@var_parent_id", _ParentId);
                cmd.Parameters.AddWithValue("@var_id", id);
                cmd.ExecuteNonQuery();
            }

            return Redirect("/Admin/Categories");
        }

        public IActionResult Create()
        {
            ViewBag.formAction = "/Admin/Categories/CreatePost";
            return View("CreateUpdate");
        }

        [HttpPost]
        public IActionResult CreatePost(IFormCollection fc)
        {
            string _Name = fc["Name"];
            int _DisplayHomePage = !String.IsNullOrEmpty(fc["DisplayHomePage"]) ? 1 : 0;
            int _ParentId = Convert.ToInt32(fc["ParentId"]);

            string strConnectionString = MyClass.GetConnectionString();
            using (SqlConnection conn = new SqlConnection(strConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Categories(Name, ParentId, DisplayHomePage) VALUES(@var_name, @var_parent_id, @var_display_home_page)",
                    conn);

                cmd.Parameters.AddWithValue("@var_display_home_page", _DisplayHomePage);
                cmd.Parameters.AddWithValue("@var_name", _Name);
                cmd.Parameters.AddWithValue("@var_parent_id", _ParentId);
                cmd.ExecuteNonQuery();
            }

            return Redirect("/Admin/Categories");
        }

        public IActionResult Delete(int id)
        {
            string strConnectionString = MyClass.GetConnectionString();
            using (SqlConnection conn = new SqlConnection(strConnectionString))
            {
                conn.Open();

                // Xóa tất cả các danh mục con và cháu
                // Cấp 3: Xóa các danh mục cháu (có ParentId là con của id)
                SqlCommand cmdDeleteGrandChildren = new SqlCommand(
                    "DELETE FROM Categories WHERE ParentId IN (SELECT Id FROM Categories WHERE ParentId = @var_id)",
                    conn);
                cmdDeleteGrandChildren.Parameters.AddWithValue("@var_id", id);
                cmdDeleteGrandChildren.ExecuteNonQuery();

                // Cấp 2: Xóa các danh mục con trực tiếp
                SqlCommand cmdDeleteChildren = new SqlCommand(
                    "DELETE FROM Categories WHERE ParentId = @var_id",
                    conn);
                cmdDeleteChildren.Parameters.AddWithValue("@var_id", id);
                cmdDeleteChildren.ExecuteNonQuery();

                // Cấp 1: Xóa danh mục chính
                SqlCommand cmdDelete = new SqlCommand(
                    "DELETE FROM Categories WHERE Id = @var_id",
                    conn);
                cmdDelete.Parameters.AddWithValue("@var_id", id);
                cmdDelete.ExecuteNonQuery();
            }

            return Redirect("/Admin/Categories");
        }
    }
}