using System;
using System.Collections.Generic;
using System.Text;

namespace Gum.CompileTime
{
    public struct ModuleName
    {
        internal enum EKind
        {
            Normal,
            Internal,            
        }

        public static ModuleName Internal { get; } = new ModuleName(EKind.Internal, null);

        internal EKind Kind { get; }
        internal string? Text { get; }

        private ModuleName(EKind kind, string? text)
        {
            this.Kind = kind;
            this.Text = text;
        }

        public static implicit operator ModuleName(string name)
        {
            return new ModuleName(EKind.Normal, name);
        }

    }
}
