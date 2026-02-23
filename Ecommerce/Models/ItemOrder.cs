using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models
{
    [Table("Orders")]
    public class ItemOrder
    {
        [Key]
        public int Id { get; set; }
        public int? CustomerId { get; set; } // Nullable cho khách vãng lai
        public string? OrderCode { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime Create { get; set; }
        public double Price { get; set; }
        public int Status { get; set; } // 0: Chờ xử lý, 1: Đang xử lý, 2: Đang giao, 3: Đã giao, 4: Đã hủy

        //Payment
        public string? PaymentMethod { get; set; } // COD, MoMo, ZaloPay
        public int? PaymentStatus { get; set; } // 0: Chưa thanh toán, 1: Đã thanh toán
        public string? TransactionId { get; set; } // Mã giao dịch từ MoMo/ZaloPay
    }
}