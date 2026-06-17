
# Project Folder Hierarchy Heuristic

A reusable guideline for organizing medium–large Unity or general game/dev projects.  
It emphasizes **clarity, SOLID boundaries, and predictable dependencies**.

---

## Core Principle: *Domain → Layer → Role*

Classify everything by:
1. **Domain** — *what it belongs to* (feature/capability).  
2. **Layer** — *what it is / when it lives* (config vs. runtime vs. logic vs. presentation).  
3. **Role** — *what it does* (manager, controller, view, action, table, etc.).

This yields a one-way dependency flow and a structure that’s easy to navigate.

---

## Domains (examples)

- `Cards`, `Characters`, `Encounters`, `SectorMap`, `UI`, `Audio`, `Backgrounds`, `MusicGeneration`  
- Cross-cutting: `Enums`, `Interfaces`, `Utils`, `Extensions`

> **Tip:** Favor feature folders (domains) over technology folders.  
> Your future self will search by **feature**, not by *“all scripts with MonoBehaviour”*.

---

## Layers

- **Data** — Design-time ScriptableObjects & lookups (`*Data`, `*Database`, `*Table`).  
- **State** — Serializable runtime/save payloads only (`*State`, *no logic*).  
- **Logic** — Orchestration/services (`Managers`) and feature behavior (`Controllers`, processors).  
- **Presentation** — Views and widgets (`UI`, `<Domain>/Views`, VFX).

> Keep layers pure: Data assets shouldn’t know about scene objects; Views shouldn’t carry business rules.

---

## Roles (responsibility granularity)

- **Manager** — App/scene-level coordinator that composes subsystems.  
- **Controller** — Behavior for a feature or widget.  
- **Action / Processor** — Small units of behavior (e.g., card actions).  
- **Database / Table** — Read-only lookup assets.  
- **View** — Visual adapter (icons, lines, animations).

---

## Dependency Direction

```
Enums / Interfaces
        ↓
Data (ScriptableObjects, lookups)
        ↓
State (serializable runtime payloads)
        ↓
Logic (Managers / Controllers / Services)
        ↓
Presentation (Views / UI / VFX)
```

- Cross-cutting folders (`Enums`, `Interfaces`, `Utils`, `Extensions`) only point **down**.  
- Domains do not import each other’s internals—communicate through interfaces or data.

---

## Decision Rules (copy/paste)

- **Is it a ScriptableObject config or lookup?** → `Data/<Domain>/Configs` (or `…/Databases`/`…/Tables`).  
- **Is it serializable runtime data?** → `Data/<Domain>/State` as `*State`. No MonoBehaviours.  
- **Is it a coordinator/service?** → `Managers`.  
- **Is it behavior for a single feature/widget?** → `<Domain>/Controllers` or `UI/Controllers`.  
- **Is it purely visual?** → `<Domain>/Views` or `UI/…`.  
- **Is it a helper/shared type?** → `Utils`, `Extensions`, `Enums`, or `Interfaces`.

---

## Naming Conventions

- **`*Data`** → ScriptableObject (design-time).  
- **`*State`** → Serializable runtime/save container (no logic).  
- **`*Database` / `*Table`** → Asset that maps keys/enums → values/objects.  
- Prefer **PascalCase** and avoid spaces in folder names (`MusicGeneration`, not `Music Generation`).  
- Be consistent: `RewardDatabase`, not `RewardDataBase`.

---

## Template Structure (Unity example)

```
Assets/
└─ Scripts/
   ├─ Cards/
   │  ├─ Controllers/
   │  ├─ Views/
   │  └─ Actions/
   ├─ Characters/
   │  ├─ Audience/
   │  └─ Musicians/
   ├─ Encounters/
   ├─ SectorMap/
   │  └─ Views/
   ├─ UI/
   │  └─ Tooltips/
   ├─ Managers/
   ├─ Controllers/
   ├─ Data/
   │  ├─ Core/
   │  │  ├─ GameplayData.cs
   │  │  ├─ SceneData.cs
   │  │  └─ PersistentGameplayData.cs
   │  ├─ Cards/
   │  │  ├─ Configs/
   │  │  └─ Databases/
   │  ├─ Characters/
   │  │  ├─ Audience/
   │  │  └─ Musicians/
   │  ├─ Encounters/
   │  └─ SectorMap/
   │     ├─ Configs/
   │     └─ State/
   ├─ Enums/
   ├─ Interfaces/
   ├─ Utils/
   └─ Extensions/
```

---

## Assembly Definitions (optional)

If you use asmdefs, keep dependencies one-way and thin:
- `Data.*` assemblies have **no** dependencies.  
- `Logic`/`UI` assemblies depend on `Data.*` and on shared primitives (`Enums`, `Interfaces`).  
- Cross-domain communication goes through interfaces.

---

## Migration Tips

1. Introduce subfolders first; don’t change namespaces yet.  
2. Rename gradually (e.g., `RewardDataBase` → `RewardDatabase`) using IDE refactors.  
3. Add namespaces per domain/layer **after** the tree stabilizes.  
4. (Unity) Keep `*Data` in a `Data` assembly to speed up domain compilation.  
5. Enforce the dependency direction in code reviews.

---

## Why this works

- Mirrors how we *think* about a codebase: feature → kind → responsibility.  
- Reduces coupling and clarifies ownership.  
- Makes onboarding and refactors faster.  
- Plays nicely with SOLID and with incremental delivery.
