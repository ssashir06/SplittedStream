using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplittedStream;
using System.IO;
using System.Text;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var files = new[]{@"..\..\TestFiles\File1.txt", @"..\..\TestFiles\File2.txt"};
            using (var splittedStreamReadFiles = new SplittedStreamReadFiles(files))
            using (var textReader = new StreamReader(splittedStreamReadFiles, Encoding.UTF8))
            {
                var text = textReader.ReadToEnd();
                Console.WriteLine(text);
            }
        }
    }
}
