using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SplittedStream
{
    public abstract class SplittedStreamAbstract : Stream, IDisposable
    {
        IList<Stream> _streams = null;
        int _currentStreamIdx = 0;
        object _streamLocker = new object();

        protected SplittedStreamAbstract(IList<Stream> streams)
        {
            this._streams = streams;
        }

        private Stream CurrentStream { get { return this._streams[this._currentStreamIdx]; } }

        public override bool CanRead
        {
            get { return this._streams.All(X => X.CanRead); }
        }

        public override bool CanSeek
        {
            get { return this._streams.All(X => X.CanSeek); }
        }

        public override bool CanWrite
        {
            get { return this._streams.All(X => X.CanWrite); }
        }

        public override void Flush()
        {
            Task.WaitAll(this._streams.Select(X => X.FlushAsync()).ToArray());
        }

        public override long Length
        {
            get { return this._streams.Sum(X => X.Length); }
        }

        public override long Position
        {
            get
            {
                lock (_streamLocker)
                {
                    long p = this.CurrentStream.Position;
                    for (int i = 0; i < _currentStreamIdx; i++)
                    {
                        p += this._streams[i].Length;
                    }
                    return p;
                }
            }
            set
            {
                lock (_streamLocker)
                {
                    long p = 0;
                    int i = 0;
                    while (p + this._streams[i].Length < value)
                    {
                        p += this._streams[i].Length;
                        i++;
                    }
                    this._currentStreamIdx = i;
                    this.CurrentStream.Position = value - p;
                }
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (this._streamLocker)
            {
                var ct = 0;
                while (ct < count)
                {
                    var pos = this.CurrentStream.Position;
                    var len = (int)Math.Min(this.CurrentStream.Length - pos, (long)(count - ct));
                    this.CurrentStream.Read(buffer, ct + offset, len);
                    if (pos + len == this.CurrentStream.Length)
                    {
                        if (this._currentStreamIdx == this._streams.Count - 1)
                        {
                            return ct + len;
                        }

                        this._currentStreamIdx++;
                        this.CurrentStream.Position = 0;
                    }
                    ct += len;
                }
                return ct;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (this._streamLocker)
            {
                long p = offset;
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        p += 0;
                        if (offset < 0) p = 0;
                        break;
                    case SeekOrigin.Current:
                        p += this.Position;
                        if (offset < this.Position) p = 0;
                        else if (offset > this.Length) p = this.Length - 1;
                        break;
                    case SeekOrigin.End:
                        p += this.Length;
                        if (offset > 0) p = this.Length - 1;
                        break;
                }

                this.Position = p;
                return p;
            }
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (this._streamLocker)
            {
                var ct = 0;
                while (ct < count)
                {
                    var pos = this.CurrentStream.Position;
                    var len = (int)Math.Min(this.CurrentStream.Length - pos, (long)(count - ct));
                    this.CurrentStream.Write(buffer, ct + offset, len);
                    if (pos + len == this.CurrentStream.Length)
                    {
                        if (this._currentStreamIdx == this._streams.Count - 1)
                        {
                            throw new Exception("End of stream");
                        }

                        this._currentStreamIdx++;
                        this.CurrentStream.Position = 0;
                    }
                    ct += len;
                }
            }

        }

        #region IDisposable メンバー

        void IDisposable.Dispose()
        {
            lock (this._streamLocker)
            {
                foreach (var stream in _streams)
                {
                    stream.Dispose();
                }
                this._streams.Clear();
                this._streams = null;
            }
        }

        #endregion
    }
}
