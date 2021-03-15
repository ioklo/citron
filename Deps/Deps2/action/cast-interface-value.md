# 인터페이스 값 캐스팅

```csharp
interface I {}
interface I2 : I {}

class Derived : Base, I2 {}

// 암시적 캐스트, Derived -> I2
I2 i2 = new Derived();

// 1. 암시적 캐스트 I2 -> I
I i = i2;

// 2. 명시적 인터페이스 캐스트 시도 I -> I2
nullable<I2> i22 = i as I2;

// 3. 명시적 클래스 캐스트 시도.. I -> Derived
nullable<Derived> d = i as Derived;

// 4. covariant/contravariant
interface J<in TIn, out TOut> { }
void Func(J<Base, Derived> j0)
{
    J<Derived, Base> j1 = j0; // in은 타입은 더 구체화되고, out은 더 추상화될 수 있다
}

```
