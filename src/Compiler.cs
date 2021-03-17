using System;
using System.Collections.Generic;

class Compiler
{
    readonly Scanner scanner;
    readonly string source;
    Chunk compilingChunk;
    Token previous, current;
    bool hadError, isPanicking, canAssign, hasReturned, isInsideConditional, canUse;

    internal List<Local> locals;
    internal int scopeDepth, localCount;

    internal Compiler(in string source)
    {
        scanner = new Scanner(source);
        this.source = source;
        locals = new List<Local>();
        rules = new Dictionary<TokenType, ParseRule>
        {
            { TokenType.LEFT_PAREN,     new ParseRule(Grouping,     Call,       Precedence.CALL) },
            { TokenType.RIGHT_PAREN,    new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.LEFT_BRACE,     new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.RIGHT_BRACE,    new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.LEFT_SQUARE,    new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.RIGHT_SQUARE,   new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.COLON,          new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.COMMA,          new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.DOT,            new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.MINUS,          new ParseRule(Unary,        Binary,     Precedence.TERM) },
            { TokenType.MODULUS,        new ParseRule(null,         Binary,     Precedence.FACTOR) },
            { TokenType.PLUS,           new ParseRule(null,         Binary,     Precedence.TERM) },
            { TokenType.SEMICOLON,      new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.SLASH,          new ParseRule(null,         Binary,     Precedence.FACTOR) },
            { TokenType.STAR,           new ParseRule(null,         Binary,     Precedence.FACTOR) },

            { TokenType.NOT,            new ParseRule(Unary,        null,       Precedence.NONE) },
            { TokenType.NOT_EQUAL,      new ParseRule(null,         Binary,     Precedence.EQUALITY) },
            { TokenType.EQUAL,          new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.EQUAL_EQUAL,    new ParseRule(null,         Binary,     Precedence.EQUALITY) },
            { TokenType.LESS,           new ParseRule(null,         Binary,     Precedence.COMPARISON) },
            { TokenType.LESS_EQUAL,     new ParseRule(null,         Binary,     Precedence.COMPARISON) },
            { TokenType.MORE,           new ParseRule(null,         Binary,     Precedence.COMPARISON) },
            { TokenType.MORE_EQUAL,     new ParseRule(null,         Binary,     Precedence.COMPARISON) },

            { TokenType.IDENTIFIER,     new ParseRule(Variable,     null,       Precedence.NONE) },
            { TokenType.STRING,         new ParseRule(String,       null,       Precedence.NONE) },
            { TokenType.NUMBER,         new ParseRule(Number,       null,       Precedence.NONE) },

            { TokenType.AND,            new ParseRule(null,         And,        Precedence.AND) },
            { TokenType.BOOL,           new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.CLASS,          new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.ELSE,           new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.FALSE,          new ParseRule(Literal,      null,       Precedence.NONE) },
            { TokenType.FOR,            new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.HELPER,         new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.IF,             new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.NIL,            new ParseRule(Literal,      null,       Precedence.NONE) },
            { TokenType.NUM,            new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.OR,             new ParseRule(null,         Or,         Precedence.OR) },
            { TokenType.PRINT,          new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.RETURN,         new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.STR,            new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.SUPER,          new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.THIS,           new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.TRUE,           new ParseRule(Literal,      null,       Precedence.NONE) },
            { TokenType.WHILE,          new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.ERROR,          new ParseRule(null,         null,       Precedence.NONE) },
            { TokenType.EOF,            new ParseRule(null,         null,       Precedence.NONE) },
        };
        canUse = true;
    }

    internal bool Compile(Chunk chunk)
    {
        compilingChunk = chunk;
        Advance();
        while(!Match(TokenType.EOF))
        {
            Declaration();
        }
        EndCompiler();
        return !hadError;
    }

    private enum Precedence
    {
        NONE, ASSIGNMENT, OR, AND, EQUALITY, COMPARISON, TERM, FACTOR, UNARY, CALL, PRIMARY
    }

    private class ParseRule
    {
        internal Action prefix, infix;
        internal Precedence precedence;

        internal ParseRule(Action pref, Action inf, Precedence prec)
        {
            prefix = pref;
            infix = inf;
            precedence = prec;
        }
    }

    private static Dictionary<TokenType, ParseRule> rules;

    private void ParsePrecedence(Precedence precedence)
    {
        Advance();
        Action prefixRule = rules[previous.type].prefix;
        if(prefixRule == null)
        {
            ErrorCurrent("Expecting expression.");
            return;
        }
        canAssign = precedence <= Precedence.ASSIGNMENT;
        prefixRule();

        while(precedence <= rules[current.type].precedence)
        {
            Advance();
            Action infixRule = rules[previous.type].infix;
            infixRule();
        }

        if(canAssign && Match(TokenType.EQUAL))
        {
            ErrorPrevious("Invalid assignment target.");
        }
        canAssign = false;
    }

    private void Declaration()
    {
        if(Match(TokenType.USING))
        {
            UsingDeclaration();
        }
        else if(!(canUse = false) && Match(TokenType.FUN))
        {
            FunctionDeclaration();
        }
        else if(Match(TokenType.CLASS))
        {
            ClassDeclaration();
        }
        else if(Match(TokenType.NUM) || Match(TokenType.BOOL) || Match(TokenType.STR))
        {
            VariableDeclaration();
        }
        else if(Match(TokenType.VOID))
        {
            ErrorPrevious("'void' is not a valid type in this context.");
        }
        else
        {
            Statement();
        }

        if(isPanicking)
        {
            Synchronize();
        }
    }

    private void UsingDeclaration()
    {
        if(!canUse)
        {
            ErrorPrevious("'using' statements can only be placed at the top of the script.");
            return;
        }

        Consume(TokenType.IDENTIFIER, "Expecting name of library to import.");
        string name = source.Lexeme(previous);
        Consume(TokenType.SEMICOLON, "Expecting ';' after 'using' statement.");
        EmitWords(OpCode.LIB, MakeConstant(StrValue.NewString(name)));
    }

    private void FunctionDeclaration()
    {
        if(scopeDepth > 0)
        {
            ErrorPrevious("Cannot declare functions in a local scope.");
            return;
        }

        ParseType("Expecting return type of function.", true);
        ValueType returnType = GetVariableType(previous.type);
        ushort constantIndex = ParseVariable("Expecting function name.");
        int jumpIndex = EmitJump(OpCode.JUMP);
        FunctionDefinition(Function.Type.FUNCTION, constantIndex, returnType, jumpIndex, previous);
        DefineFunction(constantIndex, jumpIndex + 1);
    }

    private void FunctionDefinition(Function.Type type, ushort nameIndex, ValueType returnType, int jumpIndex, Token declaration)
    {
        hasReturned = false;
        BeginScope();

        Consume(TokenType.LEFT_PAREN, "Expecting '(' after function name.");
        List<ValueType> paramTypes = new List<ValueType>();
        if (!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                ParseType("Expecting type of parameter.");
                ValueType varType = GetVariableType(previous.type);
                ushort param = ParseVariable("Expecting name of parameter.");
                DefineVariable(param);
                paramTypes.Add(varType);
            } while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expecting ')' after parameter list.");

        Consume(TokenType.LEFT_BRACE, "Expecting '{' before function body.");
        BlockStatement();

        if (!hasReturned)
        {
            EmitWord(OpCode.DEFAULT);
            EmitWord(OpCode.STORE);
        }
        EndScope();
        if (!hasReturned)
        { 
            EmitWord(OpCode.PUSH);
            EmitReturn();
        }
        PatchJump(jumpIndex);
        Function function = Function.newFunction(paramTypes, returnType, CurrentChunk().values[nameIndex] as StrValue, type);
        if(function == null)
        {
            Error(declaration, "Overloaded functions must have differing parameter lists.");
            return;
        }
        EmitConstant(function);
    }

    private void DefineFunction(ushort index, int startIndex)
    {
        EmitWords(OpCode.DEF_FUN, MakeConstant(NumValue.NewNum(startIndex)));
        DefineVariable(index);
    }

    private void ClassDeclaration()
    {
        if (scopeDepth > 0)
        {
            ErrorPrevious("Cannot declare classes in a local scope.");
            return;
        }
    }

    private void VariableDeclaration()
    {
        ValueType varType = GetVariableType(previous.type);
        ushort globalIndex = ParseVariable("Expecting variable name.");

        if(Match(TokenType.EQUAL))
        {
            Expression();
            EmitCheck(varType);
        }
        else
        {
            switch(varType)
            {
                case ValueType.NUM: EmitConstant(NumValue.ZERO); break;
                case ValueType.BOOL: EmitWord(OpCode.FALSE); break;
                case ValueType.STR: EmitConstant(StrValue.EMPTY); break;
            }
        }
        Consume(TokenType.SEMICOLON, "Expecting ';' after variable declaration.");
        DefineVariable(globalIndex);
    }

    private ValueType GetVariableType(TokenType type)
    {
        switch (type)
        {
            case TokenType.NUM: return ValueType.NUM;
            case TokenType.BOOL: return ValueType.BOOL;
            case TokenType.STR: return ValueType.STR;
            case TokenType.VOID: return ValueType.NIL;
        }
        ErrorPrevious("Fatal error: Erroneous type.");
        return ValueType.NIL;
    }

    private void ParseType(in string errorMessage, bool canReturn = false)
    {
        if (!Match(TokenType.NUM) && !Match(TokenType.BOOL) && !Match(TokenType.STR) && !Match(TokenType.VOID))
        {
            ErrorCurrent(errorMessage);
        }
        else if(!canReturn && previous.type == TokenType.VOID)
        {
            ErrorCurrent("'void' is not a valid type in this context.");
        }
    }

    private ushort ParseVariable(in string errorMessage)
    {
        Consume(TokenType.IDENTIFIER, errorMessage);

        //DeclareLocalVariable();
        //if(scopeDepth > 0)
        {
            //return 0;
        }

        return IdentifierConstant(previous);
    }

    private ushort IdentifierConstant(Token name)
    {
        string nameStr = source.Lexeme(name);
        return MakeConstant(StrValue.NewString(nameStr));
    }

    private void DefineVariable(ushort index)
    {
        //if(scopeDepth > 0)
        {
            //MarkInitialised();
            //return;
        }
        EmitWords(OpCode.DEF_GLOB, index);
    }

    private void MarkInitialised()
    {
        locals[localCount - 1].depth = scopeDepth;
    }

    private void Statement()
    {
        if(Match(TokenType.PRINT))
        {
            PrintStatement();
        }
        else if(Match(TokenType.IF))
        {
            IfStatement();
        }
        else if(Match(TokenType.ELSE))
        {
            ErrorPrevious("'else' without 'if'.");
        }
        else if(Match(TokenType.RETURN))
        {
            ReturnStatement();
        }
        else if(Match(TokenType.WHILE))
        {
            WhileStatement();
        }
        else if(Match(TokenType.FOR))
        {
            ForStatement();
        }
        else if(Match(TokenType.LEFT_BRACE))
        {
            BeginScope();
            BlockStatement();
            EndScope();
        }
        else
        {
            ExpressionStatement();
        }
    }

    private void PrintStatement()
    {
        Expression();
        Consume(TokenType.SEMICOLON, "Expecting ';' after statement.");
        EmitWord(OpCode.PRINT);
    }

    private void IfStatement()
    {
        isInsideConditional = true;
        Consume(TokenType.LEFT_PAREN, "Expecting '(' after 'if'.");
        Expression();
        EmitCheck(ValueType.BOOL);
        Consume(TokenType.RIGHT_PAREN, "Expecting ')' after condition.");

        int thenJump = EmitJump(OpCode.JUMP_IF_FALSE);
        EmitWord(OpCode.POP);
        Statement();
        int elseJump = EmitJump(OpCode.JUMP);

        PatchJump(thenJump);
        EmitWord(OpCode.POP);

        if(Match(TokenType.ELSE))
        {
            if(Match(TokenType.IF))
            {
                IfStatement();
            }
            else
            {
                Statement();
            }
        }
        PatchJump(elseJump);
        isInsideConditional = false;
    }

    private void ReturnStatement()
    {
        if(Check(TokenType.SEMICOLON))
        {
            EmitWord(OpCode.DEFAULT);
        }
        else
        {
            Expression();
        }
        EmitWord(OpCode.STORE);
        Consume(TokenType.SEMICOLON, "Expecting ';' after return statement.");
        //EndScope();
        EmitWord(OpCode.PUSH);
        EmitReturn();
        if(!isInsideConditional)
        {
            hasReturned = true;
        }
    }

    private void WhileStatement()
    {
        int loopStart = CurrentChunk().Count;

        Consume(TokenType.LEFT_PAREN, "Expecting '(' after 'while'.");
        Expression();
        EmitCheck(ValueType.BOOL);
        Consume(TokenType.RIGHT_PAREN, "Expecting ')' after condition.");

        int exitJump = EmitJump(OpCode.JUMP_IF_FALSE);

        EmitWord(OpCode.POP);
        Statement();

        EmitLoop(loopStart);

        PatchJump(exitJump);
        EmitWord(OpCode.POP);
    }

    private void ForStatement()
    {
        BeginScope();
        Consume(TokenType.LEFT_PAREN, "Expecting '(' after 'for'.");
        if(Match(TokenType.SEMICOLON)) { }
        else if(Match(TokenType.NUM) || Match(TokenType.BOOL) || Match(TokenType.STR))
        {
            VariableDeclaration();
        }
        else
        {
            ExpressionStatement();
        }

        int loopStart = CurrentChunk().Count;

        int exitJump = -1;
        if(!Match(TokenType.SEMICOLON))
        {
            Expression();
            Consume(TokenType.SEMICOLON, "Expecting ';' after loop condition.");
            EmitCheck(ValueType.BOOL);
            exitJump = EmitJump(OpCode.JUMP_IF_FALSE);
            EmitWord(OpCode.POP);
        }

        if(!Match(TokenType.RIGHT_PAREN))
        {
            int bodyJump = EmitJump(OpCode.JUMP);
            int incrementStart = CurrentChunk().Count;
            Expression();
            EmitWord(OpCode.POP);
            Consume(TokenType.RIGHT_PAREN, "Expecting ')' after for loop clauses.");

            EmitLoop(loopStart);
            loopStart = incrementStart;
            PatchJump(bodyJump);
        }

        Statement();
        EmitLoop(loopStart);

        if(exitJump != -1)
        {
            PatchJump(exitJump);
            EmitWord(OpCode.POP);
        }

        EndScope();
    }

    private void BlockStatement()
    {
        while(!Check(TokenType.RIGHT_BRACE) && !Check(TokenType.EOF))
        {
            Declaration();
        }
        Consume(TokenType.RIGHT_BRACE, "Expecting '}' to end the block.");
    }

    private void DeclareLocalVariable()
    {
        if(scopeDepth == 0)
        {
            return;
        }

        for(int i = localCount - 1; i >= 0; i--)
        {
            if(locals[i].depth != -1 && locals[i].depth < scopeDepth)
            {
                break;
            }
            if(source.Lexeme(previous) == source.Lexeme(locals[i].name))
            {
                ErrorPrevious("Cannot declare two variables with the same name in the same scope.");
            }
        }
        AddLocal(previous);
    }

    private void AddLocal(Token name)
    {
        if(locals.Count > ushort.MaxValue)
        {
            ErrorPrevious("Too many local variables in script.");
            return;
        }
        localCount++;
        locals.Add(new Local(name, -1));
    }

    private void BeginScope()
    {
        scopeDepth++;
    }

    private void EndScope()
    {
        scopeDepth--;
        while(localCount > 0 && locals[localCount - 1].depth > scopeDepth)
        {
            EmitWord(OpCode.POP);
            localCount--;
        }
    }

    private void ExpressionStatement()
    {
        Expression();
        Consume(TokenType.SEMICOLON, "Expecting ';' after statement.");
        EmitWord(OpCode.POP);
    }

    private void Expression()
    {
        ParsePrecedence(Precedence.ASSIGNMENT);
    }

    private void Or()
    {
        int endJump = EmitJump(OpCode.JUMP_IF_TRUE);
        EmitWord(OpCode.POP);
        ParsePrecedence(Precedence.OR);
        PatchJump(endJump);
    }

    private void And()
    {
        int endJump = EmitJump(OpCode.JUMP_IF_FALSE);
        EmitWord(OpCode.POP);
        ParsePrecedence(Precedence.AND);
        PatchJump(endJump);
    }

    private void Binary()
    {
        TokenType opType = previous.type;

        ParseRule rule = rules[opType];
        ParsePrecedence(rule.precedence + 1);

        switch(opType)
        {
            case TokenType.PLUS:    EmitWord(OpCode.ADD); break;
            case TokenType.MINUS:   EmitWord(OpCode.SUBTRACT); break;
            case TokenType.STAR:    EmitWord(OpCode.MULTIPLY); break;
            case TokenType.SLASH:   EmitWord(OpCode.DIVIDE); break;
            case TokenType.MODULUS: EmitWord(OpCode.MODULO); break;

            case TokenType.NOT_EQUAL:   EmitWords(OpCode.EQUAL, OpCode.NOT); break;
            case TokenType.EQUAL_EQUAL: EmitWord(OpCode.EQUAL); break;
            case TokenType.LESS:        EmitWord(OpCode.LESS); break;
            case TokenType.LESS_EQUAL:  EmitWords(OpCode.MORE, OpCode.NOT); break;
            case TokenType.MORE:        EmitWord(OpCode.MORE); break;
            case TokenType.MORE_EQUAL:  EmitWords(OpCode.LESS, OpCode.NOT); break;
        }
    }

    private void Call()
    {
        ushort args = ArgumentList();
        EmitWords(OpCode.CALL, args);
    }

    private ushort ArgumentList()
    {
        ushort args = 0;
        if(!Check(TokenType.RIGHT_PAREN))
        {
            do
            {
                if(args == ushort.MaxValue)
                {
                    ErrorPrevious("Can't have more than 65535 arguments.");
                    return 0;
                }
                Expression();
                args++;
            } while (Match(TokenType.COMMA));
        }
        Consume(TokenType.RIGHT_PAREN, "Expecting ')' after arguments.");
        
        return args;
    }

    private void Unary()
    {
        TokenType opType = previous.type;
        ParsePrecedence(Precedence.UNARY);

        switch(opType)
        {
            case TokenType.MINUS: EmitWord(OpCode.NEGATE); break;
            case TokenType.NOT: EmitWord(OpCode.NOT); break;
        }
    }

    private void Literal()
    {
        switch(previous.type)
        {
            case TokenType.FALSE: EmitWord(OpCode.FALSE); break;
            case TokenType.NIL: EmitWord(OpCode.NIL); break;
            case TokenType.TRUE: EmitWord(OpCode.TRUE); break;
        }
    }
    private void Grouping()
    {
        Expression();
        Consume(TokenType.RIGHT_PAREN, "Expecting ')' after expression.");
    }

    private void Number()
    {
        double value = double.Parse(source.Lexeme(previous));
        EmitConstant(NumValue.NewNum(value));
    }

    private void String()
    {
        string value = source.Lexeme(previous).Substring(1, previous.length - 2);
        EmitConstant(StrValue.NewString(value));
    }

    private void Variable()
    {
        NamedVariable(previous);
    }

    private void NamedVariable(Token name)
    {
        //OpCode getOp, setOp;
        //int index = ResolveLocal(name);
        //if(index == -1)
        //{
        //    index = IdentifierConstant(name);
        //    getOp = OpCode.GET_GLOB;
        //    setOp = OpCode.SET_GLOB;
        //}
        //else
        //{
        //    getOp = OpCode.GET_GLOB;
        //    setOp = OpCode.SET_GLOB;
        //}
        int index = IdentifierConstant(name);
        if (canAssign && Match(TokenType.EQUAL))
        {
            Expression();
            EmitWords(OpCode.SET_GLOB, (ushort)index);
        }
        else
        {
            EmitWords(OpCode.GET_GLOB, (ushort)index);
        }
    }

    /*private int ResolveLocal(Token name)
    {
        for(int i = localCount - 1; i >= 0; i--)
        {
            if(i < localStart)
            {
                return -1;
            }
            if(source.Lexeme(name) == source.Lexeme(locals[i].name))
            {
                if(locals[i].depth == -1)
                {
                    ErrorPrevious("Can't read a local variable in its own initializer.");
                }
                return i;
            }
        }
        return -1;
    }*/

    private void Advance()
    {
        previous = current;
        for (; ; )
        {
            current = scanner.ScanToken();
            if (current.type != TokenType.ERROR)
            {
                break;
            }
            ErrorCurrent(((ErrorToken)current).message);
        }
    }

    private void Consume(TokenType type, in string message)
    {
        if (current.type == type)
        {
            Advance();
            return;
        }
        ErrorCurrent(message);
    }

    private bool Match(TokenType type)
    {
        if(!Check(type))
        {
            return false;
        }
        Advance();
        return true;
    }

    private bool Check(TokenType type) => current.type == type;

    private void ErrorCurrent(in string message)
    {
        Error(current, message);
    }

    private void ErrorPrevious(in string message)
    {
        Error(previous, message);
    }

    private void Error(Token token, in string message)
    {
        isPanicking = true;
        Console.Error.Write("[line {0}] Error", token.line);
        if (token.type == TokenType.EOF)
        {
            Console.Error.Write(" at the end. ");
        }
        else if (token.type != TokenType.ERROR)
        {
            Console.Error.Write(" at {0}. ", source.Lexeme(token));
        }
        Console.Error.WriteLine(message);
        hadError = true;
    }

    private void Synchronize()
    {
        isPanicking = false;
        while(current.type != TokenType.EOF)
        {
            if(previous.type == TokenType.SEMICOLON)
            {
                return;
            }

            switch(current.type)
            {
                case TokenType.BOOL:
                case TokenType.CLASS:
                case TokenType.FOR:
                case TokenType.HELPER:
                case TokenType.IF:
                case TokenType.NUM:
                case TokenType.PRINT:
                case TokenType.RETURN:
                case TokenType.STR:
                case TokenType.WHILE:
                    return;
            }
            Advance();
        }
    }

    private void EmitWord(ushort word) => CurrentChunk().Write(word, previous.line);

    private void EmitWord(OpCode op) => EmitWord((ushort)op);

    private void EmitWords(ushort word1, ushort word2)
    {
        EmitWord(word1);
        EmitWord(word2);
    }

    private void EmitWords(OpCode op, ushort word) => EmitWords((ushort)op, word);

    private void EmitWords(OpCode op1, OpCode op2) => EmitWords((ushort)op1, (ushort)op2);

    private void EmitConstant(Value val)
    {
        EmitWords(OpCode.CONSTANT, MakeConstant(val));
    }

    private ushort MakeConstant(Value val)
    {
        int constantIndex = CurrentChunk().AddConstant(val);
        if(constantIndex > ushort.MaxValue)
        {
            ErrorPrevious("Too many constants in one chunk.");
            return 0;
        }
        return (ushort)constantIndex;
    }

    private void EmitCheck(ValueType type) => EmitWords(OpCode.CHK_TYPE, (ushort)type);

    private int EmitJump(OpCode op)
    {
        EmitWords(op, (ushort)0);
        return CurrentChunk().Count - 1;
    }

    private void PatchJump(int offset)
    {
        int jump = CurrentChunk().Count - offset - 1;
        if(jump > ushort.MaxValue)
        {
            ErrorPrevious("Too much code to jump over.");
        }
        CurrentChunk()[offset] = (ushort)jump;
    }

    private void EmitLoop(int loopStart)
    {
        EmitWord(OpCode.LOOP);
        int offset = CurrentChunk().Count - loopStart + 1;
        if(offset > ushort.MaxValue)
        {
            ErrorPrevious("Loop body too large.");
        }
        EmitWord((ushort)offset);
    }

    private void EmitReturn() => EmitWord(OpCode.RETURN);

    private void EndCompiler()
    {
        EmitReturn();

        //DEBUG_PRINT_CODE();
    }

    private Chunk CurrentChunk() => compilingChunk;

    private void DEBUG_PRINT_CODE()
    {
        if(!hadError)
        {
            ETC.debugger.DisassembleChunk(CurrentChunk(), "Code");
        }
    }
}

class Local
{
    internal readonly Token name;
    //internal readonly ValueType type;
    internal int depth;

    internal Local(Token name, int depth)
    {
        this.name = name;
        //this.type = type;
        this.depth = depth;
    }
}