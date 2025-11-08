using System.Text;
using Markdown.Dto;
using Markdown.TagUtils.Abstractions;
using Markdown.TokensUtils;

namespace Markdown.TagUtils.Implementations
{
    public class TagProcessor(ITagContentManager contentManager, ITagManager tagManager) : ITagProcessor
    {
        public string Process(IEnumerable<Token> tokens)
        {
            var result = new StringBuilder();
            var tagsStack = new Stack<Tag>();
            var skipNextAsMarkup = false;

            foreach (var token in tokens)
            {
                if (skipNextAsMarkup)
                {
                    contentManager.AppendToParentOrResult(tagsStack, result, token.Value);
                    skipNextAsMarkup = false;
                    continue;
                }

                switch (token.Type)
                {
                    case TokenType.Text:
                        contentManager.AppendToParentOrResult(tagsStack, result, token.Value);
                        break;

                    case TokenType.Escape:
                        contentManager.AppendToParentOrResult(tagsStack, result, token.Value);
                        skipNextAsMarkup = true;
                        break;

                    case TokenType.Italic:
                        tagManager.ItalicProcess(token, tagsStack, result);
                        break;

                    case TokenType.Strong:
                        tagManager.StrongProcess(token, tagsStack, result);
                        break;
                    
                    case TokenType.Link:
                        tagManager.LinkProcess(token, tagsStack, result);
                        break;

                    case TokenType.Header:
                        tagManager.HeaderProcess(token, tagsStack);
                        break;

                    case TokenType.End:
                        tagManager.EndProcess(tagsStack, result);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result.ToString();
        }
    }
}