using API.Extensions;
using API.SignalR;
using Core.Entities;
using Core.Entities.OrderAggregate;
using Core.Interfaces;
using Core.Specification;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Stripe;

namespace API.Controllers
{
    public class PaymentsController(IPaymentService _paymentService, IUnitOfWork _unit,
    ILogger<PaymentsController> _logger, IConfiguration _config, IHubContext<NotificationHub> _hubContext) : BaseApiController
    {
        private readonly string _whSecret = _config["StripeSettings:WhSecret"]!;


        [Authorize]
        [HttpPost("{cartId}")]
        public async Task<ActionResult<ShoppingCart>> CreateOrUpdatePaymentIntent(string cartId)
        {
            var cart = await _paymentService.CreateOrUpdatePaymentIntent(cartId);

            if (cart == null) return BadRequest("Problem with your cart");

            return Ok(cart);
        }

        [HttpGet("delivery-methods")]
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
            return Ok(await _unit.Repository<DeliveryMethod>().ListAllAsync());
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = ConstructStripeEvent(json);

                if (stripeEvent.Data.Object is not PaymentIntent intent)
                {
                    return BadRequest("Invalid event data");
                }
                await HandlePaymentIntentSucceeded(intent);
                return Ok();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, "webhook error");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred");
            }
        }

        private async Task HandlePaymentIntentSucceeded(PaymentIntent intent)
        {
            if (intent.Status == "succeeded")
            {
                var spec = new OrderSpecification(intent.Id, true);

                var order = await _unit.Repository<Order>().GetEntityWithSpec(spec)
                        ?? throw new Exception("Order not found");

                if ((long)order.GetTotal() * 100 != intent.Amount)
                {
                    order.Status = OrderStatus.PaymentMismatch;
                }
                else
                {
                    order.Status = OrderStatus.PaymentReceived;
                }
                await _unit.Complete();

                var connectionId = NotificationHub.GetConnectionIdByEmail(order.BuyerEmail);

                if (!string.IsNullOrEmpty(connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("OrderCompleteNotification", order.ToDto());
                }

            }
        }

        private Event ConstructStripeEvent(string json)
        {
            try
            {
                return EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _whSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to construct stripe event");
                throw new StripeException("Invalid signature");
            }
        }
    }
}
