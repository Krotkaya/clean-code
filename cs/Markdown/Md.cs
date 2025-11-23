using Markdown.Core.Lexing;
using Markdown.Core.Parsing;
using Markdown.Core.Rendering;

namespace Markdown;

/// <summary>
/// Принимает текст в упрощённой разметке и возвращает HTML
/// </summary>
public class Md(ILexer lexer, IParser parser, IRenderer renderer)
{
    private readonly ILexer _lexer = lexer;
    private readonly IParser _parser = parser;
    private readonly IRenderer _renderer = renderer;

    public string Render(string text)
    {
         var tokens = _lexer.Tokenize(text.AsMemory());
        var document = _parser.Parse(tokens);
        return _renderer.Render(document);
        
    }

}