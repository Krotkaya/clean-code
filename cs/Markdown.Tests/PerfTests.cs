using System.Text;
using FluentAssertions;
using Markdown.Core.Lexing;
using Markdown.Core.Parsing;
using Markdown.Core.Rendering;
using NUnit.Framework;
namespace Markdown.Tests;

/// <summary>
/// Тесты, проверяющие производительность
/// </summary>
[TestFixture]
public class PerfTests
{
    private Md _markdown;

    [SetUp]
    public void Setup()
    {
        var lexer = new Lexer();
        var parser = new Parser();
        var renderer = new Renderer();
        _markdown = new Md(lexer, parser, renderer);
    }

    [Test]
    [Timeout(2000)]
    public void Render_ShouldHandleLongInputLinearly()
    {
        const int paragraphs = 5000;

        var inputBuilder = new StringBuilder();
        var expectedBuilder = new StringBuilder();

        for (var i = 0; i < paragraphs; i++)
        {
            inputBuilder.Append($"__жирный{i}__ _курсив{i}_ текст{i}");
            if (i < paragraphs - 1)
                inputBuilder.Append("\n\n");

            expectedBuilder.Append("<p><strong>жирный").Append(i)
                .Append("</strong> <em>курсив").Append(i)
                .Append("</em> текст").Append(i)
                .Append("</p>");
        }

        var input = inputBuilder.ToString();
        var expected = expectedBuilder.ToString();

        var html = _markdown.Render(input);

        html.Should().Be(expected);
        html.Length.Should().Be(expected.Length);
    }
    
}