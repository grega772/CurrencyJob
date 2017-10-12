using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyJob
{
    public class CurrencyPriceData
    {
        public string disclaimer { get; set; }
        public string license { get; set; }
        public int timestamp { get; set; }
        public string Base { get; set; }
        public Dictionary<string, double> rates { get; set; }
    }
}
