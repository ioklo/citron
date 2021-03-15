# dict 생성

```csharp
class Base {}
class Derived1 : Base {}
class Derived2 : Base {}

dict<int, Base> d1 = { i: new Derived1(), 2: new Derived2() }; // 타입 힌트
var d2 = { "1": "hello", "this": "hi" }; // dict<string, string>
```

