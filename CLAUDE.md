@AGENTS.md
@Docs/project_instructions.md

## Claude Code only

- The instructions above were written for a Claude Project with a curated PK. Here the repo itself is the context: governed docs live in `Docs/` (`SSoT_INDEX.md`, `ssot_manifest.yaml`, `CURRENT_STATE.md`, `changelog-ssot.md`, `planning/active/`). Read files from disk instead of requesting them.
- The PK layers, `PK_Manifest.md` lifecycle rows and the File Request Protocol do not apply in Claude Code: there are no stale copies. The freshness gate becomes: code truth is the working tree plus `git log` / `git status`. When `CURRENT_STATE.md` lags behind Git, say so.
- MidiGenPlay internals live at `D:\Projects\MidiGenPlay\MidiGenPlay` (its own Git repo). The boundary rule still holds: read them only for integration questions, and do not edit that package from this project.
- Claude Code cannot run Unity, play the game or run the test suite. Smoke tests and compile results stay unverified until the user reports them.
- When the user asks for edits to be applied directly, apply them instead of writing a runbook. Otherwise keep the runbook format.
- Unity creates a `.meta` file for every new file under `Assets/`. Never hand-write or delete `.meta` files; if a file is moved or deleted, its `.meta` goes with it.
- The user writes in Spanish; answer in Spanish unless asked otherwise.
