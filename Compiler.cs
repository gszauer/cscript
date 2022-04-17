
namespace CScript {
    class Compiler {
        protected List<Token> mTokens;
        List<Pass0.Statement> mSerialized;

        public Compiler() {
            mTokens = new List<Token>();
            mSerialized = new List<Pass0.Statement>();
        }
        public void AddFile(string fileName, string fileContent) {
            Scanner scanner = new Scanner(fileName, fileContent);
            
            /*Console.WriteLine("Compiler.AddFile(" + fileName + "), Tokens:");
            foreach(Token t in scanner.Tokens) {
                Console.WriteLine(t.ToString());
            }*/

            // TODO: Run pre-processor here
            mTokens.AddRange(scanner.Tokens);
        }
        public void AddPreCompiled(string fileName, byte[] content) {
            throw new NotImplementedException();
        }
        public void DeclareExternalStructure(string structName) {
            throw new NotImplementedException();
        }
        public void DeclareExternalFunction(string returnType, string name, params string[] argumentTypes) {
            throw new NotImplementedException();
        }
        public void DeclareExternalGlobal(string type, string name) {
            throw new NotImplementedException();
        }

        public string CompileToJavascript() {
            AbstractSyntaxTree ast = CompileToFinalAST(); // run the full compiler
            // TODO: Translate to javascript
            throw new NotImplementedException();
        }
        public byte[] CompileToSingleBinary() {
            CompileToFinalAST(); // run the full compiler
            Parser parser = new Parser(mTokens);
            // Serialize the output of parser
            throw new NotImplementedException();
        }
        public AbstractSyntaxTree CompileToFinalAST() {
            mTokens.Add(new Token(TokenType.EOF, -1, "generated", "Expected EOF"));
            Parser parser = new Parser(mTokens);
            mTokens.RemoveAt(mTokens.Count - 1);

            List<Pass0.Statement> program = parser.Program;
            program.AddRange(mSerialized);

            // TODO: Other passes, lol

            return new AbstractSyntaxTree(program);
        }
    }
}