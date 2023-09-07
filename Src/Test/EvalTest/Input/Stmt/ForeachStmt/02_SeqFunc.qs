// HelloWorld

void Main()
{
    seq string F()
    {
        yield "Hello";
        yield "World";
    }

    foreach(var e in F())
        @$e
}