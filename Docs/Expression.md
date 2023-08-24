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
