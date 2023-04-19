using CryptoExchange.Net.CommonObjects;

public static class TradingBot
{
    public static bool HasSignalToOpenLong(List<Kline> klines) 
    {
        bool result = false;
        // Используя технический анализ, по набору из 1000 свечей
        // определяем, стоит ли открыть длинную позицию
        // если да, вернем true

        return result;
    }
    public static bool HasSignalToCloseLong(List<Kline> klines)
    {
        bool result = false;
        // Смотрим а есть ли открытый лонг, если да
        // то проверяем стопы прибыли или потерь
        // стоп сработал, возвращаем true

        return result;
    }
    public static bool HasSignalToOpenShort(List<Kline> klines)
    {
        bool result = false;

        return result;
    }
    public static bool HasSignalToCloseShort(List<Kline> klines)
    {
        bool result = false;

        return result;
    }
}
