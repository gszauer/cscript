using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {
    class TypeChecker : Pass0.StatementVisitor<Pass1.Statement, TypeEnvironment>, Pass0.ExpressionVisitor<Pass1.Expression, TypeEnvironment> {
        public List<Pass1.Statement> Program { get; protected set; }
        
        TypeTable mTypeTable;
        bool mDeclaringGlobals;
        public TypeId mCurrentFunctionReturnType;
        public TypeId mLastEncounteredReturnType;

        public TypeChecker(List<Pass0.Statement> program, TypeTable typeTable) {
            mTypeTable = typeTable;
            Program = new List<Pass1.Statement>();
            mDeclaringGlobals = false;
            mCurrentFunctionReturnType = null;
            mLastEncounteredReturnType = null;

            TypeEnvironment env = new TypeEnvironment(null);
            Location loc = new Location(-1, "generated - type location");
            foreach(string type in typeTable.DeclaredTypes) {
                env.DeclareVariableType(type, typeTable.GetTypeId(type, loc), loc);
            }
            // Pre-declare functions
            foreach(KeyValuePair<string, CallableDeclaration> funDecl in typeTable.Functions) {
                env.DeclareVariableType(funDecl.Key, typeTable.FunctionID, funDecl.Value.Location);
            }
            mDeclaringGlobals = true;
            // Pre-declare variables
            foreach (Pass0.Statement s in program) {
                if (s is Pass0.VarDeclStatement) {
                    Pass0.VarDeclStatement varDecl = (Pass0.VarDeclStatement)s;
                    TypeId varType = typeTable.GetTypeId(varDecl.Type.Lexeme, varDecl.Type.Location);
                    if (varDecl.Initializer != null) {
                        Pass1.Expression initializer = ResolveExpression(varDecl.Initializer, env);
                        if (initializer.Type != varType) {
                            throw new CompilerException(ExceptionSource.TYPECHECKER, varDecl.Location, "Can't initialize: " + varDecl.Name.Lexeme + " to type: " + initializer.Type.DebugName + ", it's declared as: " + varDecl.Type.Lexeme);
                        }
                    }
                    Program.Add(ResolveStatement(s, env));
                }
            }
            mDeclaringGlobals = false;

            foreach (Pass0.Statement s in program) {
                if (!(s is Pass0.VarDeclStatement)) {
                    Program.Add(ResolveStatement(s, env));
                }
            }

            mTypeTable = null;
        }

        Pass1.Statement ResolveStatement(Pass0.Statement statement, TypeEnvironment misc) {
            return statement.Accept(this, misc);
        }

        Pass1.Expression ResolveExpression(Pass0.Expression expression, TypeEnvironment misc) {
            return expression.Accept(this, misc);
        }

        public Pass1.Statement VisitPrintStatement(Pass0.PrintStatement stmt, TypeEnvironment misc) {
            Pass1.Expression expression = null;
            if (stmt.Expression != null) {
                expression = ResolveExpression(stmt.Expression, misc);
            }

            return new Pass1.PrintStatement(stmt.Location, expression);
        }
        public Pass1.Statement VisitExpressionStatement(Pass0.ExpressionStatement stmt, TypeEnvironment misc) {
            Pass1.Expression expression = ResolveExpression(stmt.Expression, misc);

            return new Pass1.ExpressionStatement(expression);
        }

        public Pass1.Expression VisitBinaryExpression(Pass0.BinaryExpression expr, TypeEnvironment misc) {
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
        public Pass1.Expression VisitUnaryExpression(Pass0.UnaryExpression expr, TypeEnvironment misc) {
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
        public Pass1.Expression VisitLiteralExpression(Pass0.LiteralExpression expr, TypeEnvironment misc) {
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
        public Pass1.Expression VisitVariableExpression(Pass0.VariableExpression expr, TypeEnvironment env) {
            if (mDeclaringGlobals) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Name.Location, "Global variables can only be instantiated with constant expressions.");
            }
            TypeId variableType = env.GetVariableType(expr.Name.Lexeme, expr.Name.Location);
            return new Pass1.VariableExpression(expr.Name, variableType);
        }

        public Pass1.Expression VisitCallExpression(Pass0.CallExpression expr, TypeEnvironment misc) {
            Pass1.Expression calee = ResolveExpression(expr.Calee, misc);
            List<Pass1.Expression> args = new List<Pass1.Expression>();
            foreach(Pass0.Expression arg in expr.Arguments) {
                args.Add(ResolveExpression(arg, misc));
            }

            if (!(calee is Pass1.VariableExpression)) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, calee.Location, "Only variable expression is callable, not: " + calee.Type.DebugName);
            }
            Pass1.VariableExpression varExpr = (Pass1.VariableExpression)calee;

            // Look up function based on argument types
            if (!mTypeTable.IsCallable(calee.Type)) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, calee.Location, "Calling function on non callable type: " + calee.Type.DebugName);
            }

            CallableDeclaration callable = mTypeTable.GetCallable(varExpr.Name, varExpr.Type, varExpr.Location);

            if (callable.ArgumentNames.Count != args.Count) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, calee.Location, "Calling " + varExpr.Name + " with wrong number of arguments: " + args.Count + " / " + callable.ArgumentNames.Count);
            }

            for (int i = 0; i < args.Count; ++i) {
                if (callable.ArgumentTypes[i] != args[i].Type) {
                    throw new CompilerException(ExceptionSource.TYPECHECKER, calee.Location, "Calling " + varExpr.Name + " with argument (" + (i + 1) + ") '" + callable.ArgumentNames[i] + "' being the wrong type, expected: " + callable.ArgumentTypes[i].DebugName + ", got: " + args[i].Type.DebugName);
                }
            }

            return new Pass1.CallExpression(calee, args, callable.ReturnType, expr.Location);
        }

        bool CanAssign(TypeId leftType, TypeId rightType) {
            return leftType == rightType;
        }
        public Pass1.Expression VisitAssignmentExpression(Pass0.AssignmentExpression expr, TypeEnvironment env) {

            TypeId variableType = env.GetVariableType(expr.Name.Lexeme, expr.Name.Location);
            Pass1.Expression variableValue = ResolveExpression(expr.Value, env);

            if (!CanAssign(variableType, variableValue.Type)) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Name.Location, "Trying to assign " + expr.Name.Lexeme + " to " + variableValue.Type.DebugName + " when it's declared as: " + variableType.DebugName);
            }

            return new Pass1.AssignmentExpression(expr.Name, variableValue, variableType);
        }

        public Pass1.Statement VisitFunDeclStatement(Pass0.FunDeclStatement expr, TypeEnvironment misc) {
            TypeEnvironment functionEnv = new TypeEnvironment(misc);
           
            TypeId returnType = mTypeTable.GetTypeId(expr.ReturnType.Lexeme, expr.ReturnType.Location);
            mCurrentFunctionReturnType = returnType;

            List<Pass1.FunParamater> paramaters = new List<Pass1.FunParamater>();
            foreach (Pass0.FunParamater param in expr.Paramaters) {
                TypeId paramType = mTypeTable.GetTypeId(param.Type.Lexeme, param.Type.Location);
                Pass1.FunParamater newParam = new Pass1.FunParamater(paramType, param.Name.Lexeme, param.Name.Location);
                paramaters.Add(newParam);

                functionEnv.DeclareVariableType(param.Name.Lexeme, paramType, param.Name.Location);
            }

            List<Pass1.Statement> body = new List<Pass1.Statement>();
            foreach(Pass0.Statement stmt in expr.Body) {
                body.Add(ResolveStatement(stmt, functionEnv));
            }

            if (mLastEncounteredReturnType == null) { // No return statement was given
                if (returnType != mTypeTable.VoidID) { // But something should be returned!
                    throw new CompilerException(ExceptionSource.TYPECHECKER, expr.Location, "No return encountered. Function should return: " + returnType.DebugName);
                }
            }

            mCurrentFunctionReturnType = null;
            mLastEncounteredReturnType = null;

            return new Pass1.FunDeclStatement(returnType, expr.Name, paramaters, body);
        }

        public Pass1.Statement VisitBlockStatement(Pass0.BlockStatement expr, TypeEnvironment misc) {
            TypeEnvironment blockEnv = new TypeEnvironment((TypeEnvironment)misc);
            List<Pass1.Statement> body = new List<Pass1.Statement>();
            foreach (Pass0.Statement stmt in expr.Body) {
                body.Add(ResolveStatement(stmt, blockEnv));
            }
            return new Pass1.BlockStatement(expr.Location, body);
        }

        public Pass1.Statement VisitVarDeclStatement(Pass0.VarDeclStatement stmt, TypeEnvironment misc) {
            TypeId variableType = mTypeTable.GetTypeId(stmt.Type.Lexeme, stmt.Type.Location);
            if (variableType == mTypeTable.VoidID) {
                throw new CompilerException(ExceptionSource.TYPECHECKER, stmt.Name.Location, "Void is an invalid variable type for: " + stmt.Name.Lexeme);
            }
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

        public Pass1.Statement VisitReturnStatement(Pass0.ReturnStatement stmt, TypeEnvironment misc) {
            if (mLastEncounteredReturnType != null) {
                throw new NotImplementedException();
            }

            Pass1.Expression returnValue = null;
            if (stmt.ReturnValue != null) {
                returnValue = ResolveExpression(stmt.ReturnValue, misc);

                if (returnValue.Type != mCurrentFunctionReturnType) {
                    throw new CompilerException(ExceptionSource.TYPECHECKER, stmt.Location, "Returning " + returnValue.Type.DebugName + " when function declares: " + mCurrentFunctionReturnType.DebugName);
                }
                mLastEncounteredReturnType = returnValue.Type;
            }
            else {
                if (mCurrentFunctionReturnType != mTypeTable.VoidID) {
                    throw new CompilerException(ExceptionSource.TYPECHECKER, stmt.Location, "Only void functions can have an empty return statement");
                }
            }

            return new Pass1.ReturnStatement(stmt.Location, returnValue);
        }
    }
}
