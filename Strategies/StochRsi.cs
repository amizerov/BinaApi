using Skender.Stock.Indicators;

public class StochRsi
{
    List<Quote> _quotes = new();
    List<StochRsiResult> _indicas = new();

    string cross = string.Empty;
    decimal trdPrice = 0;
    decimal trdQty = 0;
    decimal rlzGain = 0;
    decimal trdGain = 0;

    public StochRsi(List<Quote> quotesHistory)
    {
        _quotes = quotesHistory;
    }
    public void UpdateQuote(Quote lastQuote)
    {
        Quote lq = _quotes.Last();
        if (lq.Date == lastQuote.Date)
        {
            _quotes.Remove(lq);
            _quotes.Add(lastQuote);
        }
        else
        {
            _quotes.Add(lastQuote);
        }
        _indicas = _quotes.GetStochRsi(14, 14, 3, 1).ToList();
        int c = _indicas.Count;
        StochRsiResult e = _indicas[c - 1]; // current period
        StochRsiResult l = _indicas[c - 2]; // last (prior) period
        GetSignal(lastQuote, e, l);
    }
    public bool SignalToOpenLong
    {
        get
        {
            return trdQty == 0 && cross == "LONG";
        }
    }
    public bool SignalToCloseLong
    {
        get
        {
            return trdQty == 1 && cross == "SHORT";
        }
    }
    public bool SignalToOpenShort
    {
        get
        {
            return trdQty == 0 && cross == "SHORT";
        }
    }
    public bool SignalToCloseShort
    {
        get
        {
            return trdQty == -1 && cross == "LONG";
        }
    }
    public void StartBacktest()
    {
        /* As a result, there will always be one open LONG or SHORT
         * position that is opened and closed at signal crossover
         * points in the overbought and oversold regions of the indicator.
         */

        // calculate Stochastic RSI
        _indicas = _quotes.GetStochRsi(14, 14, 3, 1).ToList();

        // initialize
        trdPrice = 0;
        trdQty = 0;
        rlzGain = 0;

        Console.WriteLine("   Date   Close  StRSI Signal  Cross  Net Gains");
        Console.WriteLine("-------------------------------------------------------");

        // roll through history
        for (int i = 1; i < _quotes.Count; i++)
        {
            Quote q = _quotes[i];
            StochRsiResult e = _indicas[i];   // evaluation period
            StochRsiResult l = _indicas[i - 1]; // last (prior) period
            GetSignal(q, e, l);
        }

        cross = string.Empty;
        trdPrice = 0;
        trdQty = 0;
        rlzGain = 0;
        trdGain = 0;
    }
    void GetSignal(Quote q, StochRsiResult e, StochRsiResult l)
    {
        cross = string.Empty;

        // unrealized gain on open trade
        trdGain = trdQty * (q.Close - trdPrice);

        // check for LONG event
        // condition: Stoch RSI was <= 20 and Stoch RSI crosses over Signal
        if (l.StochRsi <= 20
         && l.StochRsi < l.Signal
         && e.StochRsi >= e.Signal
         && trdQty != 1)
        {
            // emulates BTC + BTO
            rlzGain += trdGain;
            trdQty = 1;
            trdPrice = q.Close;
            cross = "LONG";
        }

        // check for SHORT event
        // condition: Stoch RSI was >= 80 and Stoch RSI crosses under Signal
        if (l.StochRsi >= 80
         && l.StochRsi > l.Signal
         && e.StochRsi <= e.Signal
         && trdQty != -1)
        {
            // emulates STC + STO
            rlzGain += trdGain;
            trdQty = -1;
            trdPrice = q.Close;
            cross = "SHORT";
        }

        if (cross != string.Empty)
        {
            Console.WriteLine(
            $"{q.Date,8:hh:mm:ss} " +
            $"{q.Close,10:c2}" +
            $"{e.StochRsi,7:N1}" +
            $"{e.Signal,7:N1}" +
            $"{cross,7}" +
            $"{trdGain,13:c2}" +
            $"{rlzGain + trdGain,13:c2}");
        }
    }
    public string Report()
    {
        return $"Position: {trdQty,7}, Price: {trdPrice}, Profit: {trdGain,12:c2}/{rlzGain,12:c2}";
    }
}
