# Member Translation

Status: draft current
Area: compiler, SyntaxIR0Translator
Keywords: member translation, IrExpAndMemberName, static member, instance member, verifier

## Current Direction
- Do not fully unify all `IrExpAndMemberName...` translators.
- Extract only common interpretation steps into helpers.
- Keep result construction in each translator.
- Diagnose static/instance constraints and kind mismatch immediately during translation.
- Use verifier only as debug/CI safety net, not as the primary user-facing diagnostic path.

## Helper Direction
Introduce shared helpers such as:

```text
ResolveMemberOnStaticBase(...)
```

These helpers should perform common member interpretation work while allowing each translator to build its own output shape.

## Error Model
- Prefer typed getter or wrapper APIs.
- Use `expected<T, Error>`-style error modeling where translation can fail.
- Follow repo naming rule: `std::expected` local variables use `e_` prefix.

## Hot Areas
- `src/SyntaxIR0Translator/IrExpAndMemberName*`
- `src/SyntaxIR0Translator/`
- `src/R...`

## History
- `git history: ai/implementations/member-translation-snapshot.md`
