using System.IO;

namespace BatchProcessorAPI.StreamUtil
{
    public class NotClosingStreamReader : StreamReader
    {
        public NotClosingStreamReader(Stream stream)
            : base(stream)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(false);
        }
    }
}