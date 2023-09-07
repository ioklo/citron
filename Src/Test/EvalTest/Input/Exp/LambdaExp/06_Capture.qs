// 3

// 기본적으로 복사 capture를 한다
void Main()
{
    int x = 3;

    var f = () => { @{$x} };

    x = 4;

    f();
}

