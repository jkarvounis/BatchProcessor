using System.IO;

namespace BatchProcessorAPI.StreamUtil
{
    /// <summary>
    /// Thank you Aaron Murgatroyd
    /// https://stackoverflow.com/questions/2666888/is-there-any-way-to-close-a-streamwriter-without-closing-its-basestream
    /// </summary>
    public class NotClosingStreamWriter : StreamWriter
    {
        public NotClosingStreamWriter(Stream stream)
            : base(stream)
        {
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(false);
        }
    }
}