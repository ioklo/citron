# 검색 대상 네임스페이스 추가

```csharp
using MyNamespace;

// 타입 찾기
MyClass c; // {MyClass, MyNamespace.MyClass} 을 찾아본다

// 함수 찾기
F(); // {F, MyNamespace.F} 함수그룹에서 적절한 함수가 있는지 살펴 본다

```