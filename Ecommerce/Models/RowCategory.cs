using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models
{
    [Table("Categories")]
    public class RowCategory
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int? DisplayHomePage { get; set; }
    }
}
