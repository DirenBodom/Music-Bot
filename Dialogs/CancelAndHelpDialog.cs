// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace BronnBot
{
    public class CancelAndHelpDialog : ComponentDialog
    {
        private const string HelpMsgText = "Show help here";
        private const string CancelMsgText = "Thanks for coming in, hope to recommend you more music in the future!";

        public CancelAndHelpDialog(string id)
            : base(id)
        {
        }

        protected override async Task<DialogTurnResult> OnContinueDialogAsync(DialogContext innerDc, CancellationToken cancellationToken = default)
        {
            var result = await InterruptAsync(innerDc, cancellationToken);
            if (result != null)
            {
                return result;
            }

            return await base.OnContinueDialogAsync(innerDc, cancellationToken);
        }

        private async Task<DialogTurnResult> InterruptAsync(DialogContext innerDc, CancellationToken cancellationToken)
        {
            if (innerDc.Context.Activity.Type == ActivityTypes.Message)
            {
                var text = innerDc.Context.Activity.Text.ToLowerInvariant();

                switch (text)
                {
                    case "end":
                    case "cancel":
                    case "quit":
                        var cancelMessage = MessageFactory.Text(CancelMsgText, CancelMsgText, InputHints.IgnoringInput);
                        await innerDc.Context.SendActivityAsync(cancelMessage, cancellationToken);
                        return await innerDc.CancelAllDialogsAsync(cancellationToken);
                    case "restart":
                    case "begin":
                        var beginMessage = MessageFactory.Text("Refreshing...", "Refreshing...", InputHints.IgnoringInput);
                        await innerDc.Context.SendActivityAsync(beginMessage, cancellationToken);
                        return await innerDc.ReplaceDialogAsync(nameof(WaterfallDialog), null, cancellationToken);

                }
            }

            return null;
        }
    }
}
