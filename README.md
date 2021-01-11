# teams-bots
RND teams bots sln

## Clone cmd:
  ```bash
git clone https://github.com/dommyrock/teams-bots.git
  ```

| Endpoint | Description |
| ------ | ------ |
| /api/messages | Bot response endpoint |
| /api/notify | Notification endpoint |

| Endpoint | Url |
| ------ | ------ |
| Templating SDK .NET Adaptive cards | https://docs.microsoft.com/en-us/adaptive-cards/templating/sdk |
| Bot framework API | https://docs.microsoft.com/en-us/dotnet/api/?view=botbuilder-dotnet-stable |
| MS Graph API | https://docs.microsoft.com/en-us/graph/api/resources/channel?view=graph-rest-1.0 |

### TESTING in TEAMS:
##### STEP 1:
make sure we run ngrok cmd: ngrok http -host-header=rewrite 3978  
(check that locallhost port OF WEBAPP matches )
`You can open http://localhost:4040 to inspect web traffic while ngrok is running`
##### STEP 2:
Update Bot Endpoint in teams. AppStudio -> manifest editor->YourApp (with bot registered in it) 
->Bots ->Bot endpoint address
##### STEP 3:
Get App Id (in Bots tab, under selected bot name <GUID> , also get password (generate new one if you dont have it saved)
##### STEP 4:
Copy bot ID = bot password into appseting.json  file (used to identify bot to which we send/receive messages from
##### STEP 4.1:
(Bot Emulator): if we're debugging with BE , PASTE "Microsoft App ID" and "Microsoft App Password" 
##### STEP 5:
open Postman or call this endpoint through browser 
##### NOTE:
`(REMEMBER TO REPLACE NGROK PUBLIC URL WITH GENERATED ONE ,expires in 8hrs on free version)`
https://<NGROK URL>/api/notify (to trigger proactive bot notification)
https://<NGROK URL>/api/messages (to send/receive personal chat message to/from user)

#### App Install:
##### Proactive app install
*https://docs.microsoft.com/en-us/microsoftteams/platform/graph-api/proactive-bots-and-messages/graph-proactive-bots-and-messages?tabs=csharp

* https://blog.thoughtstuff.co.uk/2020/07/its-now-much-easier-to-send-proactive-bot-messages-to-microsoft-teams-users-thanks-to-new-permissions/
* https://docs.microsoft.com/en-us/graph/teams-proactive-messaging

#### TEAMS EXAMPLES:
*(send proactive messages to team,chat,channel)
| Endpoint | Url |
| ------ | ------ |
| Webhook | https://docs.microsoft.com/en-us/microsoftteams/platform/webhooks-and-connectors/how-to/connectors-using |
| Adaptive card | https://www.vrdmn.com/2020/07/microsoft-teams-bot-framework-mention.html |
| Proactive messages | https://www.vrdmn.com/2020/02/microsoft-bot-framework-v4-send.html |

(JS Example-proactive bots)
* https://github.com/marcoszanre/proactivemessagesteamstypescriptbots
* https://youtu.be/kEL_FUlRpY0?t=580

#### OUTLOOK EXAMPLES:
| Endpoint | Description |
| ------ | ------ |
| adaptive cards | https://youtu.be/X6Cs-MIefyo?t=2566 |
| API-actionable messages | https://docs.microsoft.com/en-us/outlook/actionable-messages/adaptive-card |
| Actionable Card -Tuttorial | https://docs.microsoft.com/en-us/learn/modules/adaptive-cards-create-engaging-messages/5-exercise-outlook-actionable-messages |
| card examples | https://amdesigner.azurewebsites.net/ |

## Prerequisites

- [.NET Core SDK](https://dotnet.microsoft.com/download) version 3.1

  ```bash
  # determine dotnet version
  dotnet --version
  ```

## Other Bot samples by .NET team

- Clone the repository

    ```bash
    git clone https://github.com/microsoft/botbuilder-samples.git
    ```

- Run the bot from a terminal or from Visual Studio:

  A) From a terminal, navigate to `samples/csharp_dotnetcore/16.proactive-messages`

  ```bash
  # run the bot
  dotnet run
  ```

  B) Or from Visual Studio

  - Launch Visual Studio
  - File -> Open -> Project/Solution
  - Navigate to `samples/csharp_dotnetcore/16.proactive-messages` folder
  - Select `ProactiveBot.csproj` file
  - Press `F5` to run the project

## Testing the bot using Bot Framework Emulator

[Bot Framework Emulator](https://github.com/microsoft/botframework-emulator) is a desktop application that allows bot developers to test and debug their bots on localhost or running remotely through a tunnel.

- Install the latest Bot Framework Emulator from [here](https://github.com/Microsoft/BotFramework-Emulator/releases)

### Connect to the bot using Bot Framework Emulator

- Launch Bot Framework Emulator
- File -> Open Bot
- Enter a Bot URL of `http://localhost:3978/api/messages`

With the Bot Framework Emulator connected to your running bot, the sample will not respond to an HTTP GET that will trigger a proactive message.  The proactive message can be triggered from the command line using `curl` or similar tooling, or can be triggered by opening a browser windows and navigating to `http://localhost:3978/api/notify`.

### Using curl

- Send a get request to `http://localhost:3978/api/notify` to proactively message users from the bot.

   ```bash
    curl get http://localhost:3978/api/notify
   ```

- Using the Bot Framework Emulator, notice a message was proactively sent to the user from the bot.

### Using the Browser

- Launch a web browser
- Navigate to `http://localhost:3978/api/notify`
- Using the Bot Framework Emulator, notice a message was proactively sent to the user from the bot.

## Interacting with the bot

In addition to responding to incoming messages, bots are frequently called on to send "proactive" messages based on activity, scheduled tasks, or external events.

In order to send a proactive message using Bot Framework, the bot must first capture a conversation reference from an incoming message using `TurnContext.getConversationReference()`. This reference can be stored for later use.

To send proactive messages, acquire a conversation reference, then use `adapter.continueConversation()` to create a TurnContext object that will allow the bot to deliver the new outgoing message.

### Avoiding Permission-Related Errors

You may encounter permission-related errors when sending a proactive message. This can often be mitigated by using `MicrosoftAppCredentials.TrustServiceUrl()`. See [the documentation](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=csharp#avoiding-401-unauthorized-errors) for more information.

## Deploy this bot to Azure

To learn more about deploying a bot to Azure, see [Deploy your bot to Azure](https://aka.ms/azuredeployment) for a complete list of deployment instructions.

## Further reading

- [Bot Framework Documentation](https://docs.botframework.com)
- [Bot Basics](https://docs.microsoft.com/azure/bot-service/bot-builder-basics?view=azure-bot-service-4.0)
- [Send proactive messages](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-howto-proactive-message?view=azure-bot-service-4.0&tabs=js)
- [continueConversation Method](https://docs.microsoft.com/en-us/javascript/api/botbuilder/botframeworkadapter#continueconversation)
- [getConversationReference Method](https://docs.microsoft.com/en-us/javascript/api/botbuilder-core/turncontext#getconversationreference)
- [Activity processing](https://docs.microsoft.com/en-us/azure/bot-service/bot-builder-concept-activity-processing?view=azure-bot-service-4.0)
- [Azure Bot Service Introduction](https://docs.microsoft.com/azure/bot-service/bot-service-overview-introduction?view=azure-bot-service-4.0)
- [Azure Bot Service Documentation](https://docs.microsoft.com/azure/bot-service/?view=azure-bot-service-4.0)
- [.NET Core CLI tools](https://docs.microsoft.com/en-us/dotnet/core/tools/?tabs=netcore2x)
- [Azure CLI](https://docs.microsoft.com/cli/azure/?view=azure-cli-latest)
- [Azure Portal](https://portal.azure.com)
- [Language Understanding using LUIS](https://docs.microsoft.com/en-us/azure/cognitive-services/luis/)
- [Channels and Bot Connector Service](https://docs.microsoft.com/en-us/azure/bot-service/bot-concepts?view=azure-bot-service-4.0)
