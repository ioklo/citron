# func_throw<> 인터페이스 값 생성

```csharp
enum E { }

// 1. 암시적 boxing
func_throw<int, int, E> f1 = (x) throw E => x; // 타입 힌트에 따라서 바로 boxing한다 (생성 후 복사가 아니다)

// 3. 명시적 boxing 및 캐스팅
var sf = (int x) throw E => x;
func_throw<int, int, E> f3 = box sf; // 복사, box sf의 타입은 box<anonymous_func0>, 
                           // struct anonymous_func0 : func_throw<int, int, E> 이므로 캐스팅 가능

func<int, int> f4 = sf;    // 에러, struct 값은 암시적으로 boxing 할 수 없다.

// 4. params 인자 리스트 대응
int F(params arglist<int> l) throw E { return 3; }
func_throw<params arglist<int>, int, E> f5 = F; // func는 일반적인 generic interface로 보이지만 아니다

// 5.
func_throw<params arglist<int>, int, E> f6 = (params arglist<int> l) throw E => 3;
```



