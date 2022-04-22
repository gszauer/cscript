namespace CScript {
    namespace Pass1 {
        interface ExpressionVisitor<T, V> {
            T VisitBinaryExpression(BinaryExpression expr, V misc);
            T VisitUnaryExpression(UnaryExpression expr, V misc);
            T VisitLiteralExpression(LiteralExpression expr, V misc);
            T VisitVariableExpression(VariableExpression expr, V misc);
            T VisitAssignmentExpression(AssignmentExpression expr, V misc);
            T VisitCallExpression(CallExpression expr, V misc);
            T VisitTypeExpression(TypeExpression expr, V misc);
            T VisitGetExpression(GetExpression expr, V misc);
            T VisitSetExpression(SetExpression expr, V misc);
            T VisitIsExpression(IsExpression expr, V misc);
            T VisitAsExpression(AsExpression expr, V misc);
        }
        class Expression {
            public TypeId Type { get; protected set; }
            public Location Location { get; protected set; }

            public Expression(TypeId type, Location location) {
                Location = location;
                Type = type;
            }

            public virtual T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
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

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
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

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitUnaryExpression(this, o);
            }
        }
        class LiteralExpression : Expression {
            public string Lexeme { get; protected set; }
            public LiteralExpression(Token token, TypeId type) : base(type, token.Location) {
                Lexeme = token.Lexeme;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitLiteralExpression(this, o);
            }
        }

        class VariableExpression : Expression {
            public string Name { get; protected set; }

            public VariableExpression(Token token, TypeId type) : base(type, token.Location) {
                Name = token.Lexeme;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
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
            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitAssignmentExpression(this, o);
            }
        }

        class CallExpression : Expression {
            public Expression Calee { get; protected set; }
            public List<Expression> Arguments { get; protected set; }
            public TypeId ResultType { get; protected set; }

            public CallExpression(Expression calee, List<Expression> arguments, TypeId resultType, Location location) : base(resultType, location) {
                Calee = calee;
                Arguments = arguments;
                ResultType = resultType;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitCallExpression(this, o);
            }
        }

        class TypeExpression : Expression {
            public Expression Expression { get; protected set; }
            public bool Dynamic { get; protected set; }

            public TypeExpression(Location l, TypeId type, Expression e, bool dyn) : base(type, l) {
                Expression = e;
                Dynamic = dyn;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitTypeExpression(this, o);
            }
        }

        class GetExpression : Expression {
            public Expression Callee { get; protected set; }
            public string Name { get; protected set; }

            public GetExpression(Token name, TypeId type, Expression e) : base(type, name.Location) {
                Name = name.Lexeme;
                Callee = e;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitGetExpression(this, o);
            }
        }

        class SetExpression : Expression {
            public Expression Callee { get; protected set; }
            public string Name { get; protected set; }
            public Expression Value { get; protected set; }

            public SetExpression(Token name, TypeId type, Expression callee, Expression value) : base(type, name.Location) {
                Callee = callee;
                Value = value;
                Name = name.Lexeme;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitSetExpression(this, o);
            }
        }

        class IsExpression : Expression {
            public Expression TestObject { get; protected set; }
            public TypeId TestType { get; protected set; }
            public IsExpression(Expression testObj, TypeId testType, TypeId boolType) : base(boolType, testObj.Location) {
                TestType = testType;
                TestObject = testObj;
            }
            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitIsExpression(this, o);
            }
        }

        class AsExpression : Expression {
            public Expression Castee { get; protected set; }

            public TypeId CastTo { get; protected set; }

            public AsExpression(Expression cObj, TypeId cType) : base(cType, cObj.Location) {
                Castee = cObj;
                CastTo = cType;
            }

            public override T Accept<T, V>(ExpressionVisitor<T, V> visitor, V o) {
                return visitor.VisitAsExpression(this, o);
            }
        }
    }
}
