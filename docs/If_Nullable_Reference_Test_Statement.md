nullable reference가 가능한 타입에 대해서 dynamic cast를 수행하고, 성공시 본문을 수행합니다

%%NOTTEST%%
```
if (<type-exp> <name> = <exp>) <embeddable-stmt> else <embeddable-stmt>
```

%%NOTTEST%%
```
IfNullableRefTestStmt(Type refType, Name varName, Exp castExp, [Stmt] body, [Stmt] elseBody)
```

%%BEGIN_EMBED(If_Nullable_Reference_Test_Statement_Basic)%%
```cs
//@ succeed
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
%%END_EMBED%%

%%TODO is로 옮길 것%%
%%BEGIN_EMBED(If_Nullable_Reference_Test_Statement_CantTestValueType)%%
```cs
//@ $Error
class C {}

void Main()
{
    var s1 = 0;
    if (s1 is C)  // wrong, 명확한 타입에 대해서는 타입비교 불가
        @false
}
```
%%END_EMBED%%

%%BEGIN_EMBED(If_Nullable_Reference_Test_Statement_TestUnrelatedClass)%%
```cs
//@ $Error
class C { }
class D { }

void Main()
{
	var c = new C();

	if (D d = c); // 미리 잡을 수 있는 경우는 최대한 잡습니다
}
```
%%END_EMBED%%

test interface is class
%%BEGIN_EMBED(If_Nullable_Reference_Test_Statement_TestInterface)%%
```cs
//@ true
interface I {}
class C : I {}

void Main()
{
	I i = new C();

	if (C c = i) @true
}
```
%%END_EMBED%%

test class implements interface
%%BEGIN_EMBED(If_Nullable_Reference_Test_Statement_TestClassImplInterface)%%
```cs
//@ true
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
%%END_EMBED%%

test interface's own type also implements interface
%%BEGIN_EMBED(If_Nullable_Reference_Test_Statement_TestInterfaceImplInterface)%%
```cs
//@ true
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
%%END_EMBED%%


# Reference
[Locations](Locations.md)
[Class](Class.md)
[Statements](Statements.md)