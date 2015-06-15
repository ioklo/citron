using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL
{
    // 레퍼런스 타입, 대입시 얕은 복사가 일어나는 타입
    public class RefType : DefaultType
    {
        // 타입 이름
        public RefType BaseType { get; private set; }
        public string Name { get; private set; }
        
        // private Dictionary<string, FuncInfo> funcs = new Dictionary<string, FuncInfo>();

        public RefType(string name, IEnumerable<IType> fields)
            : base(fields)
        {
            Name = name;
        }
    }
}

// ConstValue : Expression에 나타나야 하기 때문에 존재
// 일반 Value가 여기 있을 필요가 있나요

// String 같은거는 어떻게 존재하고 있나요 
// StringValue?
// 
// Global(1) = "안녕하세요"
// string a = "안녕하세요"
// int b = 3;

// global table
// 1 | "안녕하세요"
// 

// 
// Environment 
//    a |-> GlobalLoc(1)
//    b |-> 

