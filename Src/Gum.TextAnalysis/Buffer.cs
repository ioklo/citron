using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Gum
{
    public class Buffer
    {
        private List<int> codes;
        private TextReader reader;
        private char[] buf;

        public Buffer(TextReader reader)
        {
            this.codes = new List<int>();
            this.reader = reader;
            buf = new char[2];
        }

        public BufferPosition MakePosition()
        {
            return new BufferPosition(this, 0, -1);
        }

        internal async ValueTask<(int Code, int NextPos)> NextAsync(int nextPos)
        {
            if (nextPos == -1)
                return (-1, -1);

            Debug.Assert(nextPos <= codes.Count);

            if (nextPos < codes.Count)
                return (codes[nextPos], nextPos + 1);
            
            if (nextPos == codes.Count)
            {
                int readCount = await reader.ReadAsync(buf, 0, 1);
                if (readCount == 0)
                    return (-1, -1);

                int code = -1;
                if (char.IsSurrogate(buf[0]))
                {
                    readCount = await reader.ReadAsync(buf, 1, 1);
                    if (readCount == 0)
                        return (-1, nextPos);

                    code = char.ConvertToUtf32(buf[0], buf[1]);
                }
                else 
                {
                    code = buf[0];
                }

                codes.Add(code);
                return (code, nextPos + 1);
            }

            throw new InvalidOperationException();
        }
    }
}
