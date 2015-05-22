# SplittedStream
A C# Stream Read and Write Splitted Files
THIS PROJECT IS CURRENTLY IN WORKING.

## Usage
Read/Write splitted huge files without cat command!


## Sample code
reading splitted files with splittedStreamReadFiles class.
```c#
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

```
