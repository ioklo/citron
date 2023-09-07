// 4

int* F(int* i)
{
    return i; // 
}

int x = 3;
var* y = F(&x);
*y = 4;

@$y