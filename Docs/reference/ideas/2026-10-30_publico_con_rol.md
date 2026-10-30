# Público con rol — inventario de enemigos, ideas y estado del código (2026-10-30)

**Tipo:** idea / referencia (no autoridad). Si algo entra en la demo, se diseña en su batch
(`PUBLICO-ROL-1` en `planning/active/DEMO-1211_Plan_de_Sesiones.md`) y su verdad pasa a
`SSoT_Audience_and_Reactions.md`. Los nombres son **provisionales**: los pone el equipo.
Origen: pedido de Claudio («que los grupos estén pensados para funcionar entre ellos») y las referencias
anotadas de Monster Train (i 273, 274, 275, 278 en `referencias-alwttt/catalogo.csv`).

## 1. Lo que hay en el repo (código leído el 2026-10-30)

**Ya se puede, solo con datos** (`AudienceCharacterData` + `AudienceAbilityData`):
- Cada habilidad es una lista de acciones con tipo, objetivo, valor y estado (`CharacterActionData`).
- Acciones con efecto real: `AddVibe`, `RemoveVibe`, `BlockVibe` (sobre público), `AddStress`,
  `BlockStress`, `HealStress` (sobre la banda), `MoveToFront` (N pasos), `ApplyStatusEffect` (cualquier
  estado del catálogo SO). `AddHooked`, `AddHeckled`, `AddStun` y `AddDazzled` están en el enum pero no
  tienen procesador.
- Objetivos: `Self`, `AudienceCharacter` (el aliado menos convencido), `RandomAudienceCharacter`,
  `AllAudienceCharacters`, `AudienceTall` (el primer alto que no sea él), `Musician` (respeta Spotlight),
  `RandomMusician`, `AllMusicians`.
- Posición: el público es una fila ordenada; un alto sin convencer **tapa** a los que no son altos detrás
  (`RecalculateAudienceObstructions`); los tapados no reciben Vibe. `MoveToFront` reordena la fila.
- Gustos por miembro (`TastePreferences`): tempo, pistas, compás, tonalidad.
- **Ya hay una sinergia:** el Kid «Egg Him On» anima al alto (Cool Dude) con `AudienceTall`.
- Habilidades en patrón fijo o al azar; intención con sprite y «xN» del primer valor.

**Lo que falta para que las sinergias se *vean*** (lo más importante para la demo):
1. La intención no dice **a quién** va. Sin una línea o flecha hacia el aliado, «el Kid anima al alto»
   se lee como «el Kid hace algo». Hay un patrón parecido ya hecho: el texto de Spotlight (PRES-1).
2. El texto flotante de cada acción es el nombre interno (`AddVibe`, `MoveToFront`). Debe salir del
   glosario, como las habilidades desde TEXT-5.

**Código pequeño (≤ 1 sesión cada uno):**
- Objetivos nuevos: «el líder» (generalizar `AudienceTall`), «el de al lado», «el más convencido».
- Rasgos como lista (Alto, Pequeño, Líder…) en lugar del `bool isTall` (el propio código dice
  `// TODO Generalize`).

**Código mediano (Phase C):**
- **Disparadores**: «cuando convencen a un aliado», «cuando queda solo», «cuando el alto se mueve». Hoy
  el público solo actúa en su turno. El bus de eventos (S2/S4) ya publica lo necesario
  (`AudienceReactionEvent`, `LoopResolvedEvent`); falta el lado que escucha.
- **Irse del gig** (cuenta atrás, salir de la fila, y qué cuenta eso para ganar).
- **Desconvencer**: `IsConvinced` vuelve a `false` en `AudienceCharacterStats` (línea ~344); sin
  comprobar si es solo de Dev.
- **Guardaespaldas**: redirigir las cartas del jugador a otro miembro (el espejo de Spotlight).

**Veredicto:** la base aguanta. Para la demo basta con datos + los dos arreglos de legibilidad
(objetivo visible y texto del glosario). Los disparadores son lo que hace interesantes a los grupos de
verdad, y son Phase C.

## 2. Inventario de arte (`Assets/Resources/Sprites/Characters/Audience`)

| Archivo | Qué es |
| --- | --- |
| `enemigos.png` | Lámina con **16 enemigos**: arriba en silueta, abajo pintados (11 en color, 5 en gris) |
| `1.png`–`5.png` | 5 bocetos en línea blanca (no están en la lámina) |
| `jefe.png`, `secuas 1–3.png` | Un jefe punk morado y 3 secuaces; color casi final |
| `image.png` | **4 poses del secuaz 1** (el único con poses) |
| `Circle`, `Square`, `Triangle` | Formas de prueba |

Además, la imagen que mandó Claudio el 2026-10-30 trae **8 bocetos** más (no comprobado si están en la
carpeta). En total, unos **33 diseños**, más los 4 que ya están en el juego (Cool Dude, Kid, Big Guy,
Influencer). En `Ship/` están la nave entera, la sala de ensayo, la sala recreativa, el chillout y el
vendedor (`vendedor.png`, repetido en `Rooms/`).

## 3. Una idea por enemigo

Rol: **ATK** (estresa a la banda), **DEF** (protege a aliados), **POS** (cambia la fila), **SUP** (ayuda
a otros), **¿?** (necesita código mediano). «Datos» = se hace solo con datos.

### La lámina (de izquierda a derecha)

| # | Aspecto | Nombre provisional | Idea | Rol | Coste |
| --- | --- | --- | --- | --- | --- |
| L1 | gris, brazos como colas | **Contorsionista** | Se cuela un puesto adelante cada turno y desordena la fila: puede dejar al alto detrás y destapar a los pequeños | POS | Datos |
| L2 | peludo a rayas, seis ojos | **El que ya lo vio todo** | «Os vi en 2019, sonabais mejor». Blinda a todos con un poco de resistencia | DEF | Datos |
| L3 | búho turquesa con antena | **Antenita** | Retransmite el concierto: si lo convences, sus seguidores se convencen un poco | SUP | ¿? |
| L4 | oruga alta con espinas | **Ciempiés Alto** | Alto: tapa a los de atrás | POS | Datos |
| L5 | calamar con chistera | **Don Chistera, el promotor** | El dueño del local que nunca aplaude. Mientras siga sin convencer, su seguridad le protege | SUP | Pequeño |
| L6 | gusano gris de cuello largo | **Cuello de Grúa** | Mira por encima: nadie lo tapa. Pariente de la Reina Gusano | POS | Pequeño |
| L7 | rojo con tres antenas en flor | **Tres Mecheros** | Las antenas son mecheros: cuando lo convences, los enciende y anima a los de al lado | SUP | ¿? |
| L8 | gris alto, cabeza en punta | **Seguridad** | Alto y se pone delante del promotor; si existiera el guardaespaldas, se comería tus cartas | DEF | Datos / ¿? |
| L9 | bulto gris con trompa | **El Dormido** | Se durmió en primera fila: no hace nada y aguanta mucho. Despierta si tocas rápido | DEF | Datos + gustos |
| L10 | escarabajo alto con cuernos | **Escarabajo Metalero** | Alto y hace headbang: estrés a toda la banda | ATK | Datos |
| L11 | platillo pequeño, cara triste | **Platillo Triste** | Se aburre: si en 2 loops no lo convences, se va | ¿? | Mediano |
| L12 | gran masa azul con ojos | **La Masa** | Enorme y lenta: mucha resistencia, alta | DEF | Datos |
| L13 | cubo rojo de un ojo, falda | **Inspector del Imperio Nudo** | «No saben de arte, solo contratar»: pide un compás o un tempo y castiga si no se lo das | ATK | Datos (gustos) |
| L14 | verde, cuello largo, boca de corazón | **El Enamorado** | Groupie de un músico: le baja el estrés a su favorito y se lo sube a los demás por celos | SUP / ATK | Datos |
| L15 | manita con ojos en tallos | **Manito** | Pequeño, siempre tapado; aplaude a los de delante (les da resistencia) | SUP | Datos |
| L16 | cangrejo gris con caparazón | **Caparazón** | Se blinda y avanza para cubrir a otros | DEF / POS | Datos |

### Bocetos sueltos

| # | Aspecto | Nombre provisional | Idea | Rol |
| --- | --- | --- | --- | --- |
| B1 | perro con orejas de conejo (`1.png`) | **Perrito de Gira** | Sigue a la banda de concierto en concierto: el más fácil de convencer | — |
| B2 | cabeza de calamar con patas (`2.png`) | **Patas Largas** | Alto a ratos: se estira y tapa un turno sí y otro no | POS |
| B3 | perro de tres ojos (`3.png`) | **Olfato** | Huele qué le gusta al resto y revela sus gustos al convencerlo | SUP |
| B4 | cangrejo sonriente de muchos ojos (`4.png`) | **Sonrisas** | Ríe con los demás: cuando alguien estresa a la banda, él suma | ATK |
| B5 | sostiene una cabeza pequeña (`5.png`) | **El Ventrílocuo** | Dos opiniones en uno: dos intenciones por turno | ATK |
| B6 | alto, orejas como alas (imagen) | **Conejo Polilla** | Va hacia la luz: se mueve delante cuando hay efectos de escenario | POS |
| B7 | tubo con boca vertical (imagen) | **Bocina** | Grita: estrés al músico con más estrés | ATK |
| B8 | araña de patas largas (imagen) | **Zancudo** | Alto por las patas, pero se agacha si lo convences | POS |
| B9 | rana de puños grandes (imagen) | **Fan de Puños** | Choca los puños con un aliado: le da ánimo | SUP |
| B10 | sapo gordo con cuerno (imagen) | **Sapo Cuerno** | Se sienta y ocupa dos puestos | POS |
| B11 | coral de antenas (imagen) | **Antenas** | Recibe la música: si le gusta, se lo dice a todos | SUP |
| B12 | astronauta de brazos largos (imagen) | **Turista** | Graba con la mano larga y tapa la vista | POS |
| B13 | torre de bolas (imagen) | **La Torre** | Tres en uno: al convencerlo, se desarma en tres pequeños | ¿? |

### Jefe y secuaces

| Archivo | Idea |
| --- | --- |
| `jefe.png` | **El Jefe Punk**: no lo convences mientras sus secuaces lo animen (como el Kid con el alto) |
| `secuas 1–3` | Animan al jefe y abuchean a la banda; si convences a uno, los otros dos se pican y estresan más (¿?) |

## 4. Grupos

| Grupo | Miembros | Cómo funciona | Coste |
| --- | --- | --- | --- |
| **Primera fila** | Escarabajo Metalero (alto) + Contorsionista + Manito | El alto tapa a Manito; el Contorsionista se cuela y lo destapa sin querer. Decides a quién atacar según la fila | Datos |
| **El promotor y su seguridad** | Don Chistera + 2 Seguridad | La seguridad blinda al promotor; convences antes a la seguridad o aguantas hasta el final | Datos + «el líder» |
| **Pandilla punk** | Jefe + 3 secuaces | Los secuaces animan al jefe (patrón del Kid); convéncelos primero | Datos + «el líder» |
| **Imperio Nudo** | 2 Inspectores + Turista | Comparten el mismo gusto: una sola canción buena los convence a todos, una mala los enfada a todos | Datos (gustos) |
| **Groupies** | El Enamorado + Tres Mecheros | Cada uno tiene un músico favorito; los celos hacen que estresen al resto | Datos |

**Recomendación para la demo:** «Primera fila» (gig 1: enseña la fila y el alto) y «Pandilla punk» o
«El promotor» (gig 2: enseña que el público se ayuda). Coste: datos + objetivo visible en la intención
+ objetivo «el líder».
