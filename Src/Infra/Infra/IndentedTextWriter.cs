using System.IO;

namespace Citron.Infra
{    
    public struct IndentedTextWriter
    {
        class NeedIndent { public bool Value; }

        TextWriter tw;
        string indent;
        NeedIndent needIndent;

        public IndentedTextWriter(TextWriter tw)
        {
            this.tw = tw;
            this.indent = string.Empty;
            this.needIndent = new NeedIndent() { Value = false };
        }

        IndentedTextWriter(TextWriter tw, string indent, NeedIndent needIndent)
        {
            this.tw = tw;
            this.indent = indent;
            this.needIndent = needIndent;
        }

        public void Write(string text)
        {
            if (needIndent.Value)
            {
                tw.Write(indent);
                needIndent.Value = false;
            }
               
            tw.Write(text);
        }

        public void Write(int v)
        {
            if (needIndent.Value)
            {
                tw.Write(indent);
                needIndent.Value = false;
            }

            tw.Write(v);
        }

        public void Write(bool v)
        {
            if (needIndent.Value)
            {
                tw.Write(indent);
                needIndent.Value = false;
            }

            tw.Write(v ? "true" : "false");
        }

        public void WriteLine(string text)
        {
            if (needIndent.Value == true && text.Length != 0)
                tw.Write(indent);

            tw.WriteLine(text);
            needIndent.Value = true;
        }

        public void WriteLine()
        {
            tw.WriteLine();
            needIndent.Value = true;
        }

        public IndentedTextWriter Push()
        {
            return new IndentedTextWriter(tw, this.indent + "    ", needIndent);
        }
    }        

}
