using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Teams_Bots.Interfaces
{
    public interface IBaseBotService
    {
        /// <summary>
        /// This method gets user details , including his teams email
        /// </summary>
        /// <returns></returns>
        public Task<IList<ChannelAccount>> GetUserDetailsAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);

        public Task<IList<ChannelAccount>> GetInstallationDetailsAsync(ITurnContext<IInstallationUpdateActivity> turnContext, CancellationToken cancellationToken);

        public Task<IList<ChannelAccount>> GetConversationUserDetailsAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken);

        /// <summary>
        ///If you encounter permission-related errors when sending this message, <see cref="https://aka.ms/BotTrustServiceUrl"/>
        /// </summary>
        public Task MessageAllMembersAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);

        /// <summary>
        /// Tag user with @User.
        /// </summary>
        public Task MentionUserActivityAsync(ITurnContext turnContext, CancellationToken cancellationToken);

        public Task<MessagingExtensionResponse> RunMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionQuery query, CancellationToken cancellationToken);

        public Task<MessagingExtensionResponse> RunMessagingExtensionSelectItemAsync(ITurnContext<IInvokeActivity> turnContext, JObject query, CancellationToken cancellationToken);

        public Task ContinueConversationAsync(ITurnContext<IMessageActivity> turnContext, ConversationReference reference, CancellationToken cancellationToken);
    }
}