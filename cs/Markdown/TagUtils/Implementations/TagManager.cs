using System.Text;
using Markdown.Dto;
using Markdown.TagUtils.Abstractions;
using Markdown.TokensUtils;

namespace Markdown.TagUtils.Implementations;

public class TagManager(ITagContentManager contentManager) : ITagManager
{
    public void EndProcess(Stack<Tag> tagsStack, StringBuilder result)
    {
        while (tagsStack.Count > 0)
        {
            contentManager.CloseTopTag(tagsStack, result, true);
        }
    }

    public void HeaderProcess(Token token, Stack<Tag> tagsStack)
    {
        tagsStack.Push(new Tag(token, new StringBuilder(token.Value), true));
    }

    public void ItalicProcess(Token token, Stack<Tag> tagsStack, StringBuilder result)
    {
        ProcessTag(token, tagsStack, result, false, (parent, t) =>
            CloseContext.IsInvalidUnderScoreCloseContext(parent, t, token.Value.Length));
    }

    public void StrongProcess(Token token, Stack<Tag> tagsStack, StringBuilder result)
    {
        ProcessTag(token, tagsStack, result, true, (parent, t) =>
            CloseContext.IsInvalidUnderScoreCloseContext(parent, t, token.Value.Length));
    }

    public void LinkProcess(Token token, Stack<Tag> tagsStack, StringBuilder result)
    {
        ProcessTag(token, tagsStack, result, false, (parent, t) =>
            CloseContext.IsInvalidLinkCloseContext(parent, t, token.Value.Length));
    }

    private void ProcessTag(Token token, Stack<Tag> tagsStack, StringBuilder result, bool isStrong,
        Func<Tag, Token, bool> isInvalidCloseContext)
    {
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
                    contentManager.AppendToParentOrResult(tagsStack, result, newContent.ToString());
                }
                else
                {
                    tagsStack.Push(popParent);
                    contentManager.CloseTopTag(tagsStack, result);
                }
            }
            else
            {
                contentManager.CloseTopTag(tagsStack, result);
            }
        }
        else if (canOpen)
        {
            tagsStack.Push(new Tag(token, new StringBuilder(token.Value), true));
        }
        else
        {
            contentManager.AppendToParentOrResult(tagsStack, result, token.Value);
        }
    }
}