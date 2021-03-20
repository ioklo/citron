# full-qualified-id

```csharp

// 모듈 이름 사용
::[My Module Name]MyCompany.MyType

// 현재 함수 컨텍스트의 모듈
::MyCompany.MyType<>
::MyCompany.MyFunc

// 함수는 overloading이 되므로 exact한 함수를 찾고 싶으면 funcof를 쓴다
funcof(::MyCompany.MyFunc, int, int, int)(2, 3);


```