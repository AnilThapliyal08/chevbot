using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;



namespace Microsoft.Bot.Sample.SimpleEchoBot
{
   
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;
        private String URL = "https://westus.api.cognitive.microsoft.com/luis/v2.0/apps/";
        private String APP_ID = "80460053-8aef-4299-9954-d84cbb4f445e";
        private String URL_PARAMS = "?subscription-key=e237d78697f644d9b7999d674a90e73e&staging=true&verbose=true&timezoneOffset=330&q=";

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            var activity = context.Activity as Activity;

            if (activity.Type == ActivityTypes.Message)
            {
                var isTyping = activity.CreateReply("Trialbot is typing");
                isTyping.Type = ActivityTypes.Typing;
                var connector = new ConnectorClient(new System.Uri(activity.ServiceUrl));
                await connector.Conversations.ReplyToActivityAsync(isTyping);

            }

            var response = GetAppropriateReply(message.Text, activity.From.Name, context);
            if (response.Equals("PersonMissing"))
            {
                // PromptDialog.Text(context, BookTableAsyc, "For how many people?");
                PromptDialog.Number(
                    context,
                    AfterResetAsync,
                    "For how many people?",
                    "Didn't get that!"
                   );
            }
            else
            {

                await context.PostAsync(response);
                context.Wait(MessageReceivedAsync);
            }


        }

        private String GetAppropriateReply(String msgTxt, String name, IDialogContext context)
        {
            // Find Intent
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(URL + APP_ID);
            client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            String requestParam = URL_PARAMS + msgTxt;

            HttpResponseMessage response = client.GetAsync(requestParam).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var responseString = response.Content.ReadAsStringAsync();
                var jsonresponse = Newtonsoft.Json.Linq.JObject.Parse(responseString.Result);
                var intentonly = jsonresponse.SelectToken("topScoringIntent.intent").ToString();

                if (intentonly.Equals("WelcomeIntent"))
                {
                    var entities = jsonresponse.SelectToken("entities") as Newtonsoft.Json.Linq.JArray;
                    if (entities.HasValues)
                    {
                        var userName = jsonresponse.SelectToken("entities[0].entity").ToString();
                        return $"Hi {userName}, Welcome to chevron";
                    }
                    return "Hi Welcome to chevron";

                }
                else if (intentonly.Equals("NoOfPeopleIntent"))
                {
                    var entities = jsonresponse.SelectToken("entities") as Newtonsoft.Json.Linq.JArray;
                    if (!entities.HasValues)
                    {
                        return "PersonMissing";

                    }
                    else
                    {
                        var tableForPeople = jsonresponse.SelectToken("entities[0].entity").ToString();
                        return $"Table is reserved for {tableForPeople} person";
                    }
                }
            }
            else
            {
                Console.WriteLine("{0} ({1})", (int)response.StatusCode, response.ReasonPhrase);
            }

            return null;
        }
        private String GetWelcomeMsg(String msgTxt, String name)
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

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<long> argument)
        {
            var confirm = await argument;
            if (confirm > 0)
            {
                this.count = 1;
                await context.PostAsync($"Table is booked for {confirm} people.");
            }
            else
            {
                await context.PostAsync("Did not book the table try again.");
            }
            context.Wait(MessageReceivedAsync);
        }
        public async Task BookTableAsyc(IDialogContext context, IAwaitable<string> argument)
        {
            var confirm = await argument;

            await context.PostAsync($"Table is booked for {confirm} people.");


        }

    }
}