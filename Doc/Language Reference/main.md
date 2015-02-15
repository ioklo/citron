# 둘러 보기 #

## Hello, World! ##

	[a.gum]

	// 전역 namespace의 Main함수
	void Main() 
	{
		printf ("Hello, World!");
	}

global scope에서는 클래스, 함수 정의만 가능합니다.
	

# Reference #

## 확장자 ##

- .gum : 코드 파일 
- .gum.obj : 묶이기 전 중간 파일
- .gum.class : assembly 단위 byte-code 파일  

## Concrete Syntax ##

### 코드 파일 ###

클래스와 함수를 정의하는 부분 입니다. 파일의 이름은 네임스페이스와는 관련이 없습니다.

### 네임스페이스 ###

- namespace 문

프로그램에서 쓰일 수 있는 여러 클래스 또는 정적 변수/함수 이름들을 겹치지 않게 분할해 주는 역할을 합니다. 네임스페이스는 중복으로 정의할 수 있지만, 그 안의 이름들이 겹치게 되면 함께 묶는 과정에서 충돌 에러가 날 수 있습니다.

	[a.gum]
	namespace Gum.Test
	{
		int a;       // Gum.Test.a 변수 정의
		void Main(); // Gum.Test.Main 함수 정의 
	}

	namespace Gum
	{
		int a; // ok: Gum.a 변수 정의
	}

	[b.gum]
	namespace Gum.Test
	{
		int b;    // Gum.Test.b 변수 정의
		int Main; // error: Gum.Test.Main 함수와 충돌
	}

 - using문

현재 Scope에서 특정 namespace를 명시적으로 사용하지 않아도 되게 해줍니다.  
 
혼선을 방지 하기 위하여 Scope의 시작부분에만 정의할 수 있습니다. 

	[a.gum]
	namespace Gum.Test
	{
		int a;		
	}

	// Error: Scope 처음 부분이 아님	
	using Gum.Test;

	void Main()
	{
		using Gum.Test;

		a = 3; // ok: Gum.Test.a를 가리킴 
	}

### 타입 정의 ###

#### Reference Type ####

해당 타입으로 선언된 변수의 값이 레퍼런스인 타입을 의미합니다.  
기본적으로 Object 타입을 상속 받습니다. 

	class ReferenceType
	{
		public virtual void MemberFunc();
	}

#### Value Type ####

값을 주고 받는 타입입니다. Object를 상속받지 않습니다

	struct MyStruct
	{
		public int a;
	}

인터페이스를 적용할 수 있습니다

상속을 받을 수 있습니다. 단 캐스팅은 slicing 됩니다

	struct MyChild : MyStruct
	{
		public int b;
	}

	MyStruct ms = new MyChild(); // b is gone..

#### Property ####

Variable과 Property의 구분을 갖지 않습니다.

	class EmployInfo
	{
		public string Name.Get() { return "Employ Name"; }
		public int Age; 
	}

	EmployInfo info;
	info.Name 

#### Interface ####

#### Reference arguments ####

	void SomeFunc(ref int i)
	{
		i = 3; // Setter 호출 or i의 메모리 주소에 3 assign
	}

	void Main()
	{
		EmployInfo ei;
		SomeFunc(ref ei.Age); // ok ei.Age has Setter, 구현은 복잡해질 수 있다
	}

#### Boxing ####

ValueType<T>를 기본으로 제공합니다.

	class ValueType<T>
	{
		public T Value;
	}

	// Boxing (type-checked)
	var v = new ValueType<MyStruct>(new MyStruct());

#### Unboxing ####

	ValueType<MyStruct> v = ...;
	MyStruct struct = v.Value;

### Function ###

	void Main(string str)
	{
	}

	class SomeClass
	{
		public void InnerFunc(string str);
	}

	SomeClass cls = new SomeClass();

	Func<void, string> MyFunc = Main;
	Func<void, string> MyFunc2 = cls.InnerFunc; 
	MyFunc("this");

Main의 타입은 `Func<void, string>`입니다. InnerFunc의 타입은 `MemberFunc<SomeClass, void, string>` 입니다

#### Argument Variation ####



### Generics ###

void는 오직 return type으로만 사용할 수 있습니다.

	// Reference Type with Type Variable 
	struct Tuple<T1, T2>
	{
		public T1 t1;
		public T2 t2;
	}

	Tuple<void, string> t; // error: void cannot be used as type of member variable;

	class CustomFunc<RetType, Arg1>
	{
		public RetType Func1(Arg1 arg);
	}

	CustomFunc<void, int> i; // ok: void only used for return type.

### Variable ###


### Enumeration ###

	enum Color
	{
		Yellow, Margenta, Green
	}

확장된 enum

	enum Coordinate
	{
		XY(int x, int y),
		Polar(int angle, int radius)
	}

	Coodinate coordinate = new Coordinate.XY(3, 4);
	switch(coordinate)
	{
	case Coordinate.XY(x, y): 
		printf("%d", x); 
		break;

	case Coordinate.Polar(angle, radius):
		printf("%d", angle, radius); 
		break; 
	}

### 타입 ###

- 기본 타입
  - int: value type
  - byte: value type
  - string: reference type
  - void: void type

- 배열    
  - Array<T>


일단 1차 스펙

- 확장자: 단일 코드 컴파일 (.gum -> .gum.obj)
- 파일 안에 함수와 변수만 선언 가능 네임스페이스 없음 
- no ref argument, param value => no printf 
- array
- no class / struct / interface / enum
- no generics
 - no boxing / unboxing
 - no Func type
- void / byte / int 

// 구현 순서

기본적인 것

- variable declaration (int a;)
- const-value expression (3, 4, 5)
- unary/binary expression (= - ~ + - * / += -= *= /=)
- function declaration (void main() { })
- function call expression main();
- function local variable declaration
- return statement
- if statement
- for/do/while statement
- break statement 
- continue statement

- namespace
- using
- class declaration
- class property declaration
- class member function declaration
- dot property
- member function call 
- string built-in class
- class virtual member function
- struct declaration
- enum declaration
- switch-case statement 
- function ref argument

- generics
- array built-in class
- boxing / unboxing
- function type (delegate)
- function variant arguments
- printf
- interface

- extended enum 
- extended switch-case statement

### 타입 ###

- 기본 타입
  - int: value type
  - byte: value type
  - string: reference type
  - void: void type

- 배열    
  - Array<T>


