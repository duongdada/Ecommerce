using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace E_Commerce.Helper
{
    public class MoMoPayment
    {
        // ⭐ MOMO SANDBOX CREDENTIALS (Test - Miễn phí)
        private static string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
        private static string partnerCode = "MOMOBKUN20180529";
        private static string accessKey = "klm05TvNBzhg7h7j";
        private static string secretKey = "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
        private static string returnUrl = "https://localhost:7166/Payment/MoMoReturn";
        private static string notifyUrl = "https://localhost:7166/Payment/MoMoNotify";

        // ⭐ THÊM METHOD ĐỂ LẤY ACCESS KEY (dùng cho verify signature)
        public static string GetAccessKey()
        {
            return accessKey;
        }

        public static string CreatePaymentUrl(string orderCode, double amount, string orderInfo)
        {
            string requestId = Guid.NewGuid().ToString();
            string orderId = orderCode;
            string requestType = "captureWallet";
            string extraData = "";

            // Tạo raw signature
            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={returnUrl}&requestId={requestId}&requestType={requestType}";
            string signature = HmacSHA256(rawHash, secretKey);

            // Tạo request data
            var requestData = new
            {
                partnerCode = partnerCode,
                partnerName = "Elegance Shop",
                storeId = "EleganceStore",
                requestId = requestId,
                amount = amount,
                orderId = orderId,
                orderInfo = orderInfo,
                redirectUrl = returnUrl,
                ipnUrl = notifyUrl,
                lang = "vi",
                extraData = extraData,
                requestType = requestType,
                signature = signature
            };

            // Gửi request đến MoMo
            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
                    var response = client.PostAsync(endpoint, content).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    dynamic result = JsonConvert.DeserializeObject(responseString);

                    if (result.resultCode == 0)
                    {
                        return result.payUrl; // URL để redirect user đến trang thanh toán MoMo
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

        // Verify signature khi MoMo callback
        public static bool VerifySignature(string rawData, string signature)
        {
            string computedSignature = HmacSHA256(rawData, secretKey);
            return computedSignature == signature;
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