namespace Order.API.ViewModels
{
    public class CreateOrderVM
    {
        public Guid BuyerId { get; set; }
        public List<CreateOrderItemVM> OrderItems { get; set; }
    }
    public class CreateOrderItemVM
    {
        public string ProductId { get; set; }
        public int Count { get; set; }
        public decimal Price { get; set; }
    }
}
