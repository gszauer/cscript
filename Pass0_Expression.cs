namespace CScript {
    namespace Pass0 { 
        interface ExpressionVisitor<T> {
            T VisitBinaryExpression(BinaryExpression expr, object misc);
            T VisitUnaryExpression(UnaryExpression expr, object misc);
            T VisitLiteralExpression(LiteralExpression expr, object misc);
            T VisitVariableExpression(VariableExpression expr, object misc);
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

            public virtual T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
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

            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
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

            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
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

            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
                return visitor.VisitLiteralExpression(this, o);
            }
        }

        class VariableExpression : Expression {
            public Token Name { get; protected set; }

            public VariableExpression(Token token) : base(token) {
                Name = token;
            }

            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
                return visitor.VisitVariableExpression(this, o);
            }
        }
    }
}
