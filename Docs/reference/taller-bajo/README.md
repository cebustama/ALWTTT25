# Taller de bajo

Página privada para oír la batería y el bajo juntos y comparar el bajo de hoy con el que sigue los acentos de la batería (creada el 2026-11-12, sesión 2 de la cola de MidiGenPlay, MGP-BASS-ACCENT-1). Segunda de la serie por tipo de pista, después del Taller de batería.

- **Dónde:** artefacto https://claude.ai/artifact/4jqLinQEZaRZLQ8PByhopd y web privada `/privado/taller-bajo/` (`python -I -X utf8 build.py --web`, enlazada desde `/privado/`).
- **Archivos:** `template.html` + `build.py` → `index.html`. `build.py` lee en solo lectura `../taller-bateria/patrones.json`, los 13 fills de Music Lab (con el mismo parser del Taller de batería) y los `ChordProgressionData` en 4/4 de `Assets/Resources/ScriptableObjects/Patterns/Chords/`. Además inyecta el kit y el motor `FX` desde `../taller-bateria/template.html`, para que ese motor exista en un solo sitio.

## Qué hace

- **A / B / C:** A es el bajo de hoy (`accentFollow = Off`). B es el bajo que sigue los acentos, en modo Ataques o Refuerzo. Se cambia sin parar el audio con el botón o con la tecla **B**; la barra espaciadora toca y para.
- **Batería:** la «Parte con fills» del Taller de batería v2: groove, compases, fill cada N, fill al cerrar, fuente de los fills, semilla, contraste y crash después. Se puede silenciar para oír solo el bajo.
- **Bajo:** la progresión, la figura (nota larga o pulso de negras, corcheas o semicorcheas), el modo B, qué hacer en un fill (hueco o seguir), el umbral, los ataques por compás, el refuerzo, el contraste y el volumen. Los valores por defecto son los de la carta: 0,6, 4, +16 y −8 (Music Lab, 2026-11-12).
- **C · frases (sesión 2a bis, 2026-11-12; en C# desde el 2026-11-13 como `accentFollow = Phrases`, `BassPhrasePlanner`):** las frases empiezan en el bombo y el crash fuertes y en cada cambio de acorde; llenan el hueco hasta el ancla siguiente con una de las 12 frases de Music Lab (`MidiGenPlay/Documentation~/research/2026-11-12_bajo_frases_music_lab.md`), elegida por hash entre las más largas que caben y por estilo (rock, pop, funk, balada). Nota de paso «A» hacia la raíz siguiente (medio tono si cambia el acorde, paso de escala si no), anticipación del bombo sincopado, tope de anclas por estilo, velocidad de Music Lab (+16 × peso en el inicio, −6 dentro, −12 en la nota de paso, ±4 por hash, 40–120) y notas muertas opcionales en la caja (70 %, solo en silencios). El motor `PHR` vive en el bloque `/*ENGINE*/` y está escrito para portarlo 1:1.
- **Versiones guardadas (2026-11-12):** «Guardar esta versión» guarda todos los controles y la versión que suena (A, B o C). En el artefacto van al db compartido (colección `versiones`); en la web, a este navegador. El **código TB1** (`TB1:` + JSON en base64) lleva una versión de un sitio a otro. «Copiar para la carta» da un JSON con los nombres de campo de `RhythmCardConfigSO` y `BasslineCardConfigSO`; los campos `phrase*` y `accentFollow = Phrases` ya existen en C# (2026-11-13). En Unity, **MidiGenPlay ▸ Bass ▸ Import Bass Workshop Version…** aplica un TB1 o ese JSON a una carta de bajo y otra de ritmo (o a copias nuevas), con preview antes de escribir.
- **🎲 Aleatorio (2026-11-12, tecla R):** al terminar cada vuelta, ajustes nuevos al azar (`Math.random`, es la página, no el motor) y sigue sonando hasta parar. «Solo el bajo» cambia progresión, figura, modo B, umbral, tope, refuerzo, estilo, semilla del bajo, caja, anticipación y notas muertas; «Bajo y batería» también groove, tempo (el del patrón ±10), fills, semilla y contraste. La versión que suena (A, B o C) no cambia. El cambio de tempo conserva la hora de la frontera de la vuelta. «Guardar esta versión» guarda lo que suena.
- **Vista:** por cada compás, los carriles de la batería, la fila de acentos (los de fill rayados) y el bajo. Cada nota del bajo indica si cae en un acento (reforzada) o si es un ataque nuevo. El resumen cuenta las notas de A y de B.

## Igual al juego y distinto

- **Igual, con prueba:** `ChordArticulator.PlanHits` (Block y el pulso, con su curva de velocidad), `AccentDeriver.MarkFills` y `BassAccentFollow.PlanEvent`, combinados como en `BassTrackComposer.ExpandForAccents` (bloque `/*ENGINE*/` de la página).
- **Distinto:** la octava, que en el juego sale de `ctx.rng` y aquí es fija entre Mi1 y Re#2. El walk y el slap no están portados (en ellos el modo B solo refuerza, no ataca). El sonido es sintetizado. Las progresiones van en Do; las que empiezan en menor, en eólico.

## Paridad

```
cd paridad
node casos.mjs                                   # 608 casos (A/B) + 1.824 de C (casos-phr.json) desde la página construida
dotnet run -c Release -p:MgpDll=<MidiGenPlay.Runtime.dll>   # por defecto, Library/ScriptAssemblies de ALWTTT
```

Resultado del 2026-11-12: 608 casos, 41.724 notas de bajo (2.492 ataques nuevos) y 55.392 marcas de fill. **Todo igual.** Se usó la DLL compilada fuera de Unity con los csproj de ALWTTT. Si cambia el motor del bajo en C#, cambiar el bloque `BASS` de `template.html` y volver a correr la paridad.

Modo C (2026-11-13, MGP-BASS-PHRASE-1): 1.824 casos (19 grooves × 3 progresiones × 4 estilos × 8 combinaciones de caja, notas muertas y anticipación, con 4 semillas, 3 umbrales y hueco/seguir), 34.905 anclas y 70.535 notas (1.794 muertas) contra `BassPhrasePlanner.Render`: **todo igual**. Los eventos llevan la raíz, la calidad y la escala ya resueltas; en el juego las resuelve el compositor (raíz en la octava más grave de la banda, desde Mi1; escala real de la parte). Si cambia `PHR` o `BassPhrasePlanner`, cambiar el otro y volver a correr.

- **Enlace con los ajustes (2026-11-13):** «Copiar enlace» da una URL con los ajustes en el hash (`#tb1=` + JSON en base64url) que abre la página igual. En la web apunta a la propia página; desde el artefacto, a la copia de la web (tras Cloudflare Access). El cuadro de pegar acepta el código TB1 o el enlace.
