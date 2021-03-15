# 구조체 정의

```csharp
struct S { }
interface I1 { }
interface I2 { }

// 액세스 한정자
struct S2 : S, I1, I2 // Base타입, 인터페이스들
{
    // 멤버 엑세스 한정자: private     

    // 멤버 변수
    int x; 

    // 정적 멤버 변수
    static int x;

    // 멤버 함수
    void Func() { }

    // 정적 멤버 함수
    static void Func() { }

    // 기본 생성자
    default constructor;        
}

```