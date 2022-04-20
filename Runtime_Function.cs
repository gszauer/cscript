using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CScript {
    namespace Runtime { 
        abstract class Function {
            abstract public TypeId ReturnType();
            abstract public List<TypeId> ArgumentTypes();
            abstract public object Call(Interpreter runtime, Environment env, List<object> args);
        }

        class UserFunction : Function {
            protected TypeId mReturnType;
            protected List<TypeId> mArgTypes;
            protected List<string> mArgNames;
            protected List<Pass1.Statement> mFunctionBody;
            override public TypeId ReturnType() {
                return mReturnType;
            }
            override public List<TypeId> ArgumentTypes() {
                return mArgTypes;
            }

            public UserFunction(Pass1.FunDeclStatement declaration) {
                mReturnType = declaration.ReturnType;
                mArgTypes = new List<TypeId>();
                mArgNames = new List<string>();
                foreach (Pass1.FunParamater param in declaration.Paramaters) {
                    mArgNames.Add(param.Name);
                    mArgTypes.Add(param.Type);
                }
                mFunctionBody = declaration.Body;
            }

            override public object Call(Interpreter runtime, Environment env, List<object> args) {
                Environment functionEnv = new Environment(env);
                if (args.Count != mArgNames.Count) {
                    throw new InterpreterException(new Location(-1, "internal"), "Calling function with wrong number of arguments");
                }

                for(int i = 0; i < args.Count; ++i) {
                    functionEnv.Declare(mArgNames[i], new Location(-1, "runtime"));
                    functionEnv.Set(mArgNames[i], args[i], new Location(-1, "runtime"));
                }

                foreach(Pass1.Statement s in mFunctionBody) {
                    StatementResult result = runtime.ExecuteStatement(s, functionEnv);
                    if (result.Type == StatementResultType.RETURN) {
                        return result.Return;
                    }
                    if (result.Type != StatementResultType.NORMAL) {
                        throw new NotImplementedException();
                    }
                }

                return null;
            }
        }
    }
}
