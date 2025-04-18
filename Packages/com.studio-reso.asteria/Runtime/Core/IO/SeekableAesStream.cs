using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Asteria
{
    public class SeekableAesStream : Stream
    {
        private const int IterationCount = 1000;
        private const int KeySize = 128;

        private readonly Stream baseStream;
        private readonly AesManaged aes;
        private readonly ICryptoTransform encryptor;
        private readonly int encryptionLength;
        private readonly byte[] outBuffer;
        private readonly byte[] nonce;
        private bool disposed;

        public SeekableAesStream(Stream baseStream, string password, byte[] salt, int encryptionLength = int.MaxValue)
        {
            this.baseStream = baseStream;
            this.encryptionLength = encryptionLength;

            using var key = new Rfc2898DeriveBytes(password, salt, IterationCount);
            aes = new AesManaged
            {
                KeySize = KeySize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7,
            };
            aes.Key = key.GetBytes(aes.KeySize / 8);
            aes.IV = new byte[16];

            encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            outBuffer = new byte[aes.BlockSize / 8];
            nonce = new byte[aes.BlockSize / 8];
        }

        private void Cipher(byte[] buffer, int offset, int count, long streamPos)
        {
            if (streamPos >= encryptionLength)
            {
                return;
            }

            if (streamPos + count > encryptionLength)
            {
                count = (int) (encryptionLength - streamPos);
            }

            var blockSizeInByte = aes.BlockSize / 8;
            var blockNumber = streamPos / blockSizeInByte + 1;
            var keyPos = streamPos % blockSizeInByte;

            for (var i = offset; i < offset + count; i++)
            {
                if (keyPos == 0)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(blockNumber), 0, nonce, 0, sizeof(long));
                    encryptor.TransformBlock(nonce, 0, nonce.Length, outBuffer, 0);
                    blockNumber++;
                }

                buffer[i] ^= outBuffer[keyPos];
                keyPos++;

                if (keyPos == blockSizeInByte)
                {
                    keyPos = 0;
                }
            }
        }

        public override bool CanRead => baseStream.CanRead;
        public override bool CanSeek => baseStream.CanSeek;
        public override bool CanWrite => baseStream.CanWrite;
        public override long Length => baseStream.Length;

        public override long Position
        {
            get => baseStream.Position;
            set => baseStream.Position = value;
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public override void SetLength(long value)
        {
            baseStream.SetLength(value);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return baseStream.Seek(offset, origin);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var streamPos = Position;
            var ret = baseStream.Read(buffer, offset, count);

            if (streamPos < encryptionLength)
            {
                Cipher(buffer, offset, ret, streamPos);
            }

            return ret;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var streamPos = Position;
            var ret = await baseStream.ReadAsync(buffer, offset, count, cancellationToken);

            if (streamPos < encryptionLength)
            {
                Cipher(buffer, offset, ret, streamPos);
            }

            return ret;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var streamPos = Position;

            if (streamPos < encryptionLength)
            {
                Cipher(buffer, offset, count, streamPos);
            }

            baseStream.Write(buffer, offset, count);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var streamPos = Position;

            if (streamPos < encryptionLength)
            {
                Cipher(buffer, offset, count, streamPos);
            }

            await baseStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    encryptor?.Dispose();
                    aes?.Dispose();
                }
                disposed = true;
            }

            base.Dispose(disposing);
            GC.SuppressFinalize(this); // ファイナライゼーションを抑制
        }
    }
}
