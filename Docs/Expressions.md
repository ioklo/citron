Expression은 값을 계산합니다. 
Expression은 계산된 값을 저장할 위치를 갖고 있지 않습니다.
Expression은 런타임에 실행환경이 가리키는 result에 계산된 값을 저장합니다.

[Load_Expression](Load_Expression.md)
[Assign_Expression](Assign_Expression.md)
[Box_Expression](Box_Expression.md)
[Static_Box_Reference_Expression](Static_Box_Reference_Expression.md)
[Class_Member_Box_Reference_Expression](Class_Member_Box_Reference_Expression.md)
[Struct_Indirect_Member_Box_Reference_Expression](Struct_Indirect_Member_Box_Reference_Expression.md)
[Struct_Member_Box_Reference_Expression](Struct_Member_Box_Reference_Expression.md)
[Local_Variable_Reference_Expression](Local_Variable_Reference_Expression.md)
[Cast_Boxed_Lambda_To_Func_Expression](Cast_Boxed_Lambda_To_Func_Expression.md)

[Boolean_Literal_Expression](Boolean_Literal_Expression.md)
[Integer_Literal_Expression](Integer_Literal_Expression.md) 
[String_Expression](String_Expression.md)
[List_Expression](List_Expression.md)
[List_Iterator_Expression](List_Iterator_Expression.md)
[Call_Global_Function_Expression](Call_Global_Function_Expression.md)

[New_Class_Expression](New_Class_Expression.md)
[Call_Class_Member_Function_Expression](Call_Class_Member_Function_Expression.md)
[Cast_Class_Expression](Cast_Class_Expression.md)

[Class_Is_Class_Expression](Class_Is_Class_Expression.md)
[Class_As_Class_Expression](Class_As_Class_Expression.md)

[Class_Is_Interface_Expression](Class_Is_Interface_Expression.md)
[Class_As_Interface_Expression](Class_As_Interface_Expression.md)

[InterfaceIsClassExp](InterfaceIsClassExp.md)
[InterfaceAsClassExp](InterfaceAsClassExp.md)

[InterfaceIsInterfaceExp](InterfaceIsInterfaceExp.md)
[InterfaceAsInterfaceExp](InterfaceAsInterfaceExp.md)

[EnumIsEnumElemExp](EnumIsEnumElemExp.md)
[EnumAsEnumElemExp](EnumAsEnumElemExp.md)

[New_Struct_Expression](New_Struct_Expression.md)
[Call_Struct_Member_Function_Expression](Call_Struct_Member_Function_Expression.md)

[New_Enum_Element_Expression](New_Enum_Element_Expression.md)
[Cast_Enum_Element_To_Enum_Expression](Cast_Enum_Element_To_Enum_Expression.md)

[New_Nullable_Expression](New_Nullable_Expression.md)
[Nullable_Null_Literal_Expression](Nullable_Null_Literal_Expression.md)

[Lambda_Expression](Lambda_Expression.md)
[Call_Lambda_Expression](Call_Lambda_Expression.md)
[Inline_Block_Expression](Inline_Block_Expression.md)

[Call_Internal_Unary_Operator_Expression](Call_Internal_Unary_Operator_Expression.md)
[Call_Internal_Unary_Assign_Operator_Expression](Call_Internal_Unary_Assign_Operator_Expression.md)
[Call_Internal_Binary_Operator_Expression](Call_Internal_Binary_Operator_Expression.md)

# Value
value는 데이터입니다. value는 type으로 대표됩니다. 같은 type의 value는 value를 저장하기 위해 필요한 메모리의 크기가 같습니다. value는 자체로 저장될 위치를 갖고 있지 않습니다. value를 만들어내는 expression이 쓰이는 위치에 따라 value이 저장되는 행동이 결정됩니다.
```
int i;
i = 3;

var c = new C();
c.x = 3;
```
value `3`은 저장될 위치를 갖고 있지 않습니다. `3`이 `int i = ` 옆에 온 경우에는 stack에,  `c.x = ` 옆에 온 경우에는 heap에 저장됩니다.
# Location
value가 저장될 위치(storage)를 나타냅니다. 대입 대상으로 쓸 수 있습니다. 최종 위치는 stack, heap, static 중 하나가 됩니다.
# Expression
expression은 syntax는 똑같아도, 쓰이는 위치에 따라 value가 될 수도, location이 될 수도 있습니다.
```
int i = 0; // 지역 변수 i에 대하여 
i = i;     // 대입
```
위의 두번째 줄 대입 구문에서 첫번째 `i`는 location을 나타내고, 두번째 `i`는 value를 나타냅니다.

# Local Scope


## Nested Local Scope
`{ }` 를 사용해서 Local Scope안에 임의로 Local Scope를 추가할 수 있습니다 
```cs
void Main()
{
    int x = 0;
    {
        int x = 0;
        x = 3;
    }
    @$x
}
```
위의 코드는 0을 출력합니다.

scope를 추가하고 같은 이름의 변수를 만들면, 상위 범위에 있는 이름이 가려집니다. 이름이 같은 두 변수는 다른 위치를 가리키고 있습니다. 

`{ }` 뿐 아니라, `if`, `for`,  `while` , `foreach` 등은 본문에 Local Scope를 추가합니다.
