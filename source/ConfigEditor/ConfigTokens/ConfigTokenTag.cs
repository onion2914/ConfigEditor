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
    [Export(typeof(ITaggerProvider))]
    [Name("ConfigBaseTag")]
    [ContentType("config")]
    [TagType(typeof(ConfigTokenTag))]
    internal sealed class ConfigTokenTagProvider : ITaggerProvider
    {
        static Dictionary<ITextBuffer, ConfigTokenTagger> _textBufferDict = new Dictionary<ITextBuffer, ConfigTokenTagger>();
        
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            
            //一度読みこんだTextBufferは次回呼び出し時に同じインスタンスを返す
            if (_textBufferDict.ContainsKey(buffer)) return _textBufferDict[buffer] as ITagger<T>;

            ConfigTokenTagger tagger = new ConfigTokenTagger(buffer);

            _textBufferDict.Add(buffer, tagger);
            return tagger as ITagger<T>;
            

            //return new ConfigTokenTagger(buffer) as ITagger<T>;
        }
        
    }

    public class ConfigTokenTag : ITag
    {
        public ConfigTokenTypes type { get; private set; }

        public ConfigTokenTag(ConfigTokenTypes type)
        {
            this.type = type;
        }
    }

    internal sealed class ConfigTokenTagger : ITagger<ConfigTokenTag>
    {

        ITextBuffer _buffer;
        IDictionary<string, ConfigTokenTypes> _configTypes;
        HashSet<string> _configKeywordType;

        List<SnapshotSpan> _commentSpans;
        List<LineTokenTag> _lineTokenTags;
        List<ConfigStructToken> _configTokens;


        internal ConfigTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _configTypes = new Dictionary<string, ConfigTokenTypes>();
            _configTypes["keyword"] = ConfigTokenTypes.KeyWord;
            _configTypes["="] = ConfigTokenTypes.Operator;

            //KeyWord集合
            _configKeywordType = new HashSet<string> { "break", "case", "code", "const", "default", "dictionary", "else", "for", "func", "gui", "if", "private", "return", "rule", "struct", "switch", "type", "void", "#define",
                                                       "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal", "char", "bool", "string", "datetime", "timespan" };

            _commentSpans = new List<SnapshotSpan>();

            Task task = Task.Run(() => {
                _lineTokenTags = CreateLineTokenTags(buffer);
            });

            



        }
        

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        

        public IEnumerable<ITagSpan<ConfigTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                string tokenText = curSpan.GetText();
                //トークンリスト生成
                string[] tokens = tokenText.ToLower().Split(null);   //nullを与えると空白文字で分割
                int currentIndex = curSpan.Start.Position;

                bool structToken = false;
                foreach (string configToken in tokens)
                {
                    if (_configKeywordType.Contains(configToken))
                    {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(currentIndex, configToken.Length));
                        if (tokenSpan.IntersectsWith(curSpan))
                            yield return new TagSpan<ConfigTokenTag>(tokenSpan,
                                                                  new ConfigTokenTag(_configTypes["keyword"]));
                    
                        structToken = configToken == "struct";
                    }
                    else if (structToken)
                    {
                        structToken = false;
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(currentIndex, configToken.Length));
                        if (tokenSpan.IntersectsWith(curSpan))
                            yield return new TagSpan<ConfigTokenTag>(tokenSpan,
                                                                  new ConfigTokenTag(ConfigTokenTypes.Struct));
                    }
                    

                    currentIndex += configToken.Length + 1;
                }


            }

        }

        private List<LineTokenTag> CreateLineTokenTags(ITextBuffer buffer)
        {
            ITextSnapshot snapshot = _buffer.CurrentSnapshot;
            //var lineTokenTags = new List<LineTokenTag>();

            List<LineTokenTag> lineTokenTags = new List<LineTokenTag>();
            LineTokenTag prev = null;

            foreach (var line in snapshot.Lines) {
                var tmp = new LineTokenTag(line, prev);
                lineTokenTags.Add(tmp);
                prev = tmp;
            }

            _configTokens = ParseConfigToken(lineTokenTags);

            return lineTokenTags;
            //return snapshot.Lines.Select(line => new LineTokenTag(line, )).ToList();          
        }


        private List<ConfigStructToken> ParseConfigToken(IList<LineTokenTag> tags)
        {
            List<ConfigStructToken> configTokens = new List<ConfigStructToken>();

            //var e = tags.SelectMany(tag => tag.TokenTags).Where(tokentag => tokentag.Tag.type != ConfigTokenTypes.Comment).GetEnumerator();

            //コメント以外のタグを抽出
            var e = (from linetags in tags
                    from tokentag in linetags.TokenTags
                    where tokentag.Tag.type != ConfigTokenTypes.Comment
                    select tokentag).GetEnumerator();
                    

            try {


                while (e.MoveNext()) {
                    string token = e.Current.Text;
                    if (token == "struct") {
                        configTokens.Add(ParseStructToken(e));
                    }
                }
            } catch (Exception ex) {

            } finally {
                e.Dispose();
            }

            return configTokens;
        }

        private ConfigStructToken ParseStructToken(IEnumerator<ConfigTagSpan<ConfigTokenTag>> enumerator)
        {
            if (!enumerator.MoveNext()) throw new Exception();

            string structName = enumerator.Current.Text;
            //ConfigStructToken configToken = new ConfigStructToken(enumerator.Current.Text);
            ConfigStructToken configToken = null;

            if (!enumerator.MoveNext()) throw new Exception();
            
            if (enumerator.Current.Text == ":") {
                if (!enumerator.MoveNext()) throw new Exception();

                configToken = new ConfigStructToken(structName, enumerator.Current.Text);

                if (!enumerator.MoveNext()) throw new Exception();
            }

            configToken = configToken ?? new ConfigStructToken(structName);

            if (enumerator.Current.Text != "{") throw new Exception();


            if (!enumerator.MoveNext()) throw new Exception();
            while (enumerator.Current.Text != "};") {
                string type = enumerator.Current.Text;

                if (!enumerator.MoveNext()) throw new Exception();

                string name = enumerator.Current.Text;
                if (!name.EndsWith(";")) throw new Exception();

                //フィールドメンバ追加
                configToken.Add(new ConfigStructFieldToken(type, name.TrimEnd(';')));

                if (!enumerator.MoveNext()) throw new Exception();
            }

            return configToken;
        }   

    }

    public class ConfigTagSpan<T> : TagSpan<T> where T : ConfigTokenTag
    {
        public string Text { get; private set; }

        public ConfigTagSpan(SnapshotSpan span, T tag, string text) : base(span, tag)
        {
            Text = text;
        }
    }
}
