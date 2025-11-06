%%NOTTEST%%
```
new <symbol-id>(<argument>,...)
```

%%NOTTEST%%
```
NewClassExp(ClassConstructorSymbol symbol, [Argument] args)
```

%%BEGIN_EMBED(New_Class_Expression_Basic)%%
```
class C
{
	int x;
	int y;

	public void Print()
	{
		@$x $y
	}
}

void Main()
{
	var c = new C(2, 3);
	c.Print();
}
```
%%END_EMBED%%

%%BEGIN_EMBED(New_Class_Expression_Generics)%%
```
class C<T>
{
	T a;
	public T GetA() { return a; }
}

void Main()
{
	var c = new C<string>("hello");
	var a = c.GetA();
	@$a
}
```
%%END_EMBED%%

# Reference
[Class_Constructor](Class_Constructor.md)
[Argument](Argument.md)