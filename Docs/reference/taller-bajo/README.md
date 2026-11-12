# Taller de bajo

Página privada para oír la batería y el bajo juntos y comparar el bajo de hoy con el que sigue los acentos de la batería (creada el 2026-11-12, sesión 2 de la cola de MidiGenPlay, MGP-BASS-ACCENT-1). Segunda de la serie por tipo de pista, después del Taller de batería.

- **Dónde:** artefacto https://claude.ai/artifact/4jqLinQEZaRZLQ8PByhopd y web privada `/privado/taller-bajo/` (`python -I -X utf8 build.py --web`, enlazada desde `/privado/`).
- **Archivos:** `template.html` + `build.py` → `index.html`. `build.py` lee en solo lectura `../taller-bateria/patrones.json`, los 13 fills de Music Lab (con el mismo parser del Taller de batería) y los `ChordProgressionData` en 4/4 de `Assets/Resources/ScriptableObjects/Patterns/Chords/`. Además inyecta el kit y el motor `FX` desde `../taller-bateria/template.html`, para que ese motor exista en un solo sitio.

## Qué hace

- **A / B:** A es el bajo de hoy (`accentFollow = Off`). B es el bajo que sigue los acentos, en modo Ataques o Refuerzo. Se cambia sin parar el audio con el botón o con la tecla **B**; la barra espaciadora toca y para.
- **Batería:** la «Parte con fills» del Taller de batería v2: groove, compases, fill cada N, fill al cerrar, fuente de los fills, semilla, contraste y crash después. Se puede silenciar para oír solo el bajo.
- **Bajo:** la progresión, la figura (nota larga o pulso de negras, corcheas o semicorcheas), el modo B, qué hacer en un fill (hueco o seguir), el umbral, los ataques por compás, el refuerzo, el contraste y el volumen. Los valores por defecto son los de la carta: 0,6, 4, +16 y −8 (Music Lab, 2026-11-12).
- **Vista:** por cada compás, los carriles de la batería, la fila de acentos (los de fill rayados) y el bajo. Cada nota del bajo indica si cae en un acento (reforzada) o si es un ataque nuevo. El resumen cuenta las notas de A y de B.

## Igual al juego y distinto

- **Igual, con prueba:** `ChordArticulator.PlanHits` (Block y el pulso, con su curva de velocidad), `AccentDeriver.MarkFills` y `BassAccentFollow.PlanEvent`, combinados como en `BassTrackComposer.ExpandForAccents` (bloque `/*ENGINE*/` de la página).
- **Distinto:** la octava, que en el juego sale de `ctx.rng` y aquí es fija entre Mi1 y Re#2. El walk y el slap no están portados (en ellos el modo B solo refuerza, no ataca). El sonido es sintetizado. Las progresiones van en Do; las que empiezan en menor, en eólico.

## Paridad

```
cd paridad
node casos.mjs                                   # 608 casos desde la página construida
dotnet run -c Release -p:MgpDll=<MidiGenPlay.Runtime.dll>   # por defecto, Library/ScriptAssemblies de ALWTTT
```

Resultado del 2026-11-12: 608 casos, 41.724 notas de bajo (2.492 ataques nuevos) y 55.392 marcas de fill. **Todo igual.** Se usó la DLL compilada fuera de Unity con los csproj de ALWTTT. Si cambia el motor del bajo en C#, cambiar el bloque `BASS` de `template.html` y volver a correr la paridad.
