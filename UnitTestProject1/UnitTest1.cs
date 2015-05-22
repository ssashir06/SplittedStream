using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SplittedStream;
using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        #region ファイル結合
        
        [TestMethod]
        public void TestMethod_2ファイル()
        {
            var files = new[] { @"TestFiles\filepair\File1.txt", @"TestFiles\filepair\File2.txt" };
            using (var splittedStreamReadFiles = new SplittedStreamReadFiles(files))
            using (var textReader = new StreamReader(splittedStreamReadFiles, Encoding.UTF8))
            {
                var text = textReader.ReadToEnd();
                Console.WriteLine(text);

                Assert.AreEqual("this is a file 1.this is a file 2.", text);
            }
        }

        #endregion

        #region ファイル一覧

        void FileListTest(string original7z, string[] splitted7zFiles)
        {

            string fileNamesCorrect;
            string fileNamesSplitted;

            using (var fs = new FileStream(original7z, FileMode.Open, FileAccess.Read))
            using (var sevenzip = new SevenZip.SevenZipExtractor(fs))
            {
                var files = sevenzip.ArchiveFileNames;
                fileNamesCorrect = string.Join(Environment.NewLine, files);
                Console.WriteLine(fileNamesCorrect);
            }

            using (var fs = new SplittedStreamReadFiles(splitted7zFiles))
            using (var sevenzip = new SevenZip.SevenZipExtractor(fs))
            {
                var files = sevenzip.ArchiveFileNames;
                fileNamesSplitted = string.Join(Environment.NewLine, files);
                Console.WriteLine(fileNamesSplitted);
            }

            Debug.WriteLine("Correct:");
            Debug.WriteLine(fileNamesCorrect);
            Debug.WriteLine("Splitted:");
            Debug.WriteLine(fileNamesSplitted);

            Assert.AreEqual(fileNamesCorrect, fileNamesSplitted);
        }

        [TestMethod]
        public void TestMethod_7zファイル一覧1()
        {
            var original7zFile = @"TestFiles\sample1.7z";
            var splittedFiles = Directory.EnumerateFiles(@"TestFiles\splittedfiles_sample1.7z").ToArray();
            Array.Sort(splittedFiles);
            this.FileListTest(original7zFile, splittedFiles);
        }

        [TestMethod]
        public void TestMethod_7zファイル一覧２()
        {
            var original7zFile = @"TestFiles\sample2.7z";
            var splittedFiles = Directory.EnumerateFiles(@"TestFiles\splittedfiles_sample2.7z").ToArray();
            Array.Sort(splittedFiles);
            this.FileListTest(original7zFile, splittedFiles);
        }

        [TestMethod]
        public void TestMethod_7zファイル一覧３()
        {
            var original7zFile = @"TestFiles\sample3.7z";
            var splittedFiles = Directory.EnumerateFiles(@"TestFiles\splittedfiles_sample3.7z").ToArray();
            Array.Sort(splittedFiles);
            this.FileListTest(original7zFile, splittedFiles);
        }
        
        #endregion

        #region ファイル読み込み

        void RandomRead(string original, string[] splittedFiles, int trycount = 200)
        {
            var streamCorrect = new FileStream(original, FileMode.Open, FileAccess.Read);
            var streamSplitted = new SplittedStreamReadFiles(splittedFiles);

            var length = streamCorrect.Length;
            Assert.AreEqual(streamCorrect.Length, streamSplitted.Length);

            var rand = new Random((int)(DateTime.Now.Ticks >> 32));
            for (var i = 0; i < trycount; i++)
            {
                //var seekorigin = new[] { SeekOrigin.Begin, SeekOrigin.Current, SeekOrigin.End }[rand.Next(0, 2)];
                var seekpos = rand.Next(0, (int)length - 1);
                var count = rand.Next(0, (int)(length - seekpos - 1));
                var offset = rand.Next(0, 5) * 2;

                var bufferCorrect = new byte[count + offset];
                var bufferSplitted = new byte[count + offset];

                streamCorrect.Seek(seekpos, SeekOrigin.Begin);
                streamSplitted.Seek(seekpos, SeekOrigin.Begin);
                Assert.AreEqual(streamCorrect.Position, streamSplitted.Position);

                Debug.WriteLine(string.Format("{0}: Read {1} bytes", i, count));

                streamCorrect.Read(bufferCorrect, offset, count);
                streamSplitted.Read(bufferSplitted, offset, count);

                for (int j = 0; j < bufferCorrect.Count(); j++)
                {
                    Assert.AreEqual(bufferCorrect[j], bufferSplitted[j]);
                }

            }
        }

        [TestMethod]
        public void TestMethod_Read1()
        {
            var original7zFile = @"TestFiles\sample1.7z";
            var splittedFiles = Directory.EnumerateFiles(@"TestFiles\splittedfiles_sample1.7z").ToArray();
            Array.Sort(splittedFiles);
            this.RandomRead(original7zFile, splittedFiles);
        }

        [TestMethod]
        public void TestMethod_Read2()
        {
            var original7zFile = @"TestFiles\sample2.7z";
            var splittedFiles = Directory.EnumerateFiles(@"TestFiles\splittedfiles_sample2.7z").ToArray();
            Array.Sort(splittedFiles);
            this.RandomRead(original7zFile, splittedFiles);
        }

        [TestMethod]
        public void TestMethod_Read3()
        {
            var original7zFile = @"TestFiles\sample3.7z";
            var splittedFiles = Directory.EnumerateFiles(@"TestFiles\splittedfiles_sample3.7z").ToArray();
            Array.Sort(splittedFiles);
            this.RandomRead(original7zFile, splittedFiles, 30);
        }
        
        #endregion
    }
}
