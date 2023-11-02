using System;
using Microsoft.VisualStudio.Commanding;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor.Commanding.Commands;
using Microsoft.VisualStudio.Text;
using ZenCoding;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.Commanding;
using System.Composition;
using Microsoft.VisualStudio.Utilities;

namespace XZenCoding
{
    [Export(typeof(ICommandHandler))]
    [Name(nameof(CommandExtender))]
    [ContentType("HTML")]
    [ContentType("HTMLX")]
    [ContentType("HTMLX-ASPNET")]
    [ContentType("HTMLX-ASPNET-MVC")]
    [ContentType("HTMLX-ASPNET-MVC-C#")]
    [ContentType("HTMLX-ASPNET-MVC-VB")]
    [ContentType("HTMLX-ASPNET-VB")]
    [ContentType("HTMLX-C#")]
    [ContentType("HTMLX-VB")]
    [ContentType("HTMLX-XSLT")]
    [ContentType("HTMLX-XSLT-C#")]
    [ContentType("HTMLX-XSLT-VB")]
    [ContentType("HTML-ASPNET")]
    [ContentType("HTML-ASPNET-MVC")]
    [ContentType("HTML-ASPNET-MVC-C#")]
    [ContentType("HTML-ASPNET-MVC-VB")]
    [ContentType("HTML-ASPNET-VB")]
    [ContentType("HTML-C#")]
    [ContentType("HTML-VB")]
    [ContentType("HTML-XSLT")]
    [ContentType("HTML-XSLT-C#")]
    [ContentType("HTML-XSLT-VB")]
    [ContentType("Razor")]
    [ContentType("AspNet-Razor")]
    [ContentType("LegacyRazorCSharp")]
    [ContentType("WebForms")]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    internal class CommandExtender : ICommandHandler<TabKeyCommandArgs>
    {
        [Import]
        public IEditorCommandHandlerServiceFactory EditorCommandHandlerServiceFactory { get; set; } = default;

        public string DisplayName => "XZenCoding";

        public bool ExecuteCommand(TabKeyCommandArgs args, CommandExecutionContext executionContext)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            return InvokeCoreZenCoding(args);
        }

        public CommandState GetCommandState(TabKeyCommandArgs args)
        {
            return CommandState.Available;
        }

        /// <summary>
        /// Invoke the core zen coding engine from ZinCoding.dll
        /// </summary>
        /// <param name="args">Visual Studio command arguments</param>
        /// <returns>True if the command is handled</returns>
        private bool InvokeCoreZenCoding(TabKeyCommandArgs args)
        {
            // TODO: implement the following steps:
            // 1. Get the current caret position
            var caretPosition = args.TextView.Caret.Position.BufferPosition.Position;
            // 2. Get the current line text
            var currentLineText = args.TextView.TextSnapshot.GetLineFromPosition(caretPosition).GetText();
            // 3. Get the current line text before the caret position
            var currentLineTextBeforeCaret = currentLineText.Substring(0, caretPosition);
            // 4. Get the current line text after the caret position
            var currentLineTextAfterCaret = currentLineText.Substring(caretPosition);
            // 5. Parse syntax using coding engine
            var parser = new Parser();
            var result = parser.Parse(currentLineTextBeforeCaret, ZenType.HTML);
            // 6. Replace the current line text with the result from the core zen coding engine
            if (string.IsNullOrEmpty(result))
            {
                return false;
            }
            try
            {
                ITextSelection textSelection = args.TextView.Selection;
                SnapshotSpan snapshotSpan = new SnapshotSpan(args.TextView.TextSnapshot, new Span(
                    caretPosition,
                    currentLineText.Length));
                SnapshotPoint snapshotPoint = new SnapshotPoint(args.TextView.TextSnapshot, caretPosition);
                textSelection.Select(snapshotSpan, false);
                IEditorCommandHandlerService commandHandler = EditorCommandHandlerServiceFactory.GetService(args.TextView);
                var formatCommand = new FormatDocumentCommandArgs((ITextView)args.TextView.TextBuffer, (ITextBuffer)args.TextView);
                commandHandler.Execute(argsFactory: (textView, textBuffer) => formatCommand, nextCommandHandler: null);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
