# daily-positive-news-sms-serverless
A serverless app that sends via text message a positive news story about COVID-19.

# How to setup code environment

[Follow this tutorial and you will install VS Code and the necessary Azure extensions needed.](https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-function-vs-code?pivots=programming-language-csharp)

# Setup API keys and credentials

You will need:
- [TwilioSid, TwilioAuthToken, TwilioPhoneNumber](https://www.twilio.com/docs/usage/tutorials/how-to-use-your-free-trial-account)
- [Azure account, CognitiveServicesEndpoint and TextAnalyticsApiKeyCredential](https://docs.microsoft.com/en-us/azure/cognitive-services/text-analytics/quickstarts/text-analytics-sdk?tabs=version-3&pivots=programming-language-csharp)
- [NewsApiKey](https://newsapi.org/docs/get-started)

# Packages used

These should be included in the project when you clone it, however if there is some error, you can reinstall them.

## [Twilio](https://www.twilio.com/docs/sms/quickstart/csharp-dotnet-core])

[Install via .NET CLI](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-dotnet-cli)
```shell
dotnet add package Twilio
```
Use
```csharp
using Twilio;
using Twilio.Rest.Api.V2010.Account;
```

## [TextAnalytics v3 preview](https://www.nuget.org/packages/Azure.AI.TextAnalytics/1.0.0-preview.3)

[Install via .NET CLI](https://docs.microsoft.com/en-us/nuget/consume-packages/install-use-packages-dotnet-cli)
```shell
dotnet add package Azure.AI.TextAnalytics --version 1.0.0-preview.3
```
Use
```csharp
using Azure.AI.TextAnalytics;
```
# How to setup local.settings.json

I've excluded my local.settings.json file for obvious reasons. Make sure to include these records in there once you have them. You should have set these up in the setup API keys and credentials step.

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "<replace_with_your_webjobsstorage>",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "TextAnalyticsApiKeyCredential":"<replace>",
    "CognitiveServicesEndpoint":"<replace>",
    "TwilioSid":"<replace>",
    "TwilioAuthToken":"<replace>",
    "NewsApiKey":"<replace>",
    "TwilioPhoneNumber":"<replace>",
    "MyPhoneNumber":"<replace_with_number_you_ant_to_send_sms_to>",
    "WEBSITE_TIME_ZONE":""
  }
}
```

# How to execute

In VS code, select the run Tab on the left, then hit the Play button on the top.

![How to run](howtorun.png "How to run")

# Demo

You will get a text to the number you put into your local.settings.json

![SMS text](smstext.png "SMS text")

In the VS code console output you will also see the story it sent you.

![Console output](console.png "Console output")

You will also see it in your [Twilio SMS dashboard](https://www.twilio.com/console/sms)

![Twilio dash](twiliodash.png "Twilio dash")


# Known issues and areas of improvement

- You can fine tune the JSON returned from News API 
```json
```

