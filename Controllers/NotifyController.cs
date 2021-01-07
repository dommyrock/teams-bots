using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl

            //Added by Dominik
            //Send card when we get notified from external service (
            var cardAttachment = CreateAdaptiveCardAttachment(_cards[0]);
            await turnContext.SendActivityAsync(MessageFactory.Attachment(cardAttachment), cancellationToken);
        }

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
    }
}