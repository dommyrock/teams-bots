using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using Teams_Bots.Dialogs;

namespace Teams_Bots.Bots
{
    public class ProactiveDialogBot : WaterfallDialogBot<ProactiveBotDialog>
    {
        //NOTE :Config gets pulled from /appsetting.json
        //NOTE: If this is overkill implementation, use ProactiveBot without dialog extension
        public ProactiveDialogBot(ConversationState conversationState, UserState userState, ProactiveBotDialog dialog, ILogger<WaterfallDialogBot<ProactiveBotDialog>> logger, ConcurrentDictionary<string, ConversationReference> conversationReferences, IConfiguration config)
                  : base(conversationState, userState, dialog, logger, conversationReferences, config)
        {
        }
    }
}