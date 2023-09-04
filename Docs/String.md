# General
문자열을 표현하는 struct type입니다.

# Type Expression
심볼 경로 필요 없이 `string` 이라고 쓰면 됩니다. 실제 타입은 `System.String`입니다

# Value
- struct type이지만 생성자를 써서 만들지 않고,  문자열 literal을 사용해서 생성합니다.
%%TEST(Literal, hello)%%
```cs
void Main()
{
    string x = "hello";
    @$x
}
```

- $x처럼 써서 문자열에 변수를 삽입할 수 있습니다. $를 문자 그대로 쓰고 싶으면 두번 사용하면 됩니다.
%%TEST(InterpolationVariable, hello.3)%%
```cs
void Main()
{
    int i = 3;
    string x = "hello";
    string y = "$x.$i";
    
    @$y
}
```

- 변수 외의 expression은 `${exp}`형식으로 표현할 수 있습니다
%%TEST(InterpolationWithBraces, hello.3)%%
```cs
void Main()
{
    int i = 2;
    string x = "hell";
    string y = "${x + "o"}.${i + 1}";
    
    @${y}
}
```

# 문자열형 간에 미리 정의된 이항 연산
문자열끼리는 덧셈과 비교가 가능합니다.
- (string, string) -> string {+}
- (string, string) -> bool { <, >, <=, >=, == }

%%TEST(String, hi hello world world true true false false onetwo true false true true false false true false true true)%%
```cs
void Main()
{
    string s = "hi";
    string t = s;
    
    s = "hello"; // 
    
    @$t $s ${s = "world"} $s
    
    string t2 = "${"h"}${"i"}";
    @ ${t == t2} ${s == "world"} ${t != t2} ${s != "world"}
    
    @ ${"one" + "two"}
    
    @ ${"s1" < "s1abcd"} ${"s1abcd" < "s1"} ${"s1" <= "s1abcd"} ${"s1" <= "s1"} ${"s1abcd" <= "s1"}
    
    @ ${"s1" > "s1abcd"} ${"s1abcd" > "s1"} ${"s1" >= "s1abcd"} ${"s1" >= "s1"} ${"s1abcd" >= "s1"}
}

```
