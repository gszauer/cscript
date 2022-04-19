using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {

    class CallableDeclaration {
        public Location Location { get; protected set; }
        public TypeId ReturnType { get; protected set; }
        public string Name { get; protected set; }
        public List<string> ArgumentNames { get; protected set; }
        public List<TypeId> ArgumentTypes { get; protected set; }
        //public Dictionary<string, TypeId> Arguments { get; protected set; }

        public CallableDeclaration(TypeId rType, string name, List<string> argNames, List<TypeId> argVals, Location location) {
            Location = location;
            ReturnType = rType;
            Name = name;
            ArgumentNames = argNames;
            ArgumentTypes = argVals;
            /*Arguments = new Dictionary<string, TypeId>();
            for (int i = 0; i < argNames.Count; i++) {
                Arguments[argNames[i]] = argVals[i];
            }*/
        }
    }

    class TypeTable {
        public List<string> DeclaredTypes { get; protected set; }
        public Dictionary<string, CallableDeclaration> Functions { get; protected set; }

        public TypeId VoidID { get; protected set; }
        public TypeId IntID { get; protected set; }
        public TypeId FloatID { get; protected set; }
        public TypeId CharID { get; protected set; }
        public TypeId BoolID { get; protected set; }
        public TypeId FunctionID { get; protected set; }
        public TypeTable(List<Pass0.Statement> statements) { // Will ALWAYS BE PASS0
            DeclaredTypes = new List<string>();
            Functions = new Dictionary<string, CallableDeclaration>();

            DeclaredTypes.Add("void");
            DeclaredTypes.Add("int");
            DeclaredTypes.Add("float");
            DeclaredTypes.Add("char");
            DeclaredTypes.Add("bool");

            DeclaredTypes.Add("$struct"); // For later so i don't forget
            // Loop trough all structs and declare them here.
            // This way you can query a struct if typeid is between $struct and $function
            // Similarly, anything past $function will be a user function. Pretty clever.

            DeclaredTypes.Add("$function");

            IntID = new TypeId(DeclaredTypes.IndexOf("int"), "int");
            FloatID = new TypeId(DeclaredTypes.IndexOf("float"), "float");
            CharID = new TypeId(DeclaredTypes.IndexOf("char"), "char");
            BoolID = new TypeId(DeclaredTypes.IndexOf("bool"), "bool");
            FunctionID = new TypeId(DeclaredTypes.IndexOf("$function"), "$function");
            VoidID = new TypeId(DeclaredTypes.IndexOf("void"), "void");

            // Loop trough all functions and register their unique names here
            foreach (Pass0.Statement statement in statements) {
                if (statement is Pass0.FunDeclStatement) {
                    Pass0.FunDeclStatement funDecl = (Pass0.FunDeclStatement)statement;
                    TypeId returnType = GetTypeId(funDecl.ReturnType.Lexeme, funDecl.ReturnType.Location);

                    List<string> argNames = new List<string>();
                    List<TypeId> argTypes = new List<TypeId>();

                    foreach (Pass0.FunParamater param in funDecl.Paramaters) {
                        argNames.Add(param.Name.Lexeme);
                        argTypes.Add(GetTypeId(param.Type.Lexeme, param.Type.Location));
                    }

                    CallableDeclaration function = new CallableDeclaration(returnType, funDecl.Name.Lexeme, argNames, argTypes, funDecl.Location);
                    Functions.Add(funDecl.Name.Lexeme, function);
                }
            }
        }

        public bool ContainsType(string typeName) {
            return DeclaredTypes.Contains(typeName);
        }
        public TypeId GetTypeId(string typeName, Location location) {
            int index = DeclaredTypes.IndexOf(typeName);
            if (index == -1) {
                throw new CompilerException(ExceptionSource.TYPETABLE, location, "Trying to access undeclared type: " + typeName);
            }

            return new TypeId(index, DeclaredTypes[index]);
        }
        public bool IsCallable(TypeId type) {
            return type.Index == FunctionID.Index;
        }

        public CallableDeclaration GetCallable(string name, TypeId type, Location location) {
            if (type.Index != FunctionID.Index) {
                throw new CompilerException(ExceptionSource.TYPETABLE, location, "Only functions are callable, not: " + type.DebugName);
            }
            if (Functions.ContainsKey(name)) {
                return Functions[name];
            }
            throw new CompilerException(ExceptionSource.TYPETABLE, location, "Invalid callable: " + type.DebugName);
        }
    }
}
