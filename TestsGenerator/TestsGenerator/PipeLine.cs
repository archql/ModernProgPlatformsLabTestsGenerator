using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace lab4TestsGenerator.Core
{
    public class PipeLine
    {
        TestsGenerator _testsGenerator;

        private int _maxReadingTask;
        private int _maxProcessingTask;
        private int _maxWritingTask;

        static readonly string extensionCS = ".cs";

        private struct StringPair
        {
            public string Name { get; init; }
            public string Value { get; init; }

            public StringPair(string name, string value)
            {
                Name = name;
                Value = value;
            }
        }

        public PipeLine(int maxReadingTask, int maxProcessingTask, int maxWritingTask)
        {
            _testsGenerator = new TestsGenerator();
            _maxReadingTask = maxReadingTask;
            _maxProcessingTask = maxProcessingTask;
            _maxWritingTask = maxWritingTask;
        }
        public PipeLine(int maxTask = 10) : this(maxTask, maxTask, maxTask)
        {

        }

        public async Task Process(string srcDir, string resDir)
        {
            // prep file work
            if (!Directory.Exists(srcDir))
            {
                throw new ArgumentException(srcDir, "Supposed to be an existing directory.");
            }
            if (!Directory.Exists(resDir))
            {
                Directory.CreateDirectory(resDir);
            }
            // 1) prepare dataflow
            var readFiles = new TransformBlock<string, StringPair>(
                async path => new StringPair(Path.GetFileName(path), await FileRead(path)),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxReadingTask });

            var processFiles = new TransformBlock<StringPair, List<StringPair>>(
                content => FileProcess(content),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxProcessingTask });

            var writeFiles = new ActionBlock<List<StringPair>>(
                async content => await FileWrite(content, resDir),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = _maxWritingTask });

            // link sep dataflows to sequence of dataflows
            var linkOptions = new DataflowLinkOptions();
            linkOptions.PropagateCompletion = true;

            readFiles.LinkTo(processFiles, linkOptions);
            processFiles.LinkTo(writeFiles, linkOptions);

            foreach (var filePath in Directory.GetFiles(srcDir))
            {
                readFiles.Post(filePath);
            }
            // task ready (no more changes)
            readFiles.Complete();
            // wait for completion
            await writeFiles.Completion;
        }

        private List<StringPair> FileProcess(StringPair srcFile)
        {
            List<StringPair> results = new List<StringPair>();
            List<Test>? tests;
            _testsGenerator.Generate(srcFile.Value, out tests);
            if (tests == null)
            {
                return results;
            }

            foreach (var testContent in tests)
            {
                var resultName = srcFile.Name + "_" + testContent.NamespaceName + "_" + testContent.ClassName;
                results.Add(new StringPair(resultName, testContent.ToString()));
            }
            return results;
        }
        private async Task<string> FileRead(string file)
        {
             string result;
             using (var sr = new StreamReader(file))
             {
                 result = await sr.ReadToEndAsync();
             }
             return result;
        }

        private async Task FileWrite(List<StringPair> files, string dirto)
        {
            foreach (var filedata in files)
            {
                var resultFilePath = dirto + Path.DirectorySeparatorChar + filedata.Name + extensionCS;
                using (var sw = new StreamWriter(resultFilePath))
                {
                    await sw.WriteAsync(filedata.Value);
                }
            }
        }
    }
}
