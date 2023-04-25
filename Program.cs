using ConsoleApp1.Tools;
using CryptoExchange.Net.CommonObjects;

string Symbol = "BTCUSDT";
string Interval = "3m";
string ApiKey = Secrets.ApiKey;
string ApiSec = Secrets.ApiSecret;

List<Kline> klines = new();

// Подключаем коннектор с Бинанс и проверяем ключи
bool res = await BinaApi.CheckApiKey(ApiKey, ApiSec);
if (res)
{
    // Получаем 1500 последних свечей
    klines = await BinaApi.GetKlinesAsync(Symbol, Interval);
}
else
{
    Console.WriteLine("Ключи не прошли проверку");
    return;
}

// Проверить стратегию
StochRsi strategy = new(Converter.KlinesToQuotes(klines));
strategy.Backtest();
Console.ReadKey();

// Начинаем цикл реального робота торговца
decimal? trdPrice = 0;  // Цена по которой робот открыл позицию
decimal? trdQty = 0;    // Колличество коинов в позиции
decimal? rlzGain = 0;   // Суммарная реализованная прибыль
decimal? trdGain = 0;   // Прибыль по текущей открытой позиции
DateTime? trdTime;

// Подписались на событие обновления цены
await BinaApi.StartListenForNewTrade();
BinaApi.OnKlineUpdate += OnKlineUpdate;
BinaApi.OnNewKline += OnNewKline;

void OnKlineUpdate(Kline k)
{
    strategy.UpdateQuote(Converter.KlineToQuote(k));
    trdGain = trdQty * (k.ClosePrice - trdPrice);

    JustDoIt(k);

    Console.WriteLine($"{DateTime.Now,8:hh:mm:ss} - " +
        $"Last trade: {Symbol} {k.OpenTime,5:hh:mm} {k.ClosePrice} " +
        $"Position: {trdQty}/{trdPrice} Gain: {trdGain}/{rlzGain}");
}
void OnNewKline(Kline k)
{
    Console.WriteLine($"-{k.OpenTime,5:hh:mm}------------------------------");
}

void JustDoIt(Kline k)
{
    switch (strategy.Signal)
    {
        case "LONG":
            if (trdQty == 0)
            {
                // BTO - Buy To Open Long position
                trdQty = 1;
                trdPrice = k.ClosePrice;
                trdTime = DateTime.Now;
                // Открываем длинную позицию
                //BinaApi.SpotOrderBuy(10);
                Console.WriteLine("Open Long");
            }
            else if (trdQty == -1)
            {
                // BTC - Buy To Close Short position
                rlzGain += trdQty * (k.OpenPrice - trdPrice);
                trdQty = 0;
                trdPrice = 0;
                // Закрываем короткую позицию
                //BinaApi.SpotOrderBuy(10);
                Console.WriteLine("Close Short");
            }
            break;
        case "SHORT":
            if (trdQty == 0)
            {
                // STO - Sell To Open Short position
                trdQty = -1;
                trdPrice = k.ClosePrice;
                trdTime = DateTime.Now;
                // Открываем короткую позицию
                //BinaApi.SpotOrderSell(10);
                Console.WriteLine("Open Short");
            }
            else if (trdQty == 1)
            {
                // STC - Sell To Close Long position
                rlzGain += trdQty * (k.OpenPrice - trdPrice);
                trdQty = 0;
                trdPrice = 0;
                // Закрываем длинную позицию
                //BinaApi.SpotOrderSell(10);
                Console.WriteLine("Close Long");
            }
            break;
    }
}

// Цикл жизни бота
Console.ReadKey();

