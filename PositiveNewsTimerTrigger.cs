using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Azure.AI.TextAnalytics;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Net;
using Newtonsoft.Json;

namespace MadeByGPS.Function
{
    public static class PositiveNewsTimerTrigger
    {
        [FunctionName("PositiveNewsTimerTrigger")]
        public static void Run([TimerTrigger("0 30 6 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            // Initialize variables from local.settings.json
            string twilioSid = System.Environment.GetEnvironmentVariable ("TwilioSid");
            string twilioAuthToken = System.Environment.GetEnvironmentVariable ("TwilioAuthToken");
            string newsApiKey = System.Environment.GetEnvironmentVariable ("NewsApiKey");
            string fromNumber = System.Environment.GetEnvironmentVariable ("TwilioPhoneNumber");
            string toNumber = System.Environment.GetEnvironmentVariable ("MyPhoneNumber");
            TextAnalyticsApiKeyCredential textAnalyticsCredentials = new TextAnalyticsApiKeyCredential (System.Environment.GetEnvironmentVariable ("TextAnalyticsApiKeyCredential"));
            Uri textAnalyticsEndpoint = new Uri (System.Environment.GetEnvironmentVariable ("CognitiveServicesEndpoint"));

            // NEWS API Search parameters and URL
            string searchKeyword = "Covid";
            string sortBy = "publishedAt";
            string pageSize = "50";
            string searchLanguage = "en";
            var newAPIEndpointURL = $"https://newsapi.org/v2/everything?sortBy={sortBy}&pageSize={pageSize}&language={searchLanguage}&q={searchKeyword}&apiKey={newsApiKey}";

             // 1. Get json

            string jsonFromAPI = GetNewsFromAPI (newAPIEndpointURL);

            // 2. Deserialize into objects

            News news = JsonConvert.DeserializeObject<News> (jsonFromAPI);

            // 3. Initialize TextAnalyticsClient for sentiment detection, TwilioClient to send the message

            TextAnalyticsClient textAnalyticsClient = new TextAnalyticsClient (textAnalyticsEndpoint, textAnalyticsCredentials);

            TwilioClient.Init (twilioSid, twilioAuthToken);

            // 4. Perform sentiment detection on article titles

            string sentimentLabel = "negative";

            if (news.status.Equals ("ok")) {

                foreach (var article in news.articles) {

                    sentimentLabel = SentimentDetection (textAnalyticsClient, article.title);
                    // 5. Once a a news article with positive sentiment is found, send via text and break out of foreach loop.
                    if (sentimentLabel.Equals ("Positive")) {

                        log.LogInformation ("Found positive story: " + article.url);
                        SendMessage (fromNumber, toNumber, article.url, article.title);
                        break;
                    }
                }
            } else {
                log.LogInformation ("Error with API call");
            }      

        }
    
    
    
        static string GetNewsFromAPI (string url) {
            string jsonFromAPI = new WebClient ().DownloadString (url);
            return jsonFromAPI;
        }

        static string SentimentDetection (TextAnalyticsClient client, string textToAnalyze) {
            string sentimentLabel = "negative";

            DocumentSentiment documentSentiment = client.AnalyzeSentiment (textToAnalyze);
            sentimentLabel = (documentSentiment.Sentiment.ToString ());
            return sentimentLabel;

        }

        static void SendMessage (string fromNumber, string toNumber, string articleUrl, string articleTitle) {

            var message = MessageResource.Create (

                body: "Here is your Covid positive news story of the day 😊: \n\n" + articleUrl + "\n" + articleTitle,
                from : new Twilio.Types.PhoneNumber (fromNumber),
                to : new Twilio.Types.PhoneNumber (toNumber)

            );

        }
    
    }
}
