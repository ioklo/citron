// helloworld1

seq string F()
{
    @hello
    yield "1";
    @world
    yield "2";
    @1
}

foreach(var i in F());
