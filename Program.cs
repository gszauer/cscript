
using CScript;
using CScript.Runtime;

public class Program {
    public static void Main(string[] args) {
        try {
            Compiler compiler = new Compiler();
            compiler.AddFile("../../../test.txt", File.ReadAllText("../../../test.txt"));
            AbstractSyntaxTree ast = compiler.CompileToFinalAST();

            Interpreter interpreter = new Interpreter(ast);
            //interpreter.RunFunction("run");
        }
        catch (CompilerException e) {
            Console.WriteLine(e.Message);
        }
        catch (InterpreterException e) {
            Console.WriteLine(e.Message);
        }

        Console.WriteLine("\nPress <return> to continue...");
        Console.ReadLine();
    }
}