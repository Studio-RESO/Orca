using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Asteria
{
    public class ProgressStream : Stream
    {
        private readonly Stream stream;
        private readonly IProgress<double> progress;
        private readonly long totalLength;
        private long totalRead;

        public ProgressStream(Stream stream, IProgress<double> progress, long totalLength)
        {
            this.stream = stream;
            this.progress = progress;
            this.totalLength = totalLength;
        }

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;
        public override long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bytesRead = stream.Read(buffer, offset, count);
            ReportProgress(bytesRead);

            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var bytesRead = await stream.ReadAsync(buffer, offset, count, cancellationToken);
            ReportProgress(bytesRead);

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            stream.Write(buffer, offset, count);
        }

        private void ReportProgress(int bytesRead)
        {
            if (bytesRead > 0)
            {
                totalRead += bytesRead;
                var progressPercentage = (double) totalRead / totalLength * 100;
                progress.Report(progressPercentage);
            }
        }
    }

}
