using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace CurrencyJob
{
    class Program
    {

        private static string BaseURL = "https://openexchangerates.org/api/latest.json";
        

        static void Main(string[] args)
        {
            var CurrencyMapper = GetCurrencyMapper();

            var JsonData = GetCurrencies().Result;

            var CurrencyPriceData = JsonConvert.DeserializeObject<CurrencyPriceData>(JsonData);

            var thing = RunDbQuery(CurrencyPriceData,CurrencyMapper).Result;

            

        }

        public static async Task<string> GetCurrencies()
        {

            HttpClient Client = new HttpClient();

            string RawJson = null;

            Client.BaseAddress = new Uri(BaseURL);
            Client.DefaultRequestHeaders.Accept.Clear();
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage Response = Client.GetAsync(UrlParameter).Result;

            if (Response.IsSuccessStatusCode)
            {
                RawJson = Response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("API call successful");
            }
            else
            {
                Console.WriteLine("Job failed to call API");
            }


            return RawJson;
        }

        public static async Task<string> RunDbQuery(CurrencyPriceData CurrencyPrices,Dictionary<int,string> CurrencyMapper)
        {
            bool Checked = false;
            int CurrencyId;
            double OldPrice;
            Dictionary<int, double> CurrencyPriceDict = new Dictionary<int, double>();

            var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dt = dt.AddSeconds(CurrencyPrices.timestamp);

            var Date = dt.ToString("yyyy-MM-dd H:mm:ss");

            SqlConnection Conn = new SqlConnection(ConnectionString);

            Conn.Open();

            var CurrencyDict = new Dictionary<string, string>();

            using (var command = Conn.CreateCommand())
            {
                command.CommandText = "SELECT top 168 * FROM CurrencyPrice ORDER BY Timestamp DESC;";
                SqlDataReader Reader = await command.ExecuteReaderAsync();

                
                while (Reader.Read())
                {

                    string LastTimestamp = Reader.GetValue(4).ToString();
                   /* Console.WriteLine("Day: " + LastTimestamp.Split('/')[0]);
                    Console.WriteLine("Month: "+ LastTimestamp.Split('/')[1]);
                    Console.WriteLine("Year: " + LastTimestamp.Split(' ')[0].Split('/')[2]);
                    Console.WriteLine("Hour: " + LastTimestamp.Split(' ')[1].Split(':')[0]);*/

                    int Day = int.Parse(LastTimestamp.Split('/')[0]);
                    int Month = int.Parse(LastTimestamp.Split('/')[1]);
                    int Year = int.Parse(LastTimestamp.Split(' ')[0].Split('/')[2]);
                    int Hour = int.Parse(LastTimestamp.Split(' ')[1].Split(':')[0]);

                    DateTime LastTime = new DateTime(Year,Month,Day);
                    LastTime.AddHours(Hour);

                    Console.WriteLine(LastTime.ToString());

                    if (!Checked&&LastTime.AddHours(1)>dt.AddMinutes(10))
                    {
                        Console.WriteLine("Already ran job within the last hour");
                        return null;
                    }
                    else if (!Checked)
                    {
                        Console.WriteLine("Job is good to run, last run at" + LastTime.ToString());
                    }
                    Checked = true;

                    CurrencyPriceDict.Add(int.Parse(Reader.GetValue(2).ToString()),double.Parse(Reader.GetValue(3).ToString()));
                }

                Reader.Close();
            }

            foreach (var entry in CurrencyPriceDict)
            {
                //need to make sure this doesnt run more than once an hour

                CurrencyId = entry.Key;
                OldPrice = entry.Value;

                CurrencyMapper.TryGetValue(CurrencyId, out string CurrencyCode);

                CurrencyPrices.rates.TryGetValue(CurrencyCode, out double NewPrice);

                NewPrice = 1 / NewPrice;

                var Change = Math.Round((((( NewPrice) - OldPrice) / OldPrice) * 100), 2);

                Console.WriteLine("CurrencyID: " + CurrencyId);
                Console.WriteLine("OldPrice: " + OldPrice);
                Console.WriteLine("NewPrice: " + NewPrice);
                Console.WriteLine("Change: " + Change);

                Console.WriteLine("INSERT INTO CurrencyPrice VALUES(" + Change + ","
                    + 999 + ","
                    + (NewPrice) + ","
                    + dt + ")");

                string CommandText = "INSERT INTO CurrencyPrice VALUES(" + Change + ", "
                    + CurrencyId + ","
                    + NewPrice + ","
                    + "@1)";

                var command = Conn.CreateCommand();
                command.Parameters.AddWithValue("@1",dt);

                command.CommandText = CommandText;

                SqlDataReader Reader = await command.ExecuteReaderAsync();
                Reader.Close();
            }

            return null;
        }

        public static Dictionary<int,string> GetCurrencyMapper()
        {

            Dictionary<int, string> dictionary = new Dictionary<int, string>();

            dictionary.Add(1, "AED");
            dictionary.Add(2, "OMR");
            dictionary.Add(3, "PAB");
            dictionary.Add(4, "PEN");
            dictionary.Add(5, "PGK");
            dictionary.Add(6, "PHP");
            dictionary.Add(7, "PKR");
            dictionary.Add(8, "PLN");
            dictionary.Add(9, "PYG");
            dictionary.Add(10, "RON");
            dictionary.Add(11, "NZD");
            dictionary.Add(12, "RSD");
            dictionary.Add(13, "RUB");
            dictionary.Add(14, "RWF");
            dictionary.Add(15, "SAR");
            dictionary.Add(16, "SBD");
            dictionary.Add(17, "SCR");
            dictionary.Add(18, "SDG");
            dictionary.Add(19, "QAR");
            dictionary.Add(20, "NPR");
            dictionary.Add(21, "NOK");
            dictionary.Add(22, "NIO");
            dictionary.Add(23, "LYD");
            dictionary.Add(24, "MAD");
            dictionary.Add(25, "MDL");
            dictionary.Add(26, "MGA");
            dictionary.Add(27, "MKD");
            dictionary.Add(28, "MMK");
            dictionary.Add(29, "MNT");
            dictionary.Add(30, "MOP");
            dictionary.Add(31, "MRO");
            dictionary.Add(32, "MUR");
            dictionary.Add(33, "MVR");
            dictionary.Add(34, "MWK");
            dictionary.Add(35, "MXN");
            dictionary.Add(36, "MYR");
            dictionary.Add(37, "MZN");
            dictionary.Add(38, "NAD");
            dictionary.Add(39, "NGN");
            dictionary.Add(40, "SEK");
            dictionary.Add(41, "SGD");
            dictionary.Add(42, "SHP");
            dictionary.Add(43, "SLL");
            dictionary.Add(44, "VEF");
            dictionary.Add(45, "VND");
            dictionary.Add(46, "VUV");
            dictionary.Add(47, "WST");
            dictionary.Add(48, "XAF");
            dictionary.Add(49, "XAG");
            dictionary.Add(50, "XAU");
            dictionary.Add(51, "XCD");
            dictionary.Add(52, "XDR");
            dictionary.Add(53, "XOF");
            dictionary.Add(54, "XPD");
            dictionary.Add(55, "XPF");
            dictionary.Add(56, "XPT");
            dictionary.Add(57, "YER");
            dictionary.Add(58, "ZAR");
            dictionary.Add(59, "ZMW");
            dictionary.Add(60, "ZWL");
            dictionary.Add(61, "UZS");
            dictionary.Add(62, "LSL");
            dictionary.Add(63, "UYU");
            dictionary.Add(64, "UGX");
            dictionary.Add(65, "SOS");
            dictionary.Add(66, "SRD");
            dictionary.Add(67, "SSP");
            dictionary.Add(68, "STD");
            dictionary.Add(69, "SVC");
            dictionary.Add(70, "SYP");
            dictionary.Add(71, "SZL");
            dictionary.Add(72, "THB");
            dictionary.Add(73, "TJS");
            dictionary.Add(74, "TMT");
            dictionary.Add(75, "TND");
            dictionary.Add(76, "TOP");
            dictionary.Add(77, "TRY");
            dictionary.Add(78, "TTD");
            dictionary.Add(79, "TWD");
            dictionary.Add(80, "TZS");
            dictionary.Add(81, "UAH");
            dictionary.Add(82, "USD");
            dictionary.Add(83, "LRD");
            dictionary.Add(84, "LKR");
            dictionary.Add(85, "LBP");
            dictionary.Add(86, "BWP");
            dictionary.Add(87, "BYN");
            dictionary.Add(88, "BZD");
            dictionary.Add(89, "CAD");
            dictionary.Add(90, "CDF");
            dictionary.Add(91, "CHF");
            dictionary.Add(92, "CLF");
            dictionary.Add(93, "CLP");
            dictionary.Add(94, "CNH");
            dictionary.Add(95, "CNY");
            dictionary.Add(96, "COP");
            dictionary.Add(97, "CRC");
            dictionary.Add(98, "CUC");
            dictionary.Add(99, "CUP");
            dictionary.Add(100, "CVE");
            dictionary.Add(101, "CZK");
            dictionary.Add(102, "DJF");
            dictionary.Add(103, "BTN");
            dictionary.Add(104, "BTC");
            dictionary.Add(105, "BSD");
            dictionary.Add(106, "BRL");
            dictionary.Add(107, "AFN");
            dictionary.Add(108, "ALL");
            dictionary.Add(109, "AMD");
            dictionary.Add(110, "ANG");
            dictionary.Add(111, "AOA");
            dictionary.Add(112, "ARS");
            dictionary.Add(113, "AUD");
            dictionary.Add(114, "AWG");
            dictionary.Add(115, "DKK");
            dictionary.Add(116, "AZN");
            dictionary.Add(117, "BBD");
            dictionary.Add(118, "BDT");
            dictionary.Add(119, "BGN");
            dictionary.Add(120, "BHD");
            dictionary.Add(121, "BIF");
            dictionary.Add(122, "BMD");
            dictionary.Add(123, "BND");
            dictionary.Add(124, "BOB");
            dictionary.Add(125, "BAM");
            dictionary.Add(126, "DOP");
            dictionary.Add(127, "DZD");
            dictionary.Add(128, "EGP");
            dictionary.Add(129, "IQD");
            dictionary.Add(130, "IRR");
            dictionary.Add(131, "ISK");
            dictionary.Add(132, "JEP");
            dictionary.Add(133, "JMD");
            dictionary.Add(134, "JOD");
            dictionary.Add(135, "JPY");
            dictionary.Add(136, "KES");
            dictionary.Add(137, "KGS");
            dictionary.Add(138, "KHR");
            dictionary.Add(139, "KMF");
            dictionary.Add(140, "KPW");
            dictionary.Add(141, "KRW");
            dictionary.Add(142, "KWD");
            dictionary.Add(143, "KYD");
            dictionary.Add(144, "KZT");
            dictionary.Add(145, "LAK");
            dictionary.Add(146, "INR");
            dictionary.Add(147, "IMP");
            dictionary.Add(148, "ILS");
            dictionary.Add(149, "GIP");
            dictionary.Add(150, "EUR");
            dictionary.Add(151, "FJD");
            dictionary.Add(152, "FKP");
            dictionary.Add(153, "GBP");
            dictionary.Add(154, "GEL");
            dictionary.Add(155, "GGP");
            dictionary.Add(156, "GHS");
            dictionary.Add(157, "IDR");
            dictionary.Add(158, "GMD");
            dictionary.Add(159, "GNF");
            dictionary.Add(160, "GTQ");
            dictionary.Add(161, "GYD");
            dictionary.Add(162, "HKD");
            dictionary.Add(163, "HNL");
            dictionary.Add(164, "HRK");
            dictionary.Add(165, "HTG");
            dictionary.Add(166, "HUF");
            dictionary.Add(167, "ETB");
            dictionary.Add(168, "ERN");


            return dictionary;
        }
    }
}
