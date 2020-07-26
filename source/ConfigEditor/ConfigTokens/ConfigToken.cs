using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.Linq;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace ConfigEditor
{
    public interface IConfigToken
    {
        void Accept(IConfigTokenVisitor visitor);
    }

    public interface IConfigTokenVisitor
    {
        void Visit(ConfigStructToken token);
        void Visit(ConfigStructFieldToken token);
    }

    public class ConfigStructToken : IConfigToken
    {
        public List<ConfigStructFieldToken> Children { get; private set; }
        public string TypeName { get; private set; }
        public string BaseTypeName { get; private set; }

        public ConfigStructToken(string name) : this(name, null)
        {
        }

        public ConfigStructToken(string name, string basename)
        {
            this.TypeName = name;
            this.BaseTypeName = basename;
            Children = new List<ConfigStructFieldToken>();
        }

        public void Add(ConfigStructFieldToken token)
        {
            Children?.Add(token);
        }

        public void AddChildren(IList<ConfigStructFieldToken> tokens)
        {
            Children?.AddRange(tokens);
        }

        public void Accept(IConfigTokenVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class ConfigStructFieldToken : IConfigToken
    {
        public string TypeName { get; private set; }
        public string ValueName { get; private set; }

        public ConfigStructFieldToken(string tyepeName, string valueName)
        {
            this.TypeName = tyepeName;
            this.ValueName = valueName;
        }

        public void Accept(IConfigTokenVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class LineTokenTag
    {
        private List<ConfigTagSpan<ConfigTokenTag>> _configTokenTags;
        private ITextSnapshotLine _line;
        private bool multiLineComent;
        private LineTokenTag _prevLine;

        public LineTokenTag(ITextSnapshotLine line, LineTokenTag prev)
        {
            _line = line;
            _prevLine = prev;
            _configTokenTags = AnalizeLexical(line);

        }

        public bool MultiLineComment => multiLineComent;
        public List<ConfigTagSpan<ConfigTokenTag>> TokenTags => _configTokenTags;

        private List<ConfigTagSpan<ConfigTokenTag>> AnalizeLexical(ITextSnapshotLine line)
        {
            ITextSnapshot snapshot = line.Snapshot;
            int start = line.Start;
            string lineText = line.GetText();
            int lineLength = line.Length;
            int index = 0;
            List<ConfigTagSpan<ConfigTokenTag>> tags = new List<ConfigTagSpan<ConfigTokenTag>>();

            if (_prevLine != null && _prevLine.MultiLineComment) {
                //コメント終了(*/)を探す
                var match = Regex.Match(lineText.Substring(index), @"\*/");
                if (match.Success) {
                    //tags.Add(new SnapshotSpan(snapshot, new Span(start, match.Index + 2)));
                    tags.Add(CreateTokenTagSpan(snapshot, start, match.Index + 2, ConfigTokenTypes.Comment));
                    multiLineComent = false;
                    index += match.Index + 3;
                } else {
                    //なかったら一行丸ごとトークン化
                    //tags.Add(new SnapshotSpan(snapshot, new Span(start, lineLength)));
                    tags.Add(CreateTokenTagSpan(snapshot, start, lineLength, ConfigTokenTypes.Comment));
                    multiLineComent = true;
                    return tags;
                }
            }

            while (index < lineLength) {
                //Skip 空白
                if (char.IsWhiteSpace(lineText[index])) {
                    index++;
                    continue;
                }

                //コメント
                if (index + 1 < lineLength && lineText[index] == '/') {
                    if (lineText[index + 1] == '/') {
                        //行終了までCommentトークン
                        //var tokenSpan = new SnapshotSpan(snapshot, new Span(start + index, lineLength - index));
                        //tags.Add(new TagSpan<ConfigTokenTag>(tokenSpan, new ConfigTokenTag(ConfigTokenTypes.Comment)));
                        tags.Add(CreateTokenTagSpan(snapshot, start + index, lineLength - index, ConfigTokenTypes.Comment));

                        break;
                    } else if (lineText[index + 1] == '*') {
                        //コメント終了(*/)を探す
                        var match = Regex.Match(lineText.Substring(index), @"\*/");
                        if (match.Success) {
                            //tags.Add(new SnapshotSpan(snapshot, new Span(start + index, match.Index + 2)));
                            tags.Add(CreateTokenTagSpan(snapshot, start + index, match.Index + 2, ConfigTokenTypes.Comment));
                            index += match.Index + 3;
                            multiLineComent = false;
                        } else {
                            //行の最後までなかった
                            //tags.Add(new SnapshotSpan(snapshot, new Span(start + index, lineLength - index)));
                            tags.Add(CreateTokenTagSpan(snapshot, start + index, lineLength - index, ConfigTokenTypes.Comment));

                            multiLineComent = true;
                            break;
                        }
                        continue;
                    }
                    //識別子
                } else {
                    int tokenLength = 1;
                    while (index + tokenLength < lineLength) {
                        if (char.IsWhiteSpace(lineText[index + tokenLength])) {
                            break;
                        }

                        tokenLength++;
                    }

                    //行末またはスペース区切り
                    //tags.Add(new SnapshotSpan(snapshot, new Span(start + index, tokenLength)));
                    tags.Add(CreateTokenTagSpan(snapshot, start + index, tokenLength, ConfigTokenTypes.Unset));
                    index += tokenLength + 1;
                }


            }

            return tags;
        }

        private static ConfigTagSpan<ConfigTokenTag> CreateTokenTagSpan(ITextSnapshot snapshot, int start, int length, ConfigTokenTypes type)
        {
            var tokenSpan = new SnapshotSpan(snapshot, new Span(start, length));
            return new ConfigTagSpan<ConfigTokenTag>(tokenSpan, new ConfigTokenTag(type), tokenSpan.GetText());
        }
    }
}
