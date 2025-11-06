# General
문자열을 표현하는 struct type입니다.

# Type Expression
심볼 경로 필요 없이 `string` 이라고 쓰면 됩니다. 실제 타입은 `System.String`입니다

# Value
- struct type이지만 생성자를 써서 만들지 않고,  문자열 literal을 사용해서 생성합니다.

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
