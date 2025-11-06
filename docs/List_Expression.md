%%NOTTEST%%
```
[exp, exp, ...]
```

%%NOTTEST%%
```
ListExp([Exp] exps)
```
리스트를 만들고 환경의 result에 값을 넣습니다

%%TEST(Basic, )%% 
```cs
void Main()
{
    var l = [1, 2, 3]; // list<int>
    var s = ["2", "3", "4"]; // list<string>
    var b = [false, false, true, true]; // list<bool>

    // 그냥 pass
}
```

# Reference
[Expressions](Expressions.md)