using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Spot;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

public static class BinaApi
{
    static string? _symbol;
    static string? _interval;
    static List<Kline> _klines = new();

    static BinanceClient _restClient = new();
    static BinanceSocketClient socketClient = new();

    public static event Action<Kline>? OnKlineUpdate;
    public static event Action<Kline>? OnNewKline;

    public static async Task<bool> CheckApiKey(string apiKey, string apiSecret)
    {
        bool res = false;
        try
        {
            _restClient = new BinanceClient(
                new BinanceClientOptions()
                {
                    ApiCredentials = new BinanceApiCredentials(apiKey, apiSecret)
                });

            // Если получен доступ к балансам, ключ считается рабочим
            List<BinanceBalance> bs = await GetBalances();
            res = bs.Count > 0;

            Console.WriteLine($"CheckApiKey - Key.IsWorking: {res}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Init api key - Error: {ex.Message}");
        }
        return res;
    }
    public static async Task<List<BinanceBalance>> GetBalances() 
    { 
        List<BinanceBalance> balances = new();
        var res = await _restClient.SpotApi.Account.GetAccountInfoAsync();
        if (res.Success)
        {
            balances = res.Data.Balances.ToList();
        }
        else
        {
            Console.WriteLine("GetBalances - " +
                $"Error GetAccountInfoAsync: {res.Error?.Message}");
        }
        return balances;
    }
    public static async Task<List<Kline>> GetKlinesAsync(string symbol, string interval = "5m")
    {
        _symbol = symbol;
        _interval = interval;

        CancellationToken cancellationToken = new CancellationToken();

        var r = await _restClient.SpotApi.CommonSpotClient
            .GetKlinesAsync(_symbol, TimeSpan.FromSeconds(IntervalInSeconds(_interval)),
            null, null, 1500, cancellationToken);

        if (r.Success)
        {
            _klines = r.Data.ToList();

            Console.WriteLine($"GetKlines({_symbol}) - {_klines.Count} klines loaded");
        }
        else
        {
            Console.WriteLine($"GetKlines({_symbol}) - Error: " + r.Error?.Message);
        }
        return _klines;
    }
    public static async Task<CallResult<UpdateSubscription>> StartListenForNewTrade()
    {
        var r = await socketClient.SpotStreams.
            SubscribeToKlineUpdatesAsync(_symbol!, (KlineInterval)IntervalInSeconds(_interval!),
            msg =>
            {
                IBinanceStreamKline k = msg.Data.Data;

                Kline kline = new Kline();
                kline.HighPrice = k.HighPrice;
                kline.LowPrice = k.LowPrice;
                kline.OpenPrice = k.OpenPrice;
                kline.ClosePrice = k.ClosePrice;
                kline.Volume = k.Volume;
                kline.OpenTime = k.OpenTime;

                OnKlineUpdate?.Invoke(kline);
                if(_klines.Last().OpenTime < kline.OpenTime)
                {
                    _klines.Add(kline);
                    OnNewKline?.Invoke(kline);
                }
            });

        return r;
    }

    public static void SpotOrderBuy(decimal quantity)
    {
        _restClient.SpotApi.Trading.PlaceOrderAsync(
            _symbol!, 
            OrderSide.Buy, 
            SpotOrderType.Market, quantity);
    }
    public static void SpotOrderSell(decimal quantity)
    {
        _restClient.SpotApi.Trading.PlaceOrderAsync(
            _symbol!,
            OrderSide.Sell,
            SpotOrderType.Market, quantity);
    }
    public static void FutuOrderBuy(decimal quantity)
    {
        _restClient.CoinFuturesApi.Trading.PlaceOrderAsync(
            _symbol!,
            OrderSide.Buy, 
            FuturesOrderType.Market,
            quantity);
    }
    public static void FutuOrderSell(decimal quantity)
    {
        _restClient.CoinFuturesApi.Trading.PlaceOrderAsync(
            _symbol!,
            OrderSide.Sell,
            FuturesOrderType.Market,
            quantity);
    }
    static int IntervalInSeconds(string inter)
    {
        int seconds = 0;
        switch (inter)
        {
            case "1s":
                seconds = 1;
                break;
            case "1m":
                seconds = 60;
                break;
            case "3m":
                seconds = 3 * 60;
                break;
            case "5m":
                seconds = 5 * 60;
                break;
            case "15m":
                seconds = 15 * 60;
                break;
            case "30m":
                seconds = 30 * 60;
                break;
            case "1h":
                seconds = 60 * 60;
                break;
            case "2h":
                seconds = 2 * 60 * 60;
                break;
            case "4h":
                seconds = 4 * 60 * 60;
                break;
            case "6h":
                seconds = 6 * 60 * 60;
                break;
            case "8h":
                seconds = 8 * 60 * 60;
                break;
            case "12h":
                seconds = 12 * 60 * 60;
                break;
            case "1d":
                seconds = 24 * 60 * 60;
                break;
            case "3d":
                seconds = 3 * 24 * 60 * 60;
                break;
            case "1w":
                seconds = 7 * 24 * 60 * 60;
                break;
            case "1M":
                seconds = 30 * 24 * 60 * 60;
                break;
            default:
                break;
        }

        return seconds;
    }
}
