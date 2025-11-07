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
            var top = tagsStack.Pop();
            var content = top.Content.ToString();

            var raw = top.Token.Type == TokenType.Header
                ? TagRender.Wrap(top.Token.Type, content[1..])
                : content;

            contentManager.AppendToParentOrResult(tagsStack, result, raw);
        }
    }

    public void HeaderProcess(Token token, Stack<Tag> tagsStack)
    {
        tagsStack.Push(new Tag(token, new StringBuilder(token.Value), true));
    }

    public void ItalicProcess(Token token, Stack<Tag> tagsStack, StringBuilder result)
    {
        var canOpen = token.Role is TokenRole.Open or TokenRole.Both;
        var parent = tagsStack.Count > 0 ? tagsStack.Peek() : null;
        if (token.Role is TokenRole.Close or TokenRole.Both && tagsStack.Count > 0 &&
            !CloseContext.IsInvalidCloseContext(parent, token, 2))
        {
            contentManager.CloseTopTag(tagsStack, result);
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

    public void StrongProcess(Token token, Stack<Tag> tagsStack, StringBuilder result)
    {
        var parent = tagsStack.Count > 0 ? tagsStack.Peek() : null;

        if (token.Role == TokenRole.Close && tagsStack.Count > 0 && !CloseContext.IsInvalidCloseContext(parent, token, 2))
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
        else if (token.Role is TokenRole.Open or TokenRole.Both ||
                 (token.Role == TokenRole.Close && tagsStack.Count > 0 && tagsStack.Peek().IsOpen))
        {
            tagsStack.Push(new Tag(token, new StringBuilder(token.Value), true));
        }
        else
        {
            contentManager.AppendToParentOrResult(tagsStack, result, token.Value);
        }
    }
}