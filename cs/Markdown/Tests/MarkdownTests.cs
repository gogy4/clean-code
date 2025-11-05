using System.Diagnostics;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Markdown.Tests;

public class MarkdownTests
{
    private IRender render = new Render();

    [TestCaseSource(nameof(MarkdownCases))]
    public void Markdown_RenderText_ShouldMatchExpected(string line, string expectedAndErrorMessage)
    {
        var result = render.RenderText(line);
        result.Should().Be(expectedAndErrorMessage, expectedAndErrorMessage);
    }

    [Test]
    public void Markdown_ShouldProcessLongInputWithoutCrashing_Test()
    {
        var longLine = new string('_', 10_000) + "тест" + new string('_', 10_000);
        var result = render.RenderText(longLine);

        using (new AssertionScope())
        {
            result.Length.Should().BeGreaterThanOrEqualTo(longLine.Length, "рендер не должен обрезать длинный ввод");

            var stopwatch = Stopwatch.StartNew();
            render.RenderText(longLine);
            stopwatch.Stop();

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000, "рендер должен завершаться за приемлемое время");
        }
    }
    
    public static IEnumerable<TestCaseData> MarkdownCases
    {
        get
        {
            yield return new TestCaseData(
                "Текст, _окруженный с двух сторон_ одинарными символами подчерка",
                "Текст, <em>окруженный с двух сторон</em> одинарными символами подчерка"
            ).SetName("SingleUnderscores_Em");

            yield return new TestCaseData(
                "__Выделенный двумя символами текст__ должен становиться полужирным",
                "<strong>Выделенный двумя символами текст</strong> должен становиться полужирным"
            ).SetName("DoubleUnderscores_Strong");

            yield return new TestCaseData(
                "Любой тег можно экранировать \\<em> и даже \\<strong>",
                "Любой тег можно экранировать \\<em> и даже \\<strong>"
            ).SetName("EscapeMarkdownTags");

            yield return new TestCaseData(
                "Любой символ можно экранировать, чтобы он не считался частью разметки.\n\\_Вот это\\_, не должно выделиться тегом \\<em>. \n Также \\__Это не выделяется\\__ тегом \\<strong>.",
                "Любой символ можно экранировать, чтобы он не считался частью разметки.\n\\_Вот это\\_, не должно выделиться тегом \\<em>. \n Также \\__Это не выделяется\\__ тегом \\<strong>."
            ).SetName("EscapedUnderscores_NoRender");

            yield return new TestCaseData(
                "Внутри __двойного выделения _одинарное_ тоже__ работает.",
                "Внутри <strong>двойного выделения <em>одинарное</em> тоже</strong> работает."
            ).SetName("Nested_StrongAndEm");

            yield return new TestCaseData(
                "Но не наоборот — внутри _одинарного __двойное__ не_ работает.",
                "Но не наоборот — внутри <em>одинарного __двойное__ не</em> работает."
            ).SetName("StrongInsideEm_NoRender");

            yield return new TestCaseData(
                "Подчерки внутри текста c цифрами_12_3 или 1__123 не считаются выделением и должны оставаться символами подчерка.",
                "Подчерки внутри текста c цифрами_12_3 или 1__123 не считаются выделением и должны оставаться символами подчерка."
            ).SetName("UnderscoresAroundNumbers_NoRender");

            yield return new TestCaseData(
                "Однако выделять часть слова они могут: и в _нач_але, и в сер_еди_не, и в кон__це.__",
                "Однако выделять часть слова они могут: и в <em>нач</em>але, и в сер<em>еди</em>не, и в кон<strong>це.</strong>"
            ).SetName("UnderscoresInsideWord_Render");

            yield return new TestCaseData(
                "В то же время выделение в ра_зных сл_овах не работает.",
                "В то же время выделение в ра_зных сл_овах не работает."
            ).SetName("UnderscoresAcrossWords_NoRender");

            yield return new TestCaseData(
                "__Непарные_ символы в рамках одного абзаца не считаются выделением.",
                "__Непарные_ символы в рамках одного абзаца не считаются выделением."
            ).SetName("UnmatchedUnderscores_NoRender");

            yield return new TestCaseData(
                "За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ подчерки_ не считаются выделением \nи остаются просто символами подчерка.",
                "За подчерками, начинающими выделение, должен следовать непробельный символ. Иначе эти_ подчерки_ не считаются выделением \nи остаются просто символами подчерка."
            ).SetName("StartingUnderscoreFollowedByWhitespace_NoRender");

            yield return new TestCaseData(
                "Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти _подчерки _не считаются_ окончанием выделения \nи остаются просто символами подчерка.",
                "Подчерки, заканчивающие выделение, должны следовать за непробельным символом. Иначе эти _подчерки <em>не считаются</em> окончанием выделения \nи остаются просто символами подчерка."
            ).SetName("EndingUnderscorePrecededByWhitespace_Render");

            yield return new TestCaseData(
                "В случае __пересечения _двойных__ и одинарных_ подчерков ни один из них не считается выделением.\n",
                "В случае __пересечения _двойных__ и одинарных_ подчерков ни один из них не считается выделением.\n"
            ).SetName("IntersectingSingleAndDoubleUnderscores_NoRender");

            yield return new TestCaseData(
                "Если внутри подчерков пустая строка _____, то они остаются символами подчерка.\n",
                "Если внутри подчерков пустая строка _____, то они остаются символами подчерка.\n"
            ).SetName("EmptyDoubleUnderscores_NoRender");

            yield return new TestCaseData(
                "# Заголовок __с _разными_ символами__",
                "<h1>Заголовок <strong>с <em>разными</em> символами</strong></h1>"
            ).SetName("H1Tag_Render");

            yield return new TestCaseData(
                "___Слово___",
                "___Слово___"
            ).SetName("TripleUnderscore_NoRender");
        }
    }
}
