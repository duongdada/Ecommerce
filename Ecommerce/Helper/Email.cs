using System.Net;
using System.Net.Mail;
using E_Commerce.Models;

namespace E_Commerce.Helper
{
    public class Email
    {
        // Cấu hình Gmail SMTP
        private static string SmtpHost = "smtp.gmail.com";
        private static int SmtpPort = 587;
        private static string SenderEmail = "anhlavodoivt123@gmail.com";
        private static string SenderPassword = "yoij ldyz xefw hvar";
        private static string SenderName = "Elegance Shop";

        // Gửi email
        public static async Task<bool> SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(SmtpHost, SmtpPort))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(SenderEmail, SenderPassword);

                    var mail = new MailMessage
                    {
                        From = new MailAddress(SenderEmail, SenderName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mail.To.Add(toEmail);
                    await client.SendMailAsync(mail);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                return false;
            }
        }

        // Template email OTP đăng ký
        public static string RegisterOtpTemplate(string name, string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 12px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .logo {{ font-size: 32px; font-weight: bold; text-align: center; margin-bottom: 30px; letter-spacing: 3px; color: #000; }}
        .content {{ color: #333; line-height: 1.8; }}
        .otp-box {{ background: #f8f9fa; border: 2px dashed #000; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
        .otp-code {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #000; font-family: monospace; }}
        .note {{ background: #fff9e6; padding: 15px; border-left: 4px solid #ffc107; margin: 20px 0; font-size: 14px; color: #856404; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #999; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo'>ELEGANCE</div>
        <div class='content'>
            <h2 style='color: #000; margin-bottom: 20px;'>Xin chào {name}!</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại <strong>Elegance Shop</strong>.</p>
            <p>Để hoàn tất đăng ký, vui lòng nhập mã OTP bên dưới:</p>
            <div class='otp-box'>
                <div style='font-size: 14px; color: #666; margin-bottom: 10px;'>MÃ XÁC THỰC CỦA BẠN</div>
                <div class='otp-code'>{otp}</div>
            </div>
            <div class='note'>
                <strong>⚠ Lưu ý:</strong> Mã OTP có hiệu lực trong <strong>10 phút</strong>. Không chia sẻ mã này với bất kỳ ai!
            </div>
        </div>
        <div class='footer'>
            <p>Nếu bạn không yêu cầu đăng ký, vui lòng bỏ qua email này.</p>
            <p>&copy; 2026 Elegance Shop. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        // Template email OTP đăng nhập
        public static string LoginOtpTemplate(string name, string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 12px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .logo {{ font-size: 32px; font-weight: bold; text-align: center; margin-bottom: 30px; letter-spacing: 3px; color: #000; }}
        .content {{ color: #333; line-height: 1.8; }}
        .otp-box {{ background: #f8f9fa; border: 2px dashed #2c3e50; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
        .otp-code {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #2c3e50; font-family: monospace; }}
        .note {{ background: #e8f4fd; padding: 15px; border-left: 4px solid #2c3e50; margin: 20px 0; font-size: 14px; color: #1a5490; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #999; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo'>ELEGANCE</div>
        <div class='content'>
            <h2 style='color: #000; margin-bottom: 20px;'>Xin chào {name}!</h2>
            <p>Chúng tôi nhận được yêu cầu đăng nhập vào tài khoản của bạn.</p>
            <p>Vui lòng nhập mã OTP bên dưới để hoàn tất đăng nhập:</p>
            <div class='otp-box'>
                <div style='font-size: 14px; color: #666; margin-bottom: 10px;'>MÃ ĐĂNG NHẬP</div>
                <div class='otp-code'>{otp}</div>
            </div>
            <div class='note'>
                <strong>⚠ Lưu ý:</strong> Mã OTP có hiệu lực trong <strong>5 phút</strong>. Không chia sẻ mã này với bất kỳ ai!
            </div>
        </div>
        <div class='footer'>
            <p>Nếu bạn không thực hiện yêu cầu này, vui lòng bỏ qua email hoặc liên hệ với chúng tôi ngay.</p>
            <p>&copy; 2026 Elegance Shop. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        // Template email OTP reset password
        public static string ResetPasswordOtpTemplate(string name, string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background: white; padding: 40px; border-radius: 12px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .logo {{ font-size: 32px; font-weight: bold; text-align: center; margin-bottom: 30px; letter-spacing: 3px; color: #000; }}
        .content {{ color: #333; line-height: 1.8; }}
        .otp-box {{ background: #fff5f5; border: 2px dashed #e74c3c; border-radius: 8px; padding: 20px; text-align: center; margin: 30px 0; }}
        .otp-code {{ font-size: 36px; font-weight: bold; letter-spacing: 8px; color: #e74c3c; font-family: monospace; }}
        .note {{ background: #ffe6e6; padding: 15px; border-left: 4px solid #e74c3c; margin: 20px 0; font-size: 14px; color: #721c24; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #999; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo'>ELEGANCE</div>
        <div class='content'>
            <h2 style='color: #000; margin-bottom: 20px;'>Xin chào {name}!</h2>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p>Vui lòng nhập mã OTP bên dưới để tiếp tục:</p>
            <div class='otp-box'>
                <div style='font-size: 14px; color: #666; margin-bottom: 10px;'>MÃ ĐẶT LẠI MẬT KHẨU</div>
                <div class='otp-code'>{otp}</div>
            </div>
            <div class='note'>
                <strong>⚠ Lưu ý:</strong> Mã OTP có hiệu lực trong <strong>10 phút</strong>. Không chia sẻ mã này với bất kỳ ai!
            </div>
        </div>
        <div class='footer'>
            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này và tài khoản của bạn vẫn an toàn.</p>
            <p>&copy; 2026 Elegance Shop. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }

        // ⭐ THÊM MỚI: Template email xác nhận đơn hàng
        public static string OrderConfirmationTemplate(ItemOrder order, List<ItemOrderDetail> orderDetails)
        {
            // Tạo danh sách sản phẩm
            string productRows = "";
            foreach (var item in orderDetails)
            {
                productRows += $@"
                <tr>
                    <td style='padding: 15px; border-bottom: 1px solid #eee;'>{item.ProductName}</td>
                    <td style='padding: 15px; border-bottom: 1px solid #eee; text-align: center;'>{item.Quantity}</td>
                    <td style='padding: 15px; border-bottom: 1px solid #eee; text-align: right;'>{item.Price:N0}₫</td>
                    <td style='padding: 15px; border-bottom: 1px solid #eee; text-align: right; font-weight: bold;'>{(item.Price * item.Quantity):N0}₫</td>
                </tr>";
            }

            string statusText = order.Status switch
            {
                0 => "Chờ xử lý",
                1 => "Đang xử lý",
                2 => "Đang giao hàng",
                3 => "Đã giao hàng",
                4 => "Đã hủy",
                _ => "Không xác định"
            };

            return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
        .container {{ max-width: 700px; margin: 0 auto; background: white; padding: 40px; border-radius: 12px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }}
        .logo {{ font-size: 32px; font-weight: bold; text-align: center; margin-bottom: 10px; letter-spacing: 3px; color: #000; }}
        .header {{ text-align: center; padding: 20px 0; border-bottom: 2px solid #000; }}
        .order-code {{ font-size: 24px; color: #27ae60; font-weight: bold; margin: 10px 0; }}
        .content {{ color: #333; line-height: 1.8; margin: 30px 0; }}
        .info-box {{ background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0; }}
        .info-row {{ display: flex; justify-content: space-between; padding: 8px 0; border-bottom: 1px dashed #ddd; }}
        .info-label {{ font-weight: bold; color: #666; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th {{ background: #2c3e50; color: white; padding: 15px; text-align: left; }}
        .total-box {{ background: #fff9e6; padding: 20px; border-radius: 8px; margin: 20px 0; text-align: right; }}
        .total-amount {{ font-size: 28px; color: #e74c3c; font-weight: bold; }}
        .track-button {{ display: inline-block; background: #27ae60; color: white; padding: 15px 40px; text-decoration: none; border-radius: 8px; margin: 20px 0; font-weight: bold; }}
        .footer {{ margin-top: 30px; padding-top: 20px; border-top: 1px solid #ddd; font-size: 12px; color: #999; text-align: center; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>ELEGANCE</div>
            <p style='color: #27ae60; font-size: 18px; margin: 10px 0;'>✓ Đặt hàng thành công!</p>
            <div class='order-code'>#{order.OrderCode}</div>
        </div>

        <div class='content'>
            <h2 style='color: #000;'>Xin chào {order.Name}!</h2>
            <p>Cảm ơn bạn đã đặt hàng tại <strong>Elegance Shop</strong>. Đơn hàng của bạn đã được tiếp nhận và đang chờ xử lý.</p>
        </div>

        <div class='info-box'>
            <h3 style='margin-top: 0; color: #2c3e50;'>Thông tin đơn hàng</h3>
            <div class='info-row'>
                <span class='info-label'>Mã đơn hàng:</span>
                <span><strong>{order.OrderCode}</strong></span>
            </div>
            <div class='info-row'>
                <span class='info-label'>Ngày đặt:</span>
                <span>{order.Create:dd/MM/yyyy HH:mm}</span>
            </div>
            <div class='info-row'>
                <span class='info-label'>Trạng thái:</span>
                <span><strong style='color: #f39c12;'>{statusText}</strong></span>
            </div>
            <div class='info-row'>
                <span class='info-label'>Người nhận:</span>
                <span>{order.Name}</span>
            </div>
            <div class='info-row'>
                <span class='info-label'>Số điện thoại:</span>
                <span>{order.Phone}</span>
            </div>
            <div class='info-row' style='border-bottom: none;'>
                <span class='info-label'>Địa chỉ:</span>
                <span>{order.Address}</span>
            </div>
        </div>

        <h3 style='color: #2c3e50;'>Chi tiết sản phẩm</h3>
        <table>
            <thead>
                <tr>
                    <th>Sản phẩm</th>
                    <th style='text-align: center; width: 100px;'>Số lượng</th>
                    <th style='text-align: right; width: 120px;'>Đơn giá</th>
                    <th style='text-align: right; width: 120px;'>Thành tiền</th>
                </tr>
            </thead>
            <tbody>
                {productRows}
            </tbody>
        </table>

        <div class='total-box'>
            <p style='margin: 0 0 10px 0; font-size: 16px; color: #666;'>Tổng cộng:</p>
            <div class='total-amount'>{order.Price:N0}₫</div>
        </div>

        <div style='text-align: center; margin: 30px 0;'>
            <p style='color: #666; margin-bottom: 15px;'>Bạn có thể tra cứu đơn hàng bằng:</p>
            <p style='background: #f8f9fa; padding: 15px; border-radius: 8px; font-size: 14px;'>
                <strong>Email:</strong> {order.Email}<br>
                <strong>Mã đơn:</strong> {order.OrderCode}
            </p>
        </div>

        <div class='footer'>
            <p>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ với chúng tôi.</p>
            <p>&copy; 2026 Elegance Shop. All rights reserved.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}