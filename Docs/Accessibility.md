# General
접근성은 식별자가 사용된 **공간**에서, 그 식별자가 가리키는 심볼에 접근 가능한지를 결정합니다. 식별자가 쓰이는 공간은 심볼을 정의하는 곳일 수도 있고, 함수 본문일 수도 있습니다. 심볼 트리를 구성하는 모든 심볼들은 각각 접근성을 갖고 있습니다.

접근성에는 세가지 종류가 있습니다. `public`은 항상 해당 심볼에 접근할 수 있습니다. `private`은 형제 심볼만이 접근할 수 있습니다. `protected`는 클래스의 자식 심볼에만 지정 가능한데, 그 클래스와 그 클래스를 상속받은 derived class에서만 해당 자식 심볼에 접근이 가능합니다.

자식을 가질 수 있는 심볼들은 심볼의 종류별로 자식의 기본 접근성이 있습니다. `module`은  `private`, `struct`는 `public`, `class`는 `private`을 자식들의 기본 접근성으로 설정합니다. 이 동작을 바꾸고 싶으면 자식 노드 정의에 Access Modifier를 써서 접근성을 변경합니다. 앞에 붙인 Access Modifier가 기본 접근성이어서 의미가 없는 modifier라면 컴파일 도중에 에러가 납니다.



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