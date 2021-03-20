# 람다 값 생성

```csharp
int y; 

// 1. 완전 생략형
var l1 = x => x + y;

// 2. throw 시그니처 생략형
var l2 = x => throw E; // 리턴은 void, throw는 E

// 3. 완전체형
enum E { NotImplemented }
var l2 = [y](int x) : int throw E => {

    if (x == 3) throw NotImplemented;

    return x + y;
}

// 4. 멤버 함수 호출 생략형
struct C1 { void F1(string s) throw E { } }
C c = new C1();

var l = c.F1; // s => c.F1(s); 

class C
{
    int x;

    void F()
    {   
        // 별일 없으면, 모두 복사
        var l = () => x; 

        // 
        var l = [this]() => this.x; // this class 값 복사(얕은)        
    }
}

struct S
{
    int x;

    func<void, int> F()
    {
        // 별일없으면, this.x를 복사
        var l0 = () => x;

        // 캡쳐 힌트 []        
        // ref x 하면 로컬 변수 레퍼런스
        // x하면 복사
        // 안적으면 다 복사

        // this가 이 함수중에서는 살아있겠지만
        var l1 = [ref this]() => this.x; // S의 레퍼런스가 l1안에 들어있다.
        var l2 = [this]() => this.x;     // S전체가 복사됐다

        return box l1; // 캡쳐힌트에 ref가 들어가면 박싱 불가능
        return box l2; // 가능

        // 박싱 가능 (S전체가 복사된다)
        var l3 = box [this]() => this.x;        
        return l2; // interface 캐스팅
    }
}


```

