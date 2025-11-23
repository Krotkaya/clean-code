using System.Text;
using Markdown.Core.Lexing;
using Markdown.Core.Parsing.Nodes;

namespace Markdown.Core.Parsing;

public class Parser : IParser
{
    private IEnumerator<Token> _tokenPointer;
    private Token _currentToken;
    private readonly List<Token> _allTokens = new();
    private int _currentIndex;

    public DocumentNode Parse(IEnumerable<Token> tokens)
    {
        _allTokens.Clear();
        _allTokens.AddRange(tokens);
        
        _tokenPointer = _allTokens.GetEnumerator();
        _currentIndex = 0;
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
            var inline = ParseInline();
            if (inline != null)
                paragraph.Inlines.Add(inline);
        }

        if (_currentToken.Kind == TokenKind.NewLine)
        {
            MoveOnNextToken();
            SkipEmptyLines();
        }

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
            var inline = ParseInline();
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

    private InlineNode ParseInline()
    {
        if (_currentToken.Kind == TokenKind.Eof)
            return null;

        return _currentToken.Kind switch
        {
            TokenKind.Text => ParseText(),
            TokenKind.Underscore => ParseEmphasis(),
            TokenKind.DoubleUnderscore => ParseStrong(),
            TokenKind.Space => ParseText(),
            _ => ParseText()
        };
    }

    private TextNode ParseText()
    {
        var node = new TextNode(_currentToken.Slice.ToString());
        MoveOnNextToken();
        return node;
    }

    private InlineNode ParseEmphasis()
    {
        var startIndex = _currentIndex - 1;
        MoveOnNextToken(); 

        if (IsInvalidEmphasisStart())
            return CreateTextNode("_");

        var emphasis = new EmphasisNode();
        return ParseEmphasisContent(emphasis, startIndex);
    }

    private bool IsInvalidEmphasisStart() =>
        _currentToken.Kind is TokenKind.Space or TokenKind.NewLine or
            TokenKind.Eof;

    private InlineNode ParseEmphasisContent(EmphasisNode emphasis, int
        startIndex)
    {
        while (!IsEndOfContent())
        {
            if (_currentToken.Kind == TokenKind.Underscore)
            {
                var closeResult = TryCloseEmphasis(emphasis, startIndex);
                if (closeResult.ShouldReturn)
                    return closeResult.Node;
            }
            else if (_currentToken.Kind == TokenKind.DoubleUnderscore)
            {
                emphasis.Inlines.Add(CreateTextNode("__"));
                MoveOnNextToken();
            }
            else
            {
                var inline = ParseInline();
                if (inline != null)
                    emphasis.Inlines.Add(inline);
            }
        }

        return ConvertToTextNode(emphasis, "_");
    }


    private InlineNode ParseStrong()
    {
        var startIndex = _currentIndex - 1;
        MoveOnNextToken();

        if (IsInvalidStrongStart())
            return ConvertToTextNode(new StrongNode(), "__");

        var strong = new StrongNode();
        return ParseStrongContent(strong, startIndex);
    }

    private bool IsInvalidStrongStart() =>
        _currentToken.Kind is TokenKind.Space or TokenKind.NewLine or
            TokenKind.Eof;
    
    private TextNode CreateTextNode(string text) => new(text);
    private InlineNode ParseStrongContent(StrongNode strong, int startIndex)
    {
        while (!IsEndOfContent())
        {
            if (_currentToken.Kind == TokenKind.DoubleUnderscore)
            {
                var closeResult = TryCloseStrong(strong, startIndex);
                if (closeResult.ShouldReturn)
                    return closeResult.Node;
            }
            else if (_currentToken.Kind == TokenKind.Underscore)
            {
                var emphasis = ParseEmphasis();
                if (emphasis != null)
                    strong.Inlines.Add(emphasis);
            }
            else
            {
                var inline = ParseInline();
                if (inline != null)
                    strong.Inlines.Add(inline);
            }
        }

        return ConvertToTextNode(strong, "__");
    }

    private bool IsEndOfContent() =>
        _currentToken.Kind == TokenKind.NewLine || _currentToken.Kind ==
        TokenKind.Eof;
    
    private CloseResult TryCloseStrong(StrongNode strong, int startIndex)
    {
        if (IsValidStrongClose(startIndex))
        {
            MoveOnNextToken();
            return new CloseResult(true, strong);
        }

        strong.Inlines.Add(CreateTextNode("__"));
        MoveOnNextToken();
        return new CloseResult(false, null);
    }
    
    private bool IsValidEmphasisClose(int startIndex)
    {
        var closeIndex = _currentIndex - 1;

        if (!HasValidOpeningBoundary(startIndex) || !
                HasValidClosingBoundary(closeIndex))
            return false;
        if (startIndex + 1 == closeIndex)
            return false;
        if (IsInDigitContext(startIndex) || IsInDigitContext(closeIndex))
            return false;
        if (HasIntersectingDoubleInsideEmphasis(startIndex, closeIndex))
            return false;
        if (IsInsideWord(startIndex) && IsInsideWord(closeIndex) &&
            ContainsWhitespaceBetween(startIndex, closeIndex))
            return false;

        return true;
    }


    private bool IsInDigitContext(int index) =>
        HasDigitBefore(index) || HasDigitAfter(index);


    private bool HasDigitBefore(int index)
    {
        if (index == 0)
            return false;

        var prev = _allTokens[index - 1];
        return prev.Kind == TokenKind.Text &&
               prev.Slice.Length > 0 &&
               char.IsDigit(prev.Slice.Span[^1]);
    }

    private bool HasDigitAfter(int index)
    {
        if (index + 1 >= _allTokens.Count)
            return false;

        var next = _allTokens[index + 1];
        return next.Kind == TokenKind.Text &&
               next.Slice.Length > 0 &&
               char.IsDigit(next.Slice.Span[0]);
    }

    private bool HasValidOpeningBoundary(int startIndex)
    {
        if (startIndex + 1 >= _allTokens.Count)
            return true;

        var next = _allTokens[startIndex + 1];
        return next.Kind is not TokenKind.Space and not TokenKind.NewLine;
    }
    
    private bool HasValidClosingBoundary(int closeIndex)
    {
        if (closeIndex - 1 < 0)
            return true;

        var prev = _allTokens[closeIndex - 1];
        if (prev.Kind != TokenKind.Space)
            return true;

        if (closeIndex + 1 >= _allTokens.Count)
            return true;

        var next = _allTokens[closeIndex + 1];
        return next.Kind is TokenKind.Space or TokenKind.NewLine or TokenKind.Eof;
    }

    private bool IsValidStrongClose(int startIndex)
    {
        var closeIndex = _currentIndex - 1;

        if (!HasValidOpeningBoundary(startIndex) || !
                HasValidClosingBoundary(closeIndex))
            return false;
        if (startIndex + 1 == closeIndex)
            return false;
        if (IsInDigitContext(startIndex) || IsInDigitContext(closeIndex))
            return false;
        if (HasIntersectingDoubleUnderscore(startIndex, closeIndex))
            return false;
        if (HasIntersectingSingleInsideStrong(startIndex, closeIndex))
            return false;
        if (IsInsideWord(startIndex) && IsInsideWord(closeIndex) &&
            ContainsWhitespaceBetween(startIndex, closeIndex))
            return false;

        return true;
    }

    private bool HasIntersectingDoubleUnderscore(int startIndex, int
        closeIndex)
    {
        for (var i = startIndex + 1; i < closeIndex; i++)
            if (_allTokens[i].Kind == TokenKind.DoubleUnderscore)
                return true;
        return false;
    }

    private InlineNode ConvertToTextNode(InlineNode node, string prefix)
    {
        var textContent = new StringBuilder();
        textContent.Append(prefix);
        textContent.Append(ExtractTextFromNode(node));
        return new TextNode(textContent.ToString());
    }

    private string ExtractTextFromNode(InlineNode node)
    {
        var result = new StringBuilder();

        switch (node)
        {
            case EmphasisNode emphasis:
                foreach (var inline in emphasis.Inlines)
                {
                    result.Append(ExtractTextFromNode(inline));
                }

                break;
            case StrongNode strong:
                foreach (var inline in strong.Inlines)
                {
                    result.Append(ExtractTextFromNode(inline));
                }

                break;
            case TextNode text:
                result.Append(text.Text);
                break;
        }

        return result.ToString();
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

    private class CloseResult
    {
        public bool ShouldReturn { get; }
        public InlineNode Node { get; }

        public CloseResult(bool shouldReturn, InlineNode node)
        {
            ShouldReturn = shouldReturn;
            Node = node;
        }
    }

    private bool IsInsideWord(int index) =>
        HasLetterOrDigitBefore(index) && HasLetterOrDigitAfter(index);
    
    
    private bool HasLetterOrDigitBefore(int index)
    {
        if (index == 0)
            return false;

        var prev = _allTokens[index - 1];
        return prev.Kind == TokenKind.Text &&
               prev.Slice.Length > 0 &&
               char.IsLetterOrDigit(prev.Slice.Span[^1]);
    }

    private bool HasLetterOrDigitAfter(int index)
    {
        if (index + 1 >= _allTokens.Count)
            return false;

        var next = _allTokens[index + 1];
        return next.Kind == TokenKind.Text &&
               next.Slice.Length > 0 &&
               char.IsLetterOrDigit(next.Slice.Span[0]);
    }


    private bool ContainsWhitespaceBetween(int startIndex, int closeIndex)
    {
        for (var i = startIndex + 1; i < closeIndex; i++)
            if (_allTokens[i].Kind == TokenKind.Space)
                return true;
        return false;
    }
    
    private CloseResult TryCloseEmphasis(EmphasisNode emphasis, int
        startIndex)
    {
        if (IsValidEmphasisClose(startIndex))
        {
            MoveOnNextToken();
            return new CloseResult(true, emphasis);
        }

        emphasis.Inlines.Add(CreateTextNode("_"));
        MoveOnNextToken();
        return new CloseResult(false, null);
    }
    
    private bool HasIntersectingDoubleInsideEmphasis(int startIndex, int
        closeIndex)
    {
        var pending = false;

        for (var i = startIndex + 1; i < closeIndex; i++)
        {
            if (_allTokens[i].Kind != TokenKind.DoubleUnderscore)
                continue;

            pending = !pending;
        }

        return pending;
    }

    private bool HasIntersectingSingleInsideStrong(int startIndex, int closeIndex)
    {
        var pending = false;

        for (var i = startIndex + 1; i < closeIndex; i++)
        {
            if (_allTokens[i].Kind != TokenKind.Underscore)
                continue;

            pending = !pending;
        }

        return pending;
    }

}
