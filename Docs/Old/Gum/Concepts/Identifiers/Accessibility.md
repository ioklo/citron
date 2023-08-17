# 접근성 Accessibility

# 모듈 단위 접근성 설정

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

# 타입 단위 접근성 설정

{`class`, `struct`, `enum`, `interface` 등} 타입 정의 시, 멤버 {타입, 함수, 변수} 정의 앞에 { `public`, `private`, `protected`등 } 키워드를 붙이면, 타입 단위 접근성을 설정할 수 있습니다. 

접근이 가능한지 여부는 실행되는 함수가 어느 타입에 속해있는지에 따라 결정됩니다.

```csharp
class C
{
    public int x;        // 타입 단위, Class 멤버 변수 (기본 private)    
    public static int y;  // 타입 단위, Class Static 멤버 변수 (기본 private)
    int z;                // 타입 단위, 

    static C() // static 생성자는 접근성이 의미 없어서 쓰지 않습니다
    {
        y = 3;
    }

    public C() // 생성자, 기본 private이므로 public으로 써주면 클래스 C외부에서 생성 가능
    {
        x = 2;
    }
}

var c = new C();
Assert(c.x == 2); 
Assert(C.y == 3);
```

# 기본 접근성

각 접근성 설정은 기본값이 있습니다. 기본값을 명시적으로 쓰게 되면 에러를 냅니다