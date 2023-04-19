using Binance.Net.Clients;
using Binance.Net.Objects;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.CommonObjects;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Logging;

namespace ConsoleApp1
{
    public static class BinaApi
    {
        static string _symbol = "BTCUSDT";
        static BinanceClient _restClient = new();
        static BinanceSocketClient socketClient = new();

        public static void Init(string apiKey, string apiSecret)
        {
            try
            {
                _restClient = new BinanceClient(
                    new BinanceClientOptions()
                    {
                        ApiCredentials = new BinanceApiCredentials(apiKey, apiSecret)
                    });
            }
            catch (Exception ex)
            {
                Console.Write("CheckApiKey", $"Error: {ex.Message}");
            }
        }
        public static async Task<List<Kline>> GetKlines(string symbol, string inter)
        {
            _symbol = symbol;
            List<Kline> klines = new();
            CancellationToken cancellationToken = new CancellationToken();
            var r = await _restClient.SpotApi.CommonSpotClient
                .GetKlinesAsync(symbol, TimeSpan.FromSeconds(IntervalInSeconds(inter)),
                null, null, 1000, cancellationToken);

            if (r.Success)
            {
                klines = r.Data.ToList();
                Console.Write($"GetKlines({symbol})", $"{klines.Count} klines loaded");
            }
            else
            {
                Console.Write($"GetKlines({symbol})", "" + r.Error?.Message);
            }
            return klines;
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
