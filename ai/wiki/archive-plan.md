# Archive Plan

Status: draft plan

이 문서는 deprecated `ai/specs/`와 `ai/implementations/`를 archive로 옮길 때의 기준과 절차를 정리한다.

## Archive Target
Archive candidates:
- `ai/specs/`
- `ai/implementations/`

Do not archive yet. They remain deprecated references until wiki coverage is checked.

## Target Shape
Proposed archive location:

```text
ai/archive/specs/
ai/archive/implementations/
```

The move should preserve file paths below each directory where practical.

## Preconditions
Before moving:
- every deprecated source listed in `ai/wiki/migration-plan.md` has a corresponding wiki topic;
- `ai/wiki/current-decisions.md` summarizes major active decisions;
- `ai/wiki/language/index.md` and `ai/wiki/compiler/index.md` point to migrated topics;
- no active workflow tells agents to add new content to `ai/specs/` or `ai/implementations/`;
- recent notes needed for current decisions are linked from wiki topic history sections.

## Archive Procedure
1. Check `git status` is clean.
2. Move `ai/specs/` to `ai/archive/specs/`.
3. Move `ai/implementations/` to `ai/archive/implementations/`.
4. Update `ai/index.md`.
5. Update `ai/wiki/home.md`.
6. Search for stale links to `ai/specs/` and `ai/implementations/`.
7. Keep history links in wiki pages pointing to archive paths after the move.

## Do Not
- Do not delete the old content.
- Do not move `ai/notes/`; notes are history and remain active.
- Do not move `ai/process/` until process wiki has fully replaced it and the team explicitly decides to archive it.
