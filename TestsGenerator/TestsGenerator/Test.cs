using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab4TestsGenerator.Core
{
    public class Test
    {
        public string ClassName { get; private set; }
        public string NamespaceName { get; private set; }

        public Test(string className, string namespaceName, SyntaxTree? tree)
        {
            ClassName = className;
            NamespaceName = namespaceName;
            _tree = tree;
        }

        private SyntaxTree? _tree;
        override public string ToString()
        {
            if (_tree == null)
            {
                return "null";
            }
            return _tree.ToString();
        }
    }
}
