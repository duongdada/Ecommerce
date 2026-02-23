using E_Commerce.Models;

namespace E_Commerce.Helper
{
    public class Adv
    {
        public static MyDbContext db = new MyDbContext();
        public static List<ItemAdv> GetAdv(int _position)
        {
            List<ItemAdv> adv = db.Adv.Where(c=>c.Position == _position).OrderByDescending(x=>x.Id).ToList();
            return adv != null ? adv : new List<ItemAdv>();
        }
    }
}
