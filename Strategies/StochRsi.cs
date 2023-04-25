﻿using Skender.Stock.Indicators;

public class StochRsi
{
    List<Quote> _quotes = new();
    List<StochRsiResult> _stRsi = new();
    List<SmaResult> _sma = new();

    string cross = string.Empty;

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
        _stRsi = _quotes.GetStochRsi(14, 14, 3, 1).ToList();
        _sma = _quotes.GetSma(50).ToList();
        int c = _stRsi.Count - 1;
        StochRsiResult e = _stRsi[c]; // current period
        StochRsiResult l = _stRsi[c - 1]; // last (prior) period
        SmaResult sma = _sma[c];

        GetSignal(lastQuote, e, l, sma);
    }
    public string Signal
    {
        get
        {
            return cross;
        }
    }
    public void Backtest()
    {
        /* As a result, there will always be one open LONG or SHORT
         * position that is opened and closed at signal crossover
         * points in the overbought and oversold regions of the indicator.
         */

        // calculate Stochastic RSI
        _stRsi = _quotes.GetStochRsi(14, 14, 3, 1).ToList();
        _sma = _quotes.GetSma(50).ToList();

        // initialize
        decimal trdPrice = 0;
        decimal trdQty = 0;
        decimal rlzGain = 0;

        Console.WriteLine("   Date   Close  StRSI Signal  Cross  Net Gains");
        Console.WriteLine("-------------------------------------------------------");

        // roll through history
        for (int i = 1; i < _quotes.Count; i++)
        {
            Quote q = _quotes[i];
            StochRsiResult e = _stRsi[i];   // evaluation period
            StochRsiResult l = _stRsi[i - 1]; // last (prior) period
            SmaResult sma = _sma[i];

            // unrealized gain on open trade
            decimal trdGain = trdQty * (q.Close - trdPrice);

            GetSignal(q, e, l, sma);

            switch (cross)
            {
                case "LONG":
                    if(trdQty != 1)
                    {
                        // emulates BTC + BTO
                        rlzGain += trdGain;
                        trdQty = 1;
                        trdPrice = q.Close;
                    }
                    else cross = string.Empty;
                    break;
                case "SHORT":
                    if (trdQty != -1)
                    {
                        // emulates STC + STO
                        rlzGain += trdGain;
                        trdQty = -1;
                        trdPrice = q.Close;
                    }
                    else cross = string.Empty;
                    break;
            }
            if (cross != string.Empty)
            {
                Console.WriteLine(
                $"{q.Date,5:hh:mm} " +
                $"{q.Close,10:c2}" +
                $"{e.StochRsi,7:N1}" +
                $"{e.Signal,7:N1}" +
                $"{cross,7}" +
                $"{rlzGain + trdGain,13:c2}");
            }
        }
    }
    void GetSignal(Quote q, StochRsiResult e, StochRsiResult l, SmaResult sma)
    {
        cross = string.Empty;

        // check for LONG event
        // condition: Stoch RSI was <= 20 and Stoch RSI crosses over Signal
        if (l.StochRsi <= 20 && l.StochRsi < l.Signal && e.StochRsi >= e.Signal)
        {
            if(sma.Sma > (double)q.High)
                cross = "LONG";
        }

        // check for SHORT event
        // condition: Stoch RSI was >= 80 and Stoch RSI crosses under Signal
        if (l.StochRsi >= 80 && l.StochRsi > l.Signal && e.StochRsi <= e.Signal)
        {
            if(sma.Sma < (double)q.Low)
                cross = "SHORT";
        }
    }
}
