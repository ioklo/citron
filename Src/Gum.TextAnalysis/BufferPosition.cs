using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace Gum
{
    public struct BufferPosition
    {
        private Buffer buffer;
        private int nextPos;  
        private int code; // null이라면 

        internal BufferPosition(Buffer buffer, int nextPos, int code)
        {
            this.buffer = buffer;
            this.nextPos = nextPos;
            this.code = code;
        }

        public bool IsReachEnd()
        {
            return nextPos == -1;
        }

        public bool IsWhiteSpace()
        {
            if (code == -1) return false;

            if (!char.IsWhiteSpace(char.ConvertFromUtf32(code), 0)) return false;

            return code != '\r' && code != '\n';
        }

        public async ValueTask<BufferPosition> NextAsync()
        {
            var result = await buffer.NextAsync(nextPos);            
            return new BufferPosition(buffer, result.NextPos, result.Code);
        }

        public UnicodeCategory? GetUnicodeCategory()
        {
            if (code == -1) return null;

            return CharUnicodeInfo.GetUnicodeCategory(code);
        }

        public bool Equals(char c)
        {
            return code == c;
        }

        public void AppendTo(StringBuilder sb)
        {
            sb.Append(char.ConvertFromUtf32(code));
        }

        public override bool Equals(object? obj)
        {
            return obj is BufferPosition position &&
                   EqualityComparer<Buffer>.Default.Equals(buffer, position.buffer) &&
                   nextPos == position.nextPos &&
                   code == position.code;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(buffer, nextPos, code);
        }

        public static bool operator ==(BufferPosition left, BufferPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BufferPosition left, BufferPosition right)
        {
            return !(left == right);
        }
    }
}
