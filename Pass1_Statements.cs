namespace CScript {
    namespace Pass1 {

        interface StatementVisitor<T> {
            T VisitPrintStatement(PrintStatement stmt, Object misc);
            T VisitVarDeclStatement(VarDeclStatement stmt, Object misc);
            T VisitExpressionStatement(ExpressionStatement stmt, Object misc);
        }
        class Statement {
            public Location Location { get; protected set; }

            public Statement(Location location) {
                Location = location;
            }

            public virtual T Accept<T>(StatementVisitor<T> visitor, Object o) {
                throw new NotImplementedException();
            }
        }

        class PrintStatement : Statement {
            public Expression Expression { get; protected set; }

            public PrintStatement(Location t, Expression e) : base(t) {
                Expression = e;
            }
            public override T Accept<T>(StatementVisitor<T> visitor, object o) {
                return visitor.VisitPrintStatement(this, o);
            }
        }

        class VarDeclStatement : Statement {
            public Expression Initializer { get; protected set; }
            public string Name { get; protected set; }
            public TypeId Type { get; protected set; }
            public VarDeclStatement(TypeId type, Token name, Expression initializer) : base(name.Location) {
                Type = type;
                Initializer = initializer;
                Name = name.Lexeme;
            }

            public override T Accept<T>(StatementVisitor<T> visitor, object o) {
                return visitor.VisitVarDeclStatement(this, o);
            }
        }

        class ExpressionStatement : Statement {
            public Expression Expression { get; protected set; }

            public ExpressionStatement(Expression e) : base(e.Location) {
                Expression = e;
            }

            public override T Accept<T>(StatementVisitor<T> visitor, object o) {
                return visitor.VisitExpressionStatement(this, o);
            }
        }
    }
}
