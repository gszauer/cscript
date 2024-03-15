namespace CScript {
    internal class PrettyPrinter : ParseTree.Visitor {
        protected int Indent = 0;
        public string Result { get; protected set; }

        protected void ApplyIndent() {
            for (int j = 0; j < Indent; ++j) {
                Result += "\t";
            }
        }
        public PrettyPrinter(List<ParseTree.Declaration.File> parseTree) {
            Result = "";
            foreach (ParseTree.Declaration.File file in parseTree) {
                file.Accept(this);
            }
        }
        public object Visit(ParseTree.Type.Primitive t) {
            Result += t.GetPath();
            return t;
        }
        public object Visit(ParseTree.Type.Array t) {
            Result += t.GetPath();
            return t;
        }
        public object Visit(ParseTree.Type.Map t) {
            Result += t.GetPath();
            return t;
        }
        public object Visit(ParseTree.Declaration.File d) {
            for (int i = 0, size = d.Content.Count; i < size; ++i) {
                d.Content[i].Accept(this);
            }
            return d;
        }
        public object Visit(ParseTree.Declaration.Variable d) {
            Result += d.Type.GetPath() + " ";
            Result += d.Name.Lexeme;

            if (d.Initializer != null) {
                Result += " = ";
                d.Initializer.Accept(this);
            }
            Result += ";\n\n";

            return d;
        }
        public object Visit(ParseTree.Declaration.Function d) {
            Result += d.Return.GetPath() + " ";
            Result += d.Name.Lexeme;
            Result += "(";
            for (int i = 0, size = d.Paramaters.Count; i < size; ++i) {
                Result += d.Paramaters[i].Type.GetPath() + " ";
                Result += d.Paramaters[i].Name.Lexeme;
                if (i < size - 1) {
                    Result += ", ";
                }
            }
            Result += ")";
            if (d.Body != null) {
                Result += " ";
                d.Body.Accept(this);
            }
            else {
                Result += ";";
            }
            Result += "\n";

            return d;
        }
        public object Visit(ParseTree.Declaration.Enum d) {
            Result += "enum " + d.Name.Lexeme + " {\n";
            for (int i = 0; i < d.Members.Count; ++i) {
                Result += "\t" + d.Members[i].Name.Lexeme;
                Result += " = " + d.Members[i].Value;
                if (i < d.Members.Count - 1) {
                    Result += ",";
                }
                Result += "\n";
            }
            Result += "}\n\n";

            return d;
        }
        public object Visit(ParseTree.Declaration.Delegate d) {
            Result += "delegate ";
            Result += d.Return.GetPath() + " ";
            Result += d.Name.Lexeme;
            Result += "(";
            for (int i = 0, size = d.Paramaters.Count; i < size; ++i) {
                Result += d.Paramaters[i].Type.GetPath() + " ";
                Result += d.Paramaters[i].Name.Lexeme;
                if (i < size - 1) {
                    Result += ", ";
                }
            }
            Result += ");\n\n";

            return d;
        }
        public object Visit(ParseTree.Declaration.Struct d) {
            Result += "struct " + d.Name.Lexeme + " {\n";
            for (int i = 0, size = d.Members.Count; i < size; ++i) {
                Result += "\t";
                d.Members[i].Accept(this);
            }
            Result += "}\n\n";

            return d;
        }
        public object Visit(ParseTree.Statement.Block s) {
            Result += "{\n";
            Indent += 1;
            for (int i = 0, size = s.Body.Count; i < size; ++i) {
                for (int j = 0; j < Indent; ++j) {
                    Result += "\t";
                }
                s.Body[i].Accept(this);
            }
            Indent -= 1;
            for (int j = 0; j < Indent; ++j) {
                Result += "\t";
            }
            Result += "}\n";

            return s;
        }
        public object Visit(ParseTree.Statement.Variable s) {
            Result += s.Type.GetPath() + " ";
            Result += s.Name.Lexeme;
            if (s.Initializer != null) {
                Result += " = ";
                s.Initializer.Accept(this);
            }
            Result += ";\n";

            return s;
        }
        public object Visit(ParseTree.Statement.Expression s) {
            s.Target.Accept(this);
            Result += ";\n";
            return s;
        }
        public object Visit(ParseTree.Statement.Control s) {
            if (s.Keyword.Symbol == Symbol.CONTINUE) {
                Result += "continue;\n";
            }
            else if (s.Keyword.Symbol == Symbol.BREAK) {
                Result += "break;\n";
            }
            else {
                Result += "return";
                if (s.Value != null) {
                Result += " ";
                    s.Value.Accept(this);
                }
                Result += ";\n";
            }

            return s;
        }
        public object Visit(ParseTree.Statement.If s) {
            ParseTree.Statement.If iter = s;
            while (iter != null) {
                if (iter.Condition != null) {
                    Result += "if (";
                    iter.Condition.Accept(this);
                    Result += ")";
                }
                if (iter.Body != null) {
                    Result += " ";
                    iter.Body.Accept(this);
                }

                iter = iter.Next;
                if (iter != null) {
                    ApplyIndent();
                    Result += "else ";
                }
            }
            return s;
        }
        public object Visit(ParseTree.Statement.While s) {
            Result += "while (";
            if (s.Condition != null) {
                s.Condition.Accept(this);
            }
            Result += ")";
            if (s.Body != null) {
            Result += " ";
                s.Body.Accept(this);
            }
            else {
                Result += ";\n";
            }

            return s;
        }
        public object Visit(ParseTree.Statement.For s) {
            Result += "for(";
            if (s.Initializers != null) {
                for (int i = 0, size = s.Initializers.Count; i < size; ++i) {
                    if (i == 0) {
                        Result += s.Initializers[i].Type.GetPath() + " ";
                    }
                    Result += s.Initializers[i].Name.Lexeme;

                    if (s.Initializers[i].Initializer != null) {
                        Result += " = ";
                        s.Initializers[i].Initializer.Accept(this);
                    }
                    
                    if (i < size - 1) {
                        Result += ", ";
                    }
                }
            }
            Result += ";";
            if (s.Condition != null) {
                s.Condition.Accept(this);
            }
            Result += ";";
            if (s.Iterators != null) {
                for (int i = 0, size = s.Iterators.Count; i < size; ++i) {
                    s.Iterators[i].Accept(this);
                }
            }
            Result += ")";
            if (s.Body != null) {
                Result += " ";
                s.Body.Accept(this);
            }
            else {
                Result += ";\n";
            }

            return s;
        }
        public object Visit(ParseTree.Expression.Group e) {
            Result += "(";
            e.Target.Accept(this);
            Result += ")";
            return e;
        }
        public object Visit(ParseTree.Expression.Literal e) {
            if (e.Value.Symbol == Symbol.LIT_STRING) {
                Result += "\"";
            }
            Result += e.Value.Lexeme;
            if (e.Value.Symbol == Symbol.LIT_STRING) {
                Result += "\"";
            }
            return e;
        }
        public object Visit(ParseTree.Expression.Get e) {
            if (e.Object != null) {
                e.Object.Accept(this);
                Result += ".";
            }
            Result += e.Property.Lexeme;
            return e;
        }
        public object Visit(ParseTree.Expression.Set e) {
            if (e.Object != null) {
                e.Object.Accept(this);
                Result += ".";
            }
            Result += e.Property.Lexeme;
            if (e.Value != null) {
                Result += " = ";
                e.Value.Accept(this);
            }
            return e;
        }
        public object Visit(ParseTree.Expression.Unary e) {
            if (e.Precedence == ParseTree.Expression.Unary.OperatorType.PREFIX) {
                Result += e.Operator.Lexeme;
            }
            e.Object.Accept(this);
            if (e.Precedence == ParseTree.Expression.Unary.OperatorType.POSTFIX) {
                Result += e.Operator.Lexeme;

            }
            return e;
        }
        public object Visit(ParseTree.Expression.Binary e) {
            e.Left.Accept(this);
            Result += " " + e.Operator.Lexeme + " ";
            e.Right.Accept(this);

            return e;
        }
        public object Visit(ParseTree.Expression.Call e) {
            if (e.Object != null) {
                e.Object.Accept(this);
            }
            Result += "(";
            for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                e.Arguments[i].Accept(this);
                if (i < size - 1) {
                    Result += ", ";
                }
            }
            Result += ")";

            return e;
        }
        public object Visit(ParseTree.Expression.Cast e) {
            e.Object.Accept(this);
            Result += " as ";
            e.Target.Accept(this);
            
            return e;
        }
        public object Visit(ParseTree.Expression.New e) {
            Result += "new ";
            e.Target.Accept(this);
            Result += "(";
            for (int i = 0, size = e.Arguments.Count; i < size; ++i) {
                e.Arguments[i].Accept(this);
                if (i < size - 1) {
                    Result += ", ";
                }
            }
            Result += ")";

            return e;
        }
    }
}
