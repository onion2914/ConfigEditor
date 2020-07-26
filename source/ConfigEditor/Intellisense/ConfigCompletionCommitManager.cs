using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion;
using Microsoft.VisualStudio.Language.Intellisense.AsyncCompletion.Data;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace ConfigEditor.Intellisense
{
    class ConfigCompletionCommitManager : IAsyncCompletionCommitManager
    {
        public ConfigCompletionCommitManager()
        {
        }

        ImmutableArray<char> commitChars = new char[] { ' ', '\'', '"', ',', '.', ';', ':' }.ToImmutableArray();

        public IEnumerable<char> PotentialCommitCharacters => commitChars;

        public bool ShouldCommitCompletion(char typedChar, SnapshotPoint location, CancellationToken token)
        {
            // This method runs synchronously, potentially before CompletionItem has been computed.
            // The purpose of this method is to filter out characters not applicable at given location.

            // This method is called only when typedChar is among the PotentialCommitCharacters
            // in this simple example, all PotentialCommitCharacters do commit, so we always return true
            return true;
        }

        public CommitResult TryCommit(ITextView view, ITextBuffer buffer, CompletionItem item, ITrackingSpan span, char typedChar, CancellationToken token)
        {
            // Objects of interest here are session.TextView and session.TextView.Caret.
            // This method runs synchronously

            return CommitResult.Unhandled; // use default commit mechanism.
        }
    }
}
