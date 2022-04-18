using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {
    class TypeChecker : Pass0.StatementVisitor<Pass1.Statement>, Pass0.ExpressionVisitor<Pass1.Expression> {
        public List<Pass1.Statement> Program { get; protected set; }
        
        TypeTable mTypeTable;

        public TypeChecker(List<Pass0.Statement> program, TypeTable typeTable) {
            mTypeTable = typeTable;
            Program = new List<Pass1.Statement>();

            TypeEnvironment env = new TypeEnvironment(null);
            Location loc = new Location(-1, "generated - type location");
            foreach(string type in typeTable.DeclaredTypes) {
                env.DeclareVariableType(type, typeTable.GetTypeId(type, loc), loc);
            }

            foreach (Pass0.Statement s in program) {
                Program.Add(ResolveStatement(s, env));
            }

            mTypeTable = null;
        }

        Pass1.Statement ResolveStatement(Pass0.Statement statement, object misc) {
            return statement.Accept(this, misc);
        }

        Pass1.Expression ResolveExpression(Pass0.Expression expression, object misc) {
            return expression.Accept(this, misc);
        }

        public Pass1.Statement VisitPrintStatement(Pass0.PrintStatement stmt, Object misc) {
            Pass1.Expression expression = null;
            if (stmt.Expression != null) {
                expression = ResolveExpression(stmt.Expression, misc);
            }

            return new Pass1.PrintStatement(stmt.Location, expression);
        }
        public Pass1.Statement VisitExpressionStatement(Pass0.ExpressionStatement stmt, Object misc) {
            Pass1.Expression expression = ResolveExpression(stmt.Expression, misc);

            return new Pass1.ExpressionStatement(expression);
        }

        public Pass1.Expression VisitBinaryExpression(Pass0.BinaryExpression expr, object misc) {
            Pass1.Expression left = ResolveExpression(expr.Left, misc);
            Pass1.Expression right = ResolveExpression(expr.Right, misc);

            if (left.Type != right.Type) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Location, "Can only perform binary operation '" + expr.Operator.Lexeme + "' on the same operand types. Left: " + left.Type.DebugName + ", Right: " + right.Type.DebugName);
            }

            switch(expr.Operator.Type) {
                case TokenType.PLUS:
                case TokenType.MINUS:
                case TokenType.STAR:
                case TokenType.SLASH:
                    if (left.Type != mTypeTable.IntID && left.Type != mTypeTable.FloatID) {
                        throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Location, "Can only perform binary operation '" + expr.Operator.Lexeme + "' on integers or floating points. Left: " + left.Type.DebugName + ", Right: " + right.Type.DebugName);
                    }
                    break;
            }

            return new Pass1.BinaryExpression(left, expr.Operator, right, left.Type);
        }
        public Pass1.Expression VisitUnaryExpression(Pass0.UnaryExpression expr, object misc) {
            Pass1.Expression target = ResolveExpression(expr.Expression, misc);

            if (expr.Operator.Type == TokenType.MINUS) {
                if (target.Type != mTypeTable.IntID && target.Type != mTypeTable.FloatID) {
                    throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Location, "Unary negation can only be done on integers and floats, type: " + target.Type.DebugName);
                }
            }
            else if (expr.Operator.Type == TokenType.NOT) {
                if (target.Type != mTypeTable.BoolID) {
                    throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Location, "Unary not can only be done on booleans, type: " + target.Type.DebugName);
                }
            }

            return new Pass1.UnaryExpression(expr.Operator, target, target.Type);
        }
        public Pass1.Expression VisitLiteralExpression(Pass0.LiteralExpression expr, object misc) {
            TypeId resultType = null;
            if (expr.Type == TokenType.LIT_CHAR) {
                resultType = mTypeTable.CharID;
            }
            else if (expr.Type == TokenType.LIT_FALSE || expr.Type == TokenType.LIT_TRUE) {
                resultType = mTypeTable.BoolID;
            }
            else if (expr.Type == TokenType.LIT_NUMBER) {
                if (expr.Lexeme.Contains('.')) {
                    resultType = mTypeTable.FloatID;
                }
                else {
                    resultType = mTypeTable.IntID;
                }
            }
            else {
                throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Location, "Unknown literal type");
            }

            return new Pass1.LiteralExpression(expr.Token, resultType);
        }
        public Pass1.Expression VisitVariableExpression(Pass0.VariableExpression expr, object misc) {
            TypeEnvironment env = (TypeEnvironment)misc;

            TypeId variableType = env.GetVariableType(expr.Name.Lexeme, expr.Name.Location);
            return new Pass1.VariableExpression(expr.Name, variableType);
        }

        bool CanAssign(TypeId leftType, TypeId rightType) {
            return leftType == rightType;
        }
        public Pass1.Expression VisitAssignmentExpression(Pass0.AssignmentExpression expr, object misc) {
            TypeEnvironment env = (TypeEnvironment)misc;

            TypeId variableType = env.GetVariableType(expr.Name.Lexeme, expr.Name.Location);
            Pass1.Expression variableValue = ResolveExpression(expr.Value, env);

            if (!CanAssign(variableType, variableValue.Type)) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Name.Location, "Trying to assign " + expr.Name.Lexeme + " to " + variableValue.Type.DebugName + " when it's declared as: " + variableType.DebugName);
            }

            return new Pass1.AssignmentExpression(expr.Name, variableValue, variableType);
        }

        public Pass1.Statement VisitVarDeclStatement(Pass0.VarDeclStatement stmt, Object misc) {
            TypeId variableType = mTypeTable.GetTypeId(stmt.Type.Lexeme, stmt.Type.Location);
            string variableName = stmt.Name.Lexeme;
            Pass1.Expression initializer = null;
            if (stmt.Initializer != null) {
                initializer = ResolveExpression(stmt.Initializer, misc);
                if (!CanAssign(initializer.Type, variableType)) {
                    throw new CompilerException(ExceptionSource.TYPECHECKER, stmt.Name.Location, "Trying to initialize " + stmt.Name.Lexeme + " to " + initializer.Type.DebugName + " when it's declared as: " + variableType.DebugName);
                }
            }
            TypeEnvironment env = (TypeEnvironment)misc;
            env.DeclareVariableType(variableName, variableType, stmt.Location);

            return new Pass1.VarDeclStatement(variableType, stmt.Name, initializer);
        }
    }
}
