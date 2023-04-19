using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Interfaces;
using Binance.Net.Objects;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Logging;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;

namespace ConsoleApp1
{
    public static class BinaApi
    {
        static string _interval = "5m";
        static string _symbol = "BTCUSDT";
        static BinanceClient _restClient = new();
        static BinanceSocketClient socketClient = new();

        public static event Action<Kline>? OnKlineUpdate;
        static void KlineUpdated(Kline k) => OnKlineUpdate?.Invoke(k);

        public static async Task<List<Kline>> Init(string symbol, string apiKey, string apiSecret)
        {
            try
            {
                _symbol = symbol;
                _restClient = new BinanceClient(
                    new BinanceClientOptions()
                    {
                        ApiCredentials = new BinanceApiCredentials(apiKey, apiSecret)
                    });

                await SubsToSock();
                return await GetKlinesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Init api key - Error: {ex.Message}");
            }
            return new List<Kline>();
        }
        static async Task<List<Kline>> GetKlinesAsync()
        {
            List<Kline> klines = new();
            CancellationToken cancellationToken = new CancellationToken();
            var r = await _restClient.SpotApi.CommonSpotClient
                .GetKlinesAsync(_symbol, TimeSpan.FromSeconds(IntervalInSeconds(_interval)),
                null, null, 1000, cancellationToken);

            if (r.Success)
            {
                klines = r.Data.ToList();
                Console.WriteLine($"GetKlines({_symbol}) - {klines.Count} klines loaded");
            }
            else
            {
                Console.WriteLine($"GetKlines({_symbol}) - Error: " + r.Error?.Message);
            }
            return klines;
        }
        static async Task<CallResult<UpdateSubscription>> SubsToSock()
        {
            var r = await socketClient.SpotStreams.
                SubscribeToKlineUpdatesAsync(_symbol, (KlineInterval)IntervalInSeconds(_interval),
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

                    KlineUpdated(kline);
                    Console.WriteLine($"{DateTime.Now.ToString("t")} - Last trade: {_symbol} {k.OpenTime} {k.ClosePrice}");
                });

            return r;
        }

        public static void SpotOrderBuy(decimal quantity)
        {
            _restClient.SpotApi.Trading.PlaceOrderAsync(
                _symbol, 
                Binance.Net.Enums.OrderSide.Buy, 
                Binance.Net.Enums.SpotOrderType.Market, quantity);
        }
        public static void SpotOrderSell(decimal quantity)
        {
            _restClient.SpotApi.Trading.PlaceOrderAsync(
                _symbol,
                Binance.Net.Enums.OrderSide.Sell,
                Binance.Net.Enums.SpotOrderType.Market, quantity);
        }
        public static void FutuOrderBuy(decimal quantity)
        {
            _restClient.CoinFuturesApi.Trading.PlaceOrderAsync(
                _symbol,
                Binance.Net.Enums.OrderSide.Buy, 
                Binance.Net.Enums.FuturesOrderType.Market,
                quantity);
        }
        public static void FutuOrderSell(decimal quantity)
        {
            _restClient.CoinFuturesApi.Trading.PlaceOrderAsync(
                _symbol,
                Binance.Net.Enums.OrderSide.Sell,
                Binance.Net.Enums.FuturesOrderType.Market,
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
}
