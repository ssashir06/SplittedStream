using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplittedStream;
using System.IO;
using System.Text;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod_2ファイル()
        {
            var files = new[] { @"TestFiles\File1.txt", @"TestFiles\File2.txt" };
            using (var splittedStreamReadFiles = new SplittedStreamReadFiles(files))
            using (var textReader = new StreamReader(splittedStreamReadFiles, Encoding.UTF8))
            {
                var text = textReader.ReadToEnd();
                Console.WriteLine(text);

                Assert.AreEqual("this is a file 1.this is a file 2.", text);
            }
        }

        [TestMethod]
        public void TestMethod_7zファイル一覧()
        {
            var sz = @"TestFiles\sample1.7z";

            string fileNamesCorrect;
            string fileNamesTest;

            using (var fs = new FileStream(sz, FileMode.Open, FileAccess.Read))
            using (var sevenzip = new SevenZip.SevenZipExtractor(fs))
            {
                var files = sevenzip.ArchiveFileNames;
                fileNamesCorrect = string.Join(Environment.NewLine, files);
                Console.WriteLine(fileNamesCorrect);
            }

            var splitedFiles = Directory.EnumerateFiles(@"TestFiles\splittedfiles_sample1.7z").ToArray();
            Array.Sort(splitedFiles);
            using (var fs = new SplittedStreamReadFiles(splitedFiles))
            using (var sevenzip = new SevenZip.SevenZipExtractor(fs))
            {
                var files = sevenzip.ArchiveFileNames;
                fileNamesTest = string.Join(Environment.NewLine, files);
                Console.WriteLine(fileNamesTest);
            }

            Assert.AreEqual(fileNamesCorrect, fileNamesTest);
        }

    }
}
