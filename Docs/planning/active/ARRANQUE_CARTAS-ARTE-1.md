# Arranque CARTAS-ARTE-1 · la cara nueva de las cartas

Escrito el 2026-11-14 al cerrar ECON-2. Planning, no autoridad. Fila 5 del «Orden de entrada» de
`DEMO-1211_Plan_de_Sesiones.md` (D-PLAN-21). Si no llega antes del 27-nov, pasa a POST-DEMO D7 (CARTAS-ARTE-2).

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 CARTAS-ARTE-1 — la cara nueva de las cartas.

Lee Docs/planning/active/ARRANQUE_CARTAS-ARTE-1.md y haz CARTAS-ARTE-1. Empieza con
/buscar-referencias --all (marcos por tipo de carta, coste y recurso en la esquina, carta
heroica / ultimate con marco propio, Slay the Spire, Monster Train, Midnight Suns) y
preséntame las decisiones en formato D antes de construir.

Contexto: ECON-2 cerrada el 2026-11-14 (la Inspiración: las composiciones dan ★ al jugarlas,
los Finisher la gastan en el último loop; reglas en F12 ▸ Inspiración). La cara de la carta
todavía es la de siempre: la ★ y el coste salen en los círculos viejos y el Finisher solo se
distingue por el rótulo FINISHER.

Restricciones: logs con la etiqueta [CARTAS-ARTE-1]; no escribir .meta; no editar MidiGenPlay;
sin commit ni push sin pedirlo; ≤3 smokes jugados por Claudio; pasos de editor uno a uno
(o tutorial-artefacto si hay mucho prefab); el arte es de Matías, no se inventa.

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página
de frentes y el arranque de la siguiente sesión del orden de entrada.
```

## Qué hereda de ECON-2

1. **La ★ en la cara.** La estrella con el número arriba a la izquierda, como en la maqueta de Matías. Hoy hay
   dos círculos: `inspirationCostBadgeRoot` (coste) e `inspirationGenBadgeRoot` (★ que da), los dos en
   `CardUI.prefab` y en el prefab de la mano, ocultos con 0. Muestran los valores **efectivos**
   (`InspirationRules.EffectiveCost` / `EffectiveGenerated`): si F12 cambia una regla, la cara cambia.
   Pregunta: ¿la estrella dice lo que la carta **da** (composición) o lo que **cuesta** (Finisher), con dos
   estilos, o hay dos esquinas?
2. **El marco del Finisher** (cian, con el emblema de estrella, maqueta abajo a la derecha). Hoy el Finisher solo
   se distingue por `typeTextField` = «FINISHER» (`ui_card_type_finisher`) y la línea de regla
   `ui_card_finisher_rule`. Son 4 cartas: Overload, Psychic Waves, Gran Final y Redoble Final.
3. **El Voltaje.** Slap Groove y Super Slap piden Voltaje, pero la regla `ignoreResourceCosts` (encendida) hace
   que no se cobre, porque la cara no lo comunica. Decidir: un marcador de recurso en la cara (y apagar la regla)
   o dejarlo apagado hasta después de la demo.
4. **Marcos por tipo:** Composición / Habilidad (acción) / Finisher. Hoy el fondo por tipo lo pone
   `CardBase.ApplyTypeBackground` (S5b).
5. **Ilustraciones por instrumento** (vocalista, guitarra-bajo, batería con fondos aparte, órgano).

## Entradas

- Maquetas: `Docs/reference/ideas/img/2026-11-12_cartas_maqueta_marcos.png` y `_mano.png`.
- Arte de Matías (Google Drive): ilustraciones por instrumento
  https://drive.google.com/drive/folders/1L_VZ0T_rK6-8Znpewoaglolzuo5YtWx3 · números 0–9 dibujados (#17, para S11
  o aquí si encaja con la estrella) https://drive.google.com/drive/folders/1UtUJcoa28Jf-Mki1CwouKkPT-SEea8Oy
- Diseño: `Docs/reference/ideas/2026-11-12_inspiracion_heroismo.md` y `SSoT_Card_System.md` §2.3 (convención y
  Finisher), §10.4 (identidad visual de la cara, S5b) y §10.5 (rojo de no jugable).
- Código: `CardBase.SetCard` (badges, tipo, `ApplyTypeBackground`, `ApplyOwnerIcon`), `CardUI.prefab`,
  `CardDefinitionDescriptionExtensions` (texto del Finisher).

## Decisiones previsibles (formato D)

- **D1 · Qué dice la estrella:** lo que da y lo que cuesta en la misma esquina con dos estilos / dos esquinas /
  solo lo que cuesta.
- **D2 · El Voltaje:** marcador en la cara ahora / sigue sin cobrarse hasta después de la demo.
- **D3 · Alcance del arte:** todas las cartas de los mazos iniciales / solo marcos y estrella, ilustraciones
  donde ya existan.
- **D4 · Los números de Matías** en la estrella ahora / en S11 con el resto.

## Smokes (≤3, jugados por Claudio)

1. Una mano con composición, acción y Finisher: cada una con su marco; la ★ y el coste se leen sin explicación.
2. Cambiar en F12 ▸ Inspiración la ★ por composición y el coste del Finisher: la cara se actualiza.
3. Regresión: rojo de no jugable, tooltips y detalle de carta (clic derecho) siguen funcionando con la cara nueva.
