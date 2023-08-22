함수 본문 공간에서 만들어지는 실행 가능한 값입니다.

# 람다 만들고 사용하기
```cs

void Main()
{
    var l1 = (int x) => x + 1; // 본문 축약형
    var l2 = (string s) => { return s; } // 완전한 본문

    var v1 = l1(2);
    var v2 = l2("hi");

    // 3, "hi"
    @$v1, $v2
}
```

# 캡쳐
람다가 본문에서 사용하는 변수가 람다 바깥에 있는 경우 람다는 해당 값을 복사해서 저장합니다. 이를 캡쳐라고 합니다. 값을 복사했기 때문에, 람다가 생성된 이후로 변수의 값이 바뀌어도 적용되지 않습니다. 바꾸고 싶으면 local pointer나 box pointer를 사용해야 합니다.

```
void Main()
{
    int x = 0;
    var l = () => x;
    x = 1;

    // 0
    @${l()}
}
```

```
void Main()
{
    int x = 0;
    int* y = &x;

    // local pointer 함유 lambda, 내부에서밖에 쓸 수 없습니다
    var l = () => *y;
    x = 1;

    // 1
    @${l()}
}
```

```
void Main()
{
    box int* x = box 0; // heap을 사용하는 버전
    var l = () => *x;
    
    *x = 2;

    // 2
    @${l()}
}
```

# this 캡쳐
람다 내부에서 사용한 `this`는 람다가 선언된 본문의 `this`를 의미합니다. 바깥 본문의 this도 캡쳐 대상입니다.
```
struct S
{
    int x;
    
    void F()
    {
        var l = () => this->x + 2; // this는 캡쳐대상 S*
        
        x = 3;
        
        // 5
        @${l()}
    }
}


void Main()
{
    var s = S(3);
    s.F();
}
```

일반 로컬 변수와 this 변수의 캡쳐후 결과가 다르기 때문에, 람다 내부에서는 멤버변수를 바로 쓸 수 없습니다. 필요한 경우 로컬변수에 복사해서 씁니다.
```
struct S
{
    int x;
    
    void F()
    {
        var l = () => x + 2; // 에러
        x = 3;
        @${l()}
    }
}

void Main()
{
    var s = S(3);
    s.F();
}
```

# Nested Capture


%% 이부분 다시 작성할 것 %%
# Boxed Lambda
람다를 만들어서 다른곳에서 사용하고 싶을 수 있습니다. 람다는 캡쳐한 변수에 따라 크기가 달라지기 때문에 람다를 담을 가변타입이 필요합니다. func<>에 box로 힙에 생성된 람다를 담을 수 있습니다. local func<>는 

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





