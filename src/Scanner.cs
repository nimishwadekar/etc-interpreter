using System;

class Scanner
{
    internal string source;
    internal int start, current, line;

    internal Scanner(in string source)
    {
        this.source = source;
        start = 0;
        current = 0;
        line = 1;
    }

    internal Token ScanToken()
    {
        SkipWhitespaces();
        start = current;
        
        if(IsAtEnd())
        {
            return new Token(TokenType.EOF, this);
        }

        char c = Advance();

        if(char.IsLetter(c) || c == '_')
        {
            return IdentifierToken();
        }

        if(char.IsDigit(c))
        {
            return NumberToken();
        }

        switch(c)
        {
            case '(': return new Token(TokenType.LEFT_PAREN, this);
            case ')': return new Token(TokenType.RIGHT_PAREN, this);
            case '{': return new Token(TokenType.LEFT_BRACE, this);
            case '}': return new Token(TokenType.RIGHT_BRACE, this);
            case '[': return new Token(TokenType.LEFT_SQUARE, this);
            case ']': return new Token(TokenType.RIGHT_SQUARE, this);
            case ':': return new Token(TokenType.COLON, this);
            case ',': return new Token(TokenType.COMMA, this);
            case '.': return new Token(TokenType.DOT, this);
            case '-': return new Token(TokenType.MINUS, this);
            case '%': return new Token(TokenType.MODULUS, this);
            case '+': return new Token(TokenType.PLUS, this);
            case ';': return new Token(TokenType.SEMICOLON, this);
            case '/': return new Token(TokenType.SLASH, this);
            case '*': return new Token(TokenType.STAR, this);

            case '!': return new Token(Matches('=') ? TokenType.NOT_EQUAL : TokenType.NOT, this);
            case '=': return new Token(Matches('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL, this);
            case '<': return new Token(Matches('=') ? TokenType.LESS_EQUAL : TokenType.LESS, this);
            case '>': return new Token(Matches('=') ? TokenType.MORE_EQUAL : TokenType.MORE, this);

            case '"': return StringToken();
        }

        return new ErrorToken("Unexpected character.", this);
    }

    private Token NumberToken()
    {
        while(char.IsDigit(Peek()))
        {
            Advance();
        }

        if(Peek() == '.' && char.IsDigit(PeekPeek()))
        {
            Advance();
            while(char.IsDigit(Peek()))
            {
                Advance();
            }
        }
        return new Token(TokenType.NUMBER, this);
    }

    private Token StringToken()
    {
        while(Peek() != '"' && !IsAtEnd())
        {
            if(Peek() == '\n')
            {
                line++;
            }
            Advance();
        }

        if(IsAtEnd())
        {
            return new ErrorToken("Unterminated string.", this);
        }
        Advance();
        return new Token(TokenType.STRING, this);
    }

    private Token IdentifierToken()
    {
        while(char.IsLetterOrDigit(Peek()) || Peek() == '_')
        {
            Advance();
        }
        return new Token(IdentifierType(), this);
    }

    private TokenType IdentifierType()
    {
        switch(source[start])
        {
            case 'a': return CheckKeyword(1, 2, "nd", TokenType.AND);
            case 'b': return CheckKeyword(1, 3, "ool", TokenType.BOOL);
            case 'c': return CheckKeyword(1, 4, "lass", TokenType.CLASS);
            case 'e': return CheckKeyword(1, 3, "lse", TokenType.ELSE);
            case 'f':
                if(Length() > 1)
                {
                    switch(source[start + 1])
                    {
                        case 'a': return CheckKeyword(2, 3, "lse", TokenType.FALSE);
                        case 'o': return CheckKeyword(2, 1, "r", TokenType.FOR);
                        case 'u': return CheckKeyword(2, 1, "n", TokenType.FUN);
                    }
                }
                break;
            case 'h': return CheckKeyword(1, 5, "elper", TokenType.HELPER);
            case 'i': return CheckKeyword(1, 1, "f", TokenType.IF);
            case 'n':
                if (Length() > 1)
                {
                    switch (source[start + 1])
                    {
                        case 'i': return CheckKeyword(2, 1, "l", TokenType.NIL);
                        case 'u': return CheckKeyword(2, 1, "m", TokenType.NUM);
                    }
                }
                break;
            case 'o': return CheckKeyword(1, 1, "r", TokenType.OR);
            case 'p': return CheckKeyword(1, 4, "rint", TokenType.PRINT);
            case 'r': return CheckKeyword(1, 5, "eturn", TokenType.RETURN);
            case 's':
                if (Length() > 1)
                {
                    switch (source[start + 1])
                    {
                        case 't':
                            if(Length() < 4) return CheckKeyword(2, 1, "r", TokenType.STR);
                            else return CheckKeyword(2, 4, "ruct", TokenType.STRUCT);
                        case 'u': return CheckKeyword(2, 3, "per", TokenType.SUPER);
                    }
                }
                break;
            case 't':
                if (Length() > 1)
                {
                    switch (source[start + 1])
                    {
                        case 'h': return CheckKeyword(2, 2, "is", TokenType.THIS);
                        case 'r': return CheckKeyword(2, 2, "ue", TokenType.TRUE);
                    }
                }
                break;
            case 'u': return CheckKeyword(1, 4, "sing", TokenType.USING);
            case 'v': return CheckKeyword(1, 3, "oid", TokenType.VOID);
            case 'w': return CheckKeyword(1, 4, "hile", TokenType.WHILE);
        }

        return TokenType.IDENTIFIER;
    }

    private TokenType CheckKeyword(int start, int length, in string rest, TokenType type)
    {
        if(Length() == start + length && source.Substring(this.start + start, length) == rest)
        {
            return type;
        }
        return TokenType.IDENTIFIER;
    }

    private void SkipWhitespaces()
    {
        char c;
        for(; ; )
        {
            c = Peek();
            switch(c)
            {
                case ' ':
                case '\r':
                case '\t':
                    Advance();
                    break;

                case '\n':
                    line++;
                    Advance();
                    break;

                case '/':
                    if(PeekPeek() == '/')
                    {
                        while(Peek() != '\n' && !IsAtEnd())
                        {
                            Advance();
                        }
                    }
                    else
                    {
                        return;
                    }
                    break;

                default:
                    return;
            }
        }
    }

    private bool Matches(char expected)
    {
        if(IsAtEnd() || source[current] != expected)
        {
            return false;
        }
        current++;
        return true;
    }

    private char Advance() => source[current++];

    private char Peek() => source[current];

    private char PeekPeek()
    {
        if(IsAtEnd())
        {
            return '\0';
        }
        return source[current + 1];
    }

    private bool IsAtEnd() => source[current] == '\0';

    private int Length() => current - start;
}

class Token
{
    internal TokenType type;
    internal int start, length, line;

    internal Token(TokenType type, in Scanner scanner)
    {
        this.type = type;
        start = scanner.start;
        length = scanner.current - scanner.start;
        line = scanner.line;
    }
}

class ErrorToken : Token
{
    internal string message;

    internal ErrorToken(in string message, in Scanner scanner) : base(TokenType.ERROR, scanner)
    {
        this.message = message;
    }
}

enum TokenType
{
    LEFT_PAREN, RIGHT_PAREN,
    LEFT_BRACE, RIGHT_BRACE,
    LEFT_SQUARE, RIGHT_SQUARE,
    COLON, COMMA, DOT, MINUS, MODULUS, PLUS, SEMICOLON, SLASH, STAR,

    NOT, NOT_EQUAL,
    EQUAL, EQUAL_EQUAL,
    LESS, LESS_EQUAL,
    MORE, MORE_EQUAL,

    IDENTIFIER, STRING, NUMBER,

    AND, BOOL, CLASS, ELSE, FALSE, FOR, FUN, HELPER, IF, NIL, NUM, OR, PRINT, RETURN, STR, STRUCT, SUPER, THIS, TRUE, USING, VOID, WHILE,

    ERROR,
    EOF
}