# Arranque FEEDBACK-2 · enviar el feedback sin salir del juego

Escrito el 2026-11-14 al cerrar S9c (DEMO-FIX-1). Planning, no autoridad. Fila 3b del «Orden de entrada» de `DEMO-1211_Plan_de_Sesiones.md`, antes de ECON-2 (decisión de Claudio).

## Por qué

En S9c el botón «Enviar feedback» abría un `mailto:`. En el PC de Claudio solo abre el navegador en su página de inicio: muchos jugadores no tienen cliente de correo configurado. El botón quedó **desactivado** en `UIManager.prefab` (`FeedbackButton`, `m_IsActive: 0`); su código (`PauseMenuController.OnFeedbackPressed`, con copia al portapapeles) sigue ahí.

## Antes de la sesión (Claudio, sin código)

1. Crear un **formulario de Google** «ALWTTT demo · feedback» con estas preguntas, todas de **respuesta corta o párrafo**:
   - Mensaje (párrafo, obligatoria)
   - Versión · Sistema · Escena · Banda · Gig · Resumen de telemetría (respuesta corta, opcionales; los rellena el juego)
2. En «Respuestas», activar **Recibir notificaciones por correo** y enlazar una hoja de cálculo.
3. Pasar a la sesión el enlace de **edición** del formulario (para sacar la URL `formResponse` y los `entry.N`).

## Pegar en la sesión nueva

```
Mode: IMPLEMENTATION. DEMO-1211 FEEDBACK-2 — enviar el feedback sin salir del juego.

Lee Docs/planning/active/ARRANQUE_FEEDBACK-2.md y hazlo. Formulario: <enlace de edición>.
Empieza con /buscar-referencias --all (panel de feedback dentro del juego, aviso de privacidad)
y preséntame las decisiones en formato D antes de construir.

Restricciones: logs con la etiqueta [FEEDBACK-2]; no escribir .meta; no editar MidiGenPlay;
sin commit ni push sin pedirlo; ≤3 smokes jugados por Claudio; pasos de editor uno a uno.

Cierre: smokes jugados, docs propuestos y aplicados con OK, proyectos.md, foco.md, página
de frentes y el arranque de ECON-2 revisado.
```

## Alcance

1. **Panel en la pausa**: cuadro de texto multilínea, «Enviar», «Cancelar», una línea de privacidad («Se envía tu texto, la versión del juego y tu sistema. Nada más.») y estados «Enviando… / ¡Gracias! / No se pudo enviar».
2. **Envío** con `UnityWebRequest.Post` a `https://docs.google.com/forms/d/e/<id>/formResponse` con los `entry.N`. Funciona con `timeScale = 0` (la pausa): usar corrutina con tiempo real o `async`.
3. **Sin secretos en la build.** Bird Boxing (2020, `Bird Boxing/Assets/Scripts/SendMailto.cs`) mandaba por SMTP con la contraseña de una cuenta escrita en el código: no repetirlo.
4. **Si falla** (sin red, Google cambia algo): mostrar el error y ofrecer el respaldo de S9c (copiar al portapapeles + dirección).
5. Volver a activar el botón (`FeedbackButton`) apuntando al panel.

## Decisiones previsibles (formato D)

- **D1 · Qué datos van solos:** solo versión y sistema / + escena, banda y gig / + resumen de la última línea de telemetría.
- **D2 · Dónde vive el panel:** solo en la pausa / también en el menú principal y en la pantalla de fin de gira.
- **D3 · Si el envío falla:** solo mensaje de error / error + copiar al portapapeles / reintento en el siguiente arranque.

## Entradas

- `Assets/Scripts/UI/Pause/PauseMenuController.cs` (`OnFeedbackPressed`, `TelemetryFolder`).
- `Assets/Prefabs/Core/UIManager.prefab` (PauseCanvas ▸ MainPanel ▸ ButtonsLayout).
- `Assets/Scripts/DevMode/DevRunTelemetryLogger.cs` (`OutputFolder`).
- Referencias: formularios de Bird Boxing (Google Drive, «Bird Boxing – Demo #1», 165 respuestas, 2020); ChatGPT 2025-04-10 «ALWTTT Demo Launch Prep» (chat 1194).

## Smokes (≤3, jugados por Claudio)

1. Pausa → feedback → escribir → Enviar: «¡Gracias!», la respuesta aparece en la hoja y llega el correo de Google.
2. Sin red (Wi-Fi apagado): mensaje de error y el texto no se pierde (respaldo al portapapeles).
3. Enviar, continuar el gig: nada queda congelado ni con el foco en el cuadro de texto (Esc y la mano funcionan).
