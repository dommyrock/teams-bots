using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Teams_Bots.Cards;

namespace Teams_Bots.Bots
{
    public class BaseBot : TeamsActivityHandler
    {
        // Message to send to users when the bot receives a Conversation Update event

        private string _appId;
        private string _appPassword;

        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        private readonly ConcurrentDictionary<string, ConversationReference> _conversationReferences;

        public BaseBot(ConcurrentDictionary<string, ConversationReference> conversationReferences, IConfiguration config)
        {
            //Conversation references
            ///<see cref="https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=csharp"/>
            _conversationReferences = conversationReferences;
            // config read from /appsettings.json
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            _conversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
        }

        protected override Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            return base.OnConversationUpdateActivityAsync(turnContext, cancellationToken);
        }

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
            await turnContext.SendActivityAsync("I approve ✔", cancellationToken: cancellationToken);
        }

        #endregion Reaction handlers

        //Fetching team all team members data
        /// <see cref="https://docs.microsoft.com/en-us/microsoftteams/platform/bots/how-to/conversations/send-proactive-messages?tabs=dotnet#get-the-user-id-or-teamchannel-id"/>
        // NOTE: From this it seems that we can get all team-members data when we install the App/bot in a team
        //Proactive INSTALL: https://blog.thoughtstuff.co.uk/2020/07/its-now-much-easier-to-send-proactive-bot-messages-to-microsoft-teams-users-thanks-to-new-permissions/

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            AddConversationReference(turnContext.Activity as Activity);

            var user_info = await GetUserDetailsAsync(turnContext, cancellationToken);

            var text = turnContext.Activity.Text?.Trim().ToLower();
            var value = turnContext.Activity.Value;//value is null when we send text message  (when we receive card submit it has response obect)

            if (!string.IsNullOrEmpty(text))
            {
                switch (text)
                {
                    case "all members":
                        //Gets all team members details and sends them notification
                        await MessageAllMembersAsync(turnContext, cancellationToken);
                        break;

                    case "adaptive":
                        ConversationReference conversationReference = turnContext.Activity.GetConversationReference();
                        await turnContext.Adapter.ContinueConversationAsync(_appId, conversationReference, BotCallback, default(CancellationToken));
                        break;

                    case "help":
                        var response = MessageFactory.Attachment(CardHelper.GetHeroCard().ToAttachment());
                        await turnContext.SendActivityAsync(response, cancellationToken);
                        break;

                    case "hi":
                        await MentionUserActivityAsync(turnContext, cancellationToken);
                        break;

                    default:
                        // Echo back what the user said
                        await turnContext.SendActivityAsync(MessageFactory.Text($"Human {user_info.First().Properties["givenName"]} sent '{turnContext.Activity.Text}'"), cancellationToken);
                        break;
                }
            }
            else
            {
                var cardObject = JsonConvert.DeserializeObject<CardObject>(value.ToString());
                if (cardObject.Card_Id == "AdaptiveCombisCard")
                {
                    //repond success
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Data submitted for card {cardObject.Card_Id}."), cancellationToken);
                    await turnContext.SendActivityAsync(MessageFactory.Text($"With Comment: [{ cardObject.Comment}], and Date: {cardObject.DueDate.ToShortDateString()}"), cancellationToken);
                }
            }
        }

        #region Helper methods

        /// <summary>
        /// This method gets user details , including his teams email
        /// </summary>
        /// <returns></returns>
        private async Task<IList<ChannelAccount>> GetUserDetailsAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            /// GET Teams User Email and other data
            /// <see cref="https://stackoverflow.com/questions/56918152/is-it-possible-to-get-user-email-from-ms-teams-with-a-bot-using-sdk4"/>

            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            var connector = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), credentials);
            var conversationId = turnContext.Activity.Conversation.Id;
            var userInfo = await connector.Conversations.GetConversationMembersAsync(conversationId);
            return userInfo;
        }

        // If you encounter permission-related errors when sending this message, see
        // https://aka.ms/BotTrustServiceUrl
        private async Task MessageAllMembersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var teamsChannelId = turnContext.Activity.TeamsGetChannelId();
            var serviceUrl = turnContext.Activity.ServiceUrl;
            var credentials = new MicrosoftAppCredentials(_appId, _appPassword);
            ConversationReference conversationReference = null;

            var members = await GetPagedMembers(turnContext, cancellationToken);

            foreach (var teamMember in members)
            {
                var proactiveMessage = MessageFactory.Text($"Hello {teamMember.GivenName} {teamMember.Surname}. I'm a Teams conversation bot.");

                //NOTE: ConversationParameters class contains team, group, chat users data
                var conversationParameters = new ConversationParameters
                {
                    IsGroup = false,
                    Bot = turnContext.Activity.Recipient,
                    Members = new ChannelAccount[] { teamMember },
                    TenantId = turnContext.Activity.Conversation.TenantId,
                };

                await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                    teamsChannelId,
                    serviceUrl,
                    credentials,
                    conversationParameters,
                    async (t1, c1) =>
                    {
                        conversationReference = t1.Activity.GetConversationReference();
                        await ((BotFrameworkAdapter)turnContext.Adapter).ContinueConversationAsync(
                            _appId,
                            conversationReference,
                            async (t2, c2) =>
                            {
                                await t2.SendActivityAsync(proactiveMessage, c2);
                            },
                            cancellationToken);
                    },
                    cancellationToken);
            }

            await turnContext.SendActivityAsync(MessageFactory.Text("All messages have been sent."), cancellationToken);
        }

        private static async Task<List<TeamsChannelAccount>> GetPagedMembers(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var members = new List<TeamsChannelAccount>();
            string continuationToken = null;

            do
            {
                var currentPage = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
                continuationToken = currentPage.ContinuationToken;
                members = members.Concat(currentPage.Members).ToList();
            }
            while (continuationToken != null);

            return members;
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

        private static async Task MentionUserActivityAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var mention = new Mention
            {
                Mentioned = turnContext.Activity.From,
                Text = $"<at>{XmlConvert.EncodeName(turnContext.Activity.From.Name)}</at>",
            };

            var replyActivity = MessageFactory.Text($"Hello human {mention.Text}");
            replyActivity.Entities = new List<Entity> { mention };

            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        #endregion Helper methods
    }

    class CardObject
    {
        public DateTime DueDate { get; set; }
        public string Comment { get; set; }
        public string Card_Id { get; set; }
    }
}