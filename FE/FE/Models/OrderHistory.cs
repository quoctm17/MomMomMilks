﻿namespace FE.Models
{
    public class OrderHistory
    {
        public int Id { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public float TotalAmount { get; set; }
        public int BuyerId { get; set; }
        public int AddressId { get; set; }
        public string PaymentType { get; set; }
        public int TransactionId { get; set; }
        public string Shipper { get; set; }
        public string OrderStatus { get; set; }
    }
}
