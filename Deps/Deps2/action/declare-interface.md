# 인터페이스 정의

```csharp
interface I : BaseI, BaseI2 // 인터페이스 상속
{
    I(int i);            // constructor 정의
    void Func();         // 멤버 함수
    static StaticFunc(); // 정적 멤버 함수   
}

```