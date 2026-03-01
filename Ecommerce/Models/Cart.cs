using E_Commerce.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace E_Commerce.Models
{
    public class Cart
    {
        protected static readonly MyDbContext db = new MyDbContext();

        public static T GetObjectFromJson<T>(ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }

        public static List<Item> GetCart(ISession session)
        {
            List<Item> cart = Cart.GetObjectFromJson<List<Item>>(session, "cart");
            return cart;
        }

        // SỬA: Thêm tham số size và color
        public static void CartAdd(ISession session, int id, string size = null, string color = null)
        {
            if (Cart.GetObjectFromJson<List<Item>>(session, "cart") == null)
            {
                List<Item> cart = new List<Item>();
                ItemProduct item = db.Products.Where(tbl => tbl.Id == id).FirstOrDefault();
                cart.Add(new Item
                {
                    ProductRecord = item,
                    Quantity = 1,
                    SelectedSize = size,
                    SelectedColor = color
                });
                session.SetString("cart", JsonConvert.SerializeObject(cart));
            }
            else
            {
                List<Item> cart = Cart.GetObjectFromJson<List<Item>>(session, "cart");
                int index = Cart.isExist(session, id, size, color);
                if (index != -1)
                {
                    cart[index].Quantity++;
                }
                else
                {
                    ItemProduct item = db.Products.Where(tbl => tbl.Id == id).FirstOrDefault();
                    cart.Add(new Item
                    {
                        ProductRecord = item,
                        Quantity = 1,
                        SelectedSize = size,
                        SelectedColor = color
                    });
                }
                session.SetString("cart", JsonConvert.SerializeObject(cart));
            }
        }

        public static void CartRemove(ISession session, int id)
        {
            List<Item> cart = Cart.GetObjectFromJson<List<Item>>(session, "cart");
            int index = isExist(session, id);
            cart.RemoveAt(index);
            session.SetString("cart", JsonConvert.SerializeObject(cart));
        }

        public static void CartDestroy(ISession session)
        {
            List<Item> cart = new List<Item>();
            session.SetString("cart", JsonConvert.SerializeObject(cart));
        }

        public static void CartUpdate(ISession session, int id, int quantity)
        {
            List<Item> cart = Cart.GetObjectFromJson<List<Item>>(session, "cart");
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].ProductRecord.Id == id)
                {
                    cart[i].Quantity = quantity;
                }
            }
            session.SetString("cart", JsonConvert.SerializeObject(cart));
        }

        // THÊM MỚI: isExist với size và color
        private static int isExist(ISession session, int id, string size = null, string color = null)
        {
            List<Item> cart = Cart.GetObjectFromJson<List<Item>>(session, "cart");
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].ProductRecord.Id == id &&
                    cart[i].SelectedSize == size &&
                    cart[i].SelectedColor == color)
                {
                    return i;
                }
            }
            return -1;
        }

        // GIỮ LẠI: isExist cũ để không bị lỗi
        private static int isExist(ISession session, int id)
        {
            List<Item> cart = Cart.GetObjectFromJson<List<Item>>(session, "cart");
            for (int i = 0; i < cart.Count; i++)
            {
                if (cart[i].ProductRecord.Id == id)
                {
                    return i;
                }
            }
            return -1;
        }

        public static double CartTotal(ISession session)
        {
            List<Item> items_cart = Cart.GetCart(session);
            if (items_cart != null)
            {
                double total = 0;
                foreach (var item in items_cart)
                {
                    total += item.Quantity * (item.ProductRecord.Price - (item.ProductRecord.Price * item.ProductRecord.Discount) / 100);
                }
                return total;
            }
            else
                return 0;
        }

        public static int CartQuantity(ISession session)
        {
            List<Item> items_cart = Cart.GetCart(session);
            if (items_cart != null)
            {
                return items_cart.Count;
            }
            else
                return 0;
        }

        public static void CartCheckOut(ISession session, int customer_id, string paymentMethod = "COD")
        {
            MyDbContext db = new MyDbContext();
            List<Item> _cart = Cart.GetCart(session);

            var customer = db.Customers.FirstOrDefault(c => c.Id == customer_id);
            string orderCode = GenerateOrderCode();

            ItemOrder _RecordOrder = new ItemOrder();
            _RecordOrder.CustomerId = customer_id;
            _RecordOrder.OrderCode = orderCode;
            _RecordOrder.Name = customer?.Name;
            _RecordOrder.Email = customer?.Email;
            _RecordOrder.Phone = customer?.Phone;
            _RecordOrder.Address = customer?.Address;
            _RecordOrder.Create = DateTime.Now;
            _RecordOrder.Price = _cart.Sum(tbl => (tbl.ProductRecord.Price - (tbl.ProductRecord.Price * tbl.ProductRecord.Discount) / 100) * tbl.Quantity);
            _RecordOrder.Status = 0;
            _RecordOrder.PaymentMethod = paymentMethod;
            _RecordOrder.PaymentStatus = paymentMethod == "COD" ? 0 : 0;
            _RecordOrder.TransactionId = null;
            db.Orders.Add(_RecordOrder);
            db.SaveChanges();

            int order_id = _RecordOrder.Id;

            foreach (var item in _cart)
            {
                ItemOrderDetail _RecordOrdersDetail = new ItemOrderDetail();
                _RecordOrdersDetail.OrderId = order_id;
                _RecordOrdersDetail.ProductId = item.ProductRecord.Id;
                _RecordOrdersDetail.ProductName = item.ProductRecord.Name;
                _RecordOrdersDetail.Price = item.ProductRecord.Price - (item.ProductRecord.Price * item.ProductRecord.Discount) / 100;
                _RecordOrdersDetail.Quantity = item.Quantity;
                _RecordOrdersDetail.SelectedSize = item.SelectedSize;
                _RecordOrdersDetail.SelectedColor = item.SelectedColor;
                db.OrdersDetail.Add(_RecordOrdersDetail);
                db.SaveChanges();
            }

            Cart.CartDestroy(session);
        }

        public static string CartCheckOutGuest(ISession session, string name, string email, string phone, string address, string paymentMethod = "COD")
        {
            MyDbContext db = new MyDbContext();
            List<Item> _cart = Cart.GetCart(session);

            if (_cart == null || _cart.Count == 0)
            {
                return null;
            }

            string orderCode = GenerateOrderCode();

            ItemOrder _RecordOrder = new ItemOrder();
            _RecordOrder.CustomerId = null;
            _RecordOrder.OrderCode = orderCode;
            _RecordOrder.Name = name;
            _RecordOrder.Email = email;
            _RecordOrder.Phone = phone;
            _RecordOrder.Address = address;
            _RecordOrder.Create = DateTime.Now;
            _RecordOrder.Price = _cart.Sum(tbl => (tbl.ProductRecord.Price - (tbl.ProductRecord.Price * tbl.ProductRecord.Discount) / 100) * tbl.Quantity);
            _RecordOrder.Status = 0;
            _RecordOrder.PaymentStatus = paymentMethod == "COD" ? 0 : 0;
            _RecordOrder.TransactionId = null;
            db.Orders.Add(_RecordOrder);
            db.SaveChanges();

            int order_id = _RecordOrder.Id;

            foreach (var item in _cart)
            {
                ItemOrderDetail _RecordOrdersDetail = new ItemOrderDetail();
                _RecordOrdersDetail.OrderId = order_id;
                _RecordOrdersDetail.ProductId = item.ProductRecord.Id;
                _RecordOrdersDetail.ProductName = item.ProductRecord.Name;
                _RecordOrdersDetail.Price = item.ProductRecord.Price - (item.ProductRecord.Price * item.ProductRecord.Discount) / 100;
                _RecordOrdersDetail.Quantity = item.Quantity;
                _RecordOrdersDetail.SelectedSize = item.SelectedSize;
                _RecordOrdersDetail.SelectedColor = item.SelectedColor;
                db.OrdersDetail.Add(_RecordOrdersDetail);
                db.SaveChanges();
            }

            Cart.CartDestroy(session);

            return orderCode;
        }

        private static string GenerateOrderCode()
        {
            MyDbContext db = new MyDbContext();
            string dateCode = DateTime.Now.ToString("yyMMdd");

            int todayOrderCount = db.Orders
                .Where(o => o.OrderCode != null && o.OrderCode.Contains($"EL-{dateCode}"))
                .Count();

            string orderNumber = (todayOrderCount + 1).ToString("D3");

            return $"EL-{dateCode}-{orderNumber}";
        }
    }
}