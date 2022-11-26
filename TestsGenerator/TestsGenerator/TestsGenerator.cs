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
            tests = new List<Test>();
            // get all namespaces
            var namespaces = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            var fileScopedNamespaces = root.DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>();
            bool hasFilescopedNamespace = false;
            FileScopedNamespaceDeclarationSyntax fileScopedNamespace = null;
            foreach (var fsn in fileScopedNamespaces)
            {
                if (fsn != null)
                {
                    Console.WriteLine("wewdeswe");
                    fileScopedNamespace = SyntaxFactory.FileScopedNamespaceDeclaration(
                        SyntaxFactory.QualifiedName(fsn.Name, SyntaxFactory.IdentifierName(NamespaceName)));
                    hasFilescopedNamespace = true;
                }
            }
            // get all using directives and classes declarations
            List<UsingDirectiveSyntax> usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().
                Where(u => !u.StaticKeyword.HasTrailingTrivia).ToList();
            // add them to using
            foreach(var n in namespaces)
            {
                usings.Add(SyntaxFactory.UsingDirective(n.Name));
            }
            // add defaults to usings
            //_usings.Add(
            //    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System"))
            //);
            //_usings.Add(
            //    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Collections.Generic"))
            //);
            //_usings.Add(
            //    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Linq"))
            //);
            //_usings.Add(
            //    SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("System.Text"))
            //);
            usings.Add(
                SyntaxFactory.UsingDirective(SyntaxFactory.IdentifierName("NUnit.Framework"))
            );
            // generate tests for all classes
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where(_class => _class.Modifiers.Any(SyntaxKind.PublicKeyword))
                .Where(_class => ! _class.Modifiers.Any(SyntaxKind.StaticKeyword));

            foreach (var c in classes)
            {
                tests.Add(GenerateTest(c, usings, fileScopedNamespace, hasFilescopedNamespace));
            }
            // eok
            return true;
        }

        private Test GenerateTest(ClassDeclarationSyntax classDeclaration, List<UsingDirectiveSyntax> _usings, FileScopedNamespaceDeclarationSyntax fileScopedNamespace, bool hasFilescopedNamespace)
        {
            var className = classDeclaration.Identifier.Text;
            // methods
            var members = new List<MemberDeclarationSyntax>();
            var sourceMethods = classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .Where(sourceMethod => sourceMethod.Modifiers.Any(SyntaxKind.PublicKeyword)).ToList();

            Dictionary<string, int> map = new Dictionary<string, int>();
            foreach (var method in sourceMethods)
            {
                string name = method.Identifier.ValueText + "Test";
                if (map.ContainsKey(method.Identifier.ValueText))
                {
                    map[method.Identifier.ValueText]++;
                    name += map[method.Identifier.ValueText];
                }
                else
                {
                    map.Add(method.Identifier.ValueText, 0);
                }

                var memAttrList = SyntaxFactory.SingletonList(SyntaxFactory.AttributeList(SyntaxFactory.SingletonSeparatedList(SyntaxFactory.Attribute(SyntaxFactory.IdentifierName("Test")))));
                var memModfList = SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword));
                var memBody = SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName("Assert"), SyntaxFactory.IdentifierName("Fail")
                            )
                        )
                        .WithArgumentList(
                            SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(
                                            SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal("autogenerated stub")))))
                        )));
                members.Add(SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), name)
                            .WithAttributeLists(memAttrList)
                            .WithModifiers(memModfList)
                            .WithBody(memBody));
            }
            // 
            MemberDeclarationSyntax classDecl = SyntaxFactory.ClassDeclaration($"TestOf{className}")
                                    .WithAttributeLists(
                                        SyntaxFactory.SingletonList(
                                            SyntaxFactory.AttributeList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Attribute(
                                                        SyntaxFactory.IdentifierName("TestFixture"))))))
                                    .WithModifiers(
                                        SyntaxFactory.TokenList(
                                            SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                                    .WithMembers(
                                        SyntaxFactory.List(members));
            //


            // using ...
            NamespaceDeclarationSyntax? currNamespace = classDeclaration.Parent as NamespaceDeclarationSyntax;

            SyntaxTree tree; string namespaceName = "";
            if (hasFilescopedNamespace)
            {
                tree = CSharpSyntaxTree.Create(
                    SyntaxFactory.CompilationUnit()
                        .WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(fileScopedNamespace
                        .WithUsings(SyntaxFactory.List(_usings))
                        .AddMembers(classDecl)
                        )).NormalizeWhitespace()
                    );
                namespaceName = fileScopedNamespace.Name.ToString();
            } 
            else
            {
                if (currNamespace != null)
                {
                    namespaceName = currNamespace.Name.ToString();
                }
                tree = CSharpSyntaxTree.Create(
                    SyntaxFactory.CompilationUnit()
                        .WithUsings(SyntaxFactory.List(_usings))
                        .AddMembers(SyntaxFactory.NamespaceDeclaration(
                                SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName(namespaceName), SyntaxFactory.IdentifierName(NamespaceName)))
                            .AddMembers(classDecl))
                        .NormalizeWhitespace()
                    );
            }

            //
            return new Test(className, namespaceName, tree);
        }

    }
}