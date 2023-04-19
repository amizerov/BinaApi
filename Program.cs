﻿using ConsoleApp1;
using CryptoExchange.Net.CommonObjects;

BinaApi.Init("Key", "Secret");
List<Kline> klines = await BinaApi.GetKlinesAsync("BTCUSDT", "5m");
BinaApi.OnKlineUpdate += OnKlineUpdate;
await BinaApi.SubsToSock("5m");

while (true)
    await Task.Delay(100);

void OnKlineUpdate(Kline k)
{
    UpdateKlines(k);

    if (TradingBot.HasSignalToOpenLong(klines))
    {
        BinaApi.SpotOrderBuy(10);
    }
    if (TradingBot.HasSignalToCloseLong(klines))
    {
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
void UpdateKlines(Kline k)
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
}
