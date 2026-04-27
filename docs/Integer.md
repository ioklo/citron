# General
32비트 정수형을 나타내는 struct type입니다. \[-2,147,483,648, 2,147,483,647\] 범위를 표현할 수 있습니다.

# Type Expression
심볼 경로 필요 없이 `int` 라고 쓰면 됩니다. 실제 타입은 `System.Int32`입니다

# Value
struct type이지만 생성자를 써서 만들지 않고,  숫자 literal을 사용해서 생성합니다.

<!--BEGIN_EMBED(Integer_Literal)-->
```cs
//@ 1024
void Main() 
{ 
    int i = 1024;
    @$i
}
```
<!--END_EMBED-->

# 정수형 간에 미리 정의된 단항 연산, 대입 연산
<!--BEGIN_EMBED(Integer_UnaryOperation)-->
```cs
//@ -3 3 -3 -2 -2 -3
void Main()
{
    int i = -3;
    @$i ${-i}
    @ ${i++} ${i--} ${++i} ${--i}
}
```
<!--END_EMBED-->

# 정수형 간에 미리 정의된 이항 연산
정수형끼리는 덧셈, 뺄셈, 곱셈, 나눗셈, 나머지 연산을 할 수 있습니다. 정수형끼리 비교도 가능합니다.
- (int, int) -> int: { +, -, \*, /, % }
- (int, int) -> bool: { <, >, <=, >=, == }

<!--BEGIN_EMBED(Integer_BinaryOperation)-->
```cs
//@ -3 4 4 false true true -4 -6 -26 2 3 true false true true false false true false true true
void Main()
{
    int i = uninit;
    
    i = -3; // assignment
    
    @$i ${i = 4} $i
    
    @ ${3 == 4} ${3 == 3} ${-3 == -3}
    
    @ ${-3 - 3 + 2} ${-3 - 3} ${2 + 4 * -7} ${3 - 3 / 2} ${2 + 7 % 3}
    
    @ ${2 < 4} ${4 < 2} ${2 <= 2} ${1 <= 2} ${3 <= 2} ${-10 > 20} ${20 > -10} ${-10 >= 20} ${20 >= -10} ${28 >= 28}
}
```
<!--END_EMBED-->
