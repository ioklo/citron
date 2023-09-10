%%NOTTEST%%
```
if (<exp> is <class-id>) <embeddable-stmt> else <embeddable-stmt>
if (<class-id> <name> = <exp>) <embeddable-stmt> else <embeddable-stmt>
```

%%NOTTEST%%
```
IfTestClassStmt(Loc target, ClassSymbol symbol, Name? varName, [Stmt] body, [Stmt] elseBody)
```

%%TEST(Basic, succeed)%%
```cs
class B { }
class C : B { }

void Main()
{
    B b = new C();
    if (C c = b)
    {
        @succeed
    }
}
```

%%TEST(CantTestValueType, $Error)%%
```cs
class C {}

void Main()
{
    var s1 = 0;
    if (s1 is C)  // wrong, 명확한 타입에 대해서는 타입비교 불가
        @false
}
```

%%TEST(TestUnrelatedClass, $Error)%%
```cs
class C { }
class D { }

void Main()
{
	var c = new C();

	if (D d = c);
}
```

test interface is class
%%TEST(TestInterface, true)%%
```cs
interface I {}
class C : I {}

void Main()
{
	I i = new C();

	if (C c = i) @true
}
```

test class implements interface
%%TEST(TestClassImplInterface, true)%%
```cs
interface I {}
class B { }
class C : B, I { }

void Main()
{
	var b = new C();
	if (I i = b)
	{
		@true
	}
}
```

test interface's own type also implements interface
%%TEST(TestInterfaceImplInterface, true)%%
```cs
interface I1 { }
interface I2 { }
class B : I1 { }
class C : B, I2 { }

void Main()
{
	I1 i = new C();

	if (I2 i2 = i)
	{
		@true
	}
}


```


# Reference
[Locations](Locations.md)
[Class](Class.md)
[Statements](Statements.md)