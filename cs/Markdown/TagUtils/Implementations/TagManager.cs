using System.Text;
using Markdown.Dto;
using Markdown.TagUtils.Abstractions;
using Markdown.TokensUtils;

namespace Markdown.TagUtils.Implementations;

public class TagManager(ITagContext context) : ITagManager
{
    public void EndProcess()
    {
        while (context.Tags.Count > 0)
        {
            context.CloseTop(true);
        }
    }

    public void HeaderProcess(Token token)
    {
        context.Open(new Tag(token, new StringBuilder(token.Value), true));
    }

    public void ItalicProcess(Token token)
    {
        ProcessTag(token, false, (parent, t) =>
            CloseContext.IsInvalidUnderScoreCloseContext(parent, t, token.Value.Length));
    }

    public void StrongProcess(Token token)
    {
        ProcessTag(token, true, (parent, t) =>
            CloseContext.IsInvalidUnderScoreCloseContext(parent, t, token.Value.Length));
    }

    public void LinkProcess(Token token)
    {
        ProcessTag(token, false, (parent, t) =>
            CloseContext.IsInvalidLinkCloseContext(parent, t, token.Value.Length));
    }

    private void ProcessTag(Token token, bool isStrong,
        Func<Tag, Token, bool> isInvalidCloseContext)
    {
        var tagsStack = context.Tags;
        var parent = tagsStack.Count > 0 ? tagsStack.Peek() : null;
        var canClose = isStrong ? token.Role is TokenRole.Close : token.Role is TokenRole.Close or TokenRole.Both;
        var canOpen = isStrong
            ? token.Role is TokenRole.Open or TokenRole.Both ||
              (token.Role == TokenRole.Close && tagsStack.Count > 0 && tagsStack.Peek().IsOpen)
            : token.Role is TokenRole.Open or TokenRole.Both;
        
        if (canClose && tagsStack.Count > 0 && !isInvalidCloseContext(parent, token))
        {
            if (isStrong)
            {
                var popParent = tagsStack.Pop();
                var grandParent = tagsStack.Count > 0 ? tagsStack.Peek() : null;
                var shouldNotClose = grandParent is not null && grandParent.Token.Type == TokenType.Italic &&
                                     grandParent.IsOpen;

                if (shouldNotClose)
                {
                    var newContent = popParent.Content.Append(popParent.Token.Value);
                    context.Append(newContent.ToString());
                }
                else
                {
                    tagsStack.Push(popParent);
                    context.CloseTop();
                }
            }
            else
            {
                context.CloseTop();
            }
        }
        else if (canOpen)
        {
            context.Open(new Tag(token, new StringBuilder(token.Value), true));
        }
        else
        {
            context.Append(token.Value);
        }
    }
}