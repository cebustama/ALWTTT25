---
titulo: Referencias de ALWTTT — informe
fecha: 2026-10-30
tipo: referencia (registro, no autoridad)
---

# Referencias de ALWTTT — informe

Análisis de `C:\Users\cebus\Dropbox\ALWTTT References` con el método de colecciones
(`D:\Projects\referencias-visuales\metodo-colecciones.md`). Los originales no se tocaron.

- **Wrapped privado:** https://claude.ai/artifact/4cWfxzHDegxTuEJMTHP7YW
- **Catálogo:** `catalogo.csv` (una fila por referencia, `i` estable). Etiquetas a mano: `etiquetas.csv`.
- **Scripts:** `scripts/build.py` (inventario y hojas), `scripts/merge.py` (catálogo, cifras, datos),
  `scripts/wrapped_build.py` + `scripts/wrapped_plantilla.html` (página). Hojas y miniaturas, fuera de git.

**Cómo se hizo.** El agente miró las 28 hojas de miniaturas (12 por hoja; de cada vídeo, un fotograma) y
etiquetó cada referencia: tema, juego de origen, descripción, idea y estado. El estado se comprobó
buscando en el código y en los assets de ALWTTT (estados, nombres de carta, escenas, `NodeType`), sin
jugar. Las etiquetas sirven para encontrar; no son verdad. No es un documento gobernado: si algo cambia el
plan, se aplica en su doc (`S5_DemoCutClose_Sub_Roadmap.md`, `Dossier_Claridad_UI_Deckbuilders.md`).

**Estados.** `incorporada`: la idea está en el repo. `parcial`: hay una versión, o existe fuera de la demo
(eventos, nave, mapa de sectores). `pendiente`: no está. `no-aplica`: charla, estética o curiosidad.
Columna `demo`: la idea toca la meta de la demo del 11-dic (que se entienda sin explicarla).

## Cifras

- 325 referencias: 307 imágenes y 18 vídeos. 193 sueltas en la raíz; el resto en 15 carpetas.
- Fechas: 264 del nombre del archivo, 61 de la fecha del archivo. De ago 2024 a ago 2026 (una de 2022).
- Estado: 72 incorporadas, 93 parciales, 82 pendientes, 78 no aplican. De las 247 que traen una idea:
  29 % incorporadas, 38 % parciales, 33 % pendientes.
- Origen: Monster Train 74, Slay the Spire 54, tu cuaderno 32, Griftlands 24, RimWorld 17, FTL 13,
  Midnight Suns 11. Monster Train y Slay the Spire son el 39 %.
- Por mes: jun 2025 (93), ago 2025 (64), sep 2024 (60), sep 2025 (30). En 2026, 25.
- 39 destacadas; 19 tocan la demo (13 parciales, 6 pendientes).

## Hallazgos

1. **Las notas en rojo se volvieron mecánicas** (ago 2025). «Tall characters block the view» (i 274) →
   Tall y Tapar la Vista. «Melee Weakness = Extra Vibe» (i 306) → Cautivado. «Gear estilo luces, humo»
   (i 153) → cartas de efectos de escenario (Light Show). «Trigger before/after song» (i 156) → los momentos
   «al empezar la próxima parte» y «final de la canción». Pyregel (i 310) → Expuesto.
2. **Los cuatro músicos salieron del cuaderno** (i 221–228): Robot C2, Conito (electricidad → Voltaje y
   Sobrecarga), Sibi («Singing Fields» → Campo Cantor) y Ziggy. Sus arcos (ARCO en cada ficha) y la carta
   Signature de Ziggy no están.
3. **Del boceto a la carta.** Cosmic Wink (i 202) → ¡GUIÑO! (captura del juego, i 192); Count-In (i 205) →
   Conteo; Feedback y Exposed → estados. Raw Scream, Ghostly Notes, Percussive Pulse, Catharsis, Mic Drop y
   Dead Mic siguen en papel.
4. **Junio de 2025, 93 capturas en un mes**, el mes de la «ALWTTT Gamescom Demo» (cuaderno, 24-06-25,
   i 243–250).
5. **La pantalla previa al gig es la idea que más vuelve sin hacerse** (i 145, 151, 152, 155, 158, 246):
   quién es el público, qué le gusta, condiciones, recompensa.
6. **Deshacer** (i 157: «llevo como 100 horas y recién lo vi»; i 110). No existe.
7. **La intención del público está, pero en texto.** Into the Breach (i 29) y Slay the Spire (i 123, 189,
   190, 259–271) la muestran con icono y número. Coincide con el hueco «lo que recibes» del dossier.
8. **Griftlands: negociar es convencer** (i 127–145, 197, 208, 210, 267, 316–317). Las cargas de Rook
   (i 210) son el antecedente de Voltaje.
9. **La nave y el mapa ya existen fuera de la demo** (i 290–304): escena `2 - Ship Interior`, `1 - Sector
   Map`, nodos Rehearsal / RandomEvent / Recruit / Boss y `RandomEventCanvas`.
10. **Un público con oficios**, anotado y pendiente: apoyo (i 278), sanadores que «desconvencen» (i 275), el
    que se va (i 273), efectos que se disparan con el colapso (i 154, 171, 312).
11. **Tenacious D en «Specials»** (i 297): el movimiento especial de la banda no existe.
12. **Casi no hay música**: 5 referencias de música y una de juego musical (Hi-Fi Rush, i 4). Lo musical
    vive en MidiGenPlay (i 280–282).

## Lo que falta

**Para la demo** (columna `demo`): previsualizar el efecto antes de jugar (i 64–67, LEGIB-1); intención como
icono (i 123, 189, 190, 259); efectos que se disparan solos más visibles (i 149, 258); pantalla previa al
gig (i 151, 152, 158, 246); derrota (i 33, 126, COH-1 en curso).

**Después (Phase C):** economía de la partida (tienda, reliquias, mejoras, consumibles: 13 referencias,
ninguna hecha), relaciones entre músicos, público con roles, deshacer, nodo de descanso (i 148),
meta-progresión y desbloqueos.

## Reflexiones

- **De guardar a hacer.** 2024: guardar y charlas (33 de 77 no aplican). 2025: el grueso, con anotaciones
  propias desde agosto. 2026: 25 referencias, casi todas bocetos y capturas del juego (10 incorporadas,
  2 pendientes). La carpeta dejó de ser bandeja y pasó a ser registro.
- **Qué se adopta.** Se adoptó sobre todo lo que se ve en una carta o un estado (cartas: 22 de 39
  incorporadas). Lo que vive entre gigs (tienda, relaciones, pre-gig) sigue pendiente, coherente con el
  recorte de la demo.
- **De dónde mira.** Juegos de EE. UU., Europa y Japón y anime japonés; ninguna referencia de América
  Latina. No es un fallo de la colección, que es de deckbuilders, pero el público y los escenarios podrían
  beber de otras escenas (ver los «puntos ciegos» de Letterboxd).

## Dudas para revisar

- Duplicados: i 38 (= 37), 225–228 (= 221–224), 296 (= 291).
- Sin identificar: i 120, 146, 147, 167, 183, 209, 211, 216, 217, 277, 313–315, 320–323. Los vídeos i 320 y
  321 se ven negros con partículas.
- Origen probable, no seguro: los fotogramas de anime de i 172–181 (Cowboy Bebop) y i 279 (Slay the Spire 2).
- Estados no comprobados en el build: atajo de Terminar turno (i 25), flecha de objetivo (i 191), ordenar
  la mano (i 187).
