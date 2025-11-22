# Sistema de Audiencia y Convencimiento – *A Long Way to the Top*

## 1. Concepto General
La audiencia funciona como el “enemigo” del juego. Cada espectador tiene:
- **VIBE** (equivalente a HP).
- **Acciones planificadas** visibles como íconos (inspirado en *Slay the Spire*).
- **Preferencias musicales** que determinan cómo reaccionan a la música.
- **Estados emocionales** que modifican sus comportamientos.

---

## 2. VIBE
- Mide qué tan convencido está el espectador.
- Al llenarse, el espectador queda **convencido**.
- Si el VIBE colectivo supera un umbral, el concierto es exitoso.

### Modificadores del VIBE
- Cartas musicales.
- Estados de la banda.
- Mutadores de canción.
- Acciones del público.

---

## 3. Tipos de Audiencia / Arquetipos
Ejemplos:
- **Fan apasionado**: sensible a melodías/solos.
- **Crítico exigente**: sensible a complejidad y errores.
- **Desinteresado**: requiere ritmo o energía.
- **Hostil**: sabotea y reduce VIBE.
- **Entusiasta contagioso**: contagia emoción.
- **Influencer**: afecta el VIBE global.

Cada arquetipo tiene acciones y curvas emocionales propias.

---

## 4. Acciones de la Audiencia
Se muestran como **intentos** con íconos.
Ejemplos:
- Ovación
- Aburrimiento
- Resistencia
- Abucheo
- Aplauso sincronizado

Afectan:
- Motivación/estrés de la banda.
- VIBE individual o global.

---

## 5. Interacciones con las Cartas
Las cartas pueden:
- Aumentar o disminuir VIBE según afinidades.
- Alterar tempo, intensidad o complejidad.
- Quitar o aplicar estados.
- Afectar individuos o grupos.

---

## 6. Preferencias y Resistencias
Tags posibles:
- **Género** (rock, funk, balada…)
- **Estado de ánimo** (calm, energetic…)
- **Sesgo por músico** (vocalista, guitarrista…)
- **Complejidad** (simple/complex)

Las cartas tienen metadatos que determinan la reacción.

---

## 7. Estados (Status Effects)
### En la audiencia:
- Hyped
- Bored
- Hostile
- Resonating
- Distracted
- Chanting

### En la banda:
- Stressed
- Motivated
- Embarrassed
- Inspired

---

## 8. Convencimiento Global
El VIBE promedio determina:
- Éxito del concierto.
- Activación de **Encore** o **Clímax**.
- Penalizaciones por fracaso.

Promueve estrategias:
- Satisfacer a un segmento del público.
- Combos musicales.
- Feedback positivo/emocional.

---

## 9. Implementación Base
Cada espectador:
- Tiene un **AudienceBehavior** (ScriptableObject).
- Ejecuta acciones dependientes de VIBE y turno.
- Usa una barra de VIBE y tooltips de intención.
- Reacciona a eventos de la canción.

---

## 10. Posibles Extensiones
- Zonas del público.
- Acústica y tipo de venue.
- Mutadores avanzados.
- Sinergias específicas con músicos.

---

