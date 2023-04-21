using CryptoExchange.Net.CommonObjects;

// Подключаем коннектор с Бинанс и получаем 1000 последних свечей
List<Kline> klines = await BinaApi.Init("Key", "Secret", "BTCUSDT");
// Подписались на событие обновления цены
BinaApi.OnKlineUpdate += OnKlineUpdate;

// Цикл жизни бота
while (true)
    await Task.Delay(100);

void OnKlineUpdate(Kline k)
{
    Kline lk = klines.Last();
    if (lk.OpenTime == k.OpenTime)
    {
        klines.Remove(lk);
        klines.Add(k);
    }
    else
    {
        klines.Add(k);
    }
    JustDoIt();
}

// Основная функция бота
void JustDoIt()
{
    if (TradingBot.HasSignalToOpenLong(klines))
    {
        // Открываем длинную позицию
        BinaApi.SpotOrderBuy(10);
    }
    if (TradingBot.HasSignalToCloseLong(klines))
    {
        // Закрываем длинную позицию
        BinaApi.SpotOrderSell(10);
    }
    if (TradingBot.HasSignalToOpenShort(klines))
    {
        BinaApi.SpotOrderSell(10);
    }
    if (TradingBot.HasSignalToCloseShort(klines))
    {
        BinaApi.SpotOrderBuy(10);
    }
}
