```
<exp>(<argument>, ...) // exp가 lambda value로 계산될 때
```

```
CallLambdaExp(LambdaSymbol symbol, Loc callable, [Argument] args)
```

<!--BEGIN_EMBED(Call_Lambda_Expression_General)-->
```cs
//@ 1 3 true
void Main()
{
	var f = (int i, string s, bool b) => { 
	    @$i $s $b
	};
	
	f(1, "3", true);
}
```
<!--END_EMBED-->

<!--BEGIN_EMBED(Call_Lambda_Expression_CallInstanceMember)-->
```cs
//@ 2
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
<!--END_EMBED-->

<!--BEGIN_EMBED(Call_Lambda_Expression_CallStaticMember)-->
```cs
//@ 2
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
<!--END_EMBED-->

# Reference
[Lambda](Lambda.md)
[Locations](Locations.md)
[Argument](Argument.md)