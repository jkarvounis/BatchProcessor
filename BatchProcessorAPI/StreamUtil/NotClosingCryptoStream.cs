using System.IO;
using System.Security.Cryptography;

namespace BatchProcessorAPI.StreamUtil
{
    /// <summary>
    /// Thank you CyberBasti
    /// https://stackoverflow.com/questions/19736631/can-a-cryptostream-leave-the-base-stream-open
    /// </summary>
    class NotClosingCryptoStream : CryptoStream
    {
        public NotClosingCryptoStream( Stream stream, ICryptoTransform transform, CryptoStreamMode mode )
            : base( stream, transform, mode )
        {
        }

        protected override void Dispose( bool disposing )
        {
            if( !HasFlushedFinalBlock )
                FlushFinalBlock();

            base.Dispose( false );
        }
    }
}
