using System;
using System.Collections.Generic;
using System.Text;

namespace CurrencyJob
{
    class CurrencyPrice
    {
        public int CurrencyPriceId { get; set; }
        public int CurrencyId { get; set; }
        public double Price { get; set; }
        public DateTime TimeStamp { get; set; }
        public double ChangePercentage { get; set; }
    }
}
