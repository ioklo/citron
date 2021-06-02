// 345

void F(int i, int end)
{    
    if (end <= i) return;

    @$i
    F(i + 1, end);
}

F(3, 6);