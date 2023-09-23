%%NOTTEST%%
```
<exp>(<argument>, ...) // exp가 lambda value로 계산될 때
```

%%NOTTEST%%
```
CallLambdaExp(LambdaSymbol symbol, Loc callable, [Argument] args)
```

%%TEST(General, 1 3 true)%%
```cs
void Main()
{
	var f = (int i, string s, bool b) => { 
	    @$i $s $b
	};
	
	f(1, "3", true);
}
```

%%TEST(CallInstanceMember, 2)%%
```cs
class C
{
    func<int, void> F;

	public void InvokeF(int i)
	{
		F(i);
	}
}

void Main()
{
    C c = new C(i => {
        @$i
    });

    c.InvokeF(2);
}
```

%%TEST(CallStaticMember, 2)%%
```cs

class C
{
    public static func<int, void> F;
}

void Main()
{
    C.F = i => {
        @$i
    };


    C.F(2);
}
```

# Reference
[Lambda](Lambda.md)
[Locations](Locations.md)
[Argument](Argument.md)