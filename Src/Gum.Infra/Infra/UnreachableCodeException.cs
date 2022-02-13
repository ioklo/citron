using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Infra
{
    // 실제로 syntax에서 절대로 도달하지 못하는 경우,
    // 로직상 도달 못하지만, 잘못된 입력에 따라서 도달하게 되는 경우는 RuntimeFatalException을 던져야 한다
    public class UnreachableCodeException : Exception
    {
    }
}
