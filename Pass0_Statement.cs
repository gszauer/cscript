namespace CScript {
    namespace Pass0 {

        interface StatementVisitor<T> {
            T VisitPrintStatement(PrintStatement stmt, Object misc);
            T VisitVarDeclStatement(VarDeclStatement stmt, Object misc);
            T VisitExpressionStatement(ExpressionStatement stmt, Object misc);
        }
        class Statement {
            public Token Token { get; protected set; }

            public Location Location {
                get {
                    return Token.Location;
                }
            }
            public Statement(Token token) {
                Token = token;
            }

            public virtual T Accept<T>(StatementVisitor<T> visitor, Object o) {
                throw new NotImplementedException();
            }
        }

        class PrintStatement : Statement {
            public Expression Expression { get; protected set; }

            public PrintStatement(Token t, Expression e) : base(t) {
                Expression = e; 
            }
            public override T Accept<T>(StatementVisitor<T> visitor, object o) {
                return visitor.VisitPrintStatement(this, o);
            }
        }

        class VarDeclStatement : Statement {
            public Expression Initializer { get; protected set; }
            public Token Name { get; protected set; }
            public Token Type { get; protected set; }
            public VarDeclStatement(Token type, Token name, Expression initializer) : base(name) {
                Type = type;
                Initializer = initializer;
                Name = name;
            }

            public override T Accept<T>(StatementVisitor<T> visitor, object o) {
                return visitor.VisitVarDeclStatement(this, o);
            }
        }

        class ExpressionStatement : Statement {
            public Expression Expression { get; protected set; }

            public ExpressionStatement(Expression e) : base(e.Token) {
                Expression= e;
            }

            public override T Accept<T>(StatementVisitor<T> visitor, object o) {
                return visitor.VisitExpressionStatement(this, o);
            }
        }
    }
}
