using System.Text;
using Markdown.Dto;
using Markdown.TagUtils.Abstractions;
using Markdown.TokensUtils;

namespace Markdown.TagUtils.Implementations
{
    public class TagProcessor(ITagContext context, ITagManager tagManager) : ITagProcessor
    {
        public string Process(IEnumerable<Token> tokens)
        {
            using (context)
            {
                var skipNextAsMarkup = false;

                foreach (var token in tokens)
                {
                    if (skipNextAsMarkup)
                    {
                        context.Append(token.Value);
                        skipNextAsMarkup = false;
                        continue;
                    }

                    switch (token.Type)
                    {
                        case TokenType.Text:
                            context.Append(token.Value);                        
                            break;

                        case TokenType.Escape:
                            context.Append(token.Value);
                            skipNextAsMarkup = true;
                            break;

                        case TokenType.Italic:
                            tagManager.ItalicProcess(token);
                            break;

                        case TokenType.Strong:
                            tagManager.StrongProcess(token);
                            break;
                    
                        case TokenType.Link:
                            tagManager.LinkProcess(token);
                            break;

                        case TokenType.Header:
                            tagManager.HeaderProcess(token);
                            break;

                        case TokenType.End:
                            tagManager.EndProcess();
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return context.Content.ToString();
            }
        }
    }
}