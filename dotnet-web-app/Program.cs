
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors((options) =>
{
    options.AddPolicy("CORSPolicy", builder =>
    builder.AllowAnyMethod().AllowAnyHeader().
    WithOrigins("http://localhost:3000"));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CORSPolicy");
app.MapGet("/api/exchange-list", async (context) =>
           {

               var values = new List<Exchange>
                  {
                       new Exchange { id = 0, name = "Binance" },
                       new Exchange { id = 1, name = "CoinBase" },
                       new Exchange { id = 2, name = "HuobiPro" }
                  };
               var json = JsonConvert.SerializeObject(values);

               await context.Response.WriteAsync(json);


           });

app.MapGet("/api/symbol-list/{exchangeName}", async (HttpContext context, string exchangeName) =>
            {
                var json = await System.IO.File.ReadAllTextAsync("./data.json");


                List<Data> dataObject = System.Text.Json.JsonSerializer.Deserialize<List<Data>>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });


                context.Response.ContentType = "application/json";

                switch (exchangeName)
                {
                    case "Binance":
                        var binanceSym = JsonConvert.SerializeObject(dataObject[0].SymbolData);
                        await context.Response.WriteAsync(binanceSym);

                        break;
                    case "HuobiPro":
                        var huobiSym = JsonConvert.SerializeObject(dataObject[1].SymbolData);
                        await context.Response.WriteAsync(huobiSym);

                        break;

                    case "CoinBase":
                        var coinSym = JsonConvert.SerializeObject(dataObject[2].SymbolData);
                        await context.Response.WriteAsync(coinSym);
                        break;
                    default:

                        break;
                }
            });

app.MapGet("/api/latest-trades/{exchangeName}/{symbol}", async (HttpContext context, string exchangeName, string symbol) =>
            {


                switch (exchangeName)
                {
                    case "Binance":

                        using (var client = new HttpClient())
                        {
                            string[] symbols = symbol.Split("-");
                            string assetSym = symbols[0];
                            string quoteSym = symbols[1];

                            string endpoint = "https://api.binance.com/api/v3/trades?symbol=" + assetSym + quoteSym + "&limit=100";

                            var response = await client.GetAsync(endpoint);

                            if (response.IsSuccessStatusCode)
                            {
                                try
                                {
                                    var result = await response.Content.ReadAsStringAsync();
                                    List<Binance> latestTradesBinance = System.Text.Json.JsonSerializer.Deserialize<List<Binance>>
                                       (result, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                                    List<LatestTrade> latestTrades = new List<LatestTrade>();
                                    int i = 0;

                                    foreach (Binance binance in latestTradesBinance)
                                    {
                                        if (i == 100)
                                            break;
                                        DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(binance.time / 1000d)).ToLocalTime();
                                        latestTrades.Add(new LatestTrade
                                        {
                                            id = i,
                                            assetSymbol = assetSym,
                                            assetQuantity = String.Format("{0:0.###}", Double.Parse(binance.qty)),
                                            quoteSymbol = quoteSym,
                                            quoteQuantity = String.Format("{0:0.###}", Double.Parse(binance.quoteQty)),
                                            date = dt.ToString()

                                        });
                                        i++;
                                    }

                                    var resp = JsonConvert.SerializeObject(latestTrades);
                                    await context.Response.WriteAsync(resp);



                                }
                                catch (Exception error)
                                {
                                    Console.WriteLine(error.Message);
                                    List<LatestTrade> latestTrades = new List<LatestTrade>();
                                    var resp = JsonConvert.SerializeObject(latestTrades);
                                    await context.Response.WriteAsync(resp);

                                }
                            }
                            else
                            {
                                List<LatestTrade> latestTrades = new List<LatestTrade>();
                                var resp = JsonConvert.SerializeObject(latestTrades);
                                await context.Response.WriteAsync(resp);
                            }
                        }
                        break;


                    case "HuobiPro":
                        using (var client = new HttpClient())
                        {
                            string[] symbols = symbol.Split("-");
                            string assetSym = symbols[0];
                            string quoteSym = symbols[1];

                            client.DefaultRequestHeaders.Add("User-Agent", "curl/7.58.0");
                            string endpoint = "https://api.huobi.pro/market/history/trade?symbol=" + assetSym.ToLower() + quoteSym.ToLower() + "&size=100";

                            var response = await client.GetAsync(endpoint);



                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();



                                try
                                {
                                    HuobiPro latestTradesHuobiPro = Newtonsoft.Json.JsonConvert.DeserializeObject<HuobiPro>(result);
                                    int i = 0;
                                    List<LatestTrade> latestTrades = new List<LatestTrade>();
                                    foreach (DataHuobi data in latestTradesHuobiPro.data)
                                    {
                                        if (i == 100)
                                            break;
                                        foreach (TradeDataHuobi dataInner in data.data)
                                        {
                                            if (i == 100)
                                                break;
                                            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Math.Round(dataInner.ts / 1000d)).ToLocalTime();
                                            latestTrades.Add(new LatestTrade
                                            {
                                                id = i,
                                                assetSymbol = assetSym,
                                                assetQuantity = String.Format("{0:0.###}", dataInner.amount),
                                                quoteSymbol = quoteSym,
                                                quoteQuantity = String.Format("{0:0.###}", dataInner.price * dataInner.amount),
                                                date = dt.ToString()

                                            });

                                            i++;
                                        }
                                    }
                                    var resp = JsonConvert.SerializeObject(latestTrades);
                                    await context.Response.WriteAsync(resp);

                                }
                                catch (Exception error)
                                {
                                    Console.WriteLine(error.Message);
                                    List<LatestTrade> latestTrades = new List<LatestTrade>();
                                    var resp = JsonConvert.SerializeObject(latestTrades);
                                    await context.Response.WriteAsync(resp);


                                }
                            }
                            else
                            {
                                List<LatestTrade> latestTrades = new List<LatestTrade>();
                                var resp = JsonConvert.SerializeObject(latestTrades);
                                await context.Response.WriteAsync(resp);


                            }
                        }



                        break;

                    case "CoinBase":
                        using (var client = new HttpClient())


                        {


                            string[] symbols = symbol.Split("-");
                            string assetSym = symbols[0];
                            string quoteSym = symbols[1];

                            client.DefaultRequestHeaders.Add("User-Agent", "curl/7.58.0");


                            string endpoint = "https://api.exchange.coinbase.com/products/" + symbol + "/trades/?limit=100";


                            var response = await client.GetAsync(endpoint);

                            if (response.IsSuccessStatusCode)
                            {
                                var result = await response.Content.ReadAsStringAsync();
                                try
                                {
                                    List<CoinBasePro> latestTradesCoinBasePro = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CoinBasePro>>
                                       (result);

                                    List<LatestTrade> latestTrades = new List<LatestTrade>();
                                    int i = 0;



                                    foreach (CoinBasePro coinBasePro in latestTradesCoinBasePro)
                                    {
                                        if (i == 100)
                                            break;
                                        DateTime dt = DateTime.Parse(coinBasePro.time, null, System.Globalization.DateTimeStyles.RoundtripKind).AddHours(-5);

                                        latestTrades.Add(new LatestTrade
                                        {
                                            id = i,
                                            assetSymbol = assetSym,
                                            assetQuantity = coinBasePro.size,
                                            quoteSymbol = quoteSym,
                                            quoteQuantity = String.Format("{0:0.###}", (Double.Parse(coinBasePro.price) * Double.Parse(coinBasePro.size))),
                                            date = dt.ToString()

                                        });
                                        i++;
                                    }

                                    var resp = JsonConvert.SerializeObject(latestTrades);
                                    await context.Response.WriteAsync(resp);




                                }
                                catch (Exception error)
                                {
                                    Console.WriteLine(error.Message);
                                    List<LatestTrade> latestTrades = new List<LatestTrade>();
                                    var resp = JsonConvert.SerializeObject(latestTrades);
                                    await context.Response.WriteAsync(resp);
                                }
                            }
                            else
                            {
                                List<LatestTrade> latestTrades = new List<LatestTrade>();
                                var resp = JsonConvert.SerializeObject(latestTrades);
                                await context.Response.WriteAsync(resp);
                            }
                        }
                        break;
                    default:

                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                }
            });
app.Run();


public class Exchange
{

    public int? id { get; set; }
    public string? name { get; set; }
}
public class Data
{
    public int? Id { get; set; }
    public string? Exchange { get; set; }
    public List<SymbolDataNested>? SymbolData { get; set; }
}

public class SymbolDataNested
{
    public string? Symbol { get; set; }
    public string? Base { get; set; }

    public string? Quote { get; set; }
}

public class Binance
{
    public long id { get; set; }
    public string price { get; set; }
    public string qty { get; set; }
    public string quoteQty { get; set; }
    public long time { get; set; }
    public bool isBuyerMaker { get; set; }
    public bool isBestMatch { get; set; }
}
public class CoinBasePro
{
    public string time { get; set; }
    public int trade_id { get; set; }
    public string price { get; set; }
    public string size { get; set; }
    public string side { get; set; }
}

public class TradeDataHuobi
{
    public string id { get; set; }
    public ulong ts { get; set; }
    public string trade_id { get; set; }
    public double amount { get; set; }
    public double price { get; set; }
    public string direction { get; set; }
}

public class DataHuobi
{
    public string id { get; set; }
    public ulong ts { get; set; }
    public List<TradeDataHuobi> data { get; set; }
}

public class HuobiPro
{
    public string ch { get; set; }
    public string status { get; set; }
    public ulong ts { get; set; }
    public List<DataHuobi> data { get; set; }
}
public class LatestTrade
{
    public long? id { get; set; }
    public string? assetSymbol { get; set; }

    public string? assetQuantity { get; set; }

    public string quoteSymbol { get; set; }

    public string quoteQuantity { get; set; }

    public string date { get; set; }

}

