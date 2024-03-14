using CScript;
public partial class Program {

    //[Bootsharp.JSEvent] // Used in JS as Program.onMainInvoked.subscribe(..)
    //public static partial void OnMainInvoked (string message);

    public static void Main(string[] args) {
        string testFilesPath = null;

        for (int i = 0; i < args.Length - 1; i++) {
            if (args[i] == "-tests") {
               testFilesPath = args[++i];
            }
        }

#if DEBUG
        if (testFilesPath == null) {
            testFilesPath = "C:\\Users\\Gabor\\Desktop\\CScript\\tests";
        }
#endif

        if (testFilesPath != null) {
            RunTests(testFilesPath);
        }
    }
    protected static void RunTests(string tests) {
        string parentFolder = tests.Substring(0, tests.LastIndexOf("\\"));
        string results = parentFolder + "\\results";

        if (!Directory.Exists(tests)) {
            Directory.CreateDirectory(tests);
        }

        if (Directory.Exists(results)) {
            Directory.Delete(results, true);
        }
        Directory.CreateDirectory(results);

        var testFiles = Directory.EnumerateFiles(tests);
        foreach (string testPath in testFiles) {
            if (testPath.EndsWith(".csc")) {
                RunFileTest(testPath, results);
            }
        }

        // TODO: Folders
    }

    protected static void RunFileTest(string filePath, string results) {
        string fileName = filePath.Substring(filePath.LastIndexOf('\\'));
        string tests = filePath.Substring(0, filePath.LastIndexOf("\\"));

        Compiler comp = new Compiler();
        string result = null;
        if (fileName.EndsWith("_Good.csc")) {
            comp.AddFile(filePath, File.ReadAllText(filePath));
            result = comp.BuildDebug();
        }
        else if (fileName.EndsWith("_Bad.csc")) {
            // TODO: Handle this, i'm sure it will fail later
            comp.AddFile(filePath, File.ReadAllText(filePath));
            result = comp.BuildDebug();
        }

        if (result != null) {
            string newName = results + fileName.Replace(".csc", ".js");
            if (File.Exists(newName)) {
                File.Delete(newName);
            }
            
            File.WriteAllText(newName, result);
        }
    }
}