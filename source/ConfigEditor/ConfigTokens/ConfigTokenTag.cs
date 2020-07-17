using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using System.Text.RegularExpressions;

namespace ConfigEditor
{
    [Export(typeof(ITaggerProvider))]
    [Name("ConfigBaseTag")]
    [ContentType("config")]
    [TagType(typeof(ConfigTokenTag))]
    internal sealed class ConfigTokenTagProvider : ITaggerProvider
    {

        
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new ConfigTokenTagger(buffer) as ITagger<T>;
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

            
        }
        

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
        

        public IEnumerable<ITagSpan<ConfigTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans)
            {

                //ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                //string lineText = containingLine.GetText();
                //string tokenText = "";
                //string commentText = "";
                //int curLoc = containingLine.Start.Position;

                //List<string> tokens = new List<string>();
                //List<SnapshotSpan> tokens1 = new List<SnapshotSpan>();
                //List<SnapshotSpan> tokens2 = new List<SnapshotSpan>(); ///*コメント専用
                //bool commentArea = false;

                ////トークンリスト生成
                //int lineStartPosition = containingLine.Start.Position;
                //int tokenStartIndex = 0;
                //for (int index = 0; index < lineText.Length; index++)
                //{
                //    if (commentArea)
                //    {
                //        if (lineText.Substring(index).StartsWith("*/"))
                //        {
                //            if (index - tokenStartIndex > 0)
                //                tokens1.Add(new SnapshotSpan(curSpan.Snapshot, new Span(lineStartPosition + tokenStartIndex, index + 2 - tokenStartIndex)));
                //            tokenStartIndex = index + 2;
                //            commentArea = false;
                //        }
                //        continue;
                //    }

                //    //空白orタブ
                //    if (Char.IsWhiteSpace(lineText[index]))
                //    {
                //        if (index - tokenStartIndex > 0)
                //            tokens1.Add(new SnapshotSpan(curSpan.Snapshot, new Span(lineStartPosition + tokenStartIndex, index - tokenStartIndex)));
                //        tokenStartIndex = index + 1;
                //    }
                //    else if (lineText.Substring(index).StartsWith("//"))
                //    {
                //        if (index - tokenStartIndex > 0)
                //            tokens1.Add(new SnapshotSpan(curSpan.Snapshot, new Span(lineStartPosition + tokenStartIndex, index - tokenStartIndex)));
                //        tokenStartIndex = index;
                //        tokens1.Add(new SnapshotSpan(curSpan.Snapshot, new Span(lineStartPosition + tokenStartIndex, lineText.Length - tokenStartIndex)));
                //        break;
                //    }
                //    else if (lineText.Substring(index).StartsWith("/*"))
                //    {
                //        if (index - tokenStartIndex > 0)
                //            tokens1.Add(new SnapshotSpan(curSpan.Snapshot, new Span(lineStartPosition + tokenStartIndex, index - tokenStartIndex)));
                //        tokenStartIndex = index;
                //        commentArea = true;
                //    }
                //}

                ////*/が見つからず行が終了した場合
                //if (commentArea)
                //{
                //    tokens1.Add(new SnapshotSpan(curSpan.Snapshot, new Span(lineStartPosition + tokenStartIndex, lineText.Length - tokenStartIndex)));
                //    //ITextSnapshotLine nextLine = curSpan.Snapshot.GetLineFromLineNumber(containingLine.LineNumber + 1);
                //    //_commentSpans.Add(new SnapshotSpan(curSpan.Snapshot, new Span(nextLine.Start, nextLine.Length)));
                //}
                //else
                //{
                //    tokens1.Add(new SnapshotSpan(curSpan.Snapshot, new Span(lineStartPosition + tokenStartIndex, lineText.Length - tokenStartIndex)));
                //}


                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                string tokenText = curSpan.GetText();
                //トークンリスト生成
                string[] tokens = tokenText.ToLower().Split(null);   //nullを与えると空白文字で分割
                int currentIndex = curSpan.Start.Position;

                //foreach (var configToken in tokens1)
                //{
                //    bool comment=false;



                //    if (comment) continue;


                //    if (configToken.GetText().StartsWith("//") || configToken.GetText().StartsWith("/*"))
                //    {
                //        if (configToken.IntersectsWith(curSpan))
                //            yield return new TagSpan<ConfigTokenTag>(configToken,
                //                                                      new ConfigTokenTag(ConfigTokenTypes.Comment));
                //    }
                //    else if (_configKeywordType.Contains(configToken.GetText()))
                //    {
                //        if (configToken.IntersectsWith(curSpan))
                //            yield return new TagSpan<ConfigTokenTag>(configToken,
                //                                                  new ConfigTokenTag(_configTypes["keyword"]));
                //    }
                //    else if (_configTypes.ContainsKey(configToken.GetText()))
                //    {
                //        if (configToken.IntersectsWith(curSpan))
                //            yield return new TagSpan<ConfigTokenTag>(configToken,
                //                                                  new ConfigTokenTag(_configTypes[configToken.GetText()]));
                //    }

                //    //add an extra char location because of the space
                //    curLoc += configToken.Length + 1;
                //}

                //foreach (var configToken in tokens2)
                //{

                //    if (configToken.IntersectsWith(curSpan))
                //        yield return new TagSpan<ConfigTokenTag>(configToken,
                //                                                  new ConfigTokenTag(ConfigTokenTypes.Comment));
                //}


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
                    

                    //else if (_configTypes.ContainsKey(configToken))
                    //{
                    //    var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(currentIndex, configToken.Length));
                    //    if (tokenSpan.IntersectsWith(curSpan))
                    //        yield return new TagSpan<ConfigTokenTag>(tokenSpan,
                    //                                              new ConfigTokenTag(_configTypes[configToken]));
                    //}

                    //add an extra char location because of the space
                    currentIndex += configToken.Length + 1;
                }


                ////コメントを分類
                //if (!string.IsNullOrEmpty(commentText)) {
                //    var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc - 1, commentText.Length));
                //    yield return new TagSpan<ConfigTokenTag>(tokenSpan,
                //                                                  new ConfigTokenTag(ConfigTokenTypes.Comment));

                //    curLoc += commentText.Length;
                //}
            }

        }

    }
}
