# Público con rol — inventario de enemigos, ideas y estado del código (2026-10-30, v2)

**Tipo:** idea / referencia (no autoridad). Lo que entre en la demo se diseña en su batch
(`planning/active/DEMO-1211_Plan_de_Sesiones.md`) y su verdad pasa a `SSoT_Audience_and_Reactions.md`.
Los nombres sin ficha son **provisionales**: los pone el equipo.
**v2 (misma tarde):** fichas de diseño de Matías para 4 enemigos, sprites nuevos (ranas, Reina, ataque del
jefe punk), gritos al público y plano de la nave. Sustituye a la v1.

## 1. Lo que hay en el repo (código leído el 2026-10-30)

**Ya se puede, solo con datos** (`AudienceCharacterData` + `AudienceAbilityData`):
- Cada habilidad es una lista de acciones con tipo, objetivo, valor y estado (`CharacterActionData`);
  pueden ir en patrón fijo (`followAbilityPattern`) o al azar.
- Acciones con efecto real: `AddVibe`, `RemoveVibe`, `BlockVibe` (sobre público), `AddStress`,
  `BlockStress`, `HealStress` (sobre la banda), `MoveToFront` (N pasos), `ApplyStatusEffect` (cualquier
  estado del catálogo SO: Shaken, Choke, Exposed…). `AddHooked`, `AddHeckled`, `AddStun` y `AddDazzled`
  están en el enum sin procesador.
- Objetivos: `Self`, `AudienceCharacter` (el aliado menos convencido), `RandomAudienceCharacter`,
  `AllAudienceCharacters`, `AudienceTall` (el primer alto que no sea él), `Musician` (respeta Spotlight),
  `RandomMusician`, `AllMusicians`.
- Posición: fila ordenada; un alto sin convencer **tapa** a los que no son altos detrás; los tapados no
  reciben Vibe. `MoveToFront` reordena la fila.
- Gustos por miembro (`TastePreferences`): tempo, pistas activas, compás, tonalidad. No hay eje de
  «género»: el jazz o el funk se aproximan con esos cuatro.
- **Ya hay una sinergia:** el Kid «Egg Him On» anima al alto (Cool Dude).
- Hay un `speechBubblePrefab` en `AudienceCharacterBase` (lo usa `AttackRoutine`, un camino antiguo) y
  `FxManager.SpawnFloatingText`.

**Para que las sinergias se *vean*** (entra en la legibilidad de la demo):
1. La intención no dice **a quién** va: falta una línea o flecha hacia el objetivo, como el aviso de
   Spotlight (PRES-1).
2. El texto flotante es el nombre interno (`AddVibe`, `MoveToFront`); debe salir del glosario.

**Código pequeño (≤ 1 sesión cada uno):** gritos (§5); objetivos «el líder», «el de al lado», «el más
convencido»; rasgos como lista en lugar del `bool isTall` (`// TODO Generalize`); cambiar de sprite por
estado (poses).

**Código mediano (Phase C):** disparadores (al convencer a un aliado, al quedar solo); irse del gig;
desconvencer (`IsConvinced` vuelve a `false` en `AudienceCharacterStats` ~l. 344, sin comprobar si es
solo Dev); guardaespaldas (redirigir cartas del jugador); **dividirse en dos** (aparecer miembros nuevos
a mitad de gig); estirarse para volverse alto un turno.

**Veredicto:** la base aguanta. Para la demo: datos + objetivo visible + texto del glosario + gritos.

## 2. Inventario de arte (`Assets/Resources/Sprites/Characters/Audience`)

| Archivo | Qué es | Listo para el juego |
| --- | --- | --- |
| `enemigos.png` | Lámina de 16 enemigos (11 en color, 5 en gris) | No: hay que recortarlos |
| `image (1)–(4).png` | **Fichas de diseño** de 4 de la lámina (ver §3) | Referencia |
| `234.png` | Rayas, versión final con contorno | **Sí** |
| `asfasf.png` | Tentáculos (funk), versión pintada con contorno | **Sí** |
| `rana enemigo.png`, `rana enemigo 2.png` | Dos ranas oscuras, nuevas | **Sí** (esbozo pintado) |
| `jefe.png`, `secuas 1–3.png` | Jefe punk y 3 secuaces | **Sí** |
| `image.png` | 4 poses del secuaz 1 | **Sí** |
| `ataque jefe.png` | El jefe punk señalando con un bocadillo «BBUUU…» | Referencia del grito |
| `jefe (1).png` | **La Reina Gusano**: cíclope sobre un cuerpo de huevos | **Sí** |
| `1.png`–`5.png` | 5 bocetos en línea blanca | No |
| `Screenshot 2026-10-30 220359.png` | Ideas de gritos (§5) | Referencia |

Más los 8 bocetos de la imagen del 2026-10-30 (no están en la carpeta) y los 4 que ya juegan (Cool Dude,
Kid, Big Guy, Influencer). Unos **37 diseños**; **9 con sprite usable ya**: Rayas, Tentáculos, las dos
ranas, el jefe punk, sus 3 secuaces y la Reina.

## 3. Los cuatro con ficha (lo que dice la ficha → cómo jugarlo)

### Antena (verde, cuello largo, ficha `image (1)`) — le gusta el **jazz / blues**
Ficha: antena de mucha potencia que puede causar un silencio; puede pararse y dejar sin visión a los demás;
difícil de complacer, ha vivido muchas vidas; ataque de confusión; adivinar; burbujas de silencio; puede
estirarse; carácter caótico, medio siniestro.
- **Burbuja de silencio:** Ahogo (Choke) a un músico: ese músico no actúa un turno. *Datos* (comprobar que
  el estado del catálogo de músicos se aplica desde el público).
- **Se para:** alterna sentado / de pie; de pie tapa a los de atrás. *Pequeño* (alto por turno).
- **Gustos:** tempo lento + tonalidad menor (aproximación al blues). *Datos.*
- **Adivinar:** acierta qué tipo de carta jugarás y la encarece. *Phase C.*

### Tentáculos (rojo, tres tentáculos, ficha `image (2)`, sprite `asfasf.png`) — le gusta el **funk**
Ficha: si los 3 tentáculos están activos causa graves problemas; lanza cosas con los tentáculos; criatura
telequinética de carácter difícil; la música muy fuerte le hace daño; cuando no está a gusto crea confusión.
- **Tres tentáculos:** patrón fijo *carga · carga · lanzamiento*. La intención muestra 1, 2 y 3 tentáculos;
  al tercero, mucho estrés a un músico. Es la mecánica más legible de las cuatro. *Datos.*
- **La música fuerte le duele:** odia muchas pistas activas (eje de pistas). *Datos.*
- **Confusión:** si no le gusta el loop, Shaken a un músico. *Datos.*

### Cubo (cíclope cuadrado, ficha `image (3)`) — le gusta la **ópera y el metal**
Ficha: metal industrial; ataque de ópera, grito capaz de romper el vidrio; diamante con energía solar
**extraído por la raza de los Cono**, que contiene un minisol y le da telequinesis; se quita la cabeza; le
gusta dar sustos.
- **Grito de ópera:** estrés a toda la banda. *Datos.*
- **Susto:** se quita la cabeza: Shaken al músico con menos estrés. *Datos.*
- **El diamante de los Cono:** tiene algo personal con **Conito**. Sus gritos van a él (§5) y podría
  ir siempre a por Conito. *Datos* (objetivo fijo a un músico: pequeño).

### Rayas (peludo a rayas, ficha `image (4)`, sprite `234.png`) — le gusta **la música que le hace vibrar**
Ficha: criatura maleable sin huesos; pasa por agujeros pequeños; puede dividirse en dos; al dividirse,
cada mitad tiene gustos y temperamento distintos; se le pegan las cosas.
- **Escurridizo:** se cuela al frente aunque haya un alto (`MoveToFront`). *Datos.*
- **Le gusta vibrar:** quiere el bajo sonando (eje de pistas). *Datos.*
- **Se le pegan las cosas:** se queda con los estados que le pones y los devuelve. *Phase C.*
- **Dividirse:** al bajar de la mitad, se parte en dos (roja y blanca) con gustos opuestos. Es la idea más
  vistosa y la más cara. *Phase C.*

## 4. Ideas para el resto

| Enemigo | Idea | Rol | Coste |
| --- | --- | --- | --- |
| **Ranas** (las dos nuevas) | Cantan a dúo: si las dos siguen sin convencer, el croar estresa el doble. Una rosada y otra azul por el contorno | ATK | Datos |
| **Jefe punk** + 3 secuaces | Los secuaces lo animan (patrón del Kid) y abuchean a la banda; el jefe grita «BBUUU…» | SUP / ATK | Datos + «el líder» |
| **Reina Gusano** | Ver §6 | Jefa | Datos + gritos |
| Contorsionista (gris, brazos-cola) | Se cuela un puesto adelante cada turno y desordena la fila | POS | Datos |
| Ciempiés Alto (oruga naranja) | Alto: tapa a los de atrás | POS | Datos |
| Don Chistera (calamar con chistera) | El promotor que nunca aplaude; su seguridad lo protege | SUP | Pequeño |
| Seguridad (gris alto) | Alto, delante del promotor | DEF | Datos |
| El Dormido (bulto gris) | No hace nada y aguanta mucho; despierta si tocas rápido | DEF | Datos |
| Escarabajo Metalero | Alto; headbang: estrés a toda la banda | ATK | Datos |
| Platillo Triste | Se aburre y se va | — | Phase C |
| La Masa (azul) | Enorme, mucha resistencia | DEF | Datos |
| Manito (mano con ojos) | Pequeño, siempre tapado; aplaude a los de delante | SUP | Datos |
| Caparazón (cangrejo) | Se blinda y avanza para cubrir a otros | DEF | Datos |
| Antenita (búho turquesa) | Retransmite: si lo convences, sus seguidores se convencen un poco | SUP | Phase C |

## 5. Gritos: personalidad y legibilidad a la vez

Idea de la pizarra (`Screenshot 2026-10-30 220359.png`) y del ataque del jefe (`ataque jefe.png`): el
público le grita cosas a la banda, a veces a un músico concreto. Ejemplos de la pizarra: «Tu música
apesta», «Puedo hacer eso en 2 segundos con IA», «Tu mamá es watona», un fan con un cartel «TE AMO».

**Por qué vale la pena:** además de dar risa, el grito **dice qué va a pasar y a quién**. «¡Conito, tu
raza me robó el diamante!» justo antes de estresar a Conito resuelve parte del problema 1 de §1.

**Cómo:** cada habilidad lleva una lista de frases; al activarse, sale una al azar en un bocadillo sobre
el miembro del público. `{músico}` se sustituye por el nombre del objetivo ya resuelto. Textos en el
glosario (ES y EN), como las habilidades desde TEXT-5. Coste: 1 sesión.

Ejemplos (para que el equipo los reescriba):
- Cubo → Conito: «¡Tu raza me robó el diamante!» · «Cono tenías que ser.»
- Antena → a cualquiera: «He vivido nueve vidas y ninguna tan aburrida.»
- Tentáculos: «Uno…» · «Dos…» · «¡TRES!» (sus tres turnos; la cuenta es la intención).
- Jefe punk: «BBUUU…» · «¡Fuera!» · Secuaces: «¡Eso, jefe!»
- Rayas: «Más bajo… más… ¡ahí!» (pide el bajo).
- Reina → Sibi: «Hijo mío, vuelve a la colmena.» (en su ficha, Sibi siempre sigue la voluntad de la colmena).
- Cualquiera → Robot C2: «¿Te programaron para desafinar?» · «Puedo hacer eso en 2 segundos con IA.»
- Convencido: un cartel «TE AMO».

Los insultos más fuertes quedan a criterio del equipo: la demo es pública. **Decidido (Claudio, 2026-11-06):** «Tu papá es travesti» queda fuera de GRITOS-1, porque funciona como burla a una identidad.

## 6. La Reina Gusano (sprite `jefe (1).png`)

Restricción de M4: 1–2 mecánicas legibles, sin quitar poderes (Keep #113). Propuesta con datos:
- **Puesta de huevos:** patrón fijo *huevo · huevo · eclosión*. Cada huevo le da resistencia (`BlockVibe`
  sobre sí misma); la eclosión estresa a toda la banda. La intención cuenta los huevos. Se lee como los
  tentáculos y enseña lo mismo: «cuando llega a 3, cuidado».
- **La colmena llama a Sibi:** sus gritos van a Sibi y, si Sibi está en la banda, le sube el estrés a él.
  Le da historia al combate sin texto aparte.
Pendiente de Matías: si la Reina tiene acompañantes (larvas) o va sola.

## 7. Grupos para los tres gigs de la demo (solo con sprites listos)

| Gig | Público | Qué enseña |
| --- | --- | --- |
| 1 | **Las dos ranas + Rayas** | Lo básico: intención, gustos (Rayas quiere bajo), y que el dúo pega el doble mientras sigan las dos |
| 2 | **Jefe punk + 3 secuaces** (o 2 + Tentáculos) | Que el público se ayuda: los secuaces animan al jefe; los gritos dicen a quién |
| 3 | **Reina Gusano** | Contar hasta 3 y prepararse; la colmena y Sibi |
