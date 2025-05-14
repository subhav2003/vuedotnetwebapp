using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Pustakalaya.Models;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendOrderDispatchedEmail(Order order)
    {
        var bodyHtml = $@"
        <h2>Hello {order.Member?.Name},</h2>
        <p>Your order has been <strong>dispatched</strong>.</p>
        <p><strong>Claim Code:</strong> {order.ClaimCode}</p>
        <p><strong>Status:</strong> {order.OrderStatus}</p>
        <p><strong>Pickup Deadline:</strong> {order.PickupDeadline:yyyy-MM-dd}</p>

        <h3>📦 Items</h3>
        <table border='1' cellpadding='6' cellspacing='0' style='border-collapse: collapse;'>
            <thead>
                <tr style='background-color: #f3f3f3;'>
                    <th>Title</th>
                    <th>Qty</th>
                    <th>Unit Price</th>
                    <th>Line Total</th>
                </tr>
            </thead>
            <tbody>
                {string.Join("", order.OrderItems.Select(i => $@"
                <tr>
                    <td>{i.Book?.Title ?? "N/A"}</td>
                    <td>{i.Quantity}</td>
                    <td>${i.UnitPrice:F2}</td>
                    <td>${(i.UnitPrice * i.Quantity):F2}</td>
                </tr>"))}
            </tbody>
        </table>

        <h3>💰 Summary</h3>
        <ul>
            <li><strong>Subtotal:</strong> ${order.OrderItems.Sum(i => i.UnitPrice * i.Quantity):F2}</li>
            <li><strong>Discounts:</strong> ${order.DiscountAmount:F2} ({order.AppliedDiscounts})</li>
            <li><strong>Total:</strong> ${order.TotalPrice:F2}</li>
        </ul>

        <p>Thank you for using <strong>Pustakalaya</strong>.</p>";

        await SendEmail(order.Member.Email, "📦 Your Order Has Been Dispatched", bodyHtml);
    }

    public async Task SendOrderCancelledEmail(Order order)
    {
        var bodyHtml = $@"
        <h2>Hello {order.Member?.Name},</h2>
        <p>We’re writing to confirm that your order with Claim Code <strong>{order.ClaimCode}</strong> has been <strong>cancelled</strong>.</p>
        <p>If this was a mistake or you have questions, please contact us.</p>
        <p>Thank you,<br/><strong>Pustakalaya Team</strong></p>";

        await SendEmail(order.Member.Email, "⚠️ Your Order Was Cancelled", bodyHtml);
    }

    private async Task SendEmail(string toEmail, string subject, string bodyHtml)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_config["EmailSettings:From"]));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(TextFormat.Html) { Text = bodyHtml };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["EmailSettings:From"], _config["EmailSettings:AppPassword"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
    
    public async Task SendOrderPlacedEmail(Order order)
    {
        var html = $@"
        <h2>Hi {order.Member?.Name},</h2>
        <p>Thanks for placing an order at Pustakalaya!</p>
        <p><strong>Claim Code:</strong> {order.ClaimCode}</p>
        <p><strong>Pickup Deadline:</strong> {order.PickupDeadline:yyyy-MM-dd}</p>
        <h3>Items:</h3>
        <ul>
            {string.Join("", order.OrderItems.Select(i => $"<li>{i.Book?.Title} — {i.Quantity} x ${i.UnitPrice:F2}</li>"))}
        </ul>
        <p><strong>Total:</strong> ${order.TotalPrice:F2}</p>
        <p>We hope you enjoy your books!</p>";

        await SendEmail(order.Member!.Email, "📚 Order Confirmation - Pustakalaya", html);
    }

}
