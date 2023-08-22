# General

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

# Accessibility 모듈 단위 접근성 설정

전역 타입, 함수, 변수 정의 앞에 `public` 을 써 줌으로써 다른 모듈에서 접근 가능하게 만들 수 있습니다.

```csharp
// Module1
public class C { } // 모듈 단위 public 외부에서 참조 가능합니다, 전역 타입 (기본 private)
class D { }        // 모듈 단위 private, 기본이므로 써주지 않아야 합니다
public int x = 2;  // 모듈 단위 public 외부에서 참조 가능합니다, 전역 변수 (기본 private)
```

```csharp
// Module2
var c = new C();   // Module1의 C를 참조합니다
Assert(x == 2);    // Module1의 x를 참조합니다
```
