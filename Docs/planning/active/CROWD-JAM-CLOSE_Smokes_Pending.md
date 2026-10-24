# CROWD-JAM-CLOSE — smokes de CROWD-SFX-1, CROWD-CAP y JAM-3

**Paquete transitorio, no es documento gobernado.** Lo consume el cierre de CROWD-JAM-CLOSE. Si ese lote cierra sin consumirlo, se declara perdido en la misma clausura (D-DOC-5).
**Generado:** 2026-10-22, contra el commit `9924e24`.
**Origen (D1=B):** los smokes originales (CS-1..14 y los de JAM-3) no están en el repo. Estos se **reconstruyeron leyendo el código**; su numeración (CR-*, J3-*) es nueva y no equivale a la antigua.
**Inferido, pendiente de confirmar:** cómo se describen D2=A' (D2a=A) y D3=A más abajo se dedujo de los comentarios del código.

---

## 0. Preparación común

- Escena del gig con la banda de demo (Conito · Ziggy · Robot C2 · Sibi) y build `ALWTTT_DEV`.
- Inspector de `SensoryAudioAdapter`: `logVerification` = ON.
- Inspector de `AudioManager`: `logSfx` = ON.
- Consola: filtra por `CROWD-SFX-1`, `SensoryAudioAdapter`, `JAM-` y `CARD-VALIDATION`.
- **Cuidado:** lo que cambies en un ScriptableObject durante el Play mode **se queda guardado**. Si tocas `GigFlowSettingsSO` (umbrales) o `AudioManager`, apunta el valor original y restáuralo después.
- Valores por defecto en el código:

  | Valor | Por defecto |
  | --- | --- |
  | `crowdApplauseAtMean` | 0.5 |
  | `crowdBooAtMean` | −0.5 |
  | `crowdVerdictDelaySeconds` | 0.35 s |
  | `crowdVerdictMaxSeconds` | 2.5 s |
  | `crowdOutcomeMaxSeconds` | 5 s |
  | `crowdCapFadeSeconds` | 0.5 s |
  | `SongEndPause` | 3 s |

- Duración de los clips medida: Applause 5.0 s · Booing 9.6 s · Ovation 9.3 s · Jeer 11.5 s. **Todos superan su tope**, así que todos se recortan.

---

## A. CROWD-SFX-1 + CROWD-CAP

### CR-1 · Aplauso al terminar una canción
- **Preparación:** una canción que guste (media de impresión ≥ 0.5). Si no consigues una, baja temporalmente `crowdApplauseAtMean` a 0.
- **Acción:** termina la canción.
- **Resultado esperado:**
  - Log `[CROWD-SFX-1] song verdict mean=… polarity=1`.
  - Luego `[SensoryAudioAdapter] song-verdict … → CrowdApplause`.
  - Suena un aplauso unos 0.35 s después del final, **antes** del primer floater +Vibe del song end.
- **Fallo:** no suena; suena encima de los reaction stings del último loop; o la polaridad del log no cuadra con la media y los umbrales que imprime la misma línea.

### CR-2 · Abucheo
- **Preparación:** una canción que disguste (media ≤ −0.5). Si no consigues una, sube temporalmente `crowdApplauseAtMean` a 2 y `crowdBooAtMean` a 0.
- **Acción:** termina la canción.
- **Resultado esperado:** `polarity=-1` → `CrowdBoo` y suena un abucheo.
- **Fallo:** suena aplauso o silencio.

### CR-3 · Canción neutra: silencio
- **Preparación:** `crowdApplauseAtMean` = 2 y `crowdBooAtMean` = −2.
- **Acción:** termina una canción.
- **Resultado esperado:** `polarity=0` y `→ no sound (neutral)`. Solo vuelve el crowd bed.
- **Fallo:** suena cualquier sonido de público.

### CR-4 · Tope del veredicto (2.5 s con fundido)
- **Preparación:** CR-1 o CR-2 (los dos clips duran más de 2.5 s).
- **Acción:** escucha y cronometra el sonido.
- **Resultado esperado:** dura unos 2.5 s; el fundido de salida se oye en los últimos 0.5 s; termina dentro del hueco de `SongEndPause`.
- **Fallo:** suena el clip entero (5 o 9.6 s), se corta en seco (clic) o sigue sonando cuando empieza la composición siguiente.

### CR-5 · Victoria: ovación
- **Preparación:** juega para ganar el gig.
- **Acción:** convence a todo el público.
- **Resultado esperado:** `gig-outcome won=True … → CrowdOvation`. Ovación de unos 5 s con fundido.
- **Fallo:** silencio, jeers o el clip entero (9.3 s).

### CR-6 · Derrota tras la última canción: jeers
- **Preparación:** termina la última canción con algún miembro sin convencer.
- **Acción:** deja que el gig se resuelva.
- **Resultado esperado:** `won=False cause=UnconvincedAfterFinalSong → CrowdJeer`. Unos 5 s con fundido.
- **Fallo:** silencio o la ovación.

### CR-7 · Una sola voz: el sonido nuevo sustituye al anterior
- **Preparación:** la última canción del gig (veredicto y resultado del gig salen muy seguidos).
- **Acción:** termínala.
- **Resultado esperado:**
  - Nunca suenan dos sonidos de público superpuestos: la ovación o los jeers **cortan** el veredicto.
  - El sonido nuevo arranca a volumen pleno aunque el anterior estuviera a mitad de fundido.
- **Fallo:** se superponen dos sonidos, o el nuevo arranca atenuado.

### CR-8 · La pausa congela el sonido del público y su tope
- **Preparación:** CR-1.
- **Acción:** pulsa Esc a mitad del aplauso, espera 3 s y reanuda.
- **Resultado esperado:** el aplauso se congela al instante y se reanuda en el mismo punto. La duración total sigue siendo de unos 2.5 s de audio, sin contar la pausa.
- **Fallo:** sigue sonando con el menú abierto; se reinicia; o el tope cuenta el tiempo en pausa (corte prematuro).

### CR-9 · Salir al menú con el público sonando
- **Preparación:** CR-5 o CR-6. La ovación dura 5 s, así que hay margen.
- **Acción:** sal al menú (pausa → «Volver al menú» → confirmar) mientras suena.
- **Resultado esperado:** el sonido funde a cero y no llega al Main Menu. El siguiente gig empieza con el público a volumen normal.
- **Fallo:** el aplauso se oye en el menú, o el gig siguiente arranca mudo.

### CR-10 · Volumen: grupo SFX y `crowdLevel01`
- **Preparación:** Opciones → SFX a 0.
- **Acción:** provoca un veredicto. Repite con SFX al máximo y `crowdLevel01` = 0, y otra vez con el slider de **música** a 0.
- **Resultado esperado:**
  - SFX a 0 → silencio.
  - `crowdLevel01` = 0 → silencio.
  - Música a 0 → el público **sí** suena.
- **Fallo:** el público obedece al slider de música, o no obedece al de SFX.

### CR-11 · Mezcla contra el tail (subjetivo)
- **Preparación:** song tail ON (por defecto).
- **Acción:** escucha el aplauso sobre el tail.
- **Resultado esperado:** el aplauso se entiende por encima del tail sin taparlo del todo.
- **Fallo:** queda inaudible o satura. Si falla, es de ajuste (`crowdLevel01` o el nivel del tail), no de código: anótalo con los valores.

### CR-12 · Regresión: las vías de debug siguen calladas
- **Preparación:** F12 → victoria o derrota de dev (`DevWinNormalFlow` / `DevLoseNormalFlow`), o `DevSetBandCohesion(0)`.
- **Acción:** dispárala.
- **Resultado esperado:** no hay sonido de resultado del gig ni log `gig-outcome`. Es lo diseñado (D-S4-SRC=A): estas vías no publican `GigOutcomeEvent`.
- **Fallo:** suena una ovación o unos jeers.

---

## B. JAM-3

Todas las pruebas ocurren **dentro de una misma parte** (la demo tiene una parte por canción). Las cartas son de Sibi y se pueden sacar con F12 → Catalogue → Spawn. **Ninguna lleva `progressionOverride`**, así que todas las pruebas usan paletas.

| Carta | Fuente de armonía que declara | Adopta tonalidad |
| --- | --- | --- |
| Wormus Major | `Chord Palette - Core Major` | no |
| Wormus Minor | `Chord Palette - Core Minor` | no |
| Modal Drift | `Chord Palette - Modal` | no |
| Wormus Modal | `Chord Palette - Modal` | **sí** |
| Wormus Bossa | ninguna (solo articulación) | no |

### J3-1 · Misma paleta: la jam se conserva (D2=A')
- **Preparación:** juega Wormus Major y deja pasar un loop, para que se capture la jam.
- **Acción:** vuelve a jugar Wormus Major en la misma parte.
- **Resultado esperado:** `[JAM-1] … imposing shared progression` y **ninguna** línea `[JAM-3]`. La progresión sigue sin saltar.
- **Fallo:** aparece `[JAM-3] … new harmony source`, o la progresión vuelve al principio.

### J3-2 · Paleta distinta: la carta nueva manda
- **Preparación:** J3-1.
- **Acción:** juega Wormus Minor.
- **Resultado esperado:**
  - `[JAM-3] part=… new harmony source 'Chord Palette - Core Minor' (jam came from 'Chord Palette - Core Major') -> not imposing; the new card leads`.
  - No hay `[JAM-1] imposing` en ese render.
  - Se oyen acordes menores.
- **Fallo:** se impone la jam mayor, que es el comportamiento de antes de JAM-3.

### J3-3 · Solo articulación: acompaña
- **Preparación:** Wormus Major jugada y capturada.
- **Acción:** juega Wormus Bossa.
- **Resultado esperado:** `[JAM-1] imposing` y ninguna `[JAM-3]`. La armonía continúa y cambia la articulación.
- **Fallo:** aparece `[JAM-3]`, o la armonía se reinicia.

### J3-4 · El origen se mantiene a través de una carta de articulación (D2a=A)
- **Preparación:** Wormus Major → Wormus Bossa (J3-3).
- **Acción:** juega Wormus Major otra vez.
- **Resultado esperado:** `[JAM-1] imposing` y ninguna `[JAM-3]`. El render impuesto de Bossa no borró el origen.
- **Fallo:** `[JAM-3] … (jam came from '_')`, o la progresión se reinicia.

### J3-5 · La nueva fuente pasa a ser el origen
- **Preparación:** J3-2: Wormus Minor ya manda.
- **Acción:** en el loop siguiente juega Wormus Bossa.
- **Resultado esperado:** `[JAM-1] imposing` sobre la progresión **menor**, sin `[JAM-3]`.
- **Fallo:** vuelve la progresión mayor, o aparece `[JAM-3]`.

### J3-6 · Misma paleta con adopción de tonalidad: el modo se recuerda (D3=A, caché de sesión)
- **Preparación:** juega Wormus Modal, que adopta la tonalidad de la progresión. Deja sonar dos o tres loops (los siguientes salen de la caché de sesión) con Ziggy cantando en la parte.
- **Acción:** escucha la voz en los loops 2 y siguientes; después juega Wormus Bossa.
- **Resultado esperado:**
  - La voz canta en el modo adoptado también en los loops servidos desde caché.
  - Al jugar Bossa, si aparece `[JAM-2] aligning render tonality … -> X/Y`, X/Y es el **modo adoptado**, no el del modelo.
- **Fallo:** la voz desafina desde el loop 2, o la línea `[JAM-2]` alinea al modo del modelo.

### J3-7 · Replay de bundle (D3=A, H2), en la medida de lo posible
- **Preparación:** en una canción posterior, recompón exactamente la misma parte de J3-6 (solo Wormus Modal) para que `MidiMusicManager` sirva un bundle cacheado.
- **Acción:** juega luego Wormus Bossa.
- **Resultado esperado:** igual que J3-6: el modo publicado es el adoptado.
- **Fallo:** el modo es el del modelo. Si el log no deja confirmar que hubo bundle hit, marca **N/A**, no PASS.

### J3-8 · Regresión: borrado de `wormus_test`
- **Preparación:** F12 → Catalogue con Sibi en la banda.
- **Acción:** recorre el catálogo y empieza un gig.
- **Resultado esperado:** no aparece «Test - Lab Progression» ni ninguna carta `wormus_test`. Ningún `Missing reference` en consola al cargar el catálogo de Sibi.
- **Fallo:** referencia rota o null en el catálogo.

### J3-9 · Editor: aviso de conflicto de tonalidad y guardado único (D4=C)
- **Preparación:** abre ALWTTT → Cards → Card Editor.
- **Acción:** abre Wormus Modal, edita un campo cualquiera del payload y deshaz el cambio.
- **Resultado esperado:**
  - Sin errores de GUI (`EndChangeCheck` / `Stack empty`).
  - Un solo guardado por edición.
  - Solo si la carta lleva además un `TonalityEffect`: HelpBox visible siempre y un `[CARD-VALIDATION]` por edición.
  - El único bundle con `adoptProgressionTonality` = 1 es el de Wormus Modal. No pude confirmar desde los `.asset` si su payload lleva un `TonalityEffect` (los efectos pueden ser SO aparte). Si no lo lleva, no debe aparecer HelpBox (negativo correcto). **No crees una carta solo para esta prueba.**
- **Fallo:** error de GUI, o HelpBox sin conflicto real.

---

## C. Hallazgo nuevo de esta lectura (no es un smoke)

- **F-CJ-1 (verdad de código; no sé si la rama se alcanza).** `GigManager.cs` ~1122–1146: al entrar en `PlayerTurn` con `CurrentSongIndex >= _requiredSongCount`, el gig se resuelve con `WinGig()` / `LoseGig()` **sin publicar `GigOutcomeEvent`**. Las vías normales sí lo publican (`ResolveGigOutcomeAndEnd`, `LoseGig(GigLossCause)`).
  - Si alguna vez se entra por aquí, el resultado del gig queda mudo (sin ovación ni jeers) y la telemetría TLM-1 tampoco lo registra.
  - Si en CR-5 o CR-6 un final real queda en silencio, sospecha de esta rama.
  - Es el mismo tipo de hueco que TLM-1b.

## Reporte

Para cada prueba: PASS / FAIL / N/A, y en los FAIL la línea de log o lo que se oyó.
