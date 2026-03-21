# SNAPSHOT_RETENTION_POLICY — ALWTTT

**Status:** Active archival policy  
**Purpose:** Define what to do with the preserved pre-governance snapshot after the migration is functionally complete.

---

## 1. Policy

The pre-governance snapshot should be retained as a **historical backup**, not as active documentation.

Recommended treatment:
- keep the snapshot ZIP or folder outside the active governed tree
- treat it as recovery/trace material only
- do not link to it as day-to-day authority

---

## 2. When it is safe to stop consulting the snapshot routinely

Routine use of the snapshot should stop once:
- all major concepts have governed primary homes
- the supersession map is complete enough for old->new lookup
- root docs no longer describe the old tree as a second authority surface

That condition is now effectively met for the current migration line.

---

## 3. Minimum retention recommendation

Keep the snapshot at least until:
- one stable implementation/documentation cycle has passed after migration closure, or
- repository history / external backup already guarantees recovery

After that, it may remain archived indefinitely, but it does not need to live beside the active docs tree.

---

## 4. Non-negotiable rule

The snapshot preserves history.
It does **not** preserve authority.
