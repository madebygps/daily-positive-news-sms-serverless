using System;
using System.Linq;
using System.Net;
using Azure.AI.TextAnalytics;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace MadeByGPS.Function {
    public static class PositiveNewsTimerTrigger {
        [FunctionName ("PositiveNewsTimerTrigger")]
        public static void Run ([TimerTrigger ("0 30 6 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log) {
            log.LogInformation ($"C# Timer trigger function executed at: {DateTime.Now}");

            // Initialize variables from local.settings.json
            string twilioSid = System.Environment.GetEnvironmentVariable ("TwilioSid");
            string twilioAuthToken = System.Environment.GetEnvironmentVariable ("TwilioAuthToken");
            string newsApiKey = System.Environment.GetEnvironmentVariable ("NewsApiKey");
            string fromNumber = System.Environment.GetEnvironmentVariable ("TwilioPhoneNumber");
            string toNumber = System.Environment.GetEnvironmentVariable ("MyPhoneNumber");
            TextAnalyticsApiKeyCredential textAnalyticsCredentials = new TextAnalyticsApiKeyCredential (System.Environment.GetEnvironmentVariable ("TextAnalyticsApiKeyCredential"));
            Uri textAnalyticsEndpoint = new Uri (System.Environment.GetEnvironmentVariable ("CognitiveServicesEndpoint"));

            // Incase URL of article image is null, we will use this royalty free stock photo.
            string newspaperImageURL = "https://images.unsplash.com/photo-1504711331083-9c895941bf81?ixlib=rb-1.2.1&ixid=eyJhcHBfaWQiOjEyMDd9&auto=format&fit=crop&w=2550&q=80";

            // NEWS API Search parameters and URL
            string searchKeyword = "Covid";
            string sortBy = "relevancy";
            string pageSize = "100";
            string searchLanguage = "en";
            string fromDate = DateTime.Today.AddDays (-1).ToString ("yyyy-MM-dd");
            double imageFileSize = 0.0;

            var newAPIEndpointURL = $"https://newsapi.org/v2/everything?from={fromDate}&sortBy={sortBy}&pageSize={pageSize}&language={searchLanguage}&q={searchKeyword}&apiKey={newsApiKey}";
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

                        // Checks if there is a url for image of article, if there isn't it will default to newspaperImageURL.
                        article.urlToImage = !String.IsNullOrEmpty (article.urlToImage) ? article.urlToImage : newspaperImageURL;

                        // If article image was changes to newspaperImageURL, no need to check size because newspaperImageURL is only 321KB
                        if (!(article.urlToImage.Equals (newspaperImageURL))) {
                            // Gets image file size and logs to console.
                            imageFileSize = GetMediaFileSize (article.urlToImage);
                            log.LogInformation ("The image size is: " + imageFileSize);

                            // MMS size limit is 5 MB.
                            if ((imageFileSize > 4.9)) {
                                article.urlToImage = newspaperImageURL;
                            }

                        }

                        SendMessage (fromNumber, toNumber, article.url, article.title, article.urlToImage);

                        break;
                    }
                }
            } else {
                log.LogInformation ("Error with NEWS API call");
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

        static double GetMediaFileSize (string imageUrl) {
            var fileSizeInMegaByte = 0.0;
            var webRequest = HttpWebRequest.Create (imageUrl);
            webRequest.Method = "HEAD";

            using (var webResponse = webRequest.GetResponse ()) {
                var fileSize = webResponse.Headers.Get ("Content-Length");
                fileSizeInMegaByte = Math.Round (Convert.ToDouble (fileSize) / 1024.0 / 1024.0, 2);
            }

            return fileSizeInMegaByte;
        }

        static void SendMessage (string fromNumber, string toNumber, string articleUrl, string articleTitle, string imageUrl) {

            var mediaUrl = new [] {
                new Uri (imageUrl)
            }.ToList ();

            try {

                var message = MessageResource.Create (
                    body: articleTitle + "\n\n" + articleUrl,
                    from: new Twilio.Types.PhoneNumber (fromNumber),
                    mediaUrl: mediaUrl,
                    to: new Twilio.Types.PhoneNumber (toNumber)
                );

            } catch (Exception ex) {

            }
        }
    }
}