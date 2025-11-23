using Markdown.Core.Lexing;
using Markdown.Core.Parsing.Nodes;

namespace Markdown.Core.Parsing;

public class Parser : IParser
{
    private IEnumerator<Token> _tokenPointer;
    private Token _currentToken;
    private readonly List<Token> _allTokens = [];
    private int _currentIndex;
    private readonly InlineValidator _inlineValidator = new();
    private InlineParser _inlineParser;
    
    public DocumentNode Parse(IEnumerable<Token> tokens)
    {
        _allTokens.Clear();
        _allTokens.AddRange(tokens);
        
        _tokenPointer = _allTokens.GetEnumerator();
        _currentIndex = 0;
        _inlineParser = new InlineParser(
            _allTokens,
            MoveOnNextToken,
            () => _currentToken,
            () => _currentIndex,
            IsEndOfLine,
            _inlineValidator);
        MoveOnNextToken();

        var document = new DocumentNode();
        
        while (_currentToken.Kind != TokenKind.Eof)
        {
            var block = ParseBlock();
            if (block != null)
                document.Children.Add(block);
        }
        return document;
    }

    private BlockNode? ParseBlock()
    {
        SkipEmptyLines();

        if (IsEndOfFile())
            return null;

        if (IsHeadingStart())
            return ParseHeading();

        return ParseParagraph();
    }

    private void SkipEmptyLines()
    {
        while (_currentToken.Kind == TokenKind.NewLine)
            MoveOnNextToken();
    }

    private bool IsEndOfFile() => _currentToken.Kind == TokenKind.Eof;

    private bool IsHeadingStart() =>
        _currentToken.Kind == TokenKind.Hash && IsAtStartOfLine();


    private ParagraphNode ParseParagraph()
    {
        var paragraph = new ParagraphNode();

        while (!IsEndOfLine())
        {
            var inline = _inlineParser.ParseInline();
            if (inline != null)
                paragraph.Inlines.Add(inline);
        }

        if (_currentToken.Kind != TokenKind.NewLine) return paragraph;
        MoveOnNextToken();
        SkipEmptyLines();

        return paragraph;
    }

    private bool IsEndOfLine()
    {
        return _currentToken.Kind == TokenKind.NewLine || _currentToken.Kind == TokenKind.Eof;
    }

    private HeadingNode ParseHeading()
    {
        var heading = new HeadingNode(1);

        MoveOnNextToken(); 
        SkipSpace();

        while (!IsEndOfLine())
        {
            var inline = _inlineParser.ParseInline();
            if (inline != null)
                heading.Inlines.Add(inline);
        }

        if (_currentToken.Kind == TokenKind.NewLine)
            MoveOnNextToken();

        return heading;
    }
    
    private void SkipSpace()
    {
        if (_currentToken.Kind == TokenKind.Space)
            MoveOnNextToken();
    }

    private bool IsAtStartOfLine() =>
        _currentIndex <= 1 || _allTokens[_currentIndex - 2].Kind ==
        TokenKind.NewLine;


    private Token MoveOnNextToken()
    {
        if (!_tokenPointer.MoveNext())
        {
            _currentToken = new Token(TokenKind.Eof, ReadOnlyMemory<char>.Empty, -1);
        }
        else
        {
            _currentToken = _tokenPointer.Current;
            _currentIndex++;
        }

        return _currentToken;
    }
}
