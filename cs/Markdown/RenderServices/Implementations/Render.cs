using System.Text;
using Markdown.Extensions;
using Markdown.TokensUtils;
using Markdown.TokensUtils.Abstractions;

namespace Markdown
{
    public class Render : IRender
    {
        private readonly ITokenizer tokenizer = new Tokenizer();

        public string RenderText(string markdown)
        {
            var markDownLines = markdown.EnumerateLines();
            var result = new StringBuilder();
            foreach (var line in markDownLines)
            {
                if (result.Length > 0) result.Append('\n');
                result.Append(RenderLine(line));
            }

            return result.ToString();
        }

        private string RenderLine(string line)
        {
            var tokens = tokenizer.Tokenize(line).ToList();
            var result = new StringBuilder();

            var tagStack = new Stack<(TokenType Type, string Marker, StringBuilder Content)>();
            var skipNextAsMarkup = false;

            foreach (var token in tokens)
            {
                if (skipNextAsMarkup)
                {
                    AppendToParentOrResult(tagStack, result, token.Value);
                    skipNextAsMarkup = false;
                    continue;
                }

                switch (token.Type)
                {
                    case TokenType.Text:
                        AppendToParentOrResult(tagStack, result, token.Value);
                        break;
                    case TokenType.Escape:
                        AppendToParentOrResult(tagStack, result, token.Value);
                        skipNextAsMarkup = true;
                        break;
                    case TokenType.Italic:
                        ProcessItalic(token, tagStack, result);
                        break;
                    case TokenType.Strong:
                        ProcessStrong(token, tagStack, result);
                        break;
                    case TokenType.Header:
                        ProcessHeader(tagStack);
                        break;
                    case TokenType.End:
                        ProcessEnd(tagStack, result);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result.ToString();
        }

        private void ProcessStrong(Token token, Stack<(TokenType Type, string Marker, StringBuilder Content)> tagStack,
            StringBuilder result)
        {
            if (tagStack.Count > 0)
            {
                var parent = tagStack.Peek();
                var parentType = parent.Type;

                if (parentType == TokenType.Italic)
                {
                    AppendToParentOrResult(tagStack, result, token.Value);
                }
                else if (parentType == token.Type)
                {
                    CloseTopTag(tagStack, result);
                }
                else
                {
                    tagStack.Push((token.Type, token.Value, new StringBuilder()));
                }
            }
            else
            {
                tagStack.Push((token.Type, token.Value, new StringBuilder()));
            }
        }

        private void ProcessItalic(Token token, Stack<(TokenType Type, string Marker, StringBuilder Content)> tagStack,
            StringBuilder result)
        {
            if (tagStack.Count > 0 && tagStack.Peek().Type == token.Type)
            {
                CloseTopTag(tagStack, result);
            }
            else
            {
                tagStack.Push((token.Type, token.Value, new StringBuilder()));
            }
        }

        private void ProcessHeader(Stack<(TokenType Type, string Marker, StringBuilder Content)> tagStack)
        {
            tagStack.Push((TokenType.Header, "#", new StringBuilder()));
        }

        private void ProcessEnd(Stack<(TokenType Type, string Marker, StringBuilder Content)> tagStack,
            StringBuilder result)
        {
            while (tagStack.Count > 0)
            {
                var top = tagStack.Pop();
                string rendered;

                var raw = top.Type is TokenType.Header
                    ? TagRender.Wrap(top.Type, top.Content.ToString())
                    : top.Marker +
                      top.Content;
                
                AppendToParentOrResult(tagStack, result, raw);
            }
        }

        private void AppendToParentOrResult(Stack<(TokenType Type, string Marker, StringBuilder Content)> tagStack,
            StringBuilder result, string content)
        {
            if (tagStack.Count > 0) tagStack.Peek().Content.Append(content);
            else result.Append(content);
        }

        private void CloseTopTag(Stack<(TokenType Type, string Marker, StringBuilder Content)> tagStack,
            StringBuilder result)
        {
            var top = tagStack.Pop();
            var wrapped = TagRender.Wrap(top.Type, top.Content.ToString());
            AppendToParentOrResult(tagStack, result, wrapped);
        }
    }
}