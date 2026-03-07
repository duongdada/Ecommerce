using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace E_Commerce.Helper
{
    public class ZaloPayPayment
    {
        // ⭐ ZALOPAY SANDBOX CREDENTIALS (Test - Miễn phí)
        private static string appId = "2553";
        private static string key1 = "PcY4iZIKFCIdgZvA6ueMcMHHUbRLYjPL";
        private static string key2 = "kLtgPl8HHhfvMuDHPwKfgfsY4Ydm9eIz";
        private static string endpoint = "https://sb-openapi.zalopay.vn/v2/create";
        private static string callbackUrl = "https://ruehn.store/Payment/ZaloPayCallback";

        public static string CreatePaymentUrl(string orderCode, double amount, string description)
        {
            var embedData = new { redirecturl = "https://ruehn.store/Payment/ZaloPayReturn" };
            var items = new[] { new { } };
            var appTransId = DateTime.Now.ToString("yyMMdd") + "_" + orderCode;

            var param = new Dictionary<string, string>
            {
                { "app_id", appId },
                { "app_user", "user123" },
                { "app_time", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString() },
                { "amount", amount.ToString("0") },
                { "app_trans_id", appTransId },
                { "embed_data", JsonConvert.SerializeObject(embedData) },
                { "item", JsonConvert.SerializeObject(items) },
                { "description", description },
                { "bank_code", "" },
                { "callback_url", callbackUrl }
            };

            var data = appId + "|" + param["app_trans_id"] + "|" + param["app_user"] + "|" + param["amount"]
                + "|" + param["app_time"] + "|" + param["embed_data"] + "|" + param["item"];
            param.Add("mac", HmacSHA256(data, key1));

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(param);
                    var response = client.PostAsync(endpoint, content).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    dynamic result = JsonConvert.DeserializeObject(responseString);

                    if (result.return_code == 1)
                    {
                        return result.order_url; // URL để redirect user đến trang thanh toán ZaloPay
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        // Verify callback từ ZaloPay
        public static bool VerifyCallback(string data, string mac)
        {
            string computedMac = HmacSHA256(data, key2);
            return computedMac == mac;
        }

        // HMAC SHA256
        private static string HmacSHA256(string inputData, string key)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
            }
        }
    }
}