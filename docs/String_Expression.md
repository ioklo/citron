%%NOTTEST%%
```
"text"
"with ""double quote"""
"using variable $x or dollar literal $$"
"using expression ${ "hello" }"
```

두 따옴표 사이에 표현할 문자를 넣으면 됩니다. 따옴표를 표현하고 싶으면 "" 라고 쓰면 됩니다
%%TEST(Literal, hello")%%
```cs
void Main()
{
    string x = "hello""";
    @$x
}
```

- $x처럼 써서 문자열에 변수를 삽입할 수 있습니다. $를 문자 그대로 표현하고 싶으면 두번 사용하면 됩니다.
%%TEST(InterpolationVariable, hello.3 $)%%
```cs
void Main()
{
    int i = 3;
    string x = "hello";
    string y = "$x.$i $$";
    
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

# Reference
[String](String.md)
[Expressions](Expressions.md)