using ConsoleApp1.Tools;
using CryptoExchange.Net.CommonObjects;

string Symbol = "BTCUSDT";
string ApiKey = Secrets.ApiKey;
string ApiSec = Secrets.ApiSecret;

List<Kline> klines = new();

// Подключаем коннектор с Бинанс и проверяем ключи
bool res = await BinaApi.Init(ApiKey, ApiSec);
if (res)
{
    // Получаем 1000 последних свечей
    klines = await BinaApi.GetKlinesAsync(Symbol, "5m");
}
else
{
    Console.WriteLine("Ключи не прошли проверку");
    return;
}

// Проверить стратегию
StochRsi tradingBot = new(Converter.KlinesToQuotes(klines));
tradingBot.StartBacktest();
Console.ReadKey();

// Подписались на событие обновления цены
await BinaApi.StartListenForNewTrade();
BinaApi.OnKlineUpdate += OnKlineUpdate;

// Цикл жизни бота
while (true)
    await Task.Delay(100);

void OnKlineUpdate(Kline k)
{
    tradingBot.UpdateQuote(Converter.KlineToQuote(k));

    Console.WriteLine($"{DateTime.Now,8:hh:mm:ss} - " +
        $"Last trade: {Symbol} {k.OpenTime,5:hh:mm} {k.ClosePrice} - " +
        tradingBot.Report());

    JustDoIt();
}

// Основная функция бота
void JustDoIt()
{
    if (tradingBot.SignalToOpenLong)
    {
        // Открываем длинную позицию
        //BinaApi.SpotOrderBuy(10);
        Console.WriteLine("Open Long");
    }
    if (tradingBot.SignalToCloseLong)
    {
        // Закрываем длинную позицию
        //BinaApi.SpotOrderSell(10);
        Console.WriteLine("Close Long");
    }
    if (tradingBot.SignalToOpenShort)
    {
        //BinaApi.SpotOrderSell(10);
        Console.WriteLine("Open Short");
    }
    if (tradingBot.SignalToCloseShort)
    {
        //BinaApi.SpotOrderBuy(10);
        Console.WriteLine("Close Short");
    }
}
