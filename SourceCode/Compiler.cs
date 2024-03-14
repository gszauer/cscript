namespace CScript {
    public class Compiler {
        public TypeDatabase TypeDatabase { get; protected set; }
        public Dictionary<string, ParseTree.Declaration.File> ParsedFiles { get; protected set; }

        public Compiler() {
            ParsedFiles = new Dictionary<string, ParseTree.Declaration.File>();
            this.TypeDatabase = new TypeDatabase(null);


            List<Token> scanned = Scanner.Scan("generated", CScript.Embedded.InternalCode);
            if (scanned == null) {
                Compiler.Error("Compiler", "Error scanning", new Location("generated", 0, 0));
            }

            List<ParseTree.Declaration.File> parsed = Parser.Parse(scanned, TypeDatabase);
            if (parsed == null || parsed.Count != 1) {
                Compiler.Error("Compiler", "Error parsing", new Location("generated", 0, 0));
            }

            ParseTree.Declaration.File generated = new ParseTree.Declaration.File("generated", parsed[0].Content);
            ParsedFiles.Add("generated", generated);
        }

        public void AddFile(string location, string code) {
            try {
                List<Token> scanned = Scanner.Scan(location, code);
                if (scanned == null) {
                    Compiler.Error("Compiler", "Error scanning", new Location("generated", 0, 0));
                }
                List<ParseTree.Declaration.File> parsed = Parser.Parse(scanned, TypeDatabase);
                if (parsed == null) {
                    Compiler.Error("Compiler", "Error parsing", new Location("generated", 0, 0));
                }

                foreach (ParseTree.Declaration.File file in parsed) {
                    ParsedFiles.Add(file.Path, file);
                }
            }
            catch(StopCompilingException e) {
                // Ignore
            }
        }

        public string BuildDebug() {
            try {
                List<ParseTree.Declaration.File> allFiles = new List<ParseTree.Declaration.File>(ParsedFiles.Values);
                TypeChecker typeChecker = new TypeChecker(allFiles, this.TypeDatabase);
                //FunctionGenerator functionGenerator = new FunctionGenerator(allFiles, typeChecker, this.TypeDatabase, typeChecker.ExpressionTypes);

                PrettyPrinter printer = new PrettyPrinter(allFiles);
                Javascript js = new Javascript(allFiles, this.TypeDatabase, typeChecker);
                //return this.TypeDatabase.ToString();
                //return printer.Result;
                return js.Result;
            }
            catch (StopCompilingException e) {
                // Ignore
            }

            return null;
        }

        public static void Error(string subsystem, string message, Location loc) {
            if (loc.File.EndsWith("_Bad.csc")) {
                throw new StopCompilingException(message);
            }

            Console.WriteLine("Error in " + subsystem );
            Console.WriteLine(message);
            Console.WriteLine("On line: " + loc.Line + ", column: " + loc.Column + ", file: " + loc.File);
            throw new NotImplementedException();
        }
    }

    public class StopCompilingException : Exception {
        public StopCompilingException(string message) : base(message) {
        }
    }
}
