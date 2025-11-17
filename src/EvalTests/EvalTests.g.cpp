#include <gtest/gtest.h>
#include <string>
#include <sstream>

#include "Infra/Ptr.h"

#include "Logging/Logger.h"

#include "TextAnalysis/ScriptParser.h"
#include "TextAnalysis/Buffer.h"

#include "SyntaxIR0Translator/SyntaxIR0Translator.h"
#include "IR0IR1Translator/IR0IR1Translator.h"

#include "RSymbol/RFactory.h"
#include "RSymbol/RDecl.h"
#include "RSymbol/RModule.h"

#include "NSymbol/NFactory.h"
#include "NSymbol/NFuncDecl.h"
#include "NSymbol/NModule.h"
#include "NSymbol/NGlobalFuncDecl.h"

#include "MIR/MFactory.h"

#include "QIR/QFactory.h"
#include "QIR/QBlock.h"

#include "QEvaluator/QEvaluation.h"

using namespace std;
using namespace Citron;

class CommandHandler : public IEvalQDataCommandHandler
{
    ostringstream output;

public:
    void Execute(const std::string& command) override
    {
        output << command;
    }

    string GetOutput()
    {
        return output.str();
    }
};

void DoTest(const string& code, const string& expected)
{
    // 1. TextAnalysis
    auto buffer = MakePtr<Buffer>(code);
    BufferPosition pos = buffer->MakeStartPosition();
    Lexer lexer{pos};
    SFactory sFactory;

    auto* sScript = ParseScript(&lexer, sFactory);
    EXPECT_TRUE(sScript);

    string moduleName = "MyModule";
    auto rFactory = MakePtr<RFactory>();
    auto nFactory = MakePtr<NFactory>(rFactory);
    auto logger = MakePtr<Logger>();
    auto mFactory = MakePtr<MFactory>();

    auto eNModuleMData = TranslateSyntaxToNModuleMData(moduleName, {sScript}, {}, logger, rFactory, nFactory, mFactory);
    EXPECT_TRUE(eNModuleMData);
    auto& [nModule, mData] = *eNModuleMData;

    QFactoryPtr qFactory = MakePtr<QFactory>();
    auto eQData = TranslateMDataToQData(mData, qFactory);
    EXPECT_TRUE(eQData);
    auto* qData = *eQData;

    // "Main" 찾기
    NGlobalFuncDecl* nEntry = nullptr;
    for (auto& body : qData->GetAllBodies())
    {
        if (NGlobalFuncDecl* globalFuncDecl = dynamic_cast<NGlobalFuncDecl*>(body.nFuncDecl))
        {
            auto id = body.nFuncDecl->GetNDecl()->GetRDecl()->GetIdentifier();
            if (id == RIdentifier{RName_Normal("Main"), 0, {}})
                nEntry = globalFuncDecl;
        }
    }
    EXPECT_TRUE(nEntry);

    auto commandHandler = MakePtr<CommandHandler>();
    vector<RModule*> rModules{nModule};
    auto eResult = EvaluateQData(rModules, qData, nEntry, commandHandler);
    EXPECT_TRUE(eResult);

    // 
    EXPECT_EQ(commandHandler->GetOutput(), expected);
}
TEST(Assign_Expression, Basic) 
{
    auto code = R"---(void Main()
{
	int a = 0;

	a = 10;
	@$a
}
)---";
    string expected = R"---(10)---";

    DoTest(code, expected);
}

TEST(Assign_Expression, Nested) 
{
    auto code = R"---(void Main()
{
	int a = 0;
	int b = 0;
	int c = 0;

	a = b = c = 1;

	@$a $b $c
}
)---";
    string expected = R"---(1 1 1)---";

    DoTest(code, expected);
}

TEST(Blank_Statement, For) 
{
    auto code = R"---(int Add(int i)
{
    @$i
    return i + 1;
}

void Main()
{
    for(int i = 0; i < 5; i = Add(i));
}
)---";
    string expected = R"---(01234)---";

    DoTest(code, expected);
}

TEST(Blank_Statement, Foreach) 
{
    auto code = R"---(seq string F()
{
    @hello
    yield "1";
    @world
    yield "2";
    @1
}

void Main()
{
    foreach(var i in F());
}

)---";
    string expected = R"---(helloworld1)---";

    DoTest(code, expected);
}

TEST(Block_Statement, Scope) 
{
    auto code = R"---(void Main()
{
    int a = 7;

    {
        int a = 0;
        a = 1;
    }

    @$a
}
)---";
    string expected = R"---(7)---";

    DoTest(code, expected);
}

TEST(Boolean, BinOp) 
{
    auto code = R"---(void Main()
{
    bool b;
    
    b = false; // assignment
    
    @$b ${b = true} $b
    
    @ ${false == false} ${false == true} ${true == false} ${true == true}
    
    @ ${false != false} ${false != true} ${true != false} ${true != true}
}
)---";
    string expected = R"---(false true true true false false true false true true false)---";

    DoTest(code, expected);
}

TEST(Boolean_Literal_Expression, Literal) 
{
    auto code = R"---(void Main()
{
    bool t = true;
    bool f = false;
    @$t $f
}
)---";
    string expected = R"---(true false)---";

    DoTest(code, expected);
}

TEST(Boolean, UnaryOperation) 
{
    auto code = R"---(void Main()
{
    bool t = true;
    bool f = false;

    @$t ${!t} $f ${!f}
}
)---";
    string expected = R"---(true false false true)---";

    DoTest(code, expected);
}

TEST(Box_Expression, Basic) 
{
    auto code = R"---(void Main()
{
	@${*(box 5)}
}
)---";
    string expected = R"---(5)---";

    DoTest(code, expected);
}

TEST(Break_Statement, For) 
{
    auto code = R"---(void Main()
{
    for (int i = 1; i < 6; i++)
    {
        @$i
        if (i % 3 == 0) break;
    }

    @end
}
)---";
    string expected = R"---(123end)---";

    DoTest(code, expected);
}

TEST(Break_Statement, Foreach) 
{
    auto code = R"---(void Main()
{
    foreach (int e in [6, 7, 1, 1, 4])
    {
        @$e
        if (e % 2 == 1) break;
    }

    @end
}
)---";
    string expected = R"---(67end)---";

    DoTest(code, expected);
}

TEST(Break_Statement, NestedFor) 
{
    auto code = R"---(void Main()
{
    for(int i = 0; i < 2; i++)
    {
        foreach (int i in [6, 7, 1, 1, 4])
        {
            @$i
            if (i % 2 == 1) break;
        }
    }
}
)---";
    string expected = R"---(6767)---";

    DoTest(code, expected);
}

TEST(Call_Class_Member_Function_Expression, Instance) 
{
    auto code = R"---(class X
{
    int x;
    
	public void F(int i)
    {
        @$x $i
    }
}

void Main()
{
	X x = new X(2);
    x.F(4);
}
)---";
    string expected = R"---(2 4)---";

    DoTest(code, expected);
}

TEST(Call_Class_Member_Function_Expression, Static) 
{
    auto code = R"---(class X
{
	public static void Print(int a)
	{
		@X: $a
	}
}

void Main()
{
	X.Print(3);
}
)---";
    string expected = R"---(X: 3)---";

    DoTest(code, expected);
}

TEST(Call_Global_Function_Expression, General) 
{
    auto code = R"---(void F(int i, string s, bool b)
{    
    @$i $s $b
}

void Main()
{
	F(1, "2", false);
}
)---";
    string expected = R"---(1 2 false)---";

    DoTest(code, expected);
}

TEST(Call_Global_Function_Expression, Generator) 
{
    auto code = R"---(seq int Func()
{
    yield 1;
    yield 2;
    yield 3;
}

void Main()
{
    foreach(var i in Func())
        @$i
}
)---";
    string expected = R"---(123)---";

    DoTest(code, expected);
}

TEST(Call_Global_Function_Expression, Recursive) 
{
    auto code = R"---(void F(int i, int end)
{    
    if (end <= i) return;

    @$i
    F(i + 1, end);
}

void Main()
{
	F(3, 6);
}
)---";
    string expected = R"---(345)---";

    DoTest(code, expected);
}

TEST(Call_Lambda_Expression, CallInstanceMember) 
{
    auto code = R"---(class C
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
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Call_Lambda_Expression, CallStaticMember) 
{
    auto code = R"---(class C
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
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Call_Lambda_Expression, General) 
{
    auto code = R"---(void Main()
{
	var f = (int i, string s, bool b) => { 
	    @$i $s $b
	};
	
	f(1, "3", true);
}
)---";
    string expected = R"---(1 3 true)---";

    DoTest(code, expected);
}

TEST(Call_Struct_Member_Function_Expression, Instance) 
{
    auto code = R"---(struct S
{
	string s;
	void Print()
	{
		@$s
	}
}

void Main()
{
	var s = S("hello");
	s.Print();
}
)---";
    string expected = R"---(hello)---";

    DoTest(code, expected);
}

TEST(Call_Struct_Member_Function_Expression, Static) 
{
    auto code = R"---(struct S
{
	static void Print()
	{
		@hello
	}
}
)---";
    string expected = R"---(hello)---";

    DoTest(code, expected);
}

TEST(Cast_Boxed_Lambda_To_Func_Expression, Basic) 
{
    auto code = R"---(void Main()
{
	var bf = box () => { return 3; }
	func<int> f = bf; // CastBoxedLambdaToFuncExp(LoadExp(LocalVarLoc("bf")), func<int>)

	@${f()}
}
)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(Cast_Box_Pointer_To_Local_Pointer, AsArgument) 
{
    auto code = R"---(void F(int *s)
{
	*s = 3;
}

void Main()
{
	F(box 3); // box 3의 결과는 temporary variable에 저장되고, statement가 끝날때 사라집니다.
}

)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Cast_Class_Expression, Upcast) 
{
    auto code = R"---(class B { }
class C : B { }

void Main()
{
	var c = new C(); 
	B b = c; // CastClassExp(NewClassExp(C, []), B)
}

)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Cast_Enum_Element_To_Enum_Expression, Basic) 
{
    auto code = R"---(enum E { First, Second(int i) }

void Main()
{
	E.Second s = E.Second(2);
	E e = s; // CastEnumElemToEnumExp(LoadExp(LocalVarLoc(s)), E)
}

)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Class_As_Class_Expression, Basic) 
{
    auto code = R"---(class B { }
class C : B { public int x; }

void Main()
{
	var b = new C(2);
	
	var c = b is C; // c는 nullable C 타입
	if (c != null)	
		@${c.x}
}
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Class_As_Class_Expression, NotRelated) 
{
    auto code = R"---(class C { }
class D { }

void Main()
{
	var c = new C();
	var d = c as D;
	
	if (d == null)
		@ok
}
)---";
    string expected = R"---(ok)---";

    DoTest(code, expected);
}

TEST(Class_Is_Class_Expression, Basic) 
{
    auto code = R"---(class B { }
class C : B { }

void Main()
{
	var b = new C();
	
	var t0 = b is B;
	var t1 = b is C;

	@$t0 $t1
}
)---";
    string expected = R"---(true true)---";

    DoTest(code, expected);
}

TEST(Class_Is_Class_Expression, NotRelated) 
{
    auto code = R"---(class C { }
class D { }

void Main()
{
	var c = new C();
	var t = c is D;
	@$t
}
)---";
    string expected = R"---(false)---";

    DoTest(code, expected);
}

TEST(Class_Is_Interface_Expression, Basic) 
{
    auto code = R"---(interface I { }

class B { }
class C : I { }

void Main()
{
	var b = new C();
	var t = b is I;

	@$t
}
)---";
    string expected = R"---(true)---";

    DoTest(code, expected);
}

TEST(Class_Is_Interface_Expression, NotRelated) 
{
    auto code = R"---(interface I { }
class C { }

void Main()
{
	var c = new C();
	var t = c is I;
	@$t
}
)---";
    string expected = R"---(false)---";

    DoTest(code, expected);
}

TEST(Class_Member_Box_Reference_Expression, Basic) 
{
    auto code = R"---(class C
{
	int x;
	
	box int* GetX()
	{
		return &x; // ClassMemberBoxExp(this, C.x)
	}

	void PrintX()
	{
		@$x
	}
}

void Main()
{
	var c = new C(3);
	box var* pX = c.GetX();
	*pX = 4;
	
	c.PrintX();
}

)---";
    string expected = R"---(4)---";

    DoTest(code, expected);
}

TEST(Class_Member_Variable_Location, Basic) 
{
    auto code = R"---(class X
{
    public int x;
    public X(int x) { this.x = x; }
}

void Main()
{
    X x = new X(2);
    @${x.x}
}
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Class_Member_Variable_Location, Static) 
{
    auto code = R"---(// 11
class C
{
    public static int x = 0;
    public void F()
    {
	    @$x
	}
}

void Main()
{
    C.x = 1;
    @${C.x}

	var c = new C();
	c.F();
}
)---";
    string expected = R"---(11)---";

    DoTest(code, expected);
}

TEST(Command_Statement, Basic) 
{
    auto code = R"---(void Main()
{
    @hi
}
)---";
    string expected = R"---(hi)---";

    DoTest(code, expected);
}

TEST(Command_Statement, Block) 
{
    auto code = R"---(void Main()
{
    // plain, ignore blank lines, trailing blanks
    @{

        <- no ignore 8 blanks
        
        hello world

    }

    // with other statements
    if (true)
    @{
        good
    }
}
)---";
    string expected = R"---(         <- no ignore 8 blanks        hello world        good)---";

    DoTest(code, expected);
}

TEST(Command_Statement, Interpolated) 
{
    auto code = R"---(void Main()
{
    int i = 177;
    string s = "world";
    bool b = false;

    @abc$i abc${s}def $b.84
}
)---";
    string expected = R"---(abc177 abcworlddef false.84)---";

    DoTest(code, expected);
}

TEST(Continue_Statement, For) 
{
    auto code = R"---(void Main()
{
    for (int i = 0; i < 6; i++)
    {
        if (i % 2 == 0) continue;
        @$i
    }
}
)---";
    string expected = R"---(135)---";

    DoTest(code, expected);
}

TEST(Continue_Statement, Foreach) 
{
    auto code = R"---(void Main()
{
    foreach (int e in [6, 7, 1, 1, 4])
    {
        if (e % 2 == 0) continue;
        @$e
    }
}
)---";
    string expected = R"---(711)---";

    DoTest(code, expected);
}

TEST(Continue_Statement, NestedFor) 
{
    auto code = R"---(void Main()
{
    for(int i = 0; i < 2; i++)
    {
        foreach (int i in [6, 7, 1, 1, 4])
        {
            if (i % 2 == 0) continue;
            @$i
        }
    }
}
)---";
    string expected = R"---(711711)---";

    DoTest(code, expected);
}

TEST(Enum, Complex) 
{
    auto code = R"---(// 선언
enum Coord2D<T>
{
    Rect(T x, T y),            // 기본 C syntax와 비슷한 느낌을 주려고 콤마로 구분합니다
    Polar(T radius, T angle),
}

int GetLengthSq(Coord2D<int> m)
{
    if (m is .Rect)
        return m.x * m.x + m.y * m.y;

    else if (m is .Polar) 
        return m.radius * m.radius;
}

void Main()
{
    Coord2D<int> m = .Rect(20, 30); // 타입힌트가 있어서 Coord2D<int>.Rect로 쓰지 않아도 됩니다
    var lenSq = GetLengthSq(m);

    @$lenSq
}
)---";
    string expected = R"---(1300)---";

    DoTest(code, expected);
}

TEST(Enum, ConstructStandalone) 
{
    auto code = R"---(enum E { First }

void Main()
{   
    var e = E.First; // e는 E 타입입니다
    
}
)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Enum, ConstructWIthArgument) 
{
    auto code = R"---(enum E { Second(int x) }

void Main()
{
    var e = E.Second(2); // e는 E 타입입니다
}
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Enum, Generics) 
{
    auto code = R"---(enum Option<T>
{
    None,
    Some(T value)
}

Option<int> i = None;
Option<string> s = Some("Hi");

if (s is Option<string>.Some some)
    @${some.value}

)---";
    string expected = R"---(Hi)---";

    DoTest(code, expected);
}

TEST(Enum, IfTest) 
{
    auto code = R"---(enum E { First, Second(int x) }

void Main()
{
    var e = E.First;

    if (e is E.First)
        @true
    else if (e is E.Second s)
        @{s.x}
}

)---";
    string expected = R"---(true)---";

    DoTest(code, expected);
}

TEST(Enum, SwitchTest) 
{
    auto code = R"---(enum E { First, Second(int x, bool y), Third(string s) }
void Main()
{
    var e = E.Second(2);
    switch (e)
    {
        case E.First:
            @First
        
        case E.Second(var x, _):
            @$x
            
        case E.Third x:
            @${x.s}
    }
}
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Enum, TypeHint) 
{
    auto code = R"---(enum E 
{
    First,
    Second(int x)
}


// 함수 인자
void F1(E e)
{
}

E F2()
{
    return .First;
}

void Main()
{
    // 1. local variable declaration의 initialization 부분
    E e = .Second(2);

    // 2. 함수 인자
    F1(.First);
    
    // 3. 함수 리턴
    e = F2();
}

)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Expression_Statement, AssignAllowed) 
{
    auto code = R"---(void Main()
{
    int a = 0;
    a = 3 + 7;
}

)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Foreach_Statement, Basic) 
{
    auto code = R"---(void Main()
{
    foreach(var e in [1, 2, 3, 4])
        @$e
}
)---";
    string expected = R"---(1234)---";

    DoTest(code, expected);
}

TEST(Foreach_Statement, LambdaAsItem) 
{
    auto code = R"---(void Main()
{
    list<func<void>> fs = [];

    foreach(var x in [1, 2, 3, 4])
        fs.Add(() => @{$x});

    int a = 1, b = 2;
    fs.Add(() => ${a + b}); // a, b두개가 캡쳐되서 들어간다

    foreach(var f in fs)
        f();
}
)---";
    string expected = R"---(12343)---";

    DoTest(code, expected);
}

TEST(Foreach_Statement, Scope) 
{
    auto code = R"---(void Main()
{
    int i = 7;

    foreach(var i in [1, 2, 3, 4]);

    @$i
}
)---";
    string expected = R"---(7)---";

    DoTest(code, expected);
}

TEST(Foreach_Statement, WithManualEnumerable) 
{
    auto code = R"---(struct EnumerableX
{
	EnumeratorX GetEnumerator()
	{
		return EnumeratorX();
	}
}

struct EnumeratorX
{
	int i;
	int count;
	
	EnumeratorX()
	{
		this->i = 0;
		this->count = 5;
	}

	bool Next(out int* x)
	{
		if (i == count) return false;
	
		*x = i;
		i++;
		return true;
	}
}

void Main()
{
	foreach(var i in EnumerableX())
	{
		@$i
	}
}
)---";
    string expected = R"---(01234)---";

    DoTest(code, expected);
}

TEST(Foreach_Statement, WithSeqFunc) 
{
    auto code = R"---(void Main()
{
    seq string F()
    {
        yield "Hello";
        yield "World";
    }

    foreach(var e in F())
        @$e
}
)---";
    string expected = R"---( HelloWorld)---";

    DoTest(code, expected);
}

TEST(For_Statement, Basic) 
{
    auto code = R"---(void Main()
{
    for(int i = 0; i < 5; i++)
        @$i
}
)---";
    string expected = R"---(01234)---";

    DoTest(code, expected);
}

TEST(For_Statement, EmptyAll) 
{
    auto code = R"---(void Main()
{
    int i = 0;
    for(;;)
    {
        if (5 <= i) break;
        @$i
        i++;
    }
}
)---";
    string expected = R"---(01234)---";

    DoTest(code, expected);
}

TEST(For_Statement, EmptyCond) 
{
    auto code = R"---(void Main()
{
    for(int i = 0; ; i++)
    {
        if (5 <= i) break;
        @$i
    }
}
)---";
    string expected = R"---(01234)---";

    DoTest(code, expected);
}

TEST(For_Statement, EmptyContinueExp) 
{
    auto code = R"---(void Main()
{
    for(int i = 0; i < 5;)
    {
        @$i
        i++;
    }
}
)---";
    string expected = R"---(01234 )---";

    DoTest(code, expected);
}

TEST(For_Statement, EmptyInitializer) 
{
    auto code = R"---(void Main()
{
    int i = 0;

    for(; i < 5; i++)
        @$i
}

)---";
    string expected = R"---(01234)---";

    DoTest(code, expected);
}

TEST(For_Statement, Initializer) 
{
    auto code = R"---(void F()
{
    @hi
}

void Main()
{
    int i = 2;
    for(F(); i < 5; i++)
        @$i
}
)---";
    string expected = R"---(hi234)---";

    DoTest(code, expected);
}

TEST(For_Statement, Scope) 
{
    auto code = R"---(void Main()
{
    int i = 3, j = 4;

    for(int i = 0; i < 5; i++)
    {
        int j = i * 2;
        @$i$j
    }

    @$i$j
}
)---";
    string expected = R"---(001224364834)---";

    DoTest(code, expected);
}

TEST(Function, Out) 
{
    auto code = R"---(void F(out int* i)
{
    *i = 2;
}

int j = 3;
F(out &j); // out을 반드시 써줘야 합니다

@$j
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(If_Nullable_Reference_Test_Statement, Basic) 
{
    auto code = R"---(class B { }
class C : B { }

void Main()
{
    B b = new C();
    if (C c = b)
    {
        @succeed
    }
}
)---";
    string expected = R"---(succeed)---";

    DoTest(code, expected);
}

TEST(If_Nullable_Reference_Test_Statement, TestClassImplInterface) 
{
    auto code = R"---(interface I {}
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
)---";
    string expected = R"---(true)---";

    DoTest(code, expected);
}

TEST(If_Nullable_Reference_Test_Statement, TestInterface) 
{
    auto code = R"---(interface I {}
class C : I {}

void Main()
{
	I i = new C();

	if (C c = i) @true
}
)---";
    string expected = R"---(true)---";

    DoTest(code, expected);
}

TEST(If_Nullable_Reference_Test_Statement, TestInterfaceImplInterface) 
{
    auto code = R"---(interface I1 { }
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


)---";
    string expected = R"---(true)---";

    DoTest(code, expected);
}

TEST(If_Statement, Basic) 
{
    auto code = R"---(void Main()
{
    if (1 < 2) @good

    if (1 > 2)
    { 
        @bad
    }
}
)---";
    string expected = R"---(good)---";

    DoTest(code, expected);
}

TEST(If_Statement, BasicElse) 
{
    auto code = R"---(void Main()
{
    if (2 < 1) { }
    else @{pass}
}
)---";
    string expected = R"---(pass)---";

    DoTest(code, expected);
}

TEST(If_Statement, NestedIf) 
{
    auto code = R"---(void Main()
{
    if (false)
        if (true) {}
        else @wrong

    @completed
}
)---";
    string expected = R"---(completed)---";

    DoTest(code, expected);
}

TEST(Inline_Block_Expression, Basic) 
{
    auto code = R"---(void Main()
{
    int s = 2;
    int x = inline {
        return (s + 4) / 2;
    };

    @$x;
}

)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(Inline_Block_Expression, HintTypeFirst) 
{
    auto code = R"---(class B { }
class C1 : B { }
class C2 : B { }

void Main()
{
	B x = inline {
		if (true) 
		{
			return new C1();
		}
		else 
		{
			return new C2();
		}
	};
}
)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Inline_Block_Expression, InferByAssignTargetType) 
{
    auto code = R"---(void Main()
{
	int x;
	x = inline { return 3; };
	@$x
}
)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(Inline_Block_Expression, InferByFunctionParameter) 
{
    auto code = R"---(void F(int x)
{
	@$x
}

void Main()
{
	F(inline { return 3; });
}
)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(Inline_Block_Expression, InferByReturnType) 
{
    auto code = R"---(void Main()
{
	var x = inline { return 3; };
	@$x
}
)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(Integer, BinaryOperation) 
{
    auto code = R"---(void Main()
{
    int i;
    
    i = -3; // assignment
    
    @$i ${i = 4} $i
    
    @ ${3 == 4} ${3 == 3} ${-3 == -3}
    
    @ ${-3 - 3 + 2} ${-3 - 3} ${2 + 4 * -7} ${3 - 3 / 2} ${2 + 7 % 3}
    
    @ ${2 < 4} ${4 < 2} ${2 <= 2} ${1 <= 2} ${3 <= 2} ${-10 > 20} ${20 > -10} ${-10 >= 20} ${20 >= -10} ${28 >= 28}
}
)---";
    string expected = R"---(-3 4 4 false true true -4 -6 -26 2 3 true false true true false false true false true true)---";

    DoTest(code, expected);
}

TEST(Integer, Literal) 
{
    auto code = R"---(void Main() 
{ 
    int i = 1024;
    @$i
}
)---";
    string expected = R"---(1024)---";

    DoTest(code, expected);
}

TEST(Integer_Literal_Expression, Basic) 
{
    auto code = R"---(void Main()
{
	@${123456}
}
)---";
    string expected = R"---(123456)---";

    DoTest(code, expected);
}

TEST(Integer, UnaryOperation) 
{
    auto code = R"---(void Main()
{
    int i = -3;
    @$i ${-i}
    @ ${i++} ${i--} ${++i} ${--i}
}
)---";
    string expected = R"---(-3 3 -3 -2 -2 -3)---";

    DoTest(code, expected);
}

TEST(Lambda_Expression, Basic) 
{
    auto code = R"---(void Main() 
{
    var f = () => { @{hi} };
    f();
}
)---";
    string expected = R"---(hi)---";

    DoTest(code, expected);
}

TEST(Lambda_Expression_Capture, BoxPtr) 
{
    auto code = R"---(void Main()
{
    box int* x = box 0; // heap을 사용하는 버전
    var l = () => *x;
    
    *x = 2;

    // 2
    @${l()}
}
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Lambda_Expression_Capture, Copy) 
{
    auto code = R"---(void Main()
{
    int x = 0;
    var l = () => x;
    x = 1;

    // 0
    @${l()}
}
)---";
    string expected = R"---(0)---";

    DoTest(code, expected);
}

TEST(Lambda_Expression_Capture, LocalPtr) 
{
    auto code = R"---(void Main()
{
    int x = 0;
    int* y = &x;

    // local pointer 함유 lambda, 내부에서밖에 쓸 수 없습니다
    var l = () => *y;
    x = 1;

    // 1
    @${l()}
}
)---";
    string expected = R"---(1)---";

    DoTest(code, expected);
}

TEST(Lambda_Expression_Capture, This) 
{
    auto code = R"---(struct S
{
    int x;
    
    void F()
    {
        var l = () => this->x + 2; // this는 캡쳐대상 S*
        
        x = 3;
        
        // 5
        @${l()}
    }
}


void Main()
{
    var s = S(3);
    s.F();
}
)---";
    string expected = R"---(5)---";

    DoTest(code, expected);
}

TEST(Lambda_Expression, GlobalFunctionAsLambda) 
{
    auto code = R"---(void Func()
{
    @hi
}

void Main()
{
    var f = Func;
    f();
}
)---";
    string expected = R"---(hi)---";

    DoTest(code, expected);
}

TEST(Lambda_Expression, MemberFunctionAsLambda) 
{
    auto code = R"---(struct S
{
	int x;
	
	void Func()
	{
	    @$x
	}
}

void Main()
{
	var s = S(2);
    var f = s.Func;
    f();
}
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Lambda_Expression, Usage) 
{
    auto code = R"---(void Main()
{
    var l1 = (int x) => x + 1; // 본문 축약형
    var l2 = (string s) => { return s; } // 완전한 본문

    var v1 = l1(2);
    var v2 = l2("hi");

    // 3, "hi"
    @$v1, $v2
}
)---";
    string expected = R"---(3 hi)---";

    DoTest(code, expected);
}

TEST(Lifetime_Analysis, ReturnLocalPointer) 
{
    auto code = R"---(int* F(int* i)
{
    return i;
}

int x = 3;
var* y = F(&x);
*y = 4;

@$x
)---";
    string expected = R"---(4)---";

    DoTest(code, expected);
}

TEST(Lifetime_Analysis, StructThisLifeTime) 
{
    auto code = R"---(// 05 
struct S
{
    int x;

    int* GetX()
    {
        return &x; // this의 라이프 타임
    }
}

var s = S(3);
var* x = s.GetX();
*x = 4;

@${s.x}
)---";
    string expected = R"---(4)---";

    DoTest(code, expected);
}

TEST(List_Indexer_Location, General) 
{
    auto code = R"---(void Main()
{
	var a = [1, 2, 3];
	@${a[1]}
}
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(List_Iterator_Expression, Basic) 
{
    auto code = R"---(var l = [1, 2, 3]
foreach(var i in l) // ListIterExp(LocalVarLoc("l"))
{
	@$i
}
)---";
    string expected = R"---(123)---";

    DoTest(code, expected);
}

TEST(Load_Expression, LoadLocalVar) 
{
    auto code = R"---(void Main()
{
	int a = 1;

	// a는 Local_Variable_Location이고, b에 대입하기 위해 내부적으로 load expression을 사용하게 됩니다
	// local_var_decl_stmt("b", load_exp(local_var_loc("a")))
	int b = a;

	@$b
}
)---";
    string expected = R"---(1)---";

    DoTest(code, expected);
}

TEST(Local_Variable_Declaration_Statement, Basic) 
{
    auto code = R"---(void Main()
{
    int x = 0;
    @$x
}
)---";
    string expected = R"---(0)---";

    DoTest(code, expected);
}

TEST(Local_Variable_Declaration_Statement, MultipleVarItemsInferSeparately) 
{
    auto code = R"---(void Main()
{
    int a = 0, x;

    var i = 1, s = "hello", l = [1, 2], b = false;

    x = 2;

    @$i $s ${l[0]} $b $a $x
}
)---";
    string expected = R"---(1 hello 1 false 0 2)---";

    DoTest(code, expected);
}

TEST(Local_Variable_Declaration_Statement, TypeInference) 
{
    auto code = R"---(int MakeInt()
{
    return 3;
}

void Main()
{
    var i = MakeInt();
    var s = "hello";
    var b = false;
    var l = [1, 2, 3, 4];

    var le = l[2];

    @$i $s $b $le
}
)---";
    string expected = R"---(3 hello false 3)---";

    DoTest(code, expected);
}

TEST(Local_Variable_Declaration_Statement, Uninitialized) 
{
    auto code = R"---(void Main()
{
    int a;
    int b = 1;

    a = 0;

    @$a $b
}
)---";
    string expected = R"---(0 1)---";

    DoTest(code, expected);
}

TEST(Local_Variable_Declaration_Statement, VarWithPointerForPointerValue) 
{
    auto code = R"---(void Main()
{
    var i = 3;
    var* x = &i;
    box var* y = box 3;

	int? i = null;
    var? optI = i;
}
)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Local_Variable_Declaration_Statement, VarWithPointerInferenceSeparately) 
{
    auto code = R"---(void Main()
{
    int a = 0
    string b = "hi"
    var* x = &a, y = &b;
    @${*x}, ${*y}
}
)---";
    string expected = R"---(0 hi)---";

    DoTest(code, expected);
}

TEST(Local_Variable_Reference_Expression, Basic) 
{
    auto code = R"---(void Main()
{
    int s = 3;
    var* i = &s; // LocalVarRefExp(LocalVar("i"))

    @{${*i}}
}
)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(Local_Variable_Reference_Expression, Nested) 
{
    auto code = R"---(void Main()
{
	int s = 3;
	int* i = &s;  
	int** j = &i;
	**j = 4;
}
@$s
)---";
    string expected = R"---(4)---";

    DoTest(code, expected);
}

TEST(New_Class_Expression, Basic) 
{
    auto code = R"---(class C
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
)---";
    string expected = R"---(2 3)---";

    DoTest(code, expected);
}

TEST(New_Class_Expression, Generics) 
{
    auto code = R"---(class C<T>
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
)---";
    string expected = R"---(hello)---";

    DoTest(code, expected);
}

TEST(New_Enum_Element_Expression, Basic) 
{
    auto code = R"---(enum E { First, Second(int i) }
void Main()
{
	var e = E.First;
	e = E.Second(2);
}
)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(New_Enum_Element_Expression, Shorthand) 
{
    auto code = R"---(enum E { First, Second(int i) }
void Main()
{
	E e = .First;
	e = .Second(2);
}
)---";
    string expected = R"---( )---";

    DoTest(code, expected);
}

TEST(New_Struct_Expression, Basic) 
{
    auto code = R"---(struct S
{
	int x;
}

void Main()
{
	var s = S(3);
	@${s.x}
}
)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(Nullable_Null_Literal_Expression, Basic) 
{
    auto code = R"---(void Main()
{
	int? i = null;
}
)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Progam, Basic) 
{
    auto code = R"---(void Main()
{
    @Hello World!
})---";
    string expected = R"---(Hello World!)---";

    DoTest(code, expected);
}

TEST(Return_Statement, ControlFlow) 
{
    auto code = R"---(void F()
{    
    @F

    return;

    @wrong
}

void Main()
{
    F();
}

)---";
    string expected = R"---(F)---";

    DoTest(code, expected);
}

TEST(Return_Statement, LambdaReturn) 
{
    auto code = R"---(void Main()
{
    var f = () => {
        return 3;
    };

    @${f()}
}
)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(Return_Statement, ReturnValue) 
{
    auto code = R"---(int F(int i)
{    
    @F

    return i * 2;

    @wrong
}

void Main()
{
    @${F(3)}
}
)---";
    string expected = R"---(F6)---";

    DoTest(code, expected);
}

TEST(Return_Statement, SeqReturn) 
{
    auto code = R"---(seq int F()
{
    for(int i = 0; i < 10; i++)
    {
        yield i;
        if (i == 4) return;
    }
}

void Main()
{
    foreach(var e in F())
    {
        @$e
    }
}
)---";
    string expected = R"---(01234)---";

    DoTest(code, expected);
}

TEST(Static_Box_Reference_Expression, Basic) 
{
    auto code = R"---(struct C
{
	static int x;
	static C()
	{
		x = 3;
	}
}

void Main()
{
	box var* s = &C.x; // reference operator에 box pointer로 만들어달라는 요청을 줍니다
	@${*s}
}
)---";
    string expected = R"---(3)---";

    DoTest(code, expected);
}

TEST(String, Basic) 
{
    auto code = R"---(void Main()
{
    string s = "hi";
    string t = s;
    
    s = "hello"; // 
    
    @$t $s ${s = "world"} $s
    
    string t2 = "${"h"}${"i"}";
    @ ${t == t2} ${s == "world"} ${t != t2} ${s != "world"}
    
    @ ${"one" + "two"}
    
    @ ${"s1" < "s1abcd"} ${"s1abcd" < "s1"} ${"s1" <= "s1abcd"} ${"s1" <= "s1"} ${"s1abcd" <= "s1"}
    
    @ ${"s1" > "s1abcd"} ${"s1abcd" > "s1"} ${"s1" >= "s1abcd"} ${"s1" >= "s1"} ${"s1abcd" >= "s1"}
}

)---";
    string expected = R"---(hi hello world world true true false false onetwo true false true true false false true false true true)---";

    DoTest(code, expected);
}

TEST(String_Expression, InterpolationVariable) 
{
    auto code = R"---(void Main()
{
    int i = 3;
    string x = "hello";
    string y = "$x.$i $$";
    
    @$y
}
)---";
    string expected = R"---(hello.3 $)---";

    DoTest(code, expected);
}

TEST(String_Expression, InterpolationWithBraces) 
{
    auto code = R"---(void Main()
{
    int i = 2;
    string x = "hell";
    string y = "${x + "o"}.${i + 1}";
    
    @${y}
}
)---";
    string expected = R"---(hello.3)---";

    DoTest(code, expected);
}

TEST(String_Expression, Literal) 
{
    auto code = R"---(void Main()
{
    string x = "hello""";
    @$x
}
)---";
    string expected = R"---(hello")---";

    DoTest(code, expected);
}

TEST(Struct, AutoTrivialConstructor) 
{
    auto code = R"---(// 2, 3
struct S
{
    int x;
    int y;
}

var s = new S(2, 3);
@${s.x} ${s.y}
)---";
    string expected = R"---(2 3)---";

    DoTest(code, expected);
}

TEST(Struct, Complex) 
{
    auto code = R"---(public struct B
{
    int a;
}

struct S : B
{
    int x; // default public
    private int y;

    int Sum() // default public
    {
        return a + x + y;
    }

    int GetY() 
    {
        return y;
    }
}

void Main()
{	
	// 일단 다 적고 나중에 분리
	
	var s1 = new S<int>(1, 2, 3); // a, x, y
	@${s1.x}
	@ ${s1.GetY()}
	@ ${s1.Sum()}
	
	S s2 = s1;                // 복사 생성, 오버라이드 불가능
	
	// 고급, box, boxed 타입 S*
	box var* s3 = box S(1, 2, 3);
	
	@${*s3.a}
	s2 = *s3;
}

)---";
    string expected = R"---(2 3 6 1)---";

    DoTest(code, expected);
}

TEST(Struct_Indirect_Member_Box_Reference_Expression, Basic) 
{
    auto code = R"---(struct S
{
	int i;
}

void Main()
{
	box S* bs = new S(3);
	box int* x = &bs->i; // StructIndirectMemberBoxRefExp(bs, S.i)

	*x = 2;

	@${bs->i}
}
)---";
    string expected = R"---(2)---";

    DoTest(code, expected);
}

TEST(Struct_Member_Box_Reference_Expression, Basic) 
{
    auto code = R"---(struct A { int i; }
struct S { A a; }

void Main()
{
	box var* s = box S(A(3));
	box var* x = &s->a.i; // StructMemberBoxRefExp(StructIndirectMemberBoxRefExp(s, S.a), S.i)
	*x = 5;

	@${s->a.i}
}

)---";
    string expected = R"---(5)---";

    DoTest(code, expected);
}

TEST(Task, Async) 
{
    auto code = R"---(void Main()
{
    int sum = 0, sum2 = 0;

    await 
    {
        async
        {
            // yield를 부름으로써 제어가 다음 async로 넘어간다
            @yield
    
            for(int i = 0; i < 100; i++)        
                sum = sum + i;
    
            @$sum
        }
    
        async
        {
            for(int i = 0; i < 101; i++)
                sum2 = sum2 + i;
    
            @$sum2
        }
    }
}
)---";
    string expected = R"---(50504950)---";

    DoTest(code, expected);
}

TEST(Task_Await, LocalScope) 
{
    auto code = R"---(void F()
{
    // await가 없으므로 기다리지 않는다
    async 
    {
        @yield
        @wrong
    }
}

void Main()
{
    await
    {    
        F();
    }
}
)---";
    string expected = R"---()---";

    DoTest(code, expected);
}

TEST(Task_Statement, Basic) 
{
    auto code = R"---(// 49505050

void Main()
{
    int sum = 0, sum2 = 0;

    // 동시에 실행된다는걸 테스트하려면 Event객체를 만들어서 주고 받으면 될것 같다
    await 
    {
        task
        {
            for(int i = 0; i < 100; i++)
                sum = sum + i;                
        }

        task
        {
            for(int i = 0; i < 101; i++)
                sum2 = sum2 + i;                
        }
    }

    @$sum$sum2
}
)---";
    string expected = R"---(49505050)---";

    DoTest(code, expected);
}

