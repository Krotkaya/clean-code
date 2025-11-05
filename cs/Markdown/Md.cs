using Markdown.Core.Lexing;
using Markdown.Core.Parsing;
using Markdown.Core.Rendering;

namespace Markdown;

/// <summary>
/// Принимает текст в упрощённой разметке и возвращает HTML
/// </summary>
public class Md(ILexer lexer, IParser parser, IHtmlRenderer htmlRenderer)
{
    private readonly ILexer _lexer = lexer;
    private readonly IParser _parser = parser;
    private readonly IHtmlRenderer _htmlRenderer = htmlRenderer;

    public string Render(string text)
    {
        throw new NotImplementedException();
    }
}