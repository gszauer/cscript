
using static CScript.ParseTree.Expression;
using static CScript.ParseTree.Statement;

namespace CScript {
    public enum Symbol {
        FILE, IDENTIFIER, COMMENT,

        ENUM, DELEGATE, STRUCT,

        LBRACE, RBRACE, /* { } */
        LBRACKET, RBRACKET, /* [ ] */
        LPAREN, RPAREN, /* ( ) */

        DOT, COMMA, EQUAL, NEW,
        SEMICOLON, COLON,

        PLUS_EQUAL, MINUS_EQUAL, STAR_EQUAL,
        SLASH_EQUAL, POW_EQUAL, MOD_EQUAL,
        NOT_EQUAL, EQUAL_EQUAL, TILDE_EQUAL,
        GREATER_EQUAL, LESS_EQUAL,

        PLUS, MINUS, STAR, SLASH,
        MOD, POW, NOT, TILDE, QUESTION,
        AND, OR, GREATER, LESS, AS,

        PLUS_PLUS, MINUS_MINUS,
        IF, ELSE, FOR, WHILE,
        RETURN, CONTINUE, BREAK,

        TYPE_NUM, TYPE_CHAR, TYPE_BOOL, TYPE_STRING, TYPE_OBJECT, TYPE_VOID,
        LIT_NUM, LIT_CHAR, LIT_BOOL, LIT_STRING, LIT_NULL,

        //TYPE_VEC3,

        RESERVED_THIS, RESERVED_CONSTRUCTOR
    }
    public class Location {
        public string File { get; protected set; }
        public int Line { get; protected set; }
        public int Column { get; protected set; }
        public Location(string file, int line, int column) {
            File = file;
            Line = line;
            Column = column;
        }
    }
    public class Token {
        public Symbol Symbol { get;  set; }
        public Location Location { get; protected set; }
        public string Lexeme { get;  set; }
        public Token(Symbol symbol, Location location, string lexeme) {
            Symbol = symbol;
            Location = location;
            Lexeme = lexeme;
        }
        public void AppendLexeme(string lexeme) {
            Lexeme += lexeme;
        }
        public Token MakeCopy() {
            return new Token(Symbol, Location, Lexeme);
        }
    }
    public class Scanner {
        protected class State {
            public int Current = 0;
            public int Start = 0;
            public string File = null;
            public string Source = null;
            public int Line = 1;
            public int Column = 1;
            public State(string file, string source) {
                File = file;
                Source = source;
            }
        }
        protected static Dictionary<string, Symbol> Keywords = null;
        protected static List<string> Reserved = null;
        protected static void Error(State s, string error) {
            Compiler.Error("Scanner", error, new Location(s.File, s.Line, s.Column));
        }

        protected static void PopulateKeywords() {
            if (Keywords == null) {
                Keywords = new Dictionary<string, Symbol>();
                Keywords.Add("enum", Symbol.ENUM);
                Keywords.Add("delegate", Symbol.DELEGATE);
                Keywords.Add("struct", Symbol.STRUCT);
                Keywords.Add("new", Symbol.NEW);
                //Keywords.Add("operator", Symbol.OPERATOR);
                Keywords.Add("and", Symbol.AND);
                Keywords.Add("or", Symbol.OR);
                Keywords.Add("as", Symbol.AS);
                Keywords.Add("if", Symbol.IF);
                Keywords.Add("else", Symbol.ELSE);
                Keywords.Add("for", Symbol.FOR);
                Keywords.Add("while", Symbol.WHILE);
                Keywords.Add("return", Symbol.RETURN);
                Keywords.Add("continue", Symbol.CONTINUE);
                Keywords.Add("break", Symbol.BREAK);
                Keywords.Add("true", Symbol.LIT_BOOL);
                Keywords.Add("false", Symbol.LIT_BOOL);
                Keywords.Add("null", Symbol.LIT_NULL);
                Keywords.Add("num", Symbol.TYPE_NUM);
                Keywords.Add("char", Symbol.TYPE_CHAR);
                Keywords.Add("bool", Symbol.TYPE_BOOL);
                Keywords.Add("string", Symbol.TYPE_STRING);
                //Keywords.Add("vec3", Symbol.TYPE_VEC3);
                Keywords.Add("object", Symbol.TYPE_OBJECT);
                Keywords.Add("void", Symbol.TYPE_VOID);
                Keywords.Add("this", Symbol.RESERVED_THIS);
                Keywords.Add("constructor", Symbol.RESERVED_CONSTRUCTOR);

                Reserved = new List<string>();
                Reserved.Add("abstract");
                Reserved.Add("arguments");
                Reserved.Add("await");
                Reserved.Add("boolean");
                Reserved.Add("byte");
                Reserved.Add("case");
                Reserved.Add("catch");
                Reserved.Add("class");
                Reserved.Add("const");
                Reserved.Add("debugger");
                Reserved.Add("default");
                Reserved.Add("delete");
                Reserved.Add("do");
                Reserved.Add("double");
                Reserved.Add("eval");
                Reserved.Add("export");
                Reserved.Add("extends");
                Reserved.Add("final");
                Reserved.Add("finally");
                Reserved.Add("float");
                Reserved.Add("function");
                Reserved.Add("goto");
                Reserved.Add("implements");
                Reserved.Add("import");
                Reserved.Add("in");
                Reserved.Add("instanceof");
                Reserved.Add("int");
                Reserved.Add("interface");
                Reserved.Add("let");
                Reserved.Add("long");
                Reserved.Add("native");
                Reserved.Add("package");
                Reserved.Add("private");
                Reserved.Add("protected");
                Reserved.Add("public");
                Reserved.Add("short");
                Reserved.Add("static");
                Reserved.Add("super");
                Reserved.Add("switch");
                Reserved.Add("synchronized");
                Reserved.Add("throw");
                Reserved.Add("throws");
                Reserved.Add("transient");
                Reserved.Add("try");
                Reserved.Add("typeof");
                Reserved.Add("var");
                Reserved.Add("volatile");
                Reserved.Add("with");
                Reserved.Add("yield");
                Reserved.Add("protected");
                Reserved.Add("protected");
            }
        }
        public static List<Token> Scan(string fileName, string sourceCode) {
            PopulateKeywords();

            List<Token> result = new List<Token>();
            Token StartOfFile = new Token(Symbol.FILE, new Location(fileName, 0, 0), fileName);
            result.Add(StartOfFile);


            State scannerState = new State(fileName, sourceCode);


            Token lastToken = null;
            while (!IsAtEnd(scannerState)) {
                scannerState.Start = scannerState.Current;

                Token token = ScanToken(scannerState);
                if (token == null) {
                    Compiler.Error("Scanner", "Could not scan next token.", new Location(fileName, 0, 0));
                    return null;
                }

                if (token.Symbol == Symbol.COMMENT) {
                    continue;
                }

                if (token.Symbol == Symbol.LIT_STRING) {
                    if (lastToken != null && lastToken.Symbol == Symbol.LIT_STRING) {
                        lastToken.AppendLexeme(token.Lexeme);
                        continue;
                    }
                }

                result.Add(token);
                lastToken = token;
            }

            foreach(Token t in result) {
                if (t.Symbol == Symbol.LIT_STRING) {
                    t.Lexeme = "\"" + t.Lexeme + "\"";
                }
                else if (t.Symbol == Symbol.LIT_CHAR) {
                    t.Lexeme = "'" + t.Lexeme + "'";
                }
            }

            return result;
        }
        protected static bool IsAtEnd(State s) {
            return s.Current >= s.Source.Length;
        }
        protected static char Advance(State s) {
            if (IsAtEnd(s)) {
                Error(s, "Can't advance past end of token stream");
            }

            s.Column += 1;
            if (s.Source[s.Current] == '\n') {
                s.Line += 1;
                s.Column = 1;
            }

            return s.Source[s.Current++];
        }
        protected static char Peek(State s, int offset) {
            int location = s.Current + offset;

            if (location < 0) {
                Error(s, "Can't peek below zero");
            }
            else if (location >= s.Source.Length) {
                Error(s, "Can't peek past end");
            }

            char result = s.Source[location];
            return result;
        }
        protected static bool Match(State s, char c) {
            if (IsAtEnd(s)) {
                return false;
            }

            if (Peek(s, 0) != c) {
                return false;
            }

            Advance(s);
            return true;
        }
        protected static Token MakeToken(State s, Symbol symbol) {
            string lexeme = s.Source.Substring(s.Start, s.Current - s.Start);
            Location location = new Location(s.File, s.Line, s.Column);
            return new Token(symbol, location, lexeme);
        }
        protected static Token MakeToken(State s, Symbol symbol, string lexeme) {
            Location location = new Location(s.File, s.Line, s.Column);
            return new Token(symbol, location, lexeme);
        }
        protected static bool IsNumber(char c) {
            return c >= '0' && c <= '9';
        }
        protected static bool IsAlpha(char c) {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
        protected static bool IsAlphaNumericWithUnderscore(char c) {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c == '_');
        }
        protected static bool MatchNumber(State s) {
            if (IsAtEnd(s)) {
                return false;
            }

            char peek = Peek(s, 0);
            if (peek < '0' || peek > '9') {
                return false;
            }

            Advance(s);
            return true;
        }
        protected static string GetLexeme(State s) {
            return s.Source.Substring(s.Start, s.Current - s.Start);
        }

        protected static Token ScanToken(State s) {
            char c = Advance(s);

            PopulateKeywords();
            

            if (c == '(') { return MakeToken(s, Symbol.LPAREN); }
            else if (c == ')') { return MakeToken(s, Symbol.RPAREN); }
            else if (c == '{') { return MakeToken(s, Symbol.LBRACE); }
            else if (c == '}') { return MakeToken(s, Symbol.RBRACE); }
            else if (c == '[') { return MakeToken(s, Symbol.LBRACKET); }
            else if (c == ']') { return MakeToken(s, Symbol.RBRACKET); }
            else if (c == ',') { return MakeToken(s, Symbol.COMMA); }
            else if (c == '.') { return MakeToken(s, Symbol.DOT); }
            else if (c == ':') { return MakeToken(s, Symbol.COLON); }
            else if (c == ';') { return MakeToken(s, Symbol.SEMICOLON); }
            else if (c == '?') { return MakeToken(s, Symbol.QUESTION); }
            else if (c == '=') { return MakeToken(s, Match(s, '=') ? Symbol.EQUAL_EQUAL : Symbol.EQUAL); }
            else if (c == '!') { return MakeToken(s, Match(s, '=') ? Symbol.NOT_EQUAL : Symbol.NOT); }
            else if (c == '~') { return MakeToken(s, Match(s, '=') ? Symbol.TILDE_EQUAL : Symbol.TILDE); }
            else if (c == '*') { return MakeToken(s, Match(s, '=') ? Symbol.STAR_EQUAL : Symbol.STAR); }
            else if (c == '%') { return MakeToken(s, Match(s, '=') ? Symbol.MOD_EQUAL : Symbol.MOD); }
            else if (c == '^') { return MakeToken(s, Match(s, '=') ? Symbol.POW_EQUAL : Symbol.POW); }
            else if (c == '>') { return MakeToken(s, Match(s, '=') ? Symbol.GREATER_EQUAL : Symbol.GREATER); }
            else if (c == '<') { return MakeToken(s, Match(s, '=') ? Symbol.LESS_EQUAL : Symbol.LESS); }
            else if (c == '+') {
                if (Match(s, '=')) { return MakeToken(s, Symbol.PLUS_EQUAL); }
                else if (Match(s, '+')) { return MakeToken(s, Symbol.PLUS_PLUS); }
                else { return MakeToken(s, Symbol.PLUS); }
            }
            else if (c == '-') {
                if (Match(s, '=')) { return MakeToken(s, Symbol.MINUS_EQUAL); }
                else if (Match(s, '-')) { return MakeToken(s, Symbol.MINUS_MINUS); }
                else { return MakeToken(s, Symbol.MINUS); }
            }
            else if (c == '/') {
                if (Match(s, '=')) { return MakeToken(s, Symbol.SLASH_EQUAL); }
                else if (Match(s, '/')) {
                    while (!IsAtEnd(s) && Peek(s, 0) != '\n') {
                        Advance(s);
                    }
                    return MakeToken(s, Symbol.COMMENT);
                }
                else if (Match(s, '*')) {
                    while (true) {
                        if (Peek(s, 0) == '/') {
                            if (Peek(s, -1) == '*') {
                                Advance(s); // Eat the slash
                                return MakeToken(s, Symbol.COMMENT);
                            }
                        }

                        Advance(s);
                        if (IsAtEnd(s)) {
                            Error(s, "Unterminated comment");
                        }
                    }
                }
                else {
                    return MakeToken(s, Symbol.SLASH);
                }
            }
            else if (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f') {
                c = Peek(s, 0);
                while (c == ' ' || c == '\t' || c == '\r' || c == '\n' || c == '\f') {
                    Advance(s);
                    if (IsAtEnd(s)) {
                        break;
                    }
                    c = Peek(s, 0);
                }
                return MakeToken(s, Symbol.COMMENT); // Skip white space
            }
            else if (c == '\'') {
                char literal = Advance(s);

                if (literal == '\\') {
                    literal = Advance(s);
                    if (literal != '0' && literal != 't' && literal != 'n' && literal != 'r' && literal != 'f' && literal != '\\' && literal != '\'') {
                        Error(s, "Unexpected char escape sequence: '" + literal + "'");
                    }
                }

                if (Peek(s, 0) != '\'') {
                    Error(s, "Unterminated character literal");
                }
                Advance(s); // Eat '
                return MakeToken(s, Symbol.LIT_CHAR, literal.ToString());
            }
            else if (c == '"') {
                string literal = "";

                while (!IsAtEnd(s)) {
                    if (Peek(s, 0) == '"') {
                        if (Peek(s, -1) == '\\') {
                            // Just let it go
                        }
                        else {
                            break;
                        }
                    }

                    if (Peek(s, 0) == '\n') {
                        Error(s, "Newline is not supported inside string");
                    }
                    literal += Advance(s);
                }

                if (Peek(s, 0) != '"') {
                    Error(s, "Unterminated string");
                }
                Advance(s); // Eat "

                return MakeToken(s, Symbol.LIT_STRING, literal.ToString());
            }
            else {
                if (IsNumber(c)) {
                    while (MatchNumber(s)) ;
                    if (Match(s, '.')) {
                        while (MatchNumber(s)) ;
                    }
                    return MakeToken(s, Symbol.LIT_NUM);
                }
                else if (c == '_' || IsAlpha(c)) {
                    while (IsAlphaNumericWithUnderscore(Peek(s, 0))) {
                        Advance(s);
                    }

                    string lexeme = GetLexeme(s);
                    if (Keywords.ContainsKey(lexeme)) {
                        return MakeToken(s, Keywords[lexeme]);
                    }
                    else if (Reserved.Contains(lexeme)) {
                        Compiler.Error("Scanner", "Using javascript reserved keyword: " + lexeme, new Location(s.File, s.Line, s.Column));
                    }
                    return MakeToken(s, Symbol.IDENTIFIER);
                }
            }

            Error(s, "Encountered unexpected character: '" + c + "'");
            return null;
        }
    }
}
