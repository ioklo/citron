# ref 값 생성

```csharp
// 함수 호출을 통해서만 ref 값을 생성할 수 있다
void F(ref<int> i) { }

int i = 3;
// 1. 함수 인자를 통한 ref 값 생성
F(ref i);

// 2. from boxed value
box<int> i = box 3;
F(i); // ref<int> <- box<int> 가능

```