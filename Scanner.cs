namespace CScript {
    class Scanner {
        public List<Token> Tokens { get; protected set; }

        protected Dictionary<string, TokenType> mReserved;
        protected string mSource;
        protected int mCurrentChar;
        protected int mLexemeStart;
        protected int mLine;
        protected string mFile;
        public Scanner(string fileName, string source) {
            Tokens = new List<Token>();
            mSource = source;
            mCurrentChar = 0;
            mLine = 1;
            mFile = fileName;
            mLexemeStart = 0;

            mReserved = new Dictionary<string, TokenType>() {
                { "int", TokenType.TYPE_INT },
                { "float", TokenType.TYPE_FLOAT },
                { "char", TokenType.TYPE_CHAR },
                { "bool", TokenType.TYPE_BOOL },
                { "print", TokenType.PRINT },
                { "true", TokenType.LIT_TRUE },
                { "false", TokenType.LIT_FALSE },
                { "return", TokenType.RETURN },
                { "void", TokenType.TYPE_VOID },
                { "type", TokenType.TYPE_TYPE },
                { "struct", TokenType.STRUCT },
                { "null", TokenType.LIT_NULL },
                { "object", TokenType.TYPE_OBJECT },
                { "is", TokenType.IS },
                { "as", TokenType.AS }
            };

            Token next = NextToken();
            while (next != null) {
                if (next.Type != TokenType.SLASH_SLASH) {
                    Tokens.Add(next);
                }
                mLexemeStart = mCurrentChar;
                next = NextToken();
            }

            mSource = null;
            mReserved = null;
        }
        protected char Current {
            get {
                if (mCurrentChar >= mSource.Length) {
                    return '\0';
                }
                return mSource[mCurrentChar];
            }
        }

        protected char LookAhead(int howMany) {
            if (mCurrentChar + howMany >= mSource.Length) {
                return '\0';
            }
            return mSource[mCurrentChar + howMany];
        }

        protected string Lexeme {
            get {
                return mSource.Substring(mLexemeStart, mCurrentChar - mLexemeStart);
            }
        }

        protected bool IsNumeric(char c) {
            return c >= '0' && c <= '9';
        }

        protected bool IsAlpha(char c) {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        protected Token NextToken() {
            if (mCurrentChar >= mSource.Length) {
                return null;
            }

            char c = Current;
            mCurrentChar++;
            char n = Current;

            switch (c) {
                case ' ':
                case '\t':
                case '\r': return new Token(TokenType.SLASH_SLASH, mLine, mFile, Lexeme);
                case '\n': mLine++; return new Token(TokenType.SLASH_SLASH, mLine, mFile, Lexeme);
                case '{': return new Token(TokenType.LBRACE, mLine, mFile, Lexeme);
                case '}': return new Token(TokenType.RBRACE, mLine, mFile, Lexeme);
                case '(': return new Token(TokenType.LPAREN, mLine, mFile, Lexeme);
                case ')': return new Token(TokenType.RPAREN, mLine, mFile, Lexeme);
                case '[': return new Token(TokenType.LBRACKET, mLine, mFile, Lexeme);
                case ']': return new Token(TokenType.RBRACKET, mLine, mFile, Lexeme);
                case '+': return new Token(TokenType.PLUS, mLine, mFile, Lexeme);
                case '-': return new Token(TokenType.MINUS, mLine, mFile, Lexeme);
                case '*': return new Token(TokenType.STAR, mLine, mFile, Lexeme);
                case ';': return new Token(TokenType.SEMICOLON, mLine, mFile, Lexeme);
                case '=': return new Token(TokenType.EQUAL, mLine, mFile, Lexeme);
                case '!': return new Token(TokenType.NOT, mLine, mFile, Lexeme);
                case ',': return new Token(TokenType.COMMA, mLine, mFile, Lexeme);
                case '.': return new Token(TokenType.DOT, mLine, mFile, Lexeme);
                case '/':
                    if (n == '/') {
                        while (Current != '\n' && mCurrentChar < mSource.Length) {
                            mCurrentChar++;
                        }
                        return new Token(TokenType.SLASH_SLASH, mLine, mFile, Lexeme);
                    }
                    return new Token(TokenType.SLASH, mLine, mFile, Lexeme);                    
            }

            if (c == '\'') { // Maybe a char literal
                c = Current; // The ' was already eaten. c is the actual char
                n = LookAhead(1); // Next is one ahead so if c == \ then we have an escape sequence.
                                  // If n == ', then we have a normal char. Anything else is an error
                if (n == '\'') { // 'a'
                    c = Current;
                    mCurrentChar++; // Eat char
                    mCurrentChar++; // Eat closing '
                    return new Token(TokenType.LIT_CHAR, mLine, mFile, Lexeme);
                }
                else if (c == '\\') { // '\t'
                    c = n; // c now equals the char, eq t
                    n = LookAhead(2);
                    if (n == '\'') {
                        if (c != 't' && c != 'r' && c != 'n' && c != '0') {
                            throw new CompilerException(ExceptionSource.SCANNER, new Location(mLine, mFile), "Unknown escape character: " + c);
                        }
                        mCurrentChar++; // Eat slash
                        mCurrentChar++; // Eat char
                        mCurrentChar++; // Eat closing '
                        return new Token(TokenType.LIT_CHAR, mLine, mFile, Lexeme);
                    }
                }
                throw new CompilerException(ExceptionSource.SCANNER, new Location(mLine, mFile), "Char symbol (') can only contain one character: " + c + ", Next: " + n);
            }

            // If it's a number
            if (IsNumeric(c)) {
                c = Current;
                while (IsNumeric(c)) {
                    mCurrentChar++;
                    c = Current;
                }
                if (c == '.') {
                    mCurrentChar++;
                    c = Current;

                    while (IsNumeric(c)) {
                        mCurrentChar++;
                        c = Current;
                    }
                }

                return new Token(TokenType.LIT_NUMBER, mLine, mFile, Lexeme);
            }

            // Potentially a valid identifier, or a keyword
            if (IsAlpha(c) || c == '_') {
                c = Current;
                while (IsAlpha(c) || IsNumeric(c) || c == '_') {
                    mCurrentChar++;
                    c = Current;
                }
                string lexeme = Lexeme;
                if (mReserved.ContainsKey(lexeme)) {
                    return new Token(mReserved[lexeme], mLine, mFile, lexeme);
                }
                return new Token(TokenType.IDENTIFIER, mLine, mFile, lexeme);
            }

            Location loc = new Location(mLine, mFile);
            throw new CompilerException(ExceptionSource.SCANNER, loc, "Unrecognized token. Current: " + c + ", Next: " + n);
        }
    }
}
