using Core.Entities.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Specification
{
    public class OrderSpecification : BaseSpecification<Order>
    {
        public OrderSpecification(string email) : base(x => x.BuyerEmail == email)
        {
            AddInclude(x => x.OrderItems);
            AddInclude(x => x.DeliveryMethod);
            AddOrderByDescending(x => x.OrderDate);
        }

        public OrderSpecification(string email, int id) : base(x => x.BuyerEmail == email && x.Id == id)
        {
            AddInclude("OrderItems");
            AddInclude("DeliveryMethod");
        }

        public OrderSpecification(string PaymentIntentId, bool isPaymentIntent) : base(x => x.PaymentIntentId == PaymentIntentId)
        {
            AddInclude("OrderItems");
            AddInclude("DeliveryMethod");
        }

    }
}
