using CryptoExchange.Net.CommonObjects;
using Skender.Stock.Indicators;

namespace ConsoleApp1.Tools;

public static class Converter
{
    public static List<Quote> KlinesToQuotes(List<Kline> klines)
    {
        List<Quote> res = new List<Quote>();
        foreach (var k in klines)
        {
            res.Add(KlineToQuote(k));
        }
        return res;
    }
    public static Quote KlineToQuote(Kline k)
    {
        Quote q = new Quote();
        q.Date = k.OpenTime;
        q.Volume = (decimal)k.Volume!;
        q.High = (decimal)k.HighPrice!;
        q.Low = (decimal)k.LowPrice!;
        q.Close = (decimal)k.ClosePrice!;
        q.Open = (decimal)k.OpenPrice!;

        return q;
    }
}
