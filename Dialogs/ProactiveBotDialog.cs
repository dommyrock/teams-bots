using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Teams_Bots.Cards;

namespace Teams_Bots.Dialogs
{
    public class ProactiveBotDialog : ComponentDialog
    {
        protected readonly ILogger _logger;

        /* WaterfallDialog
         * Dialog optimized for prompting a user with a series of questions.
         * Waterfalls accept a stack of functions which will be executed in sequence.
         * Each waterfall step can ask a question of the user and the user's response will be passed as an argument to the next waterfall step.
         */

        public ProactiveBotDialog(ILogger<ProactiveBotDialog> logger)
            : base(nameof(ProactiveBotDialog))
        {
            _logger = logger;
            // Define the main dialog and its related components.
            //AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                SendIntroCardAsync
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> SendIntroCardAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            ///NOT Supported in TEAMS <see cref="https://docs.microsoft.com/en-us/microsoftteams/platform/task-modules-and-cards/cards/cards-reference#cards-not-supported-in-teams"/>

            _logger.LogInformation("ProactiveBotDialog.ShowCardStepAsync");

            // Cards are sent as Attachments in the Bot Framework.
            // So we need to create a list of attachments for the reply activity.
            var attachments = new List<Attachment>();

            // Reply to the activity we received with an activity.
            var reply = MessageFactory.Attachment(attachments);

            // Display a carousel of all the rich card types. (if its not supported it will display as list in Teams)
            reply.AttachmentLayout = AttachmentLayoutTypes.Carousel;

            reply.Attachments.Add(CardHelper.GetHeroCard().ToAttachment());
            reply.Attachments.Add(CardHelper.GetSigninCard().ToAttachment());

            await stepContext.Context.SendActivityAsync(reply, cancellationToken);

            return await stepContext.EndDialogAsync();
        }
    }
}