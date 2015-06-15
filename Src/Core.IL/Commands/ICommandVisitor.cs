using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gum.Core.IL.Commands
{
    // 커맨드 수를 최소로 합니다.
    public interface ICommandVisitor
    {
        // 주소 만들기
        void Visit(GlobalRef globalLocCmd);
        void Visit(LocalRef localLocCmd);
        void Visit(FieldRef fieldLocCmd);

        // 주소만들기 - 오브젝트 생성하기
        void Visit(New newCmd);        

        // 주소에 값 넣기/가져오기  
        void Visit(Load loadCmd);
        void Visit(Store storeCmd);

        // 레지스터
        void Visit(Move moveCmd);
        void Visit(MoveReg moveRegCmd);

        // 실행 흐름
        void Visit(Jump jumpCmd);
        void Visit(IfNotJump condCmd);

        // 실행 흐름 - 함수 관련 커맨드
        void Visit(StaticCall staticCallCmd);
        void Visit(VirtualCall virtualCallCmd);
        void Visit(Return returnCmd);
    }
}
