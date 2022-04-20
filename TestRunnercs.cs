using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CScript;
using CScript.Runtime;

namespace CScript {
    class TestRunner {

        List<string> mDirectories;
        bool PrintToScreen = false;

        public TestRunner() {
            mDirectories = new List<string>();
        }
        public void AddDirectory(string directory) {
            if (!mDirectories.Contains(directory)) {
                mDirectories.Add(directory);
            }
        }

        protected void SortListOfFiles(List<string> files) {
            files.Sort(delegate (string s1, string s2) {
                int lastSlash = s1.LastIndexOf("/") + 1;
                int lastDot = s1.LastIndexOf(".");
                s1 = s1.Substring(lastSlash, lastDot - lastSlash);
                lastSlash = s2.LastIndexOf("/") + 1;
                lastDot = s2.LastIndexOf(".");
                s2 = s2.Substring(lastSlash, lastDot - lastSlash);
                return int.Parse(s1).CompareTo(int.Parse(s2)) * -1;
            });
        }

        public bool Run() {
            PrintToScreen = true; // The first file, in the first directory will ouput to screen
            
            foreach (string directory in mDirectories) {
                if (Directory.Exists(directory)) {
                    List<string> files = new List<string>();
                    string[] _files = Directory.GetFiles(directory);

                    foreach (string file in _files) {
                        if (file.EndsWith(".txt") && !file.EndsWith("_result.txt") && !file.EndsWith("_old.txt")) {
                            if (File.Exists(file)) {
                                files.Add(file.Replace("\\", "/"));
                            }
                            else {
                                throw new NotImplementedException();
                            }
                        }
                    }

                    SortListOfFiles(files);
                    foreach (string file in files) {
                        Run(file);

                        if (PrintToScreen) {
                            Console.WriteLine();
                        }
                        PrintToScreen = false;
                    }
                }
                else {
                    throw new NotImplementedException();
                }
            }

            return true;
        }
        bool Run(string fileName) {
            //Console.WriteLine("running test: " + fileName);
            
            fileName = fileName.Replace(".txt", "");

            if (!File.Exists(fileName + ".txt")) {
                PrintError("Can't run test " + fileName + ".txt, source file not found");
                return false;
            }
            if (!File.Exists(fileName + "_result.txt")) {
                PrintError("Result file " + fileName + "_result.txt doesn't exist");
                if (!OfferToCreateFile(fileName + "_result.txt")) {
                    return false;
                }
            }

            string code = File.ReadAllText(fileName + ".txt");
            string expected = File.ReadAllText(fileName + "_result.txt").Replace("\r\n", "\n");
            string result = string.Empty;

            bool runtimeError = false;
            try {
                Compiler compiler = new Compiler();
                compiler.AddFile(fileName + ".txt", code);
                AbstractSyntaxTree ast = compiler.CompileToFinalAST();

                Interpreter interpreter = new Interpreter(ast);
                interpreter.UseCustomPrint((string message) => {
                    result += message;
                });
                interpreter.RunFunction("run");
            }
            catch (CompilerException e) {
                result = e.Message;
            }
            catch (InterpreterException e) {
                result = e.Message;
                runtimeError = true;
            }

            if (PrintToScreen) {
                ConsoleColor bg = Console.BackgroundColor;
                if (runtimeError) {
                    Console.BackgroundColor = ConsoleColor.Yellow;
                }
                PrintLine(result);
                if (runtimeError) {
                    Console.BackgroundColor = bg;
                }
            }

            if (expected == result) {
                //PrintLine("Test: " + fileName + ".txt -> Success");
            }
            else {
                PrintError("Test Failed: " + fileName + ".txt -> FAIL");
                OfferToReplaceFile(fileName + "_result.txt", result);
            }

            return expected == result;
        }

        private void PrintLine(string msg) {
            Console.WriteLine(msg);
        }

        private void PrintError(string msg) {
            ConsoleColor bg = Console.BackgroundColor;
            Console.BackgroundColor = ConsoleColor.Red;

            Console.WriteLine(msg);

            Console.BackgroundColor = bg;
        }

        private bool OfferToReplaceFile(string file, string newContent) {
            Console.WriteLine("\nNew content:");
            Console.WriteLine(newContent + "\n");
            Console.WriteLine("Old content:");
            Console.WriteLine(File.ReadAllText(file) + "\n");
            Console.WriteLine("Replace: " + file + "? (y / n)");
            string input = Console.ReadLine();

            if (input.Length == 1 && input[0] == 'y') {
                if (File.Exists(file)) {
                    string backup = file.Replace("_result.txt", "_old.txt");
                    File.WriteAllText(backup, File.ReadAllText(file));
                }
                File.WriteAllText(file, newContent);
                Console.WriteLine(file + " replaced.\n");
                return true;
            }
            return false;
        }
        private bool OfferToCreateFile(string file) {
            Console.WriteLine("Create: " + file + "? (y / n)");
            string input = Console.ReadLine();

            if (input.Length == 1 && input[0] == 'y') {
                File.WriteAllText(file, string.Empty);
                Console.WriteLine(file + " created.\n");
                return true;
            }
            return false;
        }
    }
}
