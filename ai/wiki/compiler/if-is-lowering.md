# If / Is Lowering

Status: draft current
Area: compiler, MIR, SyntaxIR0Translator
Keywords: if, is, binding, IfBind, lowering, lifetime

## Current Direction
- Parser keeps `is` expression as a unified surface form.
- Semantic analysis checks whether an `is` expression contains binding and whether the context allows it.
- Binding `is` is allowed only at top-level of `if` condition in the current language direction.
- MIR lowering may use a bind-oriented node when conditional binding lifetime must be represented.
- Internal node name `IfBind` is preferred over `IfTest` for this case.

## Binding Lifetime
Body-visible bindings are computed as bindings guaranteed on every true path of the condition.

```citron
if (a is D d && d.x > 0)
{
    use(d); // ok
}
```

```citron
if (a is D d || cond)
{
    use(d); // error
}
```

## Hot Areas
- `src/Syntax/`
- `src/SyntaxIR0Translator/`
- `src/MIR/`

## History
- `git history: ai/specs/language/if-and-is.md`
- `git history: ai/implementations/if-is-binding-snapshot.md`
