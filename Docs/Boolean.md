# General
참 거짓을 나타내는 struct type입니다. 

# Type Expression
심볼 경로 필요 없이 `bool` 이라고 쓰면 됩니다. 실제 타입은 `System.Boolean`입니다

# Value
`true`, `false` 두가지 값만 존재합니다. struct type이지만 생성자를 써서 만들지 않고, 내장된 literal을 사용해서 생성합니다.

%%TEST(Literal, true false)%%
```
void Main()
{
    bool t = true;
    bool f = false;
    @$t $f
}
```

# bool형을 인자로 받는 미리 정의된 이항 연산

bool형 간에는 두 값이 같은지 비교할 수 있습니다.
- (bool, bool) -> bool: { == }

%%TEST(BinOp, false true true true false false true false true true false)%%
```
void Main()
{
    bool b;
    
    b = false; // assignment
    
    @$b ${b = true} $b
    
    @ ${false == false} ${false == true} ${true == false} ${true == true}
    
    @ ${false != false} ${false != true} ${true != false} ${true != true}
}
```
