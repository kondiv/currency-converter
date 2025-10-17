using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var history = new ConcurrentBag<ConversionRecord>();

app.MapPost("/history", (ConversionRecord record) =>
{
    if (string.IsNullOrWhiteSpace(record.From) || string.IsNullOrWhiteSpace(record.To))
        return Results.BadRequest("Поля From и To обязательны.");

    record.Timestamp = DateTime.UtcNow;
    history.Add(record);
    return Results.Ok(new { Message = "Запись добавлена", record });
});

app.MapGet("/history", () => Results.Ok(history));

app.MapGet("/history/{currency}", (string currency) =>
{
    var filtered = history
        .Where(r => r.From.Equals(currency, StringComparison.OrdinalIgnoreCase)
                    || r.To.Equals(currency, StringComparison.OrdinalIgnoreCase))
        .ToList();

    return Results.Ok(filtered);
});

app.MapDelete("/history", () =>
{
    while (!history.IsEmpty)
        history.TryTake(out _);
    return Results.Ok(new { Message = "История очищена" });
});

app.Run();

public class ConversionRecord
{
    public string From { get; set; } = default!;
    public string To { get; set; } = default!;
    public decimal Rate { get; set; }
    public decimal Amount { get; set; }
    public decimal Result => Math.Round(Amount * Rate, 6);
    public DateTime Timestamp { get; set; }
}