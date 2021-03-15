# 클래스 값 캐스팅

```csharp
interface I {}
class Derived : Base, I {}

var d = new Derived();

// 1. 암시적 캐스트 
Base b = d;

// 2. 암시적 캐스트
I i = d;

// 3. 명시적 클래스 다운 캐스트 시도
nullable<Derived> d = b as Derived;

// 4. 명시적 인터페이스 다운 캐스트 시도
nullable<I> i2 = b as I;

```

