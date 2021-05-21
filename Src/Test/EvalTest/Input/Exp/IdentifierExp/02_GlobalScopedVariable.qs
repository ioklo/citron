// 21
int x;

x = 1;

{
    int x;
    x = 2; // write
    @$x
}

// read
@$x