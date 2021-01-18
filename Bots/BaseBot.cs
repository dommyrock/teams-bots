using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Teams_Bots.Cards;
using Teams_Bots.Interfaces;
using Teams_Bots.Models;

namespace Teams_Bots.Bots
{
    public class BaseBot : TeamsActivityHandler
    {
        // Message to send to users when the bot receives a Conversation Update event

        public readonly IBaseBotService BaseBotService;
        private readonly IConfiguration Config;

        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public BaseBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, IConfiguration config, IBaseBotService baseBotService)
        {
            //Conversation references
            ///<see cref="https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=csharp"/>
            _conversationReferences = conversationReferences;
            Config = config;
            BaseBotService = baseBotService;
        }

        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            var conversationUsers = await BaseBotService.GetConversationUserDetailsAsync(turnContext, cancellationToken);

            //return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

        //protected override async Task OnInstallationUpdateActivityAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    var activityData = turnContext.Activity;
        //    var instalInfo = await BaseBotService.GetInstallationDetailsAsync(turnContext, cancellationToken);

        //    //return base.OnInstallationUpdateActivityAsync(turnContext, cancellationToken);
        //}

        //protected override async Task OnInstallationUpdateAddAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken)
        //{
        //    var activityData = turnContext.Activity;
        //    var instalInfo = await BaseBotService.GetInstallationDetailsAsync(turnContext, cancellationToken);

        //    //return base.OnInstallationUpdateAddAsync(turnContext, cancellationToken);
        //}

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                // Greet anyone that was not the target (recipient) of this message.
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text("Welcome to the Proactive Bot sample.  Navigate to http://localhost:3978/api/notify to proactively message everyone who has previously messaged this bot."), cancellationToken);
                }
            }
        }

        #region Reaction handlers

        protected override async Task OnReactionsAddedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("Nice reaction human 👀", speak: "Nice reaction human", cancellationToken: cancellationToken);
        }

        protected override async Task OnReactionsRemovedAsync(IList<MessageReaction> messageReactions, ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync("I the Bot approve ✔", cancellationToken: cancellationToken);
        }

        #endregion Reaction handlers

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            var user_info = await BaseBotService.GetUserDetailsAsync(turnContext, cancellationToken);

            var text = turnContext.Activity.Text?.Trim().ToLower();
            //value is null when we send text message  (when we receive card submit it has response obect)
            var value = turnContext.Activity.Value;

            if (!string.IsNullOrEmpty(text))
            {
                switch (text)
                {
                    case "adaptive":
                        ConversationReference conversationReference = turnContext.Activity.GetConversationReference();
                        await turnContext.Adapter.ContinueConversationAsync(Config["MicrosoftAppId"], conversationReference, BotCallback, default(CancellationToken));
                        break;

                    case "help":
                        var response = MessageFactory.Attachment(CardHelper.GetHeroCard().ToAttachment());
                        await turnContext.SendActivityAsync(response, cancellationToken);
                        break;

                    case "hi":
                        await BaseBotService.MentionUserActivityAsync(turnContext, cancellationToken);
                        break;

                    default:
                        if (text.Contains("all members"))
                            await BaseBotService.MessageAllMembersAsync(turnContext, cancellationToken);
                        break;
                        //await turnContext.SendActivityAsync(MessageFactory.Text($"Human {user_info.First().Properties["givenName"]} sent '{turnContext.Activity.Text}'"), cancellationToken);
                }
            }
            else
            {
                if (value != null)
                {
                    var cardObject = JsonConvert.DeserializeObject<CardObject>(value.ToString());
                    //if (cardObject.Card_Id == "AdaptiveCombisCard") V1 for testing
                    //{
                    //    //repond success
                    //    await turnContext.SendActivityAsync(MessageFactory.Text($"Data submitted for card {cardObject.Card_Id}."), cancellationToken);
                    //    await turnContext.SendActivityAsync(MessageFactory.Text($"With Comment: [{ cardObject.Comment}], and Date: {cardObject.DueDate.ToShortDateString()}"), cancellationToken);
                    //}
                    if (cardObject.Card_Id == "AdaptiveCombisCard")
                    {
                        //call integration service POST API to update process @
                        string url = $"http://localhost:5000/api/bot/proces-odobrenja?entityId={cardObject.ProcesOdobravanja_Id.ToString()}";

                        HttpClient client = new HttpClient();//see https://stackoverflow.com/questions/4015324/how-to-make-an-http-post-web-request
                        var response = await client.PostAsync(url, null);
                    }
                }
                //do nothing othervise  ( in case card from search extension is sent/copied to chat)
            }
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken)
        {
            var result = await BaseBotService.RunMessagingExtensionSelectItemAsync(turnContext, query, cancellationToken);
            return result;
        }

        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken)
        {
            var result = await BaseBotService.RunMessagingExtensionQueryAsync(turnContext, query, cancellationToken);
            return result;
        }

        //INSTALATION NOTES

        //Fetching team all team members data
        /// <see cref="https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages?tabs=dotnet#get-the-user-id-or-teamchannel-id"/>
        // NOTE: From this it seems that we can get all team-members data when we install the App/bot in a team
        //Proactive INSTALL: https://blog.thoughtstuff.co.uk/2020/07/its-now-much-easier-to-send-proactive-bot-messages-to-microsoft-teams-users-thanks-to-new-permissions/

        #region Helper methods

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        private async Task BotCallback(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            // If you encounter permission-related errors when sending this message, see
            // https://aka.ms/BotTrustServiceUrl

            //Added by Dominik
            //Send temp card when we get notified from external service (prod :external service will fetch CRM data and poipulate our template for Adaptive card)
            var cardAttachment = CreateAdaptiveCardAttachment(Path.Combine(".", "Resources", "AdaptiveCombisCard.json"));
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

        #endregion Helper methods
    }
}