using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {
    namespace Runtime {

        class StructureDeclaration : Function {
            public string Name { get; protected set; }
            public TypeId Type { get; protected set; }
            public List<string> VariableNames { get; protected set; }
            public List<TypeId> VariableTypes { get; protected set; }
            public List<Pass1.Expression> VariableInitializers { get; protected set; }

            public override TypeId ReturnType() {
                return Type;
            }
            public override List<TypeId> ArgumentTypes() {
                return VariableTypes;
            }
            public override object Call(Interpreter runtime, Environment env, List<object> args) {
                if (args.Count != 0) {
                    throw new InterpreterException(new Location(-1, "internal"), "Struct constructor can't have arguments");
                }
                List<object> vals = new List<object>();
                for (int i = 0; i < VariableNames.Count; ++i) {
                    if (VariableInitializers[i] == null) {
                        vals.Add(runtime.GetRuntimeValue(VariableTypes[i]));
                    }
                    else {
                        vals.Add(runtime.EvaluateExpression(VariableInitializers[i], env));
                    }
                }

                StructureInstance newStruct = new StructureInstance(this, vals);
                return newStruct;
            }

            public StructureDeclaration(Pass1.StructDeclStatement decl) {
                Name = decl.Name;
                Type = decl.Type;
                VariableNames = new List<string>();
                VariableTypes = new List<TypeId>();
                VariableInitializers = new List<Pass1.Expression>();
                foreach (Pass1.VarDeclStatement var in decl.Variables) {
                    VariableNames.Add(var.Name);
                    VariableTypes.Add(var.Type);
                    VariableInitializers.Add(var.Initializer);
                }
            }
        }
        class StructureInstance {
            public StructureDeclaration Declaration { get; protected set; }

            public string Name {
                get { return Declaration.Name; }
            }

            public TypeId Type {
                get { return Declaration.Type; }
            }
            public List<string> VariableNames {
                get {
                    return Declaration.VariableNames;
                }
            }
            public List<TypeId> VariableTypes {
                get {
                    return Declaration.VariableTypes;
                }
            }
            public Dictionary<string, object> VariableValues { get; protected set; }
            public StructureInstance(StructureDeclaration decl, List<object> values) {
                VariableValues = new Dictionary<string, object>();
                Declaration = decl;

                if (values.Count != decl.VariableNames.Count) {
                    throw new NotImplementedException();
                }

                for (int i = 0; i < values.Count; ++i) {
                    VariableValues[VariableNames[i]] = values[i];
                }
            }

        }
    }
}
