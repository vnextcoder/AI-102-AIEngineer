﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EchoBot .NET Template version v4.13.2
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace EchoBot.Bots
{
    public class EchoBot : ActivityHandler
    {
        
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var replyText = $"Echo: {turnContext.Activity.Text}";

            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        //public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        //{
        //    return turnContext.SendActivityAsync(MessageFactory.Text("hello there turn", "hello there turn"), cancellationToken);
        //    ///return base.OnTurnAsync(turnContext, cancellationToken);
        //}

        protected override Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            return base.OnMembersRemovedAsync(membersRemoved, turnContext, cancellationToken);
        }

        protected override Task OnMessageReactionActivityAsync(ITurnContext<IMessageReactionActivity> turnContext, CancellationToken cancellationToken)
        {
            return base.OnMessageReactionActivityAsync(turnContext, cancellationToken);
        }

        protected override Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            return base.OnInvokeActivityAsync(turnContext, cancellationToken);
        }

        protected override Task OnTypingActivityAsync(ITurnContext<ITypingActivity> turnContext, CancellationToken cancellationToken)
        {
            Console.WriteLine(turnContext.Activity.ToString());
            turnContext.SendActivityAsync(MessageFactory.Text("hello there turn", "hello there turn"), cancellationToken).ConfigureAwait(true).GetAwaiter().GetResult();
            return base.OnTypingActivityAsync(turnContext, cancellationToken);
        }
    }
}
