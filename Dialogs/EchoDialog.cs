using System;
using System.Threading.Tasks;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;


namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            var activity = context.Activity as Activity;
            
            if(activity.Type == ActivityTypes.Message)
            {
                var isTyping = activity.CreateReply("Trialbot is typing");
                isTyping.Type = ActivityTypes.Typing;
                var connector = new ConnectorClient(new System.Uri(activity.ServiceUrl));
                await connector.Conversations.ReplyToActivityAsync(isTyping);

            }

          
                await context.PostAsync(GetWelcomeMsg(message.Text,activity.From.Name));
                context.Wait(MessageReceivedAsync);
           
        }
        private String GetWelcomeMsg(String msgTxt,String name)
        {
            if (msgTxt.ToLower().Equals("hi"))
            {
                return $"Hi {name}";
            }
            return $"Hello {name}";
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }

    }
}