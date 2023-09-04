# General
인라인 블록은 함수 본문에서 값으로 바로 평가되는 블록입니다. 함수 호출 오버헤드가 생기지 않습니다.

```cs

void Main()
{
    int s = 2;
    int x = inline {
        return (s + 4) / 2;
    }

    @$x;
}

```