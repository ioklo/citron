# 모듈 Module

Gum을 해석해서 실행을 하는데 있어 필요한 최소 단위

```csharp
// 네임스페이스 정의
namespace MyNamespace
{
    // 프로그램 외부로 노출되는 MyNamespace.MyClass 클래스 타입 정의 (전역)
    public class MyClass
    {
    }

    // 프로그램 내부에서만 쓰는 (전역) 함수 정의
    void F<T>(int x) { }

    // 탑레벨 절(Statement), 다른 프로그램에서 이 프로그램을 로드하면 실행되는 부분이다
    @echo "Hello World";
}
```

- 소스 파일과는 다른 단위이다
- 여러 소스파일을 묶어서 하나의 모듈을 만들 수 있다
    - 소스파일의 순서는 중요하지 않도록 정의들을 모은 뒤 한꺼번에 처리한다
- 다른 외부 모듈과 연동할 수 있다