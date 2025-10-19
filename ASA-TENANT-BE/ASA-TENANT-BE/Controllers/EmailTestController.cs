using ASA_TENANT_SERVICE.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ASA_TENANT_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailTestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("test")]
        public async Task<IActionResult> TestEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var result = await _emailService.TestEmailAsync(request.Email);
                
                if (result)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Test email sent successfully!",
                        email = request.Email
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Failed to send test email",
                        email = request.Email
                    });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error sending test email",
                    error = ex.Message
                });
            }
        }

        [HttpPost("order-confirmation")]
        public async Task<IActionResult> TestOrderConfirmationEmail([FromBody] TestOrderEmailRequest request)
        {
            try
            {
                var orderDetailsHtml = $@"
                    <table style='width:100%;border-collapse:collapse;border:1px solid #e2e8f0;'>
                        <thead>
                            <tr style='background:#f8fafc;'>
                                <th style='padding:12px;text-align:left;border:1px solid #e2e8f0;font-size:14px;color:#334155;'>Sản phẩm</th>
                                <th style='padding:12px;text-align:center;border:1px solid #e2e8f0;font-size:14px;color:#334155;'>Số lượng</th>
                                <th style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#334155;'>Đơn giá</th>
                                <th style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#334155;'>Thành tiền</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr>
                                <td style='padding:12px;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>Cà phê đen</td>
                                <td style='padding:12px;text-align:center;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>2</td>
                                <td style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>25,000 VND</td>
                                <td style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;font-weight:600;'>50,000 VND</td>
                            </tr>
                            <tr>
                                <td style='padding:12px;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>Bánh mì</td>
                                <td style='padding:12px;text-align:center;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>1</td>
                                <td style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;'>15,000 VND</td>
                                <td style='padding:12px;text-align:right;border:1px solid #e2e8f0;font-size:14px;color:#0f172a;font-weight:600;'>15,000 VND</td>
                            </tr>
                        </tbody>
                    </table>";

                var result = await _emailService.SendOrderConfirmationEmailAsync(
                    request.Email,
                    request.CustomerName,
                    request.OrderId,
                    request.ShopName,
                    orderDetailsHtml,
                    request.TotalPrice,
                    request.TotalDiscount,
                    request.FinalPrice,
                    System.DateTime.Now,
                    request.Note
                );
                
                if (result)
                {
                    return Ok(new { 
                        success = true, 
                        message = "Order confirmation email sent successfully!",
                        email = request.Email
                    });
                }
                else
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Failed to send order confirmation email",
                        email = request.Email
                    });
                }
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { 
                    success = false, 
                    message = "Error sending order confirmation email",
                    error = ex.Message
                });
            }
        }
    }

    public class TestEmailRequest
    {
        public string Email { get; set; }
    }

    public class TestOrderEmailRequest
    {
        public string Email { get; set; }
        public string CustomerName { get; set; }
        public long OrderId { get; set; }
        public string ShopName { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? TotalDiscount { get; set; }
        public decimal FinalPrice { get; set; }
        public string Note { get; set; }
    }
}
