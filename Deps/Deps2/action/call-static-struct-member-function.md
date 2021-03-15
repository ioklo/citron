# 정적 구조체 멤버 함수 호출

정적 클래스 멤버 함수 호출과 거의 같다

```csharp
struct S
{
    public static F();

    public void G()
    {
        // 내부에서 호출
        F();
    }
}

// 2. 외부에서 호출, public으로 선언되어있을 때만 가능하다
S.F();

var s = new S();
s.F(); // 3. 에러, 상세한 설명은 정적 구조체 멤버 함수 호출에 적혀있음
```

