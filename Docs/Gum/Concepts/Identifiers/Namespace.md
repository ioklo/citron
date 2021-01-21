# 네임스페이스

```csharp
// 정의와 사용을 위한 네임스페이스 범위 만들기, 안 적으면 루트 네임스페이스이다
namespace X.Y;
... 

namespace X.Y.Z; // 앞으로는 X.Y.Z 네임스페이스에 정의한다

class C // X.Y.Z.C 정의
{
}

void F()
{
}

int x; // X.Y.Z.x 정의

namespace; // 빈 네임스페이스로 돌아오기

// 스코프 내에서 이름 공간을 사용할 수 있다
{    
    using X.Y;     // X.Y 네임스페이스 전체를 검색 대상에 넣는다
    Z.C c;         // X.Y.Z.C 타입 변수 선언
    X.Y.Z.F();     // 함수 호출
}

{
    using C = X.Y.Z.C; // 이 구문 이후로 C는 X.Y.Z.C 이다
    C c;               // X.Y.Z.C 타입 변수 선언
}

```