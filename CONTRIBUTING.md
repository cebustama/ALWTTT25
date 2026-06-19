# CONTRIBUTING — ALWTTT

Guía de control de versiones del proyecto. Es un documento **operativo / de proceso de desarrollo**: **no es un SSoT gobernado** y no define mecánicas, contratos ni autoridad del juego. Si el flujo de control de versiones cambia, se actualiza aquí. La lista exacta de qué se guarda en LFS vive en `.gitattributes` (raíz) — esa es la fuente de verdad.

---

## TL;DR

- El repo usa **Git + Git LFS**. Los binarios (audio, imágenes, modelos, etc.) se versionan vía LFS, no como blobs normales de Git.
- Tu día a día (consola o **Fork**) es Git normal: `stage → commit → push / pull`. Los binarios entran por LFS **solos**.
- **Única regla nueva:** si añades un *tipo* de binario que antes no existía, primero `git lfs track "*.ext"`, commitea `.gitattributes`, y luego añade los archivos.

---

## Requisitos (una vez por máquina)

- **Git** y **Git LFS** instalados. Activa LFS: `git lfs install`.
- (Opcional) **Fork**: soporta LFS de forma nativa; no necesita configuración extra.

## Primer clonado

```bash
git clone https://github.com/cebustama/ALWTTT25.git
cd ALWTTT25
git lfs install     # si no lo corriste antes en esta máquina
git lfs pull        # baja los binarios reales (no punteros)
```

Si en Unity ves sprites/audio rotos y esos archivos pesan ~130 bytes, son **punteros LFS sin resolver** → corre `git lfs install && git lfs pull`.

## Flujo diario

Igual que cualquier repo Git. En Fork: stage, commit, push/pull, ramas, como siempre. Los archivos cuyo tipo está en `.gitattributes` se convierten en **puntero LFS** al hacer stage y se suben al pushear; no hay paso manual por archivo. En el diff, Fork mostrará los binarios como *"Stored with Git LFS"* en lugar de texto — es lo correcto.

## Regla de oro: tipos de binario **nuevos**

LFS solo captura las extensiones listadas en `.gitattributes`. Si añades un tipo que no estaba (p. ej. `.webp`, `.aac`, `.bank`, `.wem`), hazlo **antes** de añadir esos archivos, o podrían colarse como blobs normales y chocar con el límite de 100 MB de GitHub:

```bash
git lfs track "*.webp"
git add .gitattributes
git commit -m "chore: track *.webp via LFS"
```

Para los tipos ya cubiertos (wav, ogg, mp3, png, jpg, jpeg, psd, exr, tga, tif, fbx, blend, mp4, mov, bytes, dll…), no hay nada que hacer.

## Qué se versiona y qué no

- **Por LFS:** audio, imágenes/texturas, modelos 3D, vídeo, fuentes y blobs binarios. Lista exacta en `.gitattributes`.
- **Nunca se versiona** (ya en `.gitignore`): `Library/`, `Temp/`, `Build*/`, `Logs/`, `UserSettings/`, `*.unitypackage`, y scratch (`*.zip`, etc.).
- **Secretos:** nunca se commitean claves ni tokens. Van en `.env` (ya ignorado); existe `.env.example` como plantilla.

## Binarios = sin fusión → bloqueo (cuando seáis equipo)

Los binarios **no se pueden fusionar**: si dos personas editan el mismo `.png`, una pierde su cambio al integrar. Por eso `.gitattributes` marca los binarios como `lockable`. Cuando trabajéis en paralelo sobre los mismos assets:

```bash
git lfs lock   Assets/ruta/al/archivo.png    # antes de editar
# ...editas, commiteas, push...
git lfs unlock Assets/ruta/al/archivo.png    # al terminar
```

(Fork lo ofrece con clic derecho sobre el archivo.)

**Trabajando en solitario:** el bloqueo deja los binarios en **solo-lectura**, lo que estorba al reimportar/sobrescribir assets en Unity. Desactívalo (solo afecta a tu copia del repo):

```bash
git config lfs.setlockablereadonly false
```

Revertir a `true` cuando empiece el trabajo en equipo y queráis usar el bloqueo.

## Cuota de LFS en GitHub

Plan gratis/Pro: ~**1 GB de almacenamiento + 1 GB de banda al mes** (verifica la cifra vigente en *Settings → Billing*; el almacenamiento es **acumulativo**, la banda se reinicia cada mes). Uso actual del repo: ~**492 MB**.

Si os acercáis al límite: *data pack* de **$5/mes por 50 GB** (almacenamiento + banda), o reducir lo versionado (ver "Pendientes conocidos"). Avisos: si superas la cuota, los pushes de objetos LFS se **rechazan en silencio**; y CI / GitHub Actions que clonen el repo **consumen banda** LFS.

## Ajuste de Unity

*Edit → Project Settings → Editor → Asset Serialization = **Force Text*** (para que `.unity` / `.prefab` / `.asset` sean texto diffeable/fusionable). Es el valor por defecto en Unity moderno; verifícalo.

## Higiene

- Commits pequeños y frecuentes; `pull` antes de empezar a trabajar.
- No dejes ramas vivir semanas sin integrar (los binarios divergen y no se fusionan).
- El VCS **no es tu único backup** de los assets grandes: mantén una copia adicional (espejo del repo o respaldo del almacén LFS).
- **No reescribas historia ya compartida** (`push --force` sobre `main`) sin avisar al equipo: obliga a todos a reclonar.

## Pendientes conocidos

- **SoundfontsDB de Maestro** (~1070 `.wav` en `Assets/MidiPlayer/.../SoundfontsDB/`) es el grueso del peso en LFS. Está pendiente evaluar **sacarla del repo** (parece re-importable; Maestro reproduce desde `*.bytes`). Es una cuestión **interna del paquete MidiGenPlay** y no se tocan sus internals desde este proyecto; se decidirá con el proyecto companion.

---

*Last updated: 2026-06-17.*
