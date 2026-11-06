# Combos de banda: el Big Num como contador de combos

Idea de Claudio, 2026-11-05, durante S3 (LEGIB-1a). Estado: **idea, sin decidir**. No es autoridad; el plan de la demo manda (`planning/active/DEMO-1211_Plan_de_Sesiones.md`).

## La idea (palabras de Claudio, resumidas)

- **Toda pista nueva sube el Big Num, sin falta.** Un mínimo garantizado cuando una pista suma. (Se trata en S3: ver abajo.)
- **Bonus cuando entra toda la banda**: destacar el momento en que empiezan a tocar los 4 juntos.
- **Los textos flotantes que describen cambios (KEY CHANGE, etc.) también en la previsión**, o al menos en el log: al pasar el ratón por Wormus Minor con Wormus Major sonando, el jugador sabe que habrá un cambio de tonalidad.
- **Combinaciones con nombre y valor de Hype**: batería y bajo coordinados, melodía del cantante y una guitarra al unísono, activar armonías…
- **El Big Num como contador de combos de un juego de pelea**: se ve el efecto de las cartas, sus combinaciones, cuánto encajan con los gustos del público y las habilidades entre músicos.
- **Combos que se desbloquean con la relación entre músicos**: gastar un nodo del mapa en la sala chill de la nave, o tras tocar juntos X veces / cumplir condiciones («skills de banda»).

## Notas del agente (✦ IA)

- **Referencia más cercana: Balatro.** Al seleccionar cartas, el juego nombra la mano («Full House») y muestra chips × mult antes de jugar. Es la misma regla que LEGIB-1a: el número que se ve es el que pasa, y además dice *por qué*.
- **Riesgo: reglas ocultas.** Un combo que suma sin aparecer en la previsión rompe el principio de LEGIB-1a. Todo combo debe pasar por la misma función que el pago (`ComputeLoopHypeDelta`) y mostrarse con su nombre en la previsión.
- **Catálogo corto y con nombre** (5–8 combos), mejor que una matriz de pares: se aprende y se puede enseñar.
- **Ya existe la base de los nombres**: los textos flotantes salen de `CompositionCardClassifier` + `ui_fx_*` (tempo, compás, tonalidad, instrumento, pista por rol, cambio mayor). Llevarlos a la previsión es barato.
- **«Toda la banda» depende de la alineación** (S6: 2, 3 o 4 músicos): «todos» = todos los de la alineación elegida.
- **Desbloqueo por relación** conecta con NAVE-1/NAVE-2 (salas «EN DESARROLLO»): la sala chill podría ser su casa futura. Después de la demo.
