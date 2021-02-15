# Lambda, Boxed Lambda
실행중에 생성되는 함수 객체를 담는 타입입니다. lambda는 스택에, boxed lambda는 힙에 생성됩니다.

lambda 는 call-site에 따라 고유한 이름이 부여되고, 직접 지정할 수 없습니다. var를 사용합니다.
boxed lambda는 `func<Arg0, ..., RetType>` 으로 나타낼 수 있습니다

```csharp
int i = 4;

// addI는 lambda 타입입니다
// 캡쳐된 i를 복사해서 가지고 있어서, i 타입 만큼 크기가 커집니다.
var addI = (int x) => x + i; 
print(addI(3)); // '7' 출력

// 캡쳐된 변수가 없기 때문에 addI와 크기가 다릅니다
var add1 = (int x) => x + 1;
print(add1(3)); // '4' 출력

// boxed lambda 타입 입니다. 시그니쳐가 같다면 크기가 달라도 갖고 있을 수 있습니다
// 복사라는 것을 명확히 하기 위해 box를 삽입해야 합니다
func<int, int> f = box addI;
f = box add1;
print(f(5)); // '6' 출력

// lambda constructor와 같이 func<>를 선언했다면 box가 생략됩니다(타입 힌트)
func<int, int> f = x => x + 2;

```





