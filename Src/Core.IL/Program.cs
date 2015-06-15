using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL
{    
    // 컴파일 결과가 꼭 Program으로 나오는건가
    // VM이 수행할 프로그램은 cfg와 시작점
    public class Program
    {
        // 프로그램이 가져야 할 것들

        public IL.Domain Domain { get; private set; }

        public Program(IL.Domain domain)
        {
            Domain = domain;
        }
    }
}
