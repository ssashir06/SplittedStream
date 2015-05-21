using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace SplittedStream
{
    public class SplittedStreamReadFiles : SplittedStreamAbstract
    {
        public SplittedStreamReadFiles(string[] files)
            : base(files.Select(X => (Stream)new FileStream(X, FileMode.Open, FileAccess.Read)).ToList())
        {
            if (!files.Any())
            {
                throw new ArgumentException();
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
