# Pedidos de arte a Matías

**Tipo:** lista viva (registro, no autoridad). Se revisa al empezar cada reunión con Matías y se actualiza
al terminarla: lo entregado pasa a «Recibido» con su fecha y ruta en `Assets/`, lo nuevo entra arriba.
Las minutas de cada reunión siguen en `Docs/reference/reuniones/AAAA-MM-DD_<tema>.md`.

**Última reunión:** 12-nov-2026 (`2026-11-12_matias.md`). **Próxima:** por fijar.

## Abiertos

| # | Pedido | Para qué (sesión del plan) | Pedido el | Estado |
| --- | --- | --- | --- | --- |
| 1 | **Corazón de cohesión**, lleno y vacío, con contorno propio para que se lea sobre los rojos y morados de la consola. Hoy: `white-heart.png` teñido de rojo sobre una franja negra semitransparente (provisional). | HUD del gig (S2, GIG-UI-1) | 2026-10-30 (lleno/vacío); contorno, 2026-11-02 | Por pedir el contorno; mostrarle la franja provisional |
| 2 | **Mano del manager** (la mano roja), PNG suelto. Irá fija a la izquierda del abanico de cartas. | HUD del gig (S2) | 2026-10-30 | En el Miró hay `Mano para mapa.png` (5400 × 3058, con transparencia, marcada «usable»), visto el 2026-11-06. Preguntar si sirve para el abanico |
| 3 | **Consola sin las luces** (o confirmar que se quedan): los corazones van justo debajo de la tira de luces. | HUD del gig (S2) | 2026-10-30 | Decidir con él viendo el gig. Va junto con dónde van los corazones: la propuesta los pone en la tira de luces (~20 %), hoy están abajo a la izquierda, junto a la estrella |
| 4 | **Marco a 1920×1080 sin margen.** El actual ya funciona como placeholder (`GigFrame_placeholder.png`). | HUD del gig (S2) | 2026-10-30 | El `encuadre juego.png` del Miró es idéntico al de `Assets/Art` (comprobado el 2026-11-06): no hay versión nueva. Preguntar |
| 5 | **Icono del nodo inicial y de la nave** del mapa lineal. | Mapa (MAP-DEMO) | 2026-10-30 | Reunión del 30-oct movida al 3-nov: preguntar |
| 6 | **Archivos sueltos** de las pantallas «Personajes» (dormitorio), «Escoge tu carta» y la del bicho en el foco; hoy solo están dentro del plano de la nave o en capturas. | Nave y alineación (S5, S6), recompensa (S11) | 2026-10-30 | «Escoge tu carta» ya está: `premio.png` en el Miró, 5400 × 3058, listo para bajar en S11. Las otras dos parecen sueltas en el Miró; falta comprobar su tamaño |
| 7 | **Salas que faltan**, si existen: cabina, dormitorio, cocina, jardín, baño. | Nave (S5) | 2026-10-30 | Por pedir |
| 8 | **Diseño de las pantallas de mazo** (robar y descarte: se abren al pulsar los iconos de la consola; hoy fondo rojo plano, cabecera gris y título en inglés). | HUD del gig (S2) / pulido | 2026-11-02 | Por pedir |
| 9 | **Pantalla de resultado.** En el Miró la nota «pantalla resultado» no tiene arte al lado. | Por decidir si entra en la demo | 2026-11-06 | Por hablar con él |
| 10 | **Colores de identidad de los músicos.** Provisionales en el juego: Sibi azul, Conito violeta, Ziggy naranja, Robot C2 cian. Se usan para marcar a quién golpea el público (bordes y líneas en S4b) y quizá para el contorno de sus iconos. Que los confirme o los cambie. | Legibilidad (S4, S4b) | 2026-11-10 | Por pedir |
| 11 | **Panel del dormitorio** (boceto «Bosquejo cabina» + «cuarto de los pk»): al marcar un músico en el panel aparece o desaparece en la habitación. Entregar: (1) el dormitorio **sin personajes**; (2) **cada personaje de la habitación en su propio PNG, del mismo tamaño que el fondo y transparente alrededor** (`Dormitorio_Fondo.png`, `Dormitorio_Zit.png`, …), para que caigan en su sitio sin medir; (3) los **retratos de las casillas solo a color** con fondo transparente (la silueta se genera tiñendo; si las quiere pintadas, mismo tamaño y encuadre); (4) el **marco de una casilla** suelto, no pintado en el fondo; (5) **placas y botones sin texto**, con borde uniforme para estirarlos, y botones en normal / encima / pulsado; (6) todo a escala de una pantalla de 1920 × 1080 (o 3840 × 2160 si quiere detalle, pero todas las piezas a la misma escala); (7) el **retrato grande** del panel izquierdo, uno por personaje con el mismo encuadre. Alternativa a (2): un PSD con una capa por personaje, pero exige instalar 2D PSD Importer. | Alineación (S6 hecha con el arte «Personajes»; el panel, cuando esté el arte) | 2026-11-11 | Por enviar |
| 12 | **Poses de Tentáculos** (`asfasf.png`): el mismo personaje con **0, 1 y 2 tentáculos arriba** (cuenta «uno… dos…») y **con la boca abierta** (el golpe «¡TRES!»). Mismo tamaño de lienzo y mismo encuadre que el sprite actual, para que los pies caigan en el mismo sitio. En general, para cada personaje del público: una pose por habilidad (mientras la anuncia) y una al actuar; el juego tendrá los huecos y basta con asignarlas. | Poses del público (S8, con GRITOS-1) | 2026-11-12 | Por pedir |
| 13 | **Marco de bocadillo para los gritos del público** (S8): un bocadillo propio del juego en lugar del clipart de cómic, como el del boceto `ataque jefe.png` (marco morado y cian, sin colita o con colita hacia abajo). Una imagen sin texto, que admita 1 a 3 líneas; el texto lo pone el juego. Opcional para la demo. (Nota para #12: los huecos de pose ya existen; las poses se escriben en `Elenco_S7.json` y entran con «ALWTTT ▸ S8 ▸ Importar gritos».) | S8 GRITOS-1 (el bocadillo ya funciona con el arte antiguo) | 2026-11-12 | Pendiente |
| 14 | **Viñeta de la eclosión de la Reina Gusano** (S9): la Reina en el momento en que revientan los huevos, como `ataque jefe.png` lo es para el jefe punk. Sale a pantalla una vez por gig, deslizándose desde la derecha, ocupando unos 0,7 del ancho; mejor con fondo transparente o con forma propia. Hoy se usa el sprite `jefe (1).png` como provisional. Y **las larvas** (sprite de cuerpo entero, mismo estilo que la Reina) y sus poses: la Reina va con larvas (reunión del 12-nov). Las larvas lloran muy fuerte: una pose de llanto sirve de pose al actuar. | S9 REINA-1 (provisional con `jefe (1).png`); larvas si llegan antes del 20-nov (D-PLAN-10) | 2026-11-12 | Por pedir |
| 15 | **Lista de sus cartas nuevas**, con nombre, qué hacen, a qué músico pertenecen, coste y arte si lo hay. Las que usen efectos que ya existen entran como datos antes del 27-nov. | Contenido (D-PLAN-7/8) | 2026-11-12 | Por pedir |
| 16 | **Arte de la carta de Sibi que cura Stress** (el gusano calma a un músico), si se decide que reemplaza a Read the Room. | DEMO-FIX-1 (D-PLAN-7) | 2026-11-12 | Si se acepta D-PLAN-7 |
| 17 | **Sus números dibujados**, para usarlos en lugar de la fuente: 0–9 y los signos que usa el juego (+, −, ×, →, /). Un PNG por signo con fondo transparente, **todos a la misma altura** y con el mismo margen abajo, o una hoja en rejilla de celdas iguales. Con contorno, para que se lean sobre la consola. Más adelante, la fuente completa dibujada. | Big Num, Stress, Vibe, costes (S11, D-PLAN-16) | 2026-11-12 | Por pedir; antes del 20-nov para que entre en la demo |

## Preguntas abiertas

- ~~¿La Reina Gusano va sola o con larvas?~~ → **Con larvas** (12-nov): lloran fuerte y meten mucho Stress, tienen
  poca Vibe y gustos distintos a los de la Reina, así que conviene convencerlas primero. Falta el arte (#14).

## Recibido

| Pedido | Llegó | Dónde quedó |
| --- | --- | --- |
| — | — | — |
