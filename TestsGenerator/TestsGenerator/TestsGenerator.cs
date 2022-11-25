using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace lab4TestsGenerator.Core
{
    public class TestsGenerator
    {
        private static readonly string NamespaceName = "HelloTestsGenerator";
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
            _usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
            _usings.Add(
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("NUnit.Framework"))
            );
            // generate tests for all classes
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(_class => _class.Modifiers.Any(SyntaxKind.PublicKeyword))
                .Where(_class => ! _class.Modifiers.Any(SyntaxKind.StaticKeyword));

            tests = classes.Select(GenerateTest).ToList();
            // eok
            return true;
        }

        private List<UsingDirectiveSyntax> _usings;

        private Test GenerateTest(ClassDeclarationSyntax classDeclaration)
        {
            

            //
            return new Test(className, tree);
        }

    }
}