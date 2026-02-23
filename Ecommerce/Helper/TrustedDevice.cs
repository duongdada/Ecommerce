namespace E_Commerce.Helper
{
    public class TrustedDevice
    {
        private const string COOKIE_NAME = "elegance_trusted_device";
        private const int TRUST_DAYS = 30; // Tin cậy thiết bị trong 30 ngày

        // Tạo token tin cậy cho thiết bị
        public static string GenerateTrustToken(string email)
        {
            // Tạo token unique dựa trên email + timestamp
            string timestamp = DateTime.Now.Ticks.ToString();
            string raw = $"{email}_{timestamp}_{Guid.NewGuid()}";
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(raw));
        }

        // Lưu token vào Cookie
        public static void SaveTrustedDevice(HttpContext context, string email, bool rememberMe)
        {
            if (!rememberMe) return; // Chỉ lưu khi user check "Ghi nhớ đăng nhập"

            string token = GenerateTrustToken(email);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true, // Bảo mật: JS không đọc được
                Secure = true,   // Chỉ gửi qua HTTPS
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(TRUST_DAYS)
            };

            // Lưu: email|token|expiry
            string cookieValue = $"{email}|{token}|{DateTime.Now.AddDays(TRUST_DAYS).Ticks}";
            context.Response.Cookies.Append(COOKIE_NAME, cookieValue, cookieOptions);
        }

        // Kiểm tra thiết bị có được tin cậy không
        public static bool IsTrustedDevice(HttpContext context, string email)
        {
            if (!context.Request.Cookies.ContainsKey(COOKIE_NAME))
            {
                return false; // Không có cookie
            }

            string cookieValue = context.Request.Cookies[COOKIE_NAME];

            try
            {
                var parts = cookieValue.Split('|');
                if (parts.Length != 3) return false;

                string savedEmail = parts[0];
                string token = parts[1];
                long expiryTicks = long.Parse(parts[2]);
                DateTime expiry = new DateTime(expiryTicks);

                // Kiểm tra email khớp và chưa hết hạn
                if (savedEmail == email && DateTime.Now < expiry)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        // Xóa cookie khi đăng xuất
        public static void RemoveTrustedDevice(HttpContext context)
        {
            context.Response.Cookies.Delete(COOKIE_NAME);
        }
    }
}