using NUnit.Framework;

using lab4TestsGenerator.Core;
using lab4TestsGenerator.ConsoleApp;
using System.Threading.Tasks;
using System;
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace lab4TestsGenerator.Tests
{
    public class Tests
    {

        private string[] args;
        PipeLine? p;
        string srcDir, resDir;

        [SetUp]
        public async Task Setup()
        {
            args = new string[]
            {
                "@tests/src/",
                "@tests/res/",
                "3"
            };
            p = Program.GetPipeLine(args, out srcDir, out resDir);
            await p.Process(srcDir, resDir);
        }

        [Test]
        public async Task GenerateWrong()
        {
            string[] args = new string[]
            {
                "@tests/src/43232/32222222222222222222222",
                "@tests/res/ewew/",
                "3"
            };
            try
            {
                PipeLine? p = Program.GetPipeLine(args, out var srcDir, out var resDir);
                await p.Process(srcDir, resDir);
            }
            catch (ArgumentException)
            {
                Assert.Pass();
            }

            Assert.Fail("why passed?");
        }

        [Test]
        public void GenerateCheckDir()
        {
            var dirInfo = new DirectoryInfo(resDir);
            var filesConut = dirInfo.GetFiles("*.*", SearchOption.TopDirectoryOnly).Length;

            Assert.That(filesConut, Is.EqualTo(3));
        }

        [Test]
        public void GenerateReturns3TestClassesWithFilescopedNamespace()
        {
            TestsGenerator testsGenerator = new TestsGenerator();
            var isOk = testsGenerator.Generate(SourceCode.ClassB,
                out var tests);
            Console.WriteLine();

            Assert.That(isOk, Is.True);
            Assert.That(tests.Count, Is.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(tests[0].NamespaceName, Is.EqualTo("MyCode.HelloTestsGenerator"));
                Assert.That(tests[1].NamespaceName, Is.EqualTo("MyCode.HelloTestsGenerator"));
                Assert.That(tests[2].NamespaceName, Is.EqualTo("MyCode.HelloTestsGenerator"));
            });
        }

        [Test]
        public void GenerateReturns3TestClasses()
        {
            TestsGenerator testsGenerator = new TestsGenerator();
            var isOk = testsGenerator.Generate(SourceCode.ClassA,
                out var tests);
            Console.WriteLine();

            Assert.That(isOk, Is.True);
            Assert.That(tests.Count, Is.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(tests[0].NamespaceName, Is.EqualTo("MyCode.HelloTestsGenerator"));
                Assert.That(tests[1].NamespaceName, Is.EqualTo("MyCode.HelloTestsGenerator"));
                Assert.That(tests[2].NamespaceName, Is.EqualTo("MyCode.HelloTestsGenerator"));
            });
        }

        [Test]
        public void Multitester()
        {
            string sourceStr;
            using (var sr = new StreamReader("@tests/res/A.cs_lab3DirectoryScanner.DirectoryScanner.HelloTestsGenerator_A.cs"))
            {
                sourceStr = sr.ReadToEnd();
            }

            Assert.IsNotNull(sourceStr);
            Assert.IsNotEmpty(sourceStr);

            var tree = CSharpSyntaxTree.ParseText(sourceStr);
            var root = tree.GetCompilationUnitRoot();

            var sourceFileScopedNamespaces = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();
            if (sourceFileScopedNamespaces.Count() == 0)
            {
                Assert.Fail();
            }

            foreach (var sourceFileScopedNamespace in sourceFileScopedNamespaces)
            {
                var namespaceName = sourceFileScopedNamespace.Name.ToString();
                if (!(namespaceName == "lab3DirectoryScanner.DirectoryScanner.HelloTestsGenerator"))
                {
                    Assert.Fail();
                }
            }

            // generate tests for all classes
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(_class => _class.Modifiers.Any(SyntaxKind.PublicKeyword))
                .Where(_class => !_class.Modifiers.Any(SyntaxKind.StaticKeyword));

            Assert.That(classes.Count, Is.EqualTo(1));

            // get all using directives and classes declarations
            List<UsingDirectiveSyntax> usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().
                Where(u => !u.StaticKeyword.HasTrailingTrivia).ToList();

            Assert.That(usings.Count, Is.EqualTo(8));

            var sourceMethods = root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(sourceMethod => sourceMethod.Modifiers.Any(SyntaxKind.PublicKeyword)).ToList();
            Assert.That(sourceMethods.Count, Is.EqualTo(5));



            Assert.Pass();
        }


        public class SourceCode
        {
            public static readonly string ClassA =
            @"using System;
            namespace MyCode
            {
                public class MyClass
                {
                    public void Method(int a)
                    {
                        Console.WriteLine(""Method (int)"");
                    }
                    public void Method(double a)
                    {
                        Console.WriteLine(""Method (double)"");
                    }
                }
                public class MyClassB
                {
                    public void Method(int a)
                    {
                        Console.WriteLine(""Method (int)"");
                    }
                    public void Method(double a)
                    {
                        Console.WriteLine(""Method (double)"");
                    }
                }
                public class MyClassC
                {

                }
            }";
            public static readonly string ClassB =
            @"using System;
            namespace MyCode;
            
                public class MyClass
                {
                    public void Method(int a)
                    {
                        Console.WriteLine(""Method (int)"");
                    }
                    public void Method(double a)
                    {
                        Console.WriteLine(""Method (double)"");
                    }
                }
                public class MyClassB
                {
                    public void Method(int a)
                    {
                        Console.WriteLine(""Method (int)"");
                    }
                    public void Method(double a)
                    {
                        Console.WriteLine(""Method (double)"");
                    }
                }
                public class MyClassC
                {

                }
            ";
        }
    }
}