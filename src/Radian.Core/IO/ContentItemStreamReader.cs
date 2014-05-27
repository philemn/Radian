using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Radian.Core.IO
{
    public class ContentItemStreamReader : StreamReader
    {
        private int _braceCount = 0;
        private char[] _tempChar = new char[1];

        public ContentItemStreamReader(Stream stream)
            : base(stream)
        {
            if (!stream.CanSeek)
            {
                throw new NotSupportedException("ContentItemStreamReader can only read seekable streams");
            }
        }

        public override int Read(char[] buffer, int index, int count)
        {
            var result = base.Read(buffer, index, count);

            if (TemplateStartPosition > 0 && _braceCount == 0) return result;

            for (var pos = 0; pos < buffer.Length; pos++)
            {
                _tempChar[0] = buffer[pos];
                switch (_tempChar[0])
                {
                    case '{':
                        _braceCount++;
                        break;
                    case '}':
                        if (--_braceCount == 0)
                        {
                            TemplateStartPosition += CurrentEncoding.GetByteCount(_tempChar) + CurrentEncoding.GetPreamble().Length;
                            return result;
                        }
                        break;
                }

                TemplateStartPosition += CurrentEncoding.GetByteCount(_tempChar);
            }

            return result;
        }

        public long TemplateStartPosition { get; protected set; }
    }
}
