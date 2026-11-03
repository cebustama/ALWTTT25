# GIG-UI-1 — Runbook: armado de la consola del manager

> **Paquete transitorio.** Lo consume el cierre de GIG-UI-1. Si GIG-UI-1 cierra sin consumirlo, se
> declara perdido en ese mismo cierre (D-DOC-5).

> **Adaptado (2026-11-01).** El marco se había probado como `Image` dentro de `GigCanvas` (Overlay) y
> tapaba las cartas. Ese montaje **no está guardado** en ningún prefab ni escena del disco, así que no hay
> nada que deshacer en Git. La fase 1 se reescribió: el marco va en un canvas propio de la escena del gig,
> atado a Main Camera. COH-1 ya está cerrado: el guion está en `planning/archive/COH-1_Guion.md`.

> **Avance.** Fase 1 ✅ (2026-11-02, Claudio): `GigFrameCanvas` en la escena del gig, cartas delante del
> marco, Main Camera Size = **5.5**, abanico ajustado en la muesca. Fase 2 aplicada por el agente
> (2026-11-02, pasos 2.1–2.10 sin cambios respecto al texto de abajo); compila sin errores (Claudio).
> **D-UI-1 = B, «Aguante»** (Claudio, 2026-11-02). Fase 3 en dos CSV importables desde `GameTextWindow`:
> `GIG-UI-1_textos/plantillas.csv` (22 ids, pestaña Templates) y `conceptos.csv` (3 descripciones,
> pestaña Concepts). **Corrección a la tabla de conceptos de abajo:** el concepto `stress` **conserva su
> nombre** «Stress» (es el daño que nombran cartas e intenciones) y solo cambia su descripción; no se
> renombra a Aguante. El colapso no rellena la barra entera, sino una fracción
> (`BreakdownStressResetFraction`); el texto de `breakdown` lo dice así.

| Campo | Valor |
| --- | --- |
| Batch | GIG-UI-1 — la nueva UI del gig: la consola del manager, el HUD sobre ella, los recortes de la comparativa y los tooltips nuevos |
| Generado | 2026-10-29 |
| Origen | Propuesta gráfica «La consola en uso» (https://claude.ai/artifact/UkJoU8tMAR4NhZtwrcmhD6) y comparativa «Cuánta información en pantalla» (https://claude.ai/artifact/18SeRUQBsoxAbx2y79QQuy) |
| Sustituye | El «montaje B» de `COH-1_Guion.md`, que ahora apunta aquí |
| Anclajes | Verificados contra el árbol de trabajo el 2026-10-29 y de nuevo el 2026-11-01 (tras COH-1, d4fe636): cada ANCHOR de la fase 2 aparece **una sola vez** en su archivo |
| Precondiciones | COH-1 compila (incluye los pips de cohesión en `GigCanvas`); montaje A de `COH-1_Guion.md` hecho |
| Orden | Fase 1 (escena) → Fase 2 (código: compila tras cada paso) → Fase 3 (glosario) → Fase 4 (prefabs) → Fase 5 (smokes) |

**Archivos tocados**

| Tipo | Archivo |
| --- | --- |
| Escena | `Assets/Scenes/3 - Gig Scene.unity` (canvas del marco, cámara, mano) |
| Prefab | `Assets/Prefabs/UI/Canvases/GigCanvas.prefab`, `Assets/Prefabs/UI/Song Composition/ContextRow.prefab`, prefab del canvas del público (`AudienceCharacterCanvas`) |
| Código | `GameTextTemplateIds.cs`, `EconPipTooltipTarget.cs` (reemplazo completo), `GigCanvas.cs`, `CharacterCanvas.cs`, `BandCharacterCanvas.cs`, `CardBase.cs`; nuevo `CohesionTooltipTarget.cs` |
| Datos | `ConceptGlossary_ES.asset`, `ConceptGlossary_EN.asset` (por `GameTextWindow`, no a mano) |
| Arte | `Assets/Art/UI/Gig/GigFrame_placeholder.png` (ya existe) |

---

## Decisión abierta

**D-UI-1 — Cómo se llama la barra del músico.**

1. **Qué hay que decidir.** En el código, la barra del músico es un **aguante**: empieza llena, el Stress
   la vacía y a 0 colapsa (S5e, `BandCharacterStats`). El glosario se contradice: «Stress: cuando se
   llena, provoca un Colapso» y «Colapso: cuando la barra de Stress llega a cero». Hay que elegir una
   sola lectura antes de escribir los tooltips de la Fase 3.
2. **Opciones.**
   - **A.** La barra se sigue llamando «Stress» y se corrigen las dos descripciones («baja con cada golpe;
     a 0, Colapso»). No hay que renombrar nada, pero es raro que una barra de Stress baje cuando te estresan.
   - **B.** La barra pasa a llamarse **«Aguante»** y «Stress» queda como el daño que la vacía («Grito: 3 de
     Stress»). Es el modelo vida/daño de Slay the Spire. Cambia un nombre de concepto y dos descripciones;
     los textos de las cartas («inflige N de Stress») siguen valiendo.
   - **C.** Se invierte solo la presentación: la barra muestra el Stress acumulado (máximo − aguante) y se
     llena hasta el colapso. Deshace la decisión de presentación de S5e (D1) y toca la predicción de la barra.
3. **Recomendación: B.** Arregla la contradicción con el mínimo de texto y encaja con lo que el jugador
   ya conoce de otros deckbuilders: una barra que baja es vida, lo que la baja es daño. Los pasos de
   código no dependen de esta decisión: solo cambian los textos de la Fase 3 (la tabla trae los de B, con
   la variante A al lado).

**Fuera de este batch (van a LEGIB-1, porque tocan la misma previsualización):** el «−N» y la efectividad
solo mientras sostienes una carta, el Big Num «Y → Z», el aguante que llega por una intención y la
Compostura como escudo pegado a la barra.

---

## Fase 1 — La consola en la escena (editor, sin código)

> Reescrita el 2026-11-01 tras leer cámaras y canvases del disco.

**Por qué en una capa propia (verificado en el disco).** El proyecto usa URP con una pila de cámaras:
`Main Camera` (Base, máscara con Default, UI y Character, ortográfica 4.8) lleva en su pila a `HandCamera`
(Overlay, solo la capa `Card`, *Clear Depth* activado; vive en `HandController.prefab`, dentro de
`DeckManager`). En URP, **todo canvas Screen Space – Overlay se dibuja después de la pila entera**: por
eso el marco dentro de `GigCanvas` (Overlay, ALWTTTCore) tapaba las cartas, y ningún *Sort Order* lo
arregla. Un canvas **Screen Space – Camera** atado a `Main Camera` se dibuja *dentro* de la pasada de la
cámara base; luego `HandCamera` pinta las cartas encima, y al final `GigCanvas` pinta el HUD encima de
todo. Orden resultante: mundo → marco → cartas → HUD. La alternativa (pasar `GigCanvas` a Screen Space –
Camera) se descarta: está en ALWTTTCore y no puede referenciar una cámara de la escena del gig.

### Paso 1.0 — Quitar el marco de `GigCanvas`
- **Dónde:** `GigCanvas` (en ALWTTTCore, o el prefab `Assets/Prefabs/UI/Canvases/GigCanvas.prefab`).
- **Acción:** si el `Image` del marco sigue ahí (en la escena abierta o en el prefab), bórralo. En el disco
  no está guardado: si Unity pregunta al cerrar la escena, **no guardes** ese cambio de ALWTTTCore.
- **Lo has logrado si** buscando `GigFrame` en la jerarquía (con ALWTTTCore abierta) no aparece nada.

### Paso 1.1 — El marco ya está importado
- **Archivo:** `Assets/Art/UI/Gig/GigFrame_placeholder.png` — ya es *Sprite (2D and UI)*, *Multiple*,
  *Full Rect*, 2048. El sprite se llama **`GigFrame_placeholder_0`**. No hay que tocar nada.

### Paso 1.2 — Crear `GigFrameCanvas`
- **Escena:** abre `Assets/Scenes/3 - Gig Scene.unity` sola (no en Play).
- **Acción:** clic derecho en un hueco vacío de la jerarquía → *UI → Canvas*. Renómbralo `GigFrameCanvas`.
  Si Unity crea un `EventSystem` nuevo, bórralo (el de ALWTTTCore basta).
  - Canvas: Render Mode = **Screen Space – Camera**; Render Camera = arrastra **Main Camera** de la
    jerarquía; Plane Distance = **1**; Sorting Layer = **Foreground**; Order in Layer = **100**.
  - Canvas Scaler: UI Scale Mode = *Scale With Screen Size*, Reference Resolution 1920×1080, Match = 0.5.
  - Graphic Raycaster: clic en los tres puntos → *Remove Component* (el marco no recibe clics).
- **Lo has logrado si** el Inspector de `GigFrameCanvas` muestra Main Camera en Render Camera y no tiene
  Graphic Raycaster.

### Paso 1.3 — Imagen del marco
- **Acción:** clic derecho en `GigFrameCanvas` → *UI → Image*, renómbralo `Frame`. Source Image =
  `GigFrame_placeholder_0`; en Rect Transform, Alt + clic en el cuadro de anclas *stretch-stretch* (abajo a
  la derecha); Left/Top/Right/Bottom = 0; **Raycast Target desactivado**; Preserve Aspect desactivado.
  Guarda la escena (Ctrl+S).
- **Lo has logrado si,** en Play desde el arranque normal y con la Game view a 1920×1080, entras a un gig
  y **las cartas de la mano se ven delante del marco** y se pueden arrastrar y jugar, mientras el HUD de
  `GigCanvas` sigue por encima del marco.
- **Si las cartas siguen detrás:** comprueba que Render Mode no se quedó en Overlay y que Render Camera no
  está vacío (si lo está, Unity lo dibuja como Overlay).

### Paso 1.4 — Encajar el mundo en la ventana
- **Acción:** con Play a 1920×1080, sube el `orthographic size` de **Main Camera** (hoy 4.8) hasta que la
  banda y el público queden dentro del óvalo, con sus intenciones y barras por encima de la consola.
  Anota el valor y escríbelo en la escena **fuera de Play**.
- **Lo has logrado si** ningún personaje, intención ni barra queda bajo la consola o fuera del óvalo.
- **Si el marco tapa a un personaje** pese al zoom, baja el Order in Layer de `GigFrameCanvas` o su
  Sorting Layer; no muevas personajes uno a uno.

### Paso 1.5 — Las cartas sobre la consola
- **Acción:** en `HandController` (hijo de `DeckManager` en la escena del gig), ajusta la posición base de
  la mano hasta que el abanico se apoye en la muesca central de la consola. Cambia el override de la
  escena, no el prefab.
- **Lo has logrado si** el abanico descansa en la muesca y las cartas se siguen arrastrando.

---

## Fase 2 — Código (cada paso deja el proyecto compilando)

### Paso 2.1 — Ids de plantilla nuevos
- **Archivo:** `Assets/Scripts/UI/Tooltips/GameTextTemplateIds.cs`
- **ANCHOR:** `        public const string UiGigSongsLeft = "ui_gig_songs_left";                    // {0} songs left`
- **Acción:** INSERT AFTER

```csharp

        // -- [GIG-UI-1] Consola del manager: cohesión, pips de jugada, resumen del músico, carta no jugable --
        public const string UiGigCohesionHeader = "ui_gig_cohesion_header";          // {0} current · {1} max
        public const string UiGigCohesionBody = "ui_gig_cohesion_body";              // --
        public const string UiEconActionHeader = "ui_econ_action_header";            // --
        public const string UiEconCompositionHeader = "ui_econ_composition_header";  // --
        public const string UiEconActionOne = "ui_econ_action_one";                  // --
        public const string UiEconActionMany = "ui_econ_action_many";                // {0} remaining
        public const string UiEconActionNone = "ui_econ_action_none";                // --
        public const string UiEconCompositionOne = "ui_econ_composition_one";        // --
        public const string UiEconCompositionMany = "ui_econ_composition_many";      // {0} remaining
        public const string UiEconCompositionNone = "ui_econ_composition_none";      // --
        public const string UiMusMeter = "ui_mus_meter";                             // {0} current · {1} max
        public const string UiMusMeterShield = "ui_mus_meter_shield";                // {0} current · {1} max · {2} Composure stacks
        public const string UiMusBreakdownRule = "ui_mus_breakdown_rule";            // --
        public const string UiUnplayableHeader = "ui_unplayable_header";             // --
        // Keyed by enum VALUE, same contract as ui_ctx_meter_*: All enumerates the enum (BuildAll), so a
        // new UnplayableReason turns the Templates tab red instead of showing a raw id.
        public const string UiUnplayablePrefix = "ui_unplayable_";
        public static string UiUnplayable(ALWTTT.Managers.UnplayableReason r) =>
            UiUnplayablePrefix + r.ToString().ToLowerInvariant();
```

- **Verificación:** compila.

### Paso 2.2 — Ids fijos en la cobertura
- **Archivo:** `Assets/Scripts/UI/Tooltips/GameTextTemplateIds.cs`
- **ANCHOR:** `            UiGigSongsLeft, UiFxVibeBonus,`
- **Acción:** REPLACE

```csharp
            UiGigSongsLeft, UiFxVibeBonus,
            // [GIG-UI-1]
            UiGigCohesionHeader, UiGigCohesionBody,
            UiEconActionHeader, UiEconCompositionHeader,
            UiEconActionOne, UiEconActionMany, UiEconActionNone,
            UiEconCompositionOne, UiEconCompositionMany, UiEconCompositionNone,
            UiMusMeter, UiMusMeterShield, UiMusBreakdownRule, UiUnplayableHeader,
```

- **Verificación:** compila. La pestaña *Templates* de `GameTextWindow` marcará en rojo los ids nuevos
  hasta la Fase 3: es lo esperado.

### Paso 2.3 — Motivos de carta no jugable en la cobertura
- **Archivo:** `Assets/Scripts/UI/Tooltips/GameTextTemplateIds.cs`
- **ANCHOR:** `            { var id = UiCardType(c); if (seen.Add(id)) list.Add(id); }`
- **Acción:** INSERT AFTER

```csharp
            // [GIG-UI-1] One id per UnplayableReason value, None included (inert, §8.13).
            foreach (ALWTTT.Managers.UnplayableReason r in System.Enum.GetValues(typeof(ALWTTT.Managers.UnplayableReason)))
            { var id = UiUnplayable(r); if (seen.Add(id)) list.Add(id); }
```

- **Verificación:** compila. Aparecen 8 ids `ui_unplayable_*` en rojo en *Templates*.

### Paso 2.4 — Pips de jugada en español (reemplazo completo)
- **Archivo:** `Assets/Scripts/UI/Tooltips/EconPipTooltipTarget.cs` — **reemplazo completo**; guárdalo en
  UTF-8. El archivo actual tiene un carácter mal codificado en un comentario.
- **Por qué:** el texto del tooltip está escrito en inglés dentro del código («1 action card use left this
  turn»). Mezclar idiomas ya confundió en el tutorial (dossier §3.6); todo texto visible va al glosario.

```csharp
// Place at: Assets/Scripts/UI/Tooltips/EconPipTooltipTarget.cs
using UnityEngine;
using UnityEngine.EventSystems;
using static ALWTTT.Tooltips.GameTextTemplateIds;

namespace ALWTTT.Tooltips
{
    /// <summary>
    /// [DEMO-FIXES-A / DF-ECONTIP / D-DF-6=A] Hover tooltip on the ECON-1
    /// per-musician play-budget pips, through the existing TooltipManager
    /// pipeline (StatusIconBase pattern, single tooltip source).
    /// Auto-attached and fed by BandCharacterCanvas.UpdateTurnPlayBudget;
    /// no prefab wiring beyond the pip Image having raycastTarget ON.
    /// [GIG-UI-1] Text from the glossary (ui_econ_*); it was hardcoded English.
    /// </summary>
    public class EconPipTooltipTarget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        public enum PipKind { Action, Composition }

        [SerializeField] private PipKind kind = PipKind.Action;
        private int _remaining;

        public void Init(PipKind pipKind) => kind = pipKind;
        public void SetRemaining(int remaining) => _remaining = remaining;

        public void OnPointerEnter(PointerEventData eventData)
        {
            var tm = TooltipManager.Instance;
            if (tm == null) return;

            bool action = kind == PipKind.Action;
            string header = GameText.Format(action ? UiEconActionHeader : UiEconCompositionHeader);
            string body;
            if (_remaining <= 0)
                body = GameText.Format(action ? UiEconActionNone : UiEconCompositionNone);
            else if (_remaining == 1)
                body = GameText.Format(action ? UiEconActionOne : UiEconCompositionOne);
            else
                body = GameText.Format(action ? UiEconActionMany : UiEconCompositionMany, _remaining);

            tm.ShowTooltip(body, header, transform, cam: null);
        }

        public void OnPointerExit(PointerEventData eventData) =>
            TooltipManager.Instance?.HideTooltip();
    }
}
```

- **Verificación:** compila; el `.meta` del archivo no cambia (es el mismo archivo).

### Paso 2.5 — Tooltip de los corazones de cohesión (archivo nuevo)
- **Archivo nuevo:** `Assets/Scripts/UI/Tooltips/CohesionTooltipTarget.cs` (Unity crea el `.meta`).

```csharp
// Place at: Assets/Scripts/UI/Tooltips/CohesionTooltipTarget.cs
using ALWTTT.Managers;
using UnityEngine;
using UnityEngine.EventSystems;
using static ALWTTT.Tooltips.GameTextTemplateIds;

namespace ALWTTT.Tooltips
{
    /// <summary>
    /// [GIG-UI-1] Hover on the cohesion hearts (COH-1). Reads PersistentGameplayData on enter,
    /// so it is always current. Needs a Graphic with raycastTarget ON on the same GameObject
    /// (a transparent Image on the CohesionPips root: the pips themselves have it OFF).
    /// </summary>
    public class CohesionTooltipTarget : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            var tm = TooltipManager.Instance;
            var gm = GameManager.Instance;
            if (tm == null || gm == null || gm.PersistentGameplayData == null) return;

            int current = gm.PersistentGameplayData.BandCohesion;
            int max = gm.GameplayData != null ? gm.GameplayData.MaxCohesion : current;
            tm.ShowTooltip(
                GameText.Format(UiGigCohesionBody),
                GameText.Format(UiGigCohesionHeader, current, max),
                transform, cam: null);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null) TooltipManager.Instance.HideTooltip();
        }
    }
}
```

- **Verificación:** compila.

### Paso 2.6 — Agotadas solo si hay alguna (campo)
- **Archivo:** `Assets/Scripts/UI/GigCanvas.cs`
- **ANCHOR:** `        [SerializeField] private TextMeshProUGUI exhaustPileTextField;`
- **Acción:** INSERT AFTER

```csharp
        // [GIG-UI-1] Optional root of the exhaust counter: hidden while the pile is empty
        // (Slay the Spire does the same). Null = always visible, as before.
        [SerializeField] private GameObject exhaustPileRoot;
```

- **Verificación:** compila.

### Paso 2.7 — Agotadas solo si hay alguna (lógica)
- **Archivo:** `Assets/Scripts/UI/GigCanvas.cs`
- **ANCHOR:** `            exhaustPileTextField.text = $"{DeckManager.ExhaustPile.Count.ToString()}";`
- **Acción:** INSERT AFTER

```csharp
            if (exhaustPileRoot != null)
                exhaustPileRoot.SetActive(DeckManager.ExhaustPile.Count > 0);
```

- **Verificación:** compila. `SetPileTexts` se llama desde `DeckManager` en cada cambio de pila (10 sitios).

### Paso 2.8 — Lectura del medidor para el tooltip del músico
- **Archivo:** `Assets/Scripts/Characters/CharacterCanvas.cs`
- **ANCHOR:** `        protected bool IsMeterFull => _meterMax > 0 && _meterCurrent >= _meterMax;`
- **Acción:** INSERT AFTER

```csharp
        // [GIG-UI-1] Read access for the band summary tooltip (BandCharacterCanvas.ShowTooltipInfo).
        protected int MeterCurrent => _meterCurrent;
        protected int MeterMax => _meterMax;
        protected StatusEffectContainer BoundContainer => _boundContainer;
```

- **Verificación:** compila. Sin cambio de comportamiento.

### Paso 2.9 — Primer panel al pasar por el músico
- **Archivo:** `Assets/Scripts/Characters/BandCharacterCanvas.cs`
- **ANCHOR:** `        public void SetCurrentStress(int current, int max, float duration)`
- **Acción:** INSERT BEFORE

```csharp
        /// <summary>
        /// [GIG-UI-1] Hover on the musician canvas: a first panel with the meter, Composure and the
        /// breakdown rule. Values are the last ones pushed through SetCurrentStress, so the panel and
        /// the bar cannot disagree. S5e: the meter is remaining fortitude; Breakdown fires at 0.
        /// Status icons keep their own tooltip (StatusIconBase) and stack under this one.
        /// </summary>
        protected override void ShowTooltipInfo()
        {
            base.ShowTooltipInfo();
            if (MeterMax <= 0) return;

            int composure = BoundContainer != null
                ? BoundContainer.GetStacks(ALWTTT.Status.CharacterStatusId.TempShieldTurn)
                : 0;
            string header = characterNameText != null ? characterNameText.text : string.Empty;
            string body = composure > 0
                ? ALWTTT.Tooltips.GameText.Format(
                    ALWTTT.Tooltips.GameTextTemplateIds.UiMusMeterShield, MeterCurrent, MeterMax, composure)
                : ALWTTT.Tooltips.GameText.Format(
                    ALWTTT.Tooltips.GameTextTemplateIds.UiMusMeter, MeterCurrent, MeterMax);
            body += "\n" + ALWTTT.Tooltips.GameText.Format(
                ALWTTT.Tooltips.GameTextTemplateIds.UiMusBreakdownRule);

            ShowTooltipInfo(ALWTTT.Tooltips.TooltipManager.Instance, body, header, descriptionRoot);
        }

```

- **Verificación:** compila. El canvas del músico ya recibe `OnPointerEnter` (es el mismo camino que usa
  el público); hasta ahora su `ShowTooltipInfo` estaba vacío.

### Paso 2.10 — Por qué no se puede jugar una carta
- **Archivo:** `Assets/Scripts/Cards/CardBase.cs`
- **ANCHOR:** `            var tooltipManager = TooltipManager.Instance;` (dentro de `protected virtual void ShowTooltipInfo()`)
- **Acción:** INSERT AFTER **la línea siguiente** (`            if (tooltipManager == null) return;`, la
  primera aparición después del anclaje)

```csharp

            // [GIG-UI-1] Why a card with the red overlay cannot be played, as the FIRST panel of
            // the stack. Same single source as the overlay (EvaluateCardPlayability, CARD-UX-1):
            // display only, nothing re-derived here. No GigManager (inventory, ship) => no panel.
            var gigForReason = GigManager.Instance;
            if (gigForReason != null && CardDefinition != null)
            {
                var reason = gigForReason.EvaluateCardPlayability(CardDefinition);
                if (reason != UnplayableReason.None)
                    tooltipManager.ShowTooltip(
                        GameText.Format(GameTextTemplateIds.UiUnplayable(reason)),
                        GameText.Format(GameTextTemplateIds.UiUnplayableHeader));
            }
```

- **Verificación:** compila. **Cambio de contrato:** el orden de la pila de `SSoT_Card_System.md` §10.2
  gana un primer elemento. El invariante de CARD-UX-1 se mantiene: ningún consumidor calcula jugabilidad;
  este solo lee el evaluador.

---

## Fase 3 — Textos (GameTextWindow ▸ Templates y ▸ Concepts)

Añade cada id en **ES y EN** desde `GameTextWindow`, no editando el `.asset` a mano. Hasta que estén, el
juego muestra el id en bruto (política de fallos de `GameText`) y no rompe nada. La columna ES trae la
opción **B** de D-UI-1; la variante A va entre corchetes.

| Id | ES | EN |
| --- | --- | --- |
| `ui_gig_cohesion_header` | `Cohesión {0}/{1}` | `Cohesion {0}/{1}` |
| `ui_gig_cohesion_body` | `Cada colapso de un músico quita 1, y perder un gig quita más. A 0, la banda se separa y la gira termina.` | `Each musician breakdown costs 1, and losing a gig costs more. At 0, the band splits up and the tour ends.` |
| `ui_econ_action_header` | `Jugadas de acción` | `Action plays` |
| `ui_econ_composition_header` | `Jugadas de composición` | `Composition plays` |
| `ui_econ_action_one` | `Te queda 1 carta de acción este turno.` | `1 action card left this turn.` |
| `ui_econ_action_many` | `Te quedan {0} cartas de acción este turno.` | `{0} action cards left this turn.` |
| `ui_econ_action_none` | `Ya usaste la carta de acción de este turno.` | `Action card already used this turn.` |
| `ui_econ_composition_one` | `Te queda 1 carta de composición este turno.` | `1 composition card left this turn.` |
| `ui_econ_composition_many` | `Te quedan {0} cartas de composición este turno.` | `{0} composition cards left this turn.` |
| `ui_econ_composition_none` | `Ya usaste la carta de composición de este turno.` | `Composition card already used this turn.` |
| `ui_mus_meter` | `Aguante {0}/{1}` [A: `Stress {0}/{1}`] | `Grit {0}/{1}` [A: `Stress {0}/{1}`] |
| `ui_mus_meter_shield` | `Aguante {0}/{1} · Compostura {2}` | `Grit {0}/{1} · Composure {2}` |
| `ui_mus_breakdown_rule` | `A 0 colapsa: queda Shaken y la banda pierde 1 de cohesión.` | `At 0 they break down: Shaken, and the band loses 1 cohesion.` |
| `ui_unplayable_header` | `No se puede jugar` | `Can't play this` |
| `ui_unplayable_none` | `—` | `—` |
| `ui_unplayable_tutorialgate` | `Ahora no: sigue la indicación del tutorial.` | `Not now: follow the tutorial.` |
| `ui_unplayable_actiontiming` | `Ahora no es momento para cartas de acción.` | `Action cards can't be played right now.` |
| `ui_unplayable_inspiration` | `No tienes Inspiración suficiente.` | `Not enough Inspiration.` |
| `ui_unplayable_budget` | `Este músico ya usó su jugada de este tipo en este turno.` | `This musician already used this kind of play this turn.` |
| `ui_unplayable_finallooplock` | `Último loop: la composición está cerrada.` | `Final loop: the composition is locked.` |
| `ui_unplayable_resource` | `No tienes Voltaje suficiente.` | `Not enough Voltage.` |
| `ui_unplayable_norunningloop` | `No hay ninguna parte sonando en bucle.` | `No part is looping right now.` |

**Conceptos que hay que corregir (▸ Concepts), ES:**

| Concepto | Hoy | Propuesta |
| --- | --- | --- |
| `cohesion` | «… A cero, el concierto se pierde.» | «La resistencia compartida de la banda. Pierde un punto cada vez que un músico colapsa, y más al perder un gig. A cero, la banda se separa y la gira termina.» (COH-1, D-DIAG-1) |
| `stress` | «Cuando se llena, provoca un Colapso.» (contradice el código) | B: renombrar a **Aguante**: «Lo que aguanta un músico. El Stress lo baja; a cero, Colapso.» · A: «Baja con cada golpe de Stress. A cero, Colapso.» |
| `breakdown` | «… cuando la barra de Stress llega a cero …» | B: «… cuando el Aguante de un músico llega a cero …» · A: sin cambios |

- **Verificación:** la pestaña *Templates* queda sin ids en rojo; el CSV de exportación lleva las filas nuevas.

---

## Fase 4 — Prefabs: el HUD sobre la consola y los recortes

### Paso 4.1 — Corazones de cohesión
- **Archivo:** `Assets/Prefabs/UI/Canvases/GigCanvas.prefab`
- **Acción:**
  1. Crea `CohesionPips` (RectTransform) sobre la tira de luces de la consola, abajo a la izquierda, con un
     **Horizontal Layout Group** (spacing 4, Child Alignment *Middle Left*, sin Control Child Size).
  2. Añádele una **Image transparente** (alpha 0, Raycast Target activado) y el componente
     **`CohesionTooltipTarget`**.
  3. Crea un prefab de pip: Image de ~32×32 con el corazón (provisional: un cuadrado redondeado naranja).
  4. En `GigCanvas`: **Cohesion Pip Root** = `CohesionPips`, **Cohesion Pip Prefab** = el pip,
     **Cohesion Pip Full / Empty** = los sprites (sin vacío, el apagado sale semitransparente).
  5. Opcional: **Cohesion Text Field** con «8/10» al lado, para la prueba de comprensión.
- **Verificación:** al empezar el gig hay 10 corazones; con Dev Stats «−», se apaga uno por clic; al pasar
  el ratón sale «Cohesión 9/10» con su texto.

### Paso 4.2 — Canciones, mazos y turno sobre la consola
- **Archivo:** `GigCanvas.prefab`
- **Acción:** mueve **canciones restantes** junto a los corazones; **robar** y **descarte** a la consola, a
  la derecha del abanico; el **botón de turno** encima de la consola, a la derecha, sin tapar el ojo morado
  del marco. Cambia su rótulo a **«Tocar»** (dice lo que pasa al pulsarlo). Arrastra el objeto padre del
  contador de agotadas a **Exhaust Pile Root**.
- **Verificación:** nada del HUD queda sobre la ventana salvo el Big Num; el contador de agotadas no aparece
  hasta que se agota la primera carta.

### Paso 4.3 — Big Num arriba a la derecha
- **Archivo:** `GigCanvas.prefab`
- **Acción:** mueve `VibeReadoutRoot` arriba a la derecha, dentro del borde del marco. Regla de prefab de
  BIGNUM-1: `vibeReadoutLabel` sigue bajo `VibeReadoutRoot`, **hermano** de `SongHype Root`, nunca su hijo.
- **Verificación:** el número late al compás y su tooltip (desglose `ui_readout_*`) se abre al pasar el ratón.

### Paso 4.4 — Tira compacta
- **Archivo:** `Assets/Prefabs/UI/Song Composition/ContextRow.prefab`
- **Acción:** **desactiva los GameObjects** de los chips de compás, tempo y mood (los que referencian
  `meterChip`, `tempoChip`, `moodChip`) y el `tempoOverrideDot`. El código ya los protege contra null y
  solo cambia `enabled`, así que un GameObject inactivo se queda oculto. Los loops (con el rombo final) y las
  filas de cada músico se quedan.
- **Verificación:** la fila de contexto muestra solo los loops; el hover de la fila sigue mostrando su panel.
- **Nota:** mover los loops bajo el Big Num cruzaría dos prefabs (tira y `GigCanvas`); queda para después.

### Paso 4.5 — Gustos solo en el tooltip
- **Archivo:** prefab del canvas del público (`AudienceCharacterCanvas`)
- **Acción:** borra el objeto del icono de gustos revelados y deja vacío **Revealed Taste Icon**. El código
  lo comprueba contra null (`revealedTasteIcon != null`); los gustos siguen en el tooltip del cuerpo.
- **Verificación:** revelar gustos (carta o Dev) no muestra icono; pasar el ratón por el espectador muestra
  «— Gustos —».

### Paso 4.6 — Comprobaciones sin cambios
- `GigPresentationSO` de la demo: `showSongHypeBar` sigue en OFF.
- Recompensas de la demo: si alguna carta cuesta o genera Inspiración, su insignia se ve (se oculta sola a 0).
  Si ninguna lo hace, no hay nada que tocar.
- **Mano del manager:** cuando llegue el PNG (`Assets/Art/UI/Gig/`), Image fija en `GigFrameCanvas`, a la
  izquierda del abanico, Raycast Target desactivado.

---

## Fase 5 — Smokes (3 humanos)

1. **La consola se lee.**
   - **Setup:** gig 1 del mapa con las Fases 1 y 4 hechas.
   - **Acción:** juega un turno completo arrastrando una carta a un músico.
   - **Se espera:** banda y público enteros dentro de la ventana; cartas delante del marco y arrastrables;
     corazones, canciones, mazos y botón «Tocar» sobre la consola; Big Num arriba a la derecha.
   - **Falla si:** el marco tapa una carta, un personaje o una intención, o un clic no llega a las cartas.
2. **Recorrido de tooltips nuevos.**
   - **Setup:** gig del mapa, con un músico con Compostura y una carta que no se pueda jugar (gasta antes
     la jugada de acción de ese músico).
   - **Acción:** pasa el ratón por: el músico, uno de sus pips, la carta roja y los corazones.
   - **Se espera:** «Aguante 12/20 · Compostura 3» + la regla del colapso; «Ya usaste la carta de acción de
     este turno.»; «No se puede jugar» como primer panel de la carta; «Cohesión 10/10» con su texto. Todo en
     español y sin ids en bruto.
   - **Falla si:** sale texto en inglés, un id como `ui_econ_action_none`, o el panel de la carta no es el primero.
3. **Regresión de tooltips existentes.**
   - **Setup:** cualquier gig.
   - **Acción:** pasa el ratón por: una carta jugable, la barra de resistencia de un espectador, su
     intención, un icono de estado y el Big Num.
   - **Se espera:** la pila de la carta empieza por su rol o tipo (sin panel de «No se puede jugar»); el
     desglose de la barra; el texto de la habilidad; el del estado; la cadena del Big Num.
   - **Falla si:** alguno no abre, abre en blanco o queda tapado por el marco.

**Lo mecánico, como línea de log o comprobación directa:** el contador de agotadas aparece con la primera
carta agotada; la Templates tab sin rojo; ninguna `NullReferenceException` en `Editor.log` al entrar al gig.

**Límite conocido:** el tooltip del músico se abre sobre el canvas del músico (su barra y sus iconos); si
el área del canvas no cubre el cuerpo del sprite, pasar el ratón por el cuerpo no lo abre. Si pasa, se
amplía el área en el prefab (no es código).

---

## Estado resultante

| Superficie | Antes | Después |
| --- | --- | --- |
| Marco | No existía | `GigFrameCanvas` en la escena del gig, entre el mundo y las cartas |
| Cohesión en el gig | No se veía (solo COH-1 en código) | 10 corazones en la consola, con tooltip |
| Tooltip de pips | Inglés, escrito en el código | Glosario ES/EN |
| Carta no jugable | Overlay rojo sin motivo | Primer panel con el motivo |
| Músico al pasar el ratón | Solo estadísticas CHR/TCH/EMT | + panel de aguante, Compostura y regla del colapso |
| Agotadas | Siempre visible | Solo con cartas |
| Tira de composición | Loops + compás + tempo + mood | Loops; el resto en el hover |
| Gustos revelados | Icono persistente + tooltip | Solo tooltip |

## Pendiente

- **LEGIB-1:** el «−N» y la efectividad solo al sostener una carta; el Big Num «Y → Z»; el aguante que llega
  por una intención (con aviso de colapso); la Compostura como escudo pegado a la barra.
- **COH-1:** el destello del corazón que se pierde (mejora opcional).
- **Arte (Matías, viernes 30-oct):** marco a 1920×1080 sin margen; corazón lleno y vacío; consola sin las
  luces si los corazones van ahí; mano roja en PNG.
- **D-UI-1:** cerrada, B «Aguante» (2026-11-02).
- **Tangente — «Agotar» no está implementado (2026-11-02):** `CardBase.Exhaust()` es un esqueleto vacío; ninguna carta entra en `ExhaustPile`. El contador queda oculto (Exhaust Pile Root conectado) y aparecerá solo cuando se implemente. Ninguna carta del juego tiene `exhaustAfterPlay` activo. Fuera de GIG-UI-1.
- **Divergencia con la propuesta (2026-11-02):** los corazones quedaron abajo a la izquierda, junto a la estrella, sobre una franja oscura; la propuesta los pone en la tira de luces (~20 %) con las canciones al lado. Se decide con Matías (pedido 3).
- **DrawPile y DiscardPile** estaban desactivados en `GigCanvas.prefab`; activados y colocados a la derecha del abanico (Claudio, 2026-11-02); sus vistas de inventario siguen funcionando.
- **Tangente — títulos de las vistas de mazo en inglés:** `InventoryHelper.cs` pasa «Draw Pile» / «Discard Pile» / «Exhaust Pile» como texto fijo; deberían salir del glosario. Fuera de GIG-UI-1.
- **Botón ▶ con rótulo (2026-11-02):** `SongCompositionUI.playButtonLabel` (opcional) + id `ui_gig_play_button` («Tocar» / «Play») en `GIG-UI-1_textos/plantillas_2.csv`.

## Actualizaciones de documentación propuestas (no aplicadas)

| Doc | Cambio | Tipo |
| --- | --- | --- |
| `SSoT_Gig_Combat_Core.md` §5.4, §6.3, §4.3 | **Divergencia con el código:** dice «Breakdown cuando `Stress >= StressMax`»; el código (S5e) vacía el aguante y colapsa a 0. Corregir y, según D-UI-1, el nombre | semántico |
| `SSoT_Tooltip_Pipeline.md` §5 | Tres consumidores nuevos: `CohesionTooltipTarget`, el resumen de `BandCharacterCanvas` y el panel de carta no jugable (regla de TIP-1-DOC: un hover nuevo añade su fila al salir) | estructural |
| `SSoT_Game_Text.md` §1 | Poblaciones nuevas: `ui_gig_cohesion_*`, `ui_econ_*`, `ui_mus_*`, `ui_unplayable_*` | estructural |
| `SSoT_Card_System.md` §10.2 | Orden de la pila: «no se puede jugar» pasa a ser el primero | semántico |
| `SSoT_Gig_Combat_Core.md` §14.7 | El tooltip de los pips sale del glosario | operativo |
| `SSoT_Gig_Encounter.md` o una SSoT de presentación | La capa del marco (`GigFrameCanvas`) y su orden de dibujo | estructural |
| `CURRENT_STATE.md`, `changelog-ssot.md`, roadmap S5 | Entrada GIG-UI-1 | operativo |

## Rehidratación

```
Mode: IMPLEMENTATION. GIG-UI-1 — Armado de la consola del manager.
Context: COH-1 dejó la cohesión como única derrota y los pips de cohesión en GigCanvas. GIG-UI-1
monta la consola (GigFrameCanvas en la escena del gig), mueve el HUD encima, recorta la tira y los
gustos, y añade tres tooltips (músico, pips en ES, carta no jugable) y el de los corazones.
Decisions locked: el marco va en un canvas Screen Space – Camera de la escena del gig (no en
ALWTTTCore); recortes de presentación solo, sin reglas; −N/efectividad/Y→Z/aguante que llega → LEGIB-1.
Open: D-UI-1 (nombre de la barra del músico; recomendado B «Aguante»).
Inputs (disco): Docs/planning/active/GIG-UI-1_Runbook.md; COH-1_Guion.md; los archivos de la Fase 2.
Task: aplicar Fases 1–4; Claudio juega los 3 smokes y pega el Editor.log.
Constraints: no tocar MidiGenPlay; no escribir .meta; textos solo por GameTextWindow.
Closure: compila; Templates sin rojo; smokes 1–3 PASS; docs de la tabla aplicados en el mismo cierre.
```
