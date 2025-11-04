using FluentAssertions;
using NUnit.Framework;

namespace Markdown.Tests;

public class MarkdownTests
{
    private IRender render = new Render();

    [Test]
    public void Markdown_ShouldBeTextWrappedWithEmTag_WhenSurroundedBySingleUnderscores_Test()
    {
        var line =
            "Текст, _окруженный с двух сторон_ одинарными символами подчерка";

        var result = render.RenderText(line);
        var expected = "Текст, <em>окруженный с двух сторон</em> одинарными символами подчерка";
        result
            .Should()
            .Be(expected);
    }

    [Test]
    public void Markdown_ShouldBeTextWrappedWithStrongTag_WhenSurroundedByDoubleUnderscores_Test()
    {
        var line = "__Выделенный двумя символами текст__ должен становиться полужирным";

        var result = render.RenderText(line);
        var expected = "<strong>Выделенный двумя символами текст</strong> должен становиться полужирным";
        result
            .Should()
            .Be(expected);
    }

    [Test]
    public void Markdown_ShouldAddEscapeCharacter_WhenTextContainsMarkdownTags_Test()
    {
        var line = "Любой тег можно экранировать \\<em> и даже \\<strong>";
        var result = render.RenderText(line);
        var expected = "Любой тег можно экранировать \\<em> и даже \\<strong>";
        result
            .Should()
            .Be(expected);
    }

    [Test]
    public void Markdown_ShouldBeTextNotWrappedWithEmTag_WhenUnderscoresAreEscaped_Test()
    {
        var line =
            "Любой символ можно экранировать, чтобы он не считался частью разметки.\n\\_Вот это\\_, не должно выделиться тегом \\<em>. \n Также \\__Это не выделяется\\__ тегом \\<strong>.";
        var result = render.RenderText(line);
        var expected =
            "Любой символ можно экранировать, чтобы он не считался частью разметки.\n\\_Вот это\\_, не должно выделиться тегом \\<em>. \n Также \\__Это не выделяется\\__ тегом \\<strong>.";
        result
            .Should()
            .Be(expected);
    }

    [Test]
    public void Markdown_ShouldRenderNestedTagsCorrectly_WhenDoubleAndSingleUnderscoresAreMixed_Test()
    {
        var line = "Внутри __двойного выделения _одинарное_ тоже__ работает.";
        var result = render.RenderText(line);
        var expected = "Внутри <strong>двойного выделения <em>одинарное</em> тоже</strong> работает.";

        result
            .Should()
            .Be(expected);
    }

    [Test]
    public void Markdown_ShouldNotRenderStrongInsideEm_WhenDoubleUnderscoresInsideSingleUnderscore_Test()
    {
        var line = "Но не наоборот — внутри _одинарного __двойное__ не_ работает.";
        var result = render.RenderText(line);
        var expected = "Но не наоборот — внутри <em>одинарного __двойное__ не</em> работает.";
        result.Should().Be(expected);
    }

    [Test]
    public void Markdown_ShouldNotRenderEmOrStrong_WhenUnderscoresSurroundNumbers_Test()
    {
        var line = "Подчерки внутри текста c цифрами_12__3 не считаются выделением и должны оставаться символами подчерка.";
        var result = render.RenderText(line);
        var expected =
            "Подчерки внутри текста c цифрами_12__3 не считаются выделением и должны оставаться символами подчерка.";

        result.Should().Be(expected);
    }

    [Test]
    public void Markdown_ShouldRenderEmOrStrong_WhenUnderscoresInsideWord_Test()
    {
        var line = "Однако выделять часть слова они могут: и в _нач_але, и в сер_еди_не, и в кон__це.__";
        var result = render.RenderText(line);
        var expected = "Однако выделять часть слова они могут: и в <em>нач</em>але, и в сер<em>еди</em>не, и в кон<strong>це.</strong>";
        result.Should().Be(expected);
    }

    [Test]
    public void Markdown_ShouldNotRenderEmOrStrong_WhenUnderscoresAcrossMultipleWords_Test()
    {
        var line = "В то же время выделение в ра_зных сл_овах не работает.";
        var result = render.RenderText(line);
        var expected = "В то же время выделение в ра_зных сл_овах не работает.";
        result.Should().Be(expected);
    }

    [Test]
    public void Markdown_ShouldNotRenderEmOrStrong_WhenUnmatchedUnderscoresInParagraph_Test()
    {
        var line = "__Непарные_ символы в рамках одного абзаца не считаются выделением.";
        var result = render.RenderText(line);
        var expected = "__Непарные_ символы в рамках одного абзаца не считаются выделением.";
        result.Should().Be(expected);
    }

    [Test]
    public void Markdown_ShouldNotRenderEmOrStrong_WhenUnderscoreFollowedByWhitespace_Test()
    {
        var line =
            "За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ подчерки_ не считаются выделением \nи остаются просто символами подчерка.";
        var result = render.RenderText(line);
        var expected =
            "За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ подчерки_ не считаются выделением \nи остаются просто символами подчерка.";
        result.Should().Be(expected);
    }

    [Test]
    public void Markdown_ShouldNotRenderEmOrStrong_WhenEndingUnderscorePrecededByWhitespace_Test()
    {
        var line =
            "Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти _подчерки _не считаются_ окончанием выделения \nи остаются просто символами подчерка.";
        var result =  render.RenderText(line);
        var expected =
            "Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти _подчерки _не считаются_ окончанием выделения \nи остаются просто символами подчерка.";
        result.Should().Be(expected);
    }

    [Test]
    public void Markdown_ShouldNotRenderEmOrStrong_WhenSingleAndDoubleUnderscoresIntersect_Test()
    {
        var line = "В случае __пересечения _двойных__ и одинарных_ подчерков ни один из них не считается выделением.\n";
        var result = render.RenderText(line);
        var expected =
            "В случае __пересечения _двойных__ и одинарных_ подчерков ни один из них не считается выделением.\n";
        result.Should().Be(expected);
    }

    [Test]
    public void Markdown_ShouldNotRenderStrongOrEm_WhenDoubleUnderscoresAreEmpty_Test()
    {
        var line = "Если внутри подчерков пустая строка _____, то они остаются символами подчерка.\n";
        var result = render.RenderText(line);
        var expected = "Если внутри подчерков пустая строка _____, то они остаются символами подчерка.\n";
        result.Should().Be(expected);
    }
}