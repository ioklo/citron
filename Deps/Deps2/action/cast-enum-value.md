# 열거형 값 캐스팅

```csharp
enum E1 { One }
enum E2 : E1 { Two }

E1 e1 = One; // 타입 힌트
E2 e2 = e1;  // 1. 암시적 캐스트 E1 -> E2, 반대로는 안될듯 하다

```