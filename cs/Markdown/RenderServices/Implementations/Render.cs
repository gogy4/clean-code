using System.Text;
using Markdown.Dto;
using Markdown.Extensions;
using Markdown.TokensUtils;
using Markdown.TokensUtils.Abstractions;
using Markdown.TokensUtils.Implementations;

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

            var tagsStack = new Stack<Tag>();
            var skipNextAsMarkup = false;

            foreach (var token in tokens)
            {
                if (skipNextAsMarkup)
                {
                    AppendToParentOrResult(tagsStack, result, token.Value);
                    skipNextAsMarkup = false;
                    continue;
                }

                switch (token.Type)
                {
                    case TokenType.Text:
                        AppendToParentOrResult(tagsStack, result, token.Value);
                        break;

                    case TokenType.Escape:
                        AppendToParentOrResult(tagsStack, result, token.Value);
                        skipNextAsMarkup = true;
                        break;

                    case TokenType.Italic:
                        ProcessItalic(token, tagsStack, result);
                        break;

                    case TokenType.Strong:
                        ProcessStrong(token, tagsStack, result);
                        break;

                    case TokenType.Header:
                        ProcessHeader(token, tagsStack);
                        break;

                    case TokenType.End:
                        ProcessEnd(tagsStack, result);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result.ToString();
        }

        private void ProcessStrong(Token token, Stack<Tag> tagsStack, StringBuilder result)
        {
            var parent = tagsStack.Count > 0 ? tagsStack.Peek() : null;

            if (token.CanClose && tagsStack.Count > 0 && !IsInvalidCloseContext(parent, token, 2))
            {
                var popParent = tagsStack.Pop();
                var grandParent = tagsStack.Count > 0 ? tagsStack.Peek() : null;
                var shouldNotClose = grandParent is not null && grandParent.Token.Type == TokenType.Italic && grandParent.isOpen;

                if (shouldNotClose)
                {
                    var newContent = popParent.Content.Append(popParent.Token.Value);
                    AppendToParentOrResult(tagsStack, result, newContent.ToString());
                }
                else
                {
                    tagsStack.Push(popParent);
                    CloseTopTag(tagsStack, result);
                }
            }
            else if (token.CanOpen || (token.CanClose && tagsStack.Count > 0 && tagsStack.Peek().isOpen))
            {
                tagsStack.Push(new Tag(token, new StringBuilder(token.Value), true));
            }
            else
            {
                AppendToParentOrResult(tagsStack, result, token.Value);
            }
        }

        private void ProcessItalic(Token token, Stack<Tag> tagsStack, StringBuilder result)
        {
            var canOpen = token.CanOpen;
            var parent = tagsStack.Count > 0 ? tagsStack.Peek() : null;
            if (token.CanClose && tagsStack.Count > 0 && !IsInvalidCloseContext(parent, token, 2))
            {
                CloseTopTag(tagsStack, result);
            }
            else if (canOpen)
            {
                tagsStack.Push(new Tag(token, new StringBuilder(token.Value), true));
            }
            else
            {
                AppendToParentOrResult(tagsStack, result, token.Value);
            }
        }

        private void ProcessHeader(Token token, Stack<Tag> tagsStack)
        {
            tagsStack.Push(new Tag(token, new StringBuilder(token.Value), true));
        }

        private void ProcessEnd(Stack<Tag> tagsStack, StringBuilder result)
        {
            while (tagsStack.Count > 0)
            {
                var top = tagsStack.Pop();
                var content = top.Content.ToString();

                var raw = top.Token.Type == TokenType.Header
                    ? TagRender.Wrap(top.Token.Type, content[1..])
                    : content;

                AppendToParentOrResult(tagsStack, result, raw);
            }
        }

        private void AppendToParentOrResult(Stack<Tag> tagsStack, StringBuilder result, string content)
        {
            if (tagsStack.Count > 0)
            {
                tagsStack.Peek().Content.Append(content);
            }
            else
            {
                result.Append(content);
            }
        }

        private void CloseTopTag(Stack<Tag> tagsStack, StringBuilder result)
        {
            var top = tagsStack.Pop();
            var content = top.Content.ToString();

            var markerLength = top.Token.Value.Length;
            var innerContent = content.Length > markerLength
                ? content[markerLength..]
                : string.Empty;

            var wrapped = TagRender.Wrap(top.Token.Type, innerContent);
            AppendToParentOrResult(tagsStack, result, wrapped);
        }
        
        private bool IsInvalidCloseContext(Tag parent, Token token, int markerLength)
        {
            if (parent == null) return true;
            if (parent.Token.Type != token.Type) return true;
            if (parent.Token.InsideWord && parent.Content.ToString().Contains(' ')) return true;

            var contentSpan = parent.Content.ToString().AsSpan(markerLength);
            return contentSpan.Length == 0 || decimal.TryParse(contentSpan, out _);
        }
    }
}