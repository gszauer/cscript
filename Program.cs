
using CScript;
using CScript.Runtime;

public class Program {
    public static void Main(string[] args) {

        TestRunner testRunner = new TestRunner();
        testRunner.AddDirectory("../../../tests");
        testRunner.AddDirectory("../../../tests/bad");
        testRunner.Run();

        Console.WriteLine("\nPress <return> to continue...");
        Console.ReadLine();
    }
}