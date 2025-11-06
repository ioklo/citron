%%NOTTEST%%
```
new <symbol-id>(<argument>,...)
```

%%NOTTEST%%
```
NewClassExp(ClassConstructorSymbol symbol, [Argument] args)
```

%%TEST(Basic, 2 3)%%
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

%%TEST(Generics, hello)%%
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

# Reference
[Class_Constructor](Class_Constructor.md)
[Argument](Argument.md)