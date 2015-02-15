using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gum.Core.IL
{
    public interface IFieldIndicator
    {
        string FieldName { get; }
    }

    public class StringFieldIndicator : IFieldIndicator
    {
        public String Name { get; set; }

        public string FieldName
        {
            get { return "@" + Name; }
        }
    }

    public class IndexFieldIndicator : IFieldIndicator
    {
        public int Index { get; set; }

        public string FieldName
        {
            get { return "#" + Index.ToString(); }
        }
    }
}
