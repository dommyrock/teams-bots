﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Post([FromBody]JObject conversationRef)
        //public async Task<IActionResult> Post([FromBody]ConversationRefModel conversationRef)//v1
        {
            //var simulatedConversationREf = _conversationReferences.First().Value;// initial msg to be sent to bot to get conversation ref
            JToken conversationJson = conversationRef["conversationReference"];
            var de_serializedRef = JsonConvert.DeserializeObject<ConversationReference>(conversationJson.ToString());
            JToken dataJson = conversationRef["data"];

            //Respond to chatbot endpoint /api/messages
            await ((BotAdapter)_adapter).ContinueConversationAsync(_appId, de_serializedRef, (ITurnContext turnContext, CancellationToken cancellationToken) => turnContext.SendActivityAsync(MessageFactory.Text(dataJson.ToString())), default(CancellationToken));

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