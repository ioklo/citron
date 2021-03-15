# 람다 값 호출

func<> constraint 호출 방식을 사용한다.

```csharp
var l = (int x) : int => x; // : int로 리턴 타입 지정

// 1. anonymous lambda 값 호출, func<int, int> constraint를 갖고 있다, 스택에 생성된 함수
l(2);

func<int, int> f = x => x; // func<int, int>로부터 타입힌트, 힙에 생성된 함수
// 2. func<> interface 호출
f(2); 
```

