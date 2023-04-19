using ConsoleApp1;
using CryptoExchange.Net.CommonObjects;

List<Kline> klines = BinaApi.GetKlines("BTCUSDT", "5m").Result;
Console.WriteLine($"got {klines.Count}");