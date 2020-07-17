using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using System.Text.RegularExpressions;

namespace ConfigEditor.Classification
{
    /// <summary>
    /// Classifier provider. It adds the classifier to the set of classifiers.
    /// </summary>

    [Export(typeof(ITaggerProvider))]
    //[ContentType("text")] // This classifier applies to all text files.
    [ContentType("config")] // This classifier applies to all text files.
    [TagType(typeof(ClassificationTag))]
    internal class ConfigClassifierProvider : ITaggerProvider
    {
        [Export]
        [Name("config")]
        [BaseDefinition("code")]
        internal static ContentTypeDefinition ConfigContentType = null;

        [Export]
        [FileExtension(".cfg")]
        [ContentType("config")]
        internal static FileExtensionToContentTypeDefinition ConfigFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<ConfigTokenTag> configTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<ConfigTokenTag>(buffer);

            return new ConfigClassifier(buffer, configTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }

    }



    /// <summary>
    /// Classifier that classifies all text as an instance of the "ConfigClassifier" classification type.
    /// </summary>
    internal class ConfigClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<ConfigTokenTag> _aggregator;
        IDictionary<ConfigTokenTypes, IClassificationType> _configTypes;

        CommentSpans _commentSpans;

        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal ConfigClassifier(ITextBuffer buffer,
                               ITagAggregator<ConfigTokenTag> tagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = tagAggregator;
            _configTypes = new Dictionary<ConfigTokenTypes, IClassificationType>();
            _configTypes[ConfigTokenTypes.KeyWord] = typeService.GetClassificationType("ConfigKeyWord");
            _configTypes[ConfigTokenTypes.Operator] = typeService.GetClassificationType("ConfigOperator");
            _configTypes[ConfigTokenTypes.Comment] = typeService.GetClassificationType("ConfigComment");
            _configTypes[ConfigTokenTypes.Struct] = typeService.GetClassificationType("ConfigStruct");

            _buffer.Changed += OnTextChanged;

            _commentSpans = new CommentSpans(_buffer);
        }

        private void OnTextChanged(object sender, TextContentChangedEventArgs e)
        {
            var commentSpans = _commentSpans.Spans.CloneAndTrackTo(e.After, SpanTrackingMode.EdgeExclusive);

            bool tagChange = false;

            foreach ( var change in e.Changes)
            {
                var oldLines = e.Before.Lines.Where(line => line.Extent.IntersectsWith(change.OldSpan));
                var newLines = e.After.Lines.Where(line => line.Extent.IntersectsWith(change.NewSpan));
                
                foreach( var line in oldLines)
                {
                    //コメント変更に関係ありそうな場合のみイベントを発生させる。
                    if (Regex.Match(line.GetText(), @"/\*|//|\*/").Success)
                        tagChange = true;
                }

                foreach (var line in newLines)
                {
                    //コメント変更に関係ありそうな場合のみイベントを発生させる。
                    if (Regex.Match(line.GetText(), @"/\*|//|\*/").Success)
                        tagChange = true;
                }

                if (tagChange) break;
            }

            if (!tagChange) return;

           _commentSpans.Update();

            if (TagsChanged != null)
            {
                var snapshot = this._buffer.CurrentSnapshot;

                TagsChanged(this, new SnapshotSpanEventArgs(
                        new SnapshotSpan(snapshot,
                            Span.FromBounds(0, snapshot.Length)))
                            );
            }
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;

        /// <summary>
        /// Search the given span for any instances of classified tags
        /// </summary>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            if (spans.Count == 0) yield break;

            //コメントのスパンなら
            NormalizedSnapshotSpanCollection commentSpans = _commentSpans.Spans.CloneAndTrackTo(spans[0].Snapshot, SpanTrackingMode.EdgeExclusive);
            NormalizedSnapshotSpanCollection comments = NormalizedSnapshotSpanCollection.Intersection(spans, commentSpans);
            NormalizedSnapshotSpanCollection notCommentSpans = NormalizedSnapshotSpanCollection.Difference(spans, comments);

            if (notCommentSpans.Count == 0)
            {
                yield return
                    new TagSpan<ClassificationTag>(spans[0],
                                                   new ClassificationTag(_configTypes[ConfigTokenTypes.Comment]));
                yield break;
            }
            else if (comments.Count != 0)
            {
                foreach (var comentSpan in comments)
                {
                    yield return
                        new TagSpan<ClassificationTag>(comentSpan,
                                                       new ClassificationTag(_configTypes[ConfigTokenTypes.Comment]));
                }
            }

            foreach (var tagSpan in _aggregator.GetTags(notCommentSpans))
            {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return
                    new TagSpan<ClassificationTag>(tagSpans[0],
                                                   new ClassificationTag(_configTypes[tagSpan.Tag.type]));
            }
        }
    }


    public class CommentSpans
    {
        private ITextBuffer _buffer;

        public CommentSpans(ITextBuffer buffer)
        {
            _buffer = buffer;

            Spans = new NormalizedSnapshotSpanCollection(GetCommentSpans());
        }

        public NormalizedSnapshotSpanCollection Spans { get; private set; }

        public void Update()
        {
            Spans = new NormalizedSnapshotSpanCollection(GetCommentSpans());
        }

        private IList<SnapshotSpan> GetCommentSpans()
        {
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            string text = snapshot.GetText();

            MatchCollection matches = Regex.Matches(text, @"//.*?\r\n|/\*.*?\*/", RegexOptions.Singleline);

            List<SnapshotSpan> snapshots = new List<SnapshotSpan>();

            foreach (Match match in matches)
            {
                snapshots.Add(new SnapshotSpan(snapshot, new Span(match.Index, match.Length)));
            }
            return snapshots;
        }

        public static bool CheckValidCommentSpans(NormalizedSnapshotSpanCollection spans)
        {
            if(spans.Count == 0)
            {
                return false;
            }

            ITextSnapshot snapshot = spans[0].Snapshot;
            foreach (var span in spans)
            {
                string text = span.GetText();

                if(!text.StartsWith("//") && !text.StartsWith("/*") && !text.EndsWith("*/"))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
