// Hi

enum Option<T>
{
    None,
    Some(T value)
}

Option<int> i = None;

Option<string> s = Some("Hi");

if (s is Option<string>.Some some)
    @${some.value}
