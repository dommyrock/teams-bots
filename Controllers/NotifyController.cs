using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Post([FromBody]JObject conversationRef)
        //public async Task<IActionResult> Post([FromBody]ConversationRefModel conversationRef)//v1
        {
            //var simulatedConversationREf = _conversationReferences.First().Value;// initial msg to be sent to bot to get conversation ref
            JToken conversationJson = conversationRef["conversationReference"];
            var de_serializedRef = JsonConvert.DeserializeObject<ConversationReference>(conversationJson.ToString());
            JToken dataJson = conversationRef["data"];

            //map card daa
            var cardData = new
            {
                Id = dataJson["ProcesOdobravanjaId"].ToString(),
                Title = "Voditelj odjela ADBS 0.5 Test nad testovima",
                Vlasnik = dataJson["OwnerId"]["Name"].ToString(),
                BrojUgovora = dataJson["cmbs_brugovora"].ToString(),
                UgovornaStrana = "Hrvatska gospodarska komora ",
                TipUgovora = "Komercijalni ugovori",
                Naziv = dataJson["cmbs_name"].ToString(),
                PredmetUgovora = dataJson["cmbs_predmet_ugovora"].ToString(),
                UgovorenaVrijednost = "",
                SazetakZaOdobrenje = "",
                Ponuda = "",
                SharepointUrl = dataJson["cmbs_ugovor_sharepointurl"].ToString(),
                vrstaOdobrenja = int.Parse(dataJson["Vrsta"].ToString()),
                Statuscode = int.Parse(dataJson["statuscode"].ToString())
            };

            //.NET SDK
            ///SDK <see cref="https://docs.microsoft.com/en-us/adaptive-cards/templating/sdk"/> <see cref="https://blog.botframework.com/2017/06/07/adaptive-card-dotnet/"/>
            ///Docs <see cref="https://github.com/microsoft/AdaptiveCards/tree/main/source/dotnet/Library/AdaptiveCards"/>
            ///
            //TODO: re-render the card after CRM Service response
            /// <see cref="https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-actions#inbound-message-example"/>
            /// <seealso cref="https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/task-modules/task-modules-bots"/>
            /// IMPERSONATE ANOTHER USER <see cref="https://docs.microsoft.com/en-us/dynamics365/customerengagement/on-premises/developer/org-service/impersonate-another-user"/>

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));
            //card.Lang //LOCALIZATION
            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = cardData.Title,
                Size = AdaptiveTextSize.Medium,
                Color = AdaptiveTextColor.Light,
                Weight = AdaptiveTextWeight.Bolder,
                Wrap = true
            });

            foreach (var item in dataJson)
            {
                var colSet = new AdaptiveColumnSet();

                var label = new AdaptiveColumn()
                {
                    Width = "stretch",
                    Items = new List<AdaptiveElement>() { new AdaptiveTextBlock() { Wrap = true, Text = "Placeholder text", Color = AdaptiveTextColor.Accent, Weight = AdaptiveTextWeight.Bolder } },
                    VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center
                };
                colSet.Columns.Add(label);

                var data = new AdaptiveColumn()
                {
                    Width = "stretch",
                    Items = new List<AdaptiveElement>() { new AdaptiveTextBlock() { Wrap = true, Text = item.ToString() } },
                };

                colSet.Columns.Add(data);

                card.Body.Add(colSet);
            }
            if (cardData.Statuscode == 550990004 || cardData.Statuscode == 550990000)
            {
                card.Actions.Add(new AdaptiveSubmitAction()
                {
                    Title = "Odobri zahtjev",
                    Style = "positive",

                    Data = new
                    {
                        card_Id = "AdaptiveCombisCard",
                        ProcesOdobravanja_Id = dataJson["ProcesOdobravanjaId"].ToString(),
                    }
                });
            }

            if (cardData.Statuscode == 550990004 || cardData.Statuscode == 550990000)
            {
                card.Body.Add(new AdaptiveChoiceSetInput()
                {
                    Choices = new List<AdaptiveChoice>() {
                    new AdaptiveChoice() { Title = "Odobri", Value = "550990001" },
                    new AdaptiveChoice() { Title = "Odobri uz napomenu", Value = "550990003" },
                    new AdaptiveChoice() { Title = "Odbij", Value = "550990002" },
                   },
                    Id = "optionset",
                    Label = "Odaberite razlog",
                    Separator = true,
                    Wrap = true,
                    Spacing = AdaptiveSpacing.ExtraLarge,
                    IsRequired = false
                });
            }
            if (cardData.Statuscode == 550990004 || cardData.Statuscode == 550990000)//add this  && $root.vrstaOdobrenja != 550990001
            {
                card.Body.Add(new AdaptiveTextInput()
                {
                    IsMultiline = true,
                    Id = "komentar",
                    Label = "Komentar:",
                    Separator = true,
                    IsRequired = true,
                });
            }
            card.Body.Add(new AdaptiveTextBlock() { Text = $"[Ugovor]({cardData.SharepointUrl})" });
            card.Actions.Add(new AdaptiveOpenUrlAction()
            {
                Url = new Uri($"{cardData.SharepointUrl}"),
                IconUrl = "https://cdn2.iconfinder.com/data/icons/pittogrammi/142/95-512.png",
                Title = "Sharepoint",
                Style = "positive"
            }); ;

            //serialize card to JSON
            var cardJson = card.ToJson();
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(cardJson),
            };
            //Respond with custom card
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, de_serializedRef, (ITurnContext turnContext, CancellationToken cancellationToken) => turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment)), default(CancellationToken));
            //also  send oauth card for test

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