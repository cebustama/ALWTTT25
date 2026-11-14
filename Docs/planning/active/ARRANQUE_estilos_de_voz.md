# Arranque: estilos de voz como assets (Zig)

Nace de la nota de Claudio a D-MELFIG-8 (2026-11-14, sesión de melodías por figura de MidiGenPlay): «Qué tan complejo sería agregar nuevos estilos para la voz? ¿Serán assets (SOs)? Me gustaría poder probar y agregar nuevos estilos de voz fácilmente. Lo mismo con las sílabas; me da curiosidad el potencial del motor de Pink Trombone.» **Claudio pidió priorizarlo:** va como siguiente sesión extra, por delante de F2–F3 de después de la demo. Planificación, no autoridad.

## Pegar en la sesión nueva

> Lee y sigue `Docs/planning/active/ARRANQUE_estilos_de_voz.md`. Objetivo: que añadir y probar un estilo de voz de Zig sea fácil: estilos como assets, probados primero en el Taller de melodías y llevados al juego con un código, como TB1 y TM1. Empieza leyendo lo que existe y preséntame las decisiones en formato D antes de construir.

## Lo que hay (releer antes de afirmar)

- `Assets/Scripts/Data/Audio/VoiceProfileSO.cs`: ya es un asset. Las perillas ★ del taller existen en él (looseness, vibrato, tenseness, brightness, transpose, pitch lead, etc.).
- `SingerVoice.cs` v7 (port de Pink Trombone, POC-FORK 1–7); prefab `Zig_SingerVoice` con `expressivityEnabled = 0`.
- Taller de melodías (`Docs/reference/taller-melodia/`): 11 modos (Tierno, Pop, Punk, Death metal, Ópera, Crooner, Falsete, Robot, Borracho…), extensiones X1 (consonantes), X2 (longitud del tracto), X3 (growl, aspereza, aire), scoop, caída, glide, ataque, legato, saturación, vocal base; zigués; «Copiar para la carta» ya da `VoiceProfileSO` + `propuestaVoiceProfileV2`.
- Wrapped de la voz con la propuesta F0–F4 y D-ZIG-1..4: https://claude.ai/artifact/GGpSCDLpNzdx7hbDfqiUf5 (`Docs/reference/zig-voz-wrapped/`). F0 (dicción) va en S9c.

## Lo que hay que decidir

1. **Corte rápido:** modos que solo usan perillas existentes = assets nuevos sin código, más un importador del JSON del taller. ¿Cuántos modos del taller caben así?
2. **`VoiceProfileSO` v2:** qué perillas de X1–X3 entran al C# y en qué orden (cada una con paridad JS = C#, como el motor de voz: −155 dB).
3. **Sílabas:** zigués y bancos de sílabas como asset propio (`SyllableBankSO`) o dentro del perfil.
4. **Coste:** ~10,5 % de CPU por voz en build; qué extensiones lo suben.
5. **Cuándo entra en la demo:** solo lo que pase la prueba de oído con Matías.
