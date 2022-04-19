namespace CScript {
    namespace Pass0 {

        interface StatementVisitor<T, V> {
            T VisitPrintStatement(PrintStatement stmt, V misc);
            T VisitVarDeclStatement(VarDeclStatement stmt, V misc);
            T VisitExpressionStatement(ExpressionStatement stmt, V misc);
            T VisitFunDeclStatement(FunDeclStatement stmt, V misc);
            T VisitBlockStatement(BlockStatement stmt, V misc);
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

            public virtual T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                throw new NotImplementedException();
            }
        }

        class PrintStatement : Statement {
            public Expression Expression { get; protected set; }

            public PrintStatement(Token t, Expression e) : base(t) {
                Expression = e; 
            }
            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
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

            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitVarDeclStatement(this, o);
            }
        }

        class ExpressionStatement : Statement {
            public Expression Expression { get; protected set; }

            public ExpressionStatement(Expression e) : base(e.Token) {
                Expression= e;
            }

            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitExpressionStatement(this, o);
            }
        }

        class FunParamater {
            public Token Type { get; protected set; }
            public Token Name { get; protected set; }
            public FunParamater(Token name, Token type) {
                Name = name;
                Type = type;
            }
        }

        class FunDeclStatement : Statement {
            public Token ReturnType { get; protected set; }
            public Token Name { get; protected set; }
            public List<FunParamater> Paramaters { get; protected set; }
            public List<Statement> Body { get; protected set;}
            public FunDeclStatement(Token rType, Token name, List<FunParamater> paramaters, List<Statement> body) : base(name) {
                ReturnType = rType;
                Name = name;
                Paramaters = paramaters;
                Body = body;
            }

            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitFunDeclStatement(this, o);
            }
        }

        class BlockStatement : Statement {
            public List<Statement> Body { get; protected set; }
            public BlockStatement(Token lbrace, List<Statement> body) : base(lbrace) {
                Body = body;
            }

            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitBlockStatement(this, o);
            }
        }
    }
}
