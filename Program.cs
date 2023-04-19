using CryptoExchange.Net.CommonObjects;

List<Kline> klines = await BinaApi.Init("Key", "Secret", "BTCUSDT");
BinaApi.OnKlineUpdate += OnKlineUpdate;

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
