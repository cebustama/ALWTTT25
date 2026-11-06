# Medición de Claude en el taller armónico (2026-11-06)

Diez frases por tarea, por el proxy real (`/api/llm` con `npm run llm:dev`) y el puente de suscripción (`web/claudiobustamante-site/tools/llm/puente-suscripcion.mjs`). Script: `web/.../tools/llm/medir-armonia.mjs`. Las frases de «crear» y lo que se considera razonable son de Music Lab (`../revision-music-lab-perillas-2026-11-06.md` §5).

- **Crear — válida:** la página la acepta (el lector no da ningún aviso, modo y compás existen, los compases suman un entero), con el reintento único de la página. **Útil:** el modo está entre los que Music Lab ve razonables para la frase (y 3/4 en la cueca). No mide si suena a lo pedido: eso lo dice el oído de Claudio.
- **Ajustar — válida:** todos los valores devueltos se aceptan. **Útil:** las perillas se mueven en el sentido que pide la frase y el cambio **se oye** con esa progresión (cambia lo que añaden los niveles 2 o 3), o no cambia nada cuando la frase pide algo imposible (cambiar acordes, adornar la vuelta 1).

## Resultado con los prompts finales (v2)

| Modelo | Crear: válidas | Crear: útiles | Ajustar: válidas | Ajustar: útiles | Archivos |
|---|---|---|---|---|---|
| Haiku 5.5 | 10/10 | 10/10 | 10/10 | 9/10 | `v2-normal.json`, `v2b-normal.json` (ajustar) |
| Sonnet 5.5 | 9/10 | 9/10 | 9/10 | 9/10 | `v2-alta.json` |

**Decisión D3 = A:** Haiku por defecto (cumple el criterio de 9/10 en las dos tareas); Sonnet con la casilla «Calidad alta».

- Fallo de Haiku: «más jazz en el 2» sobre la Axis enciende el remate rico, que en la Axis no suena (el bucle ya cierra IV→I). La página lo avisa («con esta progresión y este modo, el cambio no se oye»).
- Los dos fallos de Sonnet son de formato (`respuesta_invalida`): se corrige a sí mismo y devuelve dos objetos JSON. El puente no le pasa el razonamiento que sí tiene en producción (`effort: low`), así que en producción debería fallar menos; no está medido. El prompt ya pide un solo objeto.
- Musicalmente, Sonnet da respuestas algo más ricas (cueca `I – V7 – I – IV (0.5) – V7 (0.5)`, jazz con `im9` e `IV9`). Haiku acierta lo esencial: Creep exacta, flamenco `i – II – VII – V` en Frigio, Eólico sin bemoles mal puestos y 3/4 en la cueca.

## Historia

- `v1-*` (primer prompt): Haiku 10/10 y 10/10 según el check de entonces, pero a mano «el 2 y el 3 suenan igual» encendía una perilla sin efecto. Se añadió al check «se oye» y al prompt la regla de comprobar que el cambio se oiga. Sonnet v1 no cuenta: el proxy se recargó a mitad de la medición (`no_json`) y apareció el problema de los dos JSON.
- Caso 7 («no me toques el final») pasó de la Axis a la Royal road: en la Axis no hay remate que quitar, así que el caso no medía nada.

## Límites

- Tiempos: 10–25 s por llamada a través del puente (CLI); no representan la API.
- «Útil» es una comprobación automática y generosa; falta escuchar las progresiones creadas.
