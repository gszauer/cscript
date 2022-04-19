namespace CScript {
    namespace Pass0 { 
        interface ExpressionVisitor<T, V> {
            T VisitBinaryExpression(BinaryExpression expr, V misc);
            T VisitUnaryExpression(UnaryExpression expr, V misc);
            T VisitLiteralExpression(LiteralExpression expr, V misc);
            T VisitVariableExpression(VariableExpression expr, V misc);
            T VisitAssignmentExpression(AssignmentExpression expr, V misc);
            T VisitCallExpression(CallExpression expr, V misc);
        }
        class Expression {
            public Token Token { get; protected set; }

            public Location Location {
                get {
                    return Token.Location;
                }
            }
            public Expression(Token token) {
                Token = token;
            }

            public virtual T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                throw new NotImplementedException();
            }
        }
        class BinaryExpression : Expression {
            public Expression Left { get; protected set; }
            public Expression Right { get; protected set; }
            public Token Operator { get; protected set; }
            public BinaryExpression(Expression left, Token op, Expression right) : base(op) {
                Left = left;
                Right = right;
                Operator = op;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitBinaryExpression(this, o);
            }
        }
        class UnaryExpression : Expression {
            public Expression Expression { get; protected set; }
            public Token Operator { get; protected set;}

            public UnaryExpression(Token op, Expression e) : base(op) {
                Expression = e;
                Operator = op;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitUnaryExpression(this, o);
            }
        }
        class LiteralExpression : Expression {
            public string Lexeme {
                get {
                    return Token.Lexeme;
                }
            }
            public TokenType Type {
                get {
                    return Token.Type;
                }
            }

            public LiteralExpression(Token token) : base(token) {
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitLiteralExpression(this, o);
            }
        }

        class VariableExpression : Expression {
            public Token Name { get; protected set; }

            public VariableExpression(Token token) : base(token) {
                Name = token;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitVariableExpression(this, o);
            }
        }

        class AssignmentExpression : Expression {
            public Token Name { get; protected set; }
            public Expression Value { get; protected set; }
            public AssignmentExpression(Token name, Expression value) : base(name) {
                Name = name;
                Value = value;
            }
            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitAssignmentExpression(this, o);
            }
        }

        class CallExpression : Expression {
            public Expression Calee { get; protected set; }
            public Token CallSite { get; protected set; }
            public List<Expression> Arguments { get; protected set; }
            public CallExpression(Expression callee, Token callSite, List<Expression> arguments) : base(callSite) {
                Calee = callee;
                Arguments = arguments;
                CallSite = callSite;
            }
            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitCallExpression(this, o);
            }
        }
    }
}
