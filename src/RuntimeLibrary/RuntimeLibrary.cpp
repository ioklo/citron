#include <string>
#include <stdio.h>

using namespace std;

extern "C" void citron_command(void* cmd)
{
    auto* str = (string*)cmd;
    printf("%s", str->c_str());
}

extern "C" void citron_bool_to_string(void* dest, bool value)
{
    new (dest) string{value ? "true" : "false"};
}

extern "C" void citron_int_to_string(void* dest, int value)
{   
    new (dest) string{to_string(value)};
}

// TODO: string_destruct를 만들어야 함
extern "C" void citron_string_ctor(void* dest, const char* text)
{
    new (dest) string{text};
}

extern "C" void citron_string_copy_ctor(void* dest, void* src)
{
    auto* src_s = (string*)src;
    new (dest) string{*src_s};
}

extern "C" void citron_string_move_ctor(void* dest, void* src)
{
    auto* src_s = (string*)src;
    new (dest) string{move(*src_s)};
}

extern "C" void citron_string_copy_assign(void* dest, void* src)
{
    auto* src_s = (string*)src;
    auto* dest_s = (string*)dest;
    *dest_s = *src_s;
}

extern "C" void citron_string_move_assign(void* dest, void* src)
{
    auto* src_s = (string*)src;
    auto* dest_s = (string*)dest;
    *dest_s = std::move(*src_s);
}

extern "C" void citron_string_dtor(void* str)
{
    auto* s = (string*)str;
    s->~string();
}

extern "C" void citron_string_concat(void* dest, void* s0, void* s1)
{   
    auto* s0_s = (string*)s0;
    auto* s1_s = (string*)s1;
    new (dest) string{*s0_s + *s1_s};
}

//StringLessThan,
extern "C" bool citron_string_less_than(void* s0, void* s1)
{
    auto* s0_s = (string*)s0;
    auto* s1_s = (string*)s1;
    return *s0_s < *s1_s;
}

//StringGreaterThan,
extern "C" bool citron_string_greater_than(void* s0, void* s1)
{
    auto* s0_s = (string*)s0;
    auto* s1_s = (string*)s1;
    return *s0_s > *s1_s;
}

//StringLessThanOrEqual,
extern "C" bool citron_string_less_than_eq(void* s0, void* s1)
{
    auto* s0_s = (string*)s0;
    auto* s1_s = (string*)s1;
    return *s0_s <= *s1_s;
}

//StringGreaterThanOrEqual,
extern "C" bool citron_string_greater_than_eq(void* s0, void* s1)
{
    auto* s0_s = (string*)s0;
    auto* s1_s = (string*)s1;
    return *s0_s >= *s1_s;
}

//StringEquals,
extern "C" bool citron_string_eq(void* s0, void* s1)
{
    auto* s0_s = (string*)s0;
    auto* s1_s = (string*)s1;
    return *s0_s == *s1_s;
}

extern "C" void Main();

extern "C" int main()
{
    Main();
    return 0;
}