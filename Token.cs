namespace CScript {
    public enum TokenType {
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
        LBRACKET,
        RBRACKET,
        PLUS,
        MINUS,
        STAR,
        SLASH,
        PRINT,
        SEMICOLON,
        SLASH_SLASH,
        IDENTIFIER,
        EQUAL,
        NOT,
        DOT,
        COMMA,
        RETURN,
        STRUCT,
        OBJECT,
        IS,
        AS,

        TYPE_INT, 
        TYPE_FLOAT,
        TYPE_BOOL,
        TYPE_CHAR, 
        TYPE_VOID,
        TYPE_TYPE,
        TYPE_OBJECT,

        LIT_NUMBER, 
        LIT_TRUE,
        LIT_FALSE,
        LIT_CHAR,
        LIT_NULL,

        EOF,
    }
    class Token {
        public Location Location { get; protected set; }
        public String Lexeme { get; protected set; }
        public TokenType Type { get; protected set; }
        public Token(TokenType type, int line, string file, string lexeme) {
            Type = type;
            Location = new Location(line, file);
            Lexeme = lexeme;
        }

        public override string ToString() {
            return Type.ToString() + ", Lexeme: " + Lexeme + ", on line: " + Location.Line + ", in file: " + Location.File;
        }
    }
}
