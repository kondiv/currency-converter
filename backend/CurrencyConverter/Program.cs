var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Пример запроса: GET /rate?from=USD&to=EUR
app.MapGet("/rate", async (string from, string to) =>
{
    using var http = new HttpClient();
    
    var data = await http.GetFromJsonAsync<CbrResponse>("https://www.cbr-xml-daily.ru/latest.js");
    if (data?.Rates == null)
        return Results.BadRequest("Не удалось получить данные от ЦБ.");

    var baseCurrency = data.Base ?? "RUB";

    if (!data.Rates.TryGetValue(from, out var fromToRub) &&
        !from.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest($"Не найдена валюта {from}.");

    if (!data.Rates.TryGetValue(to, out var toToRub) &&
        !to.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase))
        return Results.BadRequest($"Не найдена валюта {to}.");
    
    decimal rate;

    if (from.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase))
        rate = 1 / toToRub; 
    else if (to.Equals(baseCurrency, StringComparison.OrdinalIgnoreCase))
        rate = fromToRub;  
    else
        rate = fromToRub / toToRub;

    return Results.Ok(new
    {
        From = from,
        To = to,
        Rate = Math.Round(rate, 6),
        Base = baseCurrency,
        Date = data.Date
    });
});

app.Run();

record CbrResponse(string Disclaimer, string Date, long Timestamp, string Base, Dictionary<string, decimal> Rates);