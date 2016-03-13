using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace Job1
{
    public class LogInData
    {
        public string token { get; set; }
        public string session { get; set; }
        public user user { get; set; }
        public string assistantLoginEventName { get; set; }
        public string error { get; set; }

    }
    public class user
    {
        public int publicId { get; set; }
        public string email { get; set; }
        public settings settings { get; set; }
    }
    public class settings
    {
        public bool pushNotificationsEnabled { get; set; }
    }

    public class AllUserData
    {

        public DateTime updated { get; set; }
        public DateTime created { get; set; }
        public string categoryType { get; set; }
        public string id { get; set; }
        public DateTime date { get; set; }
        public DateTime originalDate { get; set; }
        public bool isExtraordinary { get; set; }
        public bool isCounterEntry { get; set; }
        public float amount { get; set; }
        public string categoryId { get; set; }
        public string subcategoryId { get; set; }
        public string description { get; set; }
        public string note { get; set; }
        public string accountGroupId { get; set; }
        public bool isSplitParent { get; set; }
        public string splitGroupId { get; set; }
        public string documentId { get; set; }
        public int[] quickCategoryzationSubcategoryIds { get; set; }
        public string uploadBatchId { get; set; }
        public string accountType { get; set; }
    }

    public class TotalBalance
    {
        public float debit { get; set; }
        public float credit { get; set; }

        public void AddAmount(float ammount)
        {
            if (ammount < 0)
            {
                AddCredit(ammount);
            }
            else
            {
                AddDebit(ammount);
            }
        }

        public float CreditMinusDebit()
        {
            return (credit - debit);
        }

        void AddCredit(float ammount)
        {
            credit += ammount;
        }
        void AddDebit(float ammount)
        {
            debit += ammount;
        }

    }


    class Program
    {
        public static LogInData logInData;
        public static AllUserData[] userData;
        public static bool GetRequestFinished = false;
        static void Main(string[] args)
        {
            string logInURL = @"https://spiirmobileapi-test.azurewebsites.net/Authentication/Logon";
            string siteUrl = @"https://spiirmobileapi-test.azurewebsites.net/Postings/Get/?$orderby=Date";

            GetDataFromServer(logInURL, siteUrl);
            while (!GetRequestFinished)
            {
                System.Threading.Thread.Sleep(100);
            }



            TotalBalance[] totalBalanceByMonth;
            totalBalanceByMonth = new TotalBalance[13];
            for (int i = 0; i < totalBalanceByMonth.Length; i++)
            {
                totalBalanceByMonth[i] = new TotalBalance();
            }
            for (int i = 0; i < userData.Length; i++)
            {
                totalBalanceByMonth[0].AddAmount(userData[i].amount );
                totalBalanceByMonth[userData[i].date.Month].AddAmount(userData[i].amount);
            }

            TotalBalance totalBalanceByWeekDay = new TotalBalance();

            for (int i = 0; i < userData.Length; i++)
            {
                if (userData[i].date.Day == 1)
                {
                    totalBalanceByWeekDay.AddAmount(userData[i].amount);
                }
            }

            for (int i = 1; i < totalBalanceByMonth.Length; i++)
            {
                if (i != 0)
                {
                    Console.WriteLine("Þú eyddir aðmeðaltali " + totalBalanceByMonth[i].CreditMinusDebit() / DateTime.DaysInMonth(2015, i) + " í " + i );
                }
            }

            Console.WriteLine("Þú átt " + totalBalanceByMonth[0].CreditMinusDebit().ToString() + " .kr inná reykning");
            Console.WriteLine("Þú eyðir " + totalBalanceByWeekDay.CreditMinusDebit().ToString() + " á mánudögum");

            


            Console.Read();

        }



        async static void GetRequest(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Session", logInData.session);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(logInData.session);

                using (HttpResponseMessage resp = await client.GetAsync(url))
                {
                    using (HttpContent content = resp.Content)
                    {
                        string answare = await content.ReadAsStringAsync();
                        userData = JsonConvert.DeserializeObject<AllUserData[]>(answare);
                        GetRequestFinished = true;
                    }
                }
            }
        }
        async static void GetDataFromServer(string logInUrl, string siteUrl)
        {
            IEnumerable<KeyValuePair<string, string>> queries = new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("email","job@spiir.dk"),
                    new KeyValuePair<string, string>("password", "test")
                };
            HttpContent q = new FormUrlEncodedContent(queries);
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.PostAsync(logInUrl, q))
                {
                    using (HttpContent content = response.Content)
                    {
                        string answer = await content.ReadAsStringAsync();
                        logInData = JsonConvert.DeserializeObject<LogInData>(answer);

                        GetRequest(siteUrl);
                    }
                }
            }
        }
    }

}
