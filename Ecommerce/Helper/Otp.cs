namespace Ecommerce.Helper
{
    public class Otp
    {
        //Dictionary lưu Otp tạm thời trong memory
        //Key: email, Value: (otp, expiry, type)
        private static Dictionary<string, (string otp, DateTime expiry, string type)> _otpStorage = new();

        //Tạo mã Otp có 6 số ngẫu nhiên
        public static string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        //Lưu Otp cho email
        public static void SaveOtp(string email, string type, int expiryMinutes = 10)
        {
            string otp = GenerateOtp();
            DateTime expiry  = DateTime.Now.AddMinutes(expiryMinutes);

            //Xoá Otp cũ của email này nếu có 
            if (_otpStorage.ContainsKey($"{email}_{type}"))
            {
                _otpStorage.Remove($"{email}_{type}");
            }

            //Lưu Otp mới
            _otpStorage.Add($"{email}_{type}", (otp, expiry, type));

            //Dọn Otp hết hạn
            CleanExpiredOtp();
        }

        //Lấy Otp đã lưu
        public static string GetOtp(string email, string type)
        {
            string key = $"{email}_{type}";
            if(_otpStorage.ContainsKey(key) )
            {
                return _otpStorage[key].otp;
            }
            return null;
        }

        //Kiểm tra Otp có hợp lệ hay không
        public static bool ValidateOtp(string email, string type, string inputOtp)
        {
            string key = $"{email}_{type}";
            if(!_otpStorage.ContainsKey(key) ) {
                return false; //Không tìm thấy Otp
            }
            
            var otpData = _otpStorage[key];

            //Kiểm tra hết hạn
            if (DateTime.Now > otpData.expiry)
            {
                _otpStorage.Remove(key);
                return false; //Otp đã  hết hạn
            }

            //Kiểm tra mã Otp
            if(otpData.otp != inputOtp)
            {
                return false; //Otp không đúng
            }

            //Otp hợp lệ -> Xoá không dùng lại
            _otpStorage.Remove(key);
            return true;
        }

        //Xoá Otp
        public static void RemoveOtp(string email, string type)
        {
            string key = $"{email}_{type}";
            if(_otpStorage.ContainsKey(key))
            {
                _otpStorage.Remove(key);
            }
        }

        //Dọn dẹp Otp hết hạn
        private static void CleanExpiredOtp()
        {
            var expiryKeys = _otpStorage.Where(c=> DateTime.Now > c.Value.expiry).Select(x=>x.Key).ToList();
            
            foreach(var key in expiryKeys)
            {
                _otpStorage.Remove(key);
            }
        }

        //Kiểm tra xem còn bao nhiêu thời gian là hết hạn
        public static int GetRemainingMinutes(string email, string type)
        {
            string key = $"{email}_{type}";
            if(_otpStorage.ContainsKey(key))
            {
                var remaining = (_otpStorage[key].expiry - DateTime.Now).TotalMinutes;
                return (int)Math.Ceiling(remaining);
            }
            return 0;
            
        }
    }
}
