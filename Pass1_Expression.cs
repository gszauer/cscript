namespace CScript {
    namespace Pass1 {
        interface ExpressionVisitor<T> {
            T VisitBinaryExpression(BinaryExpression expr, object misc);
            T VisitUnaryExpression(UnaryExpression expr, object misc);
            T VisitLiteralExpression(LiteralExpression expr, object misc);
            T VisitVariableExpression(VariableExpression expr, object misc);
            T VisitAssignmentExpression(AssignmentExpression expr, object misc);
        }
        class Expression {
            public TypeId Type { get; protected set; }
            public Location Location { get; protected set; }

            public Expression(TypeId type, Location location) {
                Location = location;
                Type = type;
            }

            public virtual T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
                throw new NotImplementedException();
            }
        }
        class BinaryExpression : Expression {
            public Expression Left { get; protected set; }
            public Expression Right { get; protected set; }
            public TokenType Operator { get; protected set; }
            public BinaryExpression(Expression left, Token op, Expression right, TypeId type) : base(type, op.Location) {
                Left = left;
                Right = right;
                Operator = op.Type;
            }

            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
                return visitor.VisitBinaryExpression(this, o);
            }
        }
        class UnaryExpression : Expression {
            public Expression Expression { get; protected set; }
            public TokenType Operator { get; protected set; }

            public UnaryExpression(Token op, Expression e, TypeId type) : base(type, op.Location) {
                Expression = e;
                Operator = op.Type;
            }

            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
                return visitor.VisitUnaryExpression(this, o);
            }
        }
        class LiteralExpression : Expression {
            public string Lexeme {get; protected set; }
            public LiteralExpression(Token token, TypeId type) : base(type, token.Location) {
                Lexeme = token.Lexeme;
            }

            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
                return visitor.VisitLiteralExpression(this, o);
            }
        }

        class VariableExpression : Expression {
            public string Name { get; protected set; }

            public VariableExpression(Token token, TypeId type) : base(type, token.Location) {
                Name = token.Lexeme;
            }

            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
                return visitor.VisitVariableExpression(this, o);
            }
        }

        class AssignmentExpression : Expression {
            public string Name { get; protected set; }
            public Expression Value { get; protected set; }
            public AssignmentExpression(Token name, Expression value, TypeId type) : base(type, name.Location) {
                Name = name.Lexeme;
                Value = value;
            }
            public override T Accept<T>(ExpressionVisitor<T> visitor, Object o) {
                return visitor.VisitAssignmentExpression(this, o);
            }
        }
    }
}
