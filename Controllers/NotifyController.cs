using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using AdaptiveCards.Templating;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Teams_Bots.Models;

namespace Teams_Bots.Controllers
{
    [Route("api/notify")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter _adapter;
        private readonly string _appId;
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        private readonly string[] _cards =
        {
            Path.Combine(".", "Resources", "AdaptiveCombisCard.json"),
        };

        public NotifyController(IBotFrameworkHttpAdapter adapter, IConfiguration configuration, ConcurrentDictionary<string, ConversationReference> conversationReferences)
        {
            _adapter = adapter;
            _conversationReferences = conversationReferences;
            _appId = configuration["MicrosoftAppId"] ?? string.Empty;
        }

        //Templating SDK DOCS <see
        /// <see cref="https://docs.microsoft.com/en-us/adaptive-cards/templating/sdk"/>

        public async Task<IActionResult> Get()
        {
            foreach (var conversationReference in _conversationReferences.Values)
            {
                //Respond to chatbot endpoint /api/messages
                await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
            }

            // Let the caller know proactive messages have been sent
            return new ContentResult()
            {
                Content = $"<html><body><h1>Proactive card have been sent.</h1><h1>Card Path: {_cards[0]}</h1></body></html>",
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
            };
        }

        ///SDK <see cref="https://docs.microsoft.com/en-us/adaptive-cards/templating/sdk"/> <see cref="https://blog.botframework.com/2017/06/07/adaptive-card-dotnet/"/>
        ///Docs <see cref="https://github.com/microsoft/AdaptiveCards/tree/main/source/dotnet/Library/AdaptiveCards"/>

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Post([FromBody]JObject conversationRef)
        //public async Task<IActionResult> Post([FromBody]ConversationRefModel conversationRef)//v1
        {
            //var simulatedConversationREf = _conversationReferences.First().Value;// initial msg to be sent to bot to get conversation ref
            JToken conversationJson = conversationRef["conversationReference"];
            var de_serializedRef = JsonConvert.DeserializeObject<ConversationReference>(conversationJson.ToString());
            JToken dataJson = conversationRef["data"];

            //var ugovorJson = System.IO.File.ReadAllText(Path.Combine(".", "Resources", "UgovorCard.json"));
            var ugovorJson = System.IO.File.ReadAllText(Path.Combine(".", "Resources", "TestCard.json"));
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(ugovorJson);

            //example
            var cardData = new
            {
                Title = "Publish Adaptive Card Schema",
                Description = "Now that we have defined the main rules and features of the format, we need to produce a schema and publish it to GitHub. The schema will be the starting point of our reference documentation.",
                Creator = new
                {
                    Name = "Matt Hidinger",
                    ProfileImage = "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg"
                },
                CreatedUtc = "2017-02-14T06:08:39Z",
                ViewUrl = "https://adaptivecards.io",
                Id = dataJson["ProcesOdobravanjaId"].ToString()
            };

            //map card daa
            //var cardData = new
            //{
            //    Title = "Voditelj odjela ADBS 0.5 Test nad testovima",
            //    Vlasnik = dataJson["OwnerId"]["Name"].ToString(),
            //    BrojUgovora = dataJson["cmbs_brugovora"].ToString(),
            //    UgovornaStrana = "Hrvatska gospodarska komora ",
            //    TipUgovora = "Komercijalni ugovori",
            //    Naziv = dataJson["cmbs_name"].ToString(),
            //    PredmetUgovora = dataJson["cmbs_predmet_ugovora"].ToString(),
            //    UgovorenaVrijednost = "",
            //    SazetakZaOdobrenje = "",
            //    Ponuda = "",
            //    SharepointUrl = dataJson["cmbs_ugovor_sharepointurl"].ToString(),
            //    vrstaOdobrenja = int.Parse(dataJson["Vrsta"].ToString()),
            //    Statuscode = int.Parse(dataJson["statuscode"].ToString())
            //};

            string cardJson = template.Expand(cardData);

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };

            //Respond to chatbot endpoint /api/messages
            //await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, de_serializedRef, (ITurnContext turnContext, CancellationToken cancellationToken) => turnContext.SendActivityAsync(MessageFactory.Text(dataJson.ToString())), default(CancellationToken));

            //.NET SDK
            //AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            //var colSet = new AdaptiveColumnSet();
            //card.Body.Add(colSet);

            ////Make column collection
            //var columnCollection = new List<AdaptiveColumn>()
            //{
            //    new AdaptiveColumn()
            //    {
            //    }
            //};
            //colSet.Columns.AddRange(columnCollection);

            //card.Body.Add(new AdaptiveTextBlock()
            //{
            //    Text = "Voditelj odjela ADBS 0.5 Test nad testovima",
            //    Size = AdaptiveTextSize.Medium,
            //    Color = AdaptiveTextColor.Light
            //});
            ////card.Body.Add(new adaptivete()
            ////{
            ////    Text = "Hello",

            ////    Size = AdaptiveTextSize.Default
            ////});

            //card.Body.Add(new AdaptiveImage()
            //{
            //    Url = new Uri("http://adaptivecards.io/content/cats/1.png")
            //});

            //Respond with custom card
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, de_serializedRef, (ITurnContext turnContext, CancellationToken cancellationToken) => turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment)), default(CancellationToken));

            //Example of sending card as attachment
            //var cardAttachment = CreateAdaptiveCardAttachment(_cards[0]); //in prod replaced by niko templating logic
            //await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, conversationRef.ConversationReference, (ITurnContext turnContext, CancellationToken cancellationToken) => turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment)), default(CancellationToken));
            return Created("", dataJson);
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl

            //Added by Dominik
            //Send temp card when we get notified from external service (prod :external service will fetch CRM data and poipulate our template for Adaptive card)
            var cardAttachment = CreateAdaptiveCardAttachment(_cards[0]);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
        }

        #region Helpers

        private static Attachment CreateAdaptiveCardAttachment(string filePath)
        {
            var adaptiveCardJson = System.IO.File.ReadAllText(filePath);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(adaptiveCardJson),
            };
            return adaptiveCardAttachment;
        }

        #endregion Helpers
    }
}