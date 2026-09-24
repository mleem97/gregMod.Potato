# AGENTS.md — Notes for AI agents (gregMod.Potato)

Repo: gregMod.Potato · License: Apache-2.0 · Version: see `VERSION`.

## Duties

1. **Read first:** `README.md`, `docs/INDEX.md` — only then make changes.
2. **Do not commit secrets** (keys, tokens, `.env`). Use keys only via environment variables.
3. **Preserve history:** no `push --force`, no history rewrite without instruction.
4. **Back up changes:** before reporting done, build the mod (`dotnet build gregMod.Potato.csproj -c Release` or `./build.sh Potato`).
5. **Keep docs in sync:** for new features update `README.md` + `docs/` + `CHANGELOG.md` (Unreleased).
6. **Conventions:** Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:` …), one logical change per commit.
7. **When unsure:** stop and ask instead of guessing — especially for deletes, migrations, CI.

## Hard rules

- **Never** call `QualitySettings.SetQualityLevel` (stomps game tiers) — granular levers only.
- **Never** touch gregCore types outside `src/Core/PotatoBridge.cs` (JIT split — mod must run without gregCore.dll).
- **Never** commit `references/*.dll`, `bin/`, `obj/` (see `.gitignore`).
- Every lever: snapshot original → apply → restore. One failing lever never blocks the rest (`try/catch` everywhere).
- Camera far plane stays untouched (gameplay).

## Layout

- `src/PotatoMod.cs` — MelonMod entry, prefs, toggle, scene hooks.
- `src/Core/GregHost.cs` — soft probe (no gregCore types).
- `src/Core/PotatoBridge.cs` — ONLY gregCore-touching code.
- `src/Core/PotatoSettings.cs` — pref data (no Unity refs).
- `src/Core/PotatoApplier.cs` — snapshot/apply/restore + HDRP + scene objects.
- `docs/POTATO_SPEC.md` — behavior spec; `docs/GREGCORE_GAPS.md` — upstream proposal.
