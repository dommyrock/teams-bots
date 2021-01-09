using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Teams_Bots.Bots
{
    public class WaterfallDialogBot<T> : TeamsActivityHandler where T : Dialog
    {
        private string _appId;
        private string _appPassword;

        // Dependency injected dictionary for storing ConversationReference objects used in NotifyController to proactively message users
        protected readonly ConcurrentDictionary<string, ConversationReference> ConversationReferences;

        protected readonly BotState ConversationState;
        protected readonly Dialog Dialog;
        protected readonly ILogger Logger;
        protected readonly BotState UserState;

        public WaterfallDialogBot(ConversationState conversationState, UserState userState, T dialog, ILogger<WaterfallDialogBot<T>> logger, ConcurrentDictionary<string, ConversationReference> conversationReferences, IConfiguration config)
        {
            //Conversation references
            ///<see cref="https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=csharp"/>
            ConversationReferences = conversationReferences;
            // config read from /appsettings.json
            _appId = config["MicrosoftAppId"];
            _appPassword = config["MicrosoftAppPassword"];

            //Added for DialogBot
            ConversationState = conversationState;
            UserState = userState;
            Dialog = dialog;
            Logger = logger;
        }

        private void AddConversationReference(Activity activity)
        {
            var conversationReference = activity.GetConversationReference();
            ConversationReferences.AddOrUpdate(conversationReference.User.Id, conversationReference, (key, newValue) => conversationReference);
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
            Logger.LogInformation("Running dialog with Message Activity inside [ ProactiveBot<MainDialog> ].");

            AddConversationReference(turnContext.Activity as Activity);

            var text = turnContext.Activity.Text?.Trim().ToLower();
            var value = turnContext.Activity.Value;//value is null when we send text message  (when we receive card submit it has response obect)

            if (!string.IsNullOrEmpty(text))
            {
                switch (text)
                {
                    case "help":
                        await Dialog.RunAsync(turnContext, ConversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
                        break;

                    default:
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
    }
}