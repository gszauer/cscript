﻿namespace CScript {
    public enum TokenType {
        LPAREN,
        RPAREN,
        LBRACE,
        RBRACE,
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
        COMMA,
        RETURN,

        TYPE_INT, 
        TYPE_FLOAT,
        TYPE_BOOL,
        TYPE_CHAR, 
        TYPE_VOID,

        LIT_NUMBER, 
        LIT_TRUE,
        LIT_FALSE,
        LIT_CHAR,

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
