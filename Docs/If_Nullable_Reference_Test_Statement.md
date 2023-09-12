nullable reference가 가능한 타입에 대해서 dynamic cast를 수행하고, 성공시 본문을 수행합니다

%%NOTTEST%%
```
if (<type-exp> <name> = <exp>) <embeddable-stmt> else <embeddable-stmt>
```

%%NOTTEST%%
```
IfNullableRefTestStmt(Type refType, Name varName, Exp castExp, [Stmt] body, [Stmt] elseBody)
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

%%TODO is로 옮길 것%%
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

	if (D d = c); // 미리 잡을 수 있는 경우는 최대한 잡습니다
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