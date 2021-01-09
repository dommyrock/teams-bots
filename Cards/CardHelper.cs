using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Teams_Bots.Cards
{
    public static class CardHelper
    {
        public static Attachment CreateAdaptiveCardAttachment()
        {
            // combine path for cross platform support
            var paths = new[] { ".", "Resources", "adaptiveCard.json" };
            var adaptiveCardJson = File.ReadAllText(Path.Combine(paths));

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };

            return adaptiveCardAttachment;
        }

        public static HeroCard GetHeroCard()
        {
            var heroCard = new HeroCard
            {
                Title = "Welcome to Bot Framework!",
                Text = @"We use this opportunity to recommend a few next steps
                        for learning more creating and deploying bots.",
                Images = new List<CardImage>() { new CardImage("https://bot-framework.azureedge.net/static/200552-53fd7e087c/intercom-webui/v1.6.2/assets/landing-page/images/BotFrameworkDiagram.png") },
                Buttons = new List<CardAction>()
                {
                    new CardAction(ActionTypes.OpenUrl, "Get an overview", null, "Get an overview", "Get an overview", "https://docs.microsoft.com/en-us/azure/bot-service/?view=azure-bot-service-4.0"),
                    new CardAction(ActionTypes.OpenUrl, "Bot code samples", null, "Bot code samples", "Bot code samples", "https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore"),
                    new CardAction(ActionTypes.OpenUrl, "CombisBot Github repo", null, "CombisBot Github repo", "CombisBot Github repo", "https://github.com/dommyrock/teams-bots"),
                    new CardAction(ActionTypes.OpenUrl, "Teams rich cards", null, "Teams rich cards", "Teams rich cards", "https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#cards-not-supported-in-teams"),
                    new CardAction(ActionTypes.OpenUrl, "Intro video", null, "Intro video", "Intro video", "https://www.youtube.com/watch?v=czlvzetIdfc"),
                    new CardAction(ActionTypes.OpenUrl, "Outlook Demo", null, "Outlook Demo", "Outlook Demo", "https://www.youtube.com/watch?t=2566&v=X6Cs-MIefyo&feature=youtu.be"),
                }
            };

            return heroCard;
        }

        public static ThumbnailCard GetThumbnailCard()
        {
            var thumbnailCard = new ThumbnailCard
            {
                Title = "BotFramework Thumbnail Card",
                Subtitle = "Microsoft Bot Framework",
                Text = "Build and connect intelligent bots to interact with your users naturally wherever they are," +
                       " from text/sms to Skype, Slack, Office 365 mail and other popular services.",
                Images = new List<CardImage> { new CardImage("https://sec.ch9.ms/ch9/7ff5/e07cfef0-aa3b-40bb-9baa-7c9ef8ff7ff5/buildreactionbotframework_960.jpg") },
                Buttons = new List<CardAction> { new CardAction(ActionTypes.OpenUrl, "Get Started", value: "https://docs.microsoft.com/bot-framework") },
            };

            return thumbnailCard;
        }

        //Other supported examples in teams

        public static ReceiptCard GetReceiptCard()
        {
            var receiptCard = new ReceiptCard
            {
                Title = "John Doe",
                Facts = new List<Fact> { new Fact("Order Number", "1234"), new Fact("Payment Method", "VISA 5555-****") },
                Items = new List<ReceiptItem>
                {
                    new ReceiptItem(
                        "Data Transfer",
                        price: "$ 38.45",
                        quantity: "368",
                        image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/traffic-manager.png")),
                    new ReceiptItem(
                        "App Service",
                        price: "$ 45.00",
                        quantity: "720",
                        image: new CardImage(url: "https://github.com/amido/azure-vector-icons/raw/master/renders/cloud-service.png")),
                },
                Tax = "$ 7.50",
                Total = "$ 90.95",
                Buttons = new List<CardAction>
                {
                    new CardAction(
                        ActionTypes.OpenUrl,
                        "More information",
                        "https://account.windowsazure.com/content/6.10.1.38-.8225.160809-1618/aux-pre/images/offer-icon-freetrial.png",
                        value: "https://azure.microsoft.com/en-us/pricing/"),
                },
            };

            return receiptCard;
        }

        public static SigninCard GetSigninCard()
        {
            var signinCard = new SigninCard
            {
                Text = "BotFramework Sign-in Card",
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign-in", value: "https://login.microsoftonline.com/") },
            };

            return signinCard;
        }

        public static OAuthCard GetOAuthCard()
        {
            var oauthCard = new OAuthCard
            {
                Text = "BotFramework OAuth Card",
                ConnectionName = "OAuth connection", // Replace with the name of your Azure AD connection.
                Buttons = new List<CardAction> { new CardAction(ActionTypes.Signin, "Sign In", value: "https://example.org/signin") },
            };

            return oauthCard;
        }
    }
}