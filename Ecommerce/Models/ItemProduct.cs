using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models
{
    [Table("Products")]
    public class ItemProduct
    {
        [Key]
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Content { get; set; }
        public int Hot { get; set; }
        public string? Photo { get; set; }
        public double Price { get; set; }
        public double Discount { get; set; }

        public string? AvailableSizes { get; set; }
        public string? AvailableColors { get; set; }

        // Helper methods để chuyển string thành List
        [NotMapped]
        public List<string> SizesList
        {
            get
            {
                if (string.IsNullOrEmpty(AvailableSizes))
                    return new List<string>();
                return AvailableSizes.Split(',').Select(s => s.Trim()).ToList();
            }
        }

        [NotMapped]
        public List<string> ColorsList
        {
            get
            {
                if (string.IsNullOrEmpty(AvailableColors))
                    return new List<string>();
                return AvailableColors.Split(',').Select(c => c.Trim()).ToList();
            }
        }
    }
}