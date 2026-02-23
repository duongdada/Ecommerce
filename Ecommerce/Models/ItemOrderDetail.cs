using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models
{
    [Table("OrdersDetail")]
    public class ItemOrderDetail
    {
        [Key]
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }

        public string? SelectedSize { get; set; }
        public string? SelectedColor { get; set; }
    }
}