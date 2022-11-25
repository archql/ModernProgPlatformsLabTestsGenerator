using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace lab4TestsGenerator.Core
{
    public class TestsGenerator
    {
        public bool Generate(string src, out List<Test>? tests)
        {
            tests = null;
            // parse input
            var root = CSharpSyntaxTree.ParseText(src).GetCompilationUnitRoot();
            if (root == null)
            {
                return false;
            }
            // get all using directives and classes declarations
            var srcUsings = root.DescendantNodes().OfType<UsingDirectiveSyntax>();
            var srcClasses = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(resultClass => resultClass.Modifiers.Any(SyntaxKind.PublicKeyword)).ToList();
            // generate tests for all classes
            foreach (var srcClass in srcClasses)
            {

            }
            // eok
            return true;
        }

        public bool Save(string path)
        {

        }

        public bool Wait()
        {

        }

        public bool Stop()
        {

        }


    }
}