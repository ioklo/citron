// 3

// Assign이 없으면 Copy-Capture를 한다
{
    int x = 3;

    var f = () => { @{$x} };

    x = 4;

    f();
}

