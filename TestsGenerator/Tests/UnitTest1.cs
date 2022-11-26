using NUnit.Framework;

using lab4TestsGenerator.Core;
using lab4TestsGenerator.ConsoleApp;
using System.Threading.Tasks;

namespace lab4TestsGenerator.Tests
{
    public class Tests
    {

        private string[] args;
        PipeLine? p;

        [SetUp]
        public async Task Setup()
        {
            args = new string[]
            {
                "@tests/src/",
                "@tests/res/",
                "3"
            };
            PipeLine? p = Program.GetPipeLine(args, out var srcDir, out var resDir);
            await p.Process(srcDir, resDir);
        }

        [Test]
        public void Test1()
        {
            
        }
    }
}