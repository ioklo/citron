# Reference

참조는 두가지 방식으로 사용할 수 있습니다.

- 지역 참조
- 함수 호출인자

함수 호출 경계에서 레퍼런스는 주소 값을 참조하면서 구현합니다. (포인터로 넘어갑니다)

## 지역 참조
함수 본문에 참조 선언을 통해 참조를 만들 수 있습니다.
참조에 쓰이는 이름은 선언후엔 대상을 가리키기 때문에, 참조의 대상을 바꿀 수 없습니다

<!--BEGIN_EMBED(Ref_Decl)-->
```cs
//@ 4
void Main()
{
    int a = 3;
    int& x = a;

    x = 4;
    @$a    
}
```
<!--END_EMBED-->


## 함수 호출
```
void F(int& t) { t = 3; }

void Main()
{
    int i = 2;
    F(i);
    @$i;
}
```

### 함수 인자 지시자
함수인자에 ```[in]``` ```[move]``` ```[forward]``` 표시를 해서 인자를 넘기는 방식을 조정할 수 있습니다.

#### ```[in]``` 지시자
함수 호출시 lvalue, rvalue, talias를 모두 받을 수 있습니다. 타입이 T&처럼 참조일때 쓸 수 있습니다.
참조이므로 소유권은 호출자에 있습니다. 

```cs
void F([in] int& t) { ... }

int a = 3; F(a); // lvalue
F(5); // rvalue
int? x = 4; if (x is not_null(v)) F(v); // talias
```

#### ```[move]``` 지시자
함수 호출시 rvalue를 받을 수 있습니다. 다른 rvalue로 move 할 수도 있습니다.
```cs
struct S { int x; S([in] S& s) = default; S([move] S& s) = default; }
void G([move]S& s) 
{ 
    // 기본적으로 s는 lvalue로 사용
    @${s.x}
    S s2 = move s; // move 생성자 호출. 여기서 소비
}

void F([move]S& s) 
{ 
    G(move s); // s는 S& 타입이지만 move사용 가능
}

// var s1 = S(1); F(s1); // 에러, lvalue는 사용 불가능
var s2 = S(2); F(copy s2); // 명시적 복사를 사용해서 rvalue로 변경
var s3 = S(3); F(move s3); // 명시적 이동을 사용해서 rvalue로 변경
F(S()); // rvalue는 바로 담을 수 있음
var? s4 = S(4); if (s4 is not_null(v)) F(move v); // talias도 copy나 move를 사용해서 rvalue변경
```

#### ```[forward]``` 지시자
lvalue, rvalue를 모두 받을 수 있습니다.

```cs

struct S { int x;  }
void F([forward] S& s)
{
    S s2 = forward s; // s2는 복사되거나, 이동되거나
}

```