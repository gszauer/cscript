namespace CScript {
    namespace Pass1 {

        interface StatementVisitor<T, V> {
            T VisitPrintStatement(PrintStatement stmt, V misc);
            T VisitVarDeclStatement(VarDeclStatement stmt, V misc);
            T VisitExpressionStatement(ExpressionStatement stmt, V misc);
            T VisitFunDeclStatement(FunDeclStatement stmt, V misc);
            T VisitBlockStatement(BlockStatement stmt, V misc);
            T VisitReturnStatement(ReturnStatement stmt, V misc);
            T VisitStructDeclStatement(StructDeclStatement stmt, V misc);
        }
        class Statement {
            public Location Location { get; protected set; }

            public Statement(Location location) {
                Location = location;
            }

            public virtual T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                throw new NotImplementedException();
            }
        }

        class PrintStatement : Statement {
            public Expression Expression { get; protected set; }

            public PrintStatement(Location t, Expression e) : base(t) {
                Expression = e;
            }
            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
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

            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitVarDeclStatement(this, o);
            }
        }

        class ExpressionStatement : Statement {
            public Expression Expression { get; protected set; }

            public ExpressionStatement(Expression e) : base(e.Location) {
                Expression = e;
            }

            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitExpressionStatement(this, o);
            }
        }

        class FunParamater {
            public Location Location { get; protected set; }
            public string Name { get; protected set; }
            public TypeId Type { get; protected set; }

            public FunParamater(TypeId type, string name, Location location) {
                Type = type;
                Name = name;
                Location = location;
            }
        }

        class FunDeclStatement : Statement {
            public TypeId ReturnType { get; protected set; }
            public string Name { get; protected set; }
            public List<FunParamater> Paramaters { get; protected set;}
            public List<Statement> Body { get; protected set; }

            public FunDeclStatement(TypeId returnType, Token name, List<FunParamater> parameters, List<Statement> body) : base(name.Location) {
                ReturnType = returnType;
                Name = name.Lexeme;
                Paramaters = parameters;
                Body = body;
            }

            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitFunDeclStatement(this, o);
            }
        }

        class StructDeclStatement : Statement {
            public List<VarDeclStatement> Variables { get; protected set; }
            public string Name { get; protected set; }
            public TypeId Type { get; protected set; }

            public StructDeclStatement(Token name, TypeId type, List<VarDeclStatement> variables) : base(name.Location) {
                Name = name.Lexeme;
                Variables = variables;
                Type = type;
            }

            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitStructDeclStatement(this, o);
            }
        }

        class BlockStatement : Statement {
            public List<Statement> Body { get; protected set; }
            public BlockStatement(Location location, List<Statement> body) : base(location) {
                Body = body;
            }
            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitBlockStatement(this, o);
            }
        }

        class ReturnStatement : Statement {
            public Expression ReturnValue { get; protected set; }

            public ReturnStatement(Location location, Expression e) : base(location) {
                ReturnValue = e;
            }
            public override T Accept<T, V>(StatementVisitor<T, V> visitor, V o) {
                return visitor.VisitReturnStatement(this, o);
            }
        }

    }
}
