// 4

// 기본적으로 ref capture를 한다
{
    int x = 3;

    var f = () => { @{$x} };

    x = 4;

    f();
}

