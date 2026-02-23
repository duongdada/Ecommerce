using Microsoft.AspNetCore.Mvc.Filters;

namespace E_Commerce.Areas.Admin.Attributes
{
    public class CheckLogin :ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            /*Kiểm tra xem session admin_user_email đã tồn tại hay chưa, nếu đã tồn tại thì có nghĩ email đã login
             thành công, nếu chưa tồn tại thì di chuyển đến trang login user để đăng nhập*/
            if (String.IsNullOrEmpty(context.HttpContext.Session.GetString("admin_user_email")))
                context.HttpContext.Response.Redirect("/Admin/Account/Login");
            base.OnActionExecuting(context);
        }
    }
}
