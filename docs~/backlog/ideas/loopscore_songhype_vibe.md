
# LoopScore, SongHype y VIBE – Documento Técnico (Markdown)

## 📍 Propósito
Este documento define formalmente cómo calcular **LoopScore**, **SongHype** y su relación con **VIBE de la audiencia**, dentro del sistema musical de *A Long Way to the Top*. Es un módulo autocontenido que puede integrarse con el sistema actual de composición basada en cartas.

---

## 🎼 1. Definiciones

### **Loop**
Una iteración de reproducción de la canción.  
Cada loop evalúa:
- La calidad musical del arreglo actual.
- Sinergias activas.
- Complejidad y riesgo.
- Errores cometidos.
- Diversidad y novedad respecto al loop anterior.

### **LoopScore**
Medida cuantitativa de qué tan bien funcionó el loop musical.  
Se usa como input para:
- Modificar SongHype.
- Modificar VIBE.
- Determinar errores y estados emocionales.

Representación:
```
LoopScore: float (puede ser negativo)
```

### **SongHype**
Mide el estado emocional global del público durante ESTA canción, no entre canciones.

Representación:
```
SongHype: float en [0, 100]
```

### **VIBE**
Convencimiento acumulado del público respecto a la banda (persistente entre canciones).  
Un concierto exitoso aumenta VIBE; una canción es una contribución parcial.

Representación:
```
VIBE: valor acumulativo en cada audiencia individual
```

---

## 🔢 2. Cálculo del *LoopScore*

### **Fórmula general**

```text
LoopScore = 
    CalidadMusical
  + Sinergias
  - PenalizaciónErrores
  - PenalizaciónRepetición
```

### 2.1 **CalidadMusical**

Evalúa qué tan completa e interesante es la música desde un punto de vista estructural y de densidad.

Parámetros sugeridos:

| Factor | Descripción | Rango sugerido |
|--------|-------------|----------------|
| Pistas activas | Cantidad de músicos tocando algo | 0–6 |
| Capa rítmica | ¿Hay ritmo funcional? (batería o percusión) | 0–4 |
| Capa de groove | Bajo estable + ritmo coherente | 0–4 |
| Capa armónica | Acordes presentes | 0–4 |
| Capa melódica | Melodía explícita | 0–4 |
| Capa vocal/tema | Si es canción cantada | 0–4 |

Ejemplo de fórmula:

```text
CalidadMusical =
    (TracksActivos * 1.5)
  + (TieneRitmo ? 3 : 0)
  + (TieneBajoGroove ? 3 : 0)
  + (TieneArmonia ? 3 : 0)
  + (TieneMelodia ? 3 : 0)
  + (TieneVoz ? 3 : 0)
```

### 2.2 **Sinergias**

Aplica multiplicadores cuando se dan combinaciones significativas.

Ejemplos:

| Sinergia | Condición | Bonus |
|----------|----------|--------|
| LockGroove | Bajo + batería = ritmo sólido | +5 a +15 |
| Spotlight | Un músico destacado en clímax | +5 a +20 |
| ThemeChain | Misma temática dos loops seguidos | +5 |
| ProgressivePulse | Compás irregular + alta complejidad | +10 a +25 |

Sugerencia de implementación:

```csharp
float Sinergias = synergyEngine.CalculateBonuses(context);
```

### 2.3 **PenalizaciónErrores**

Deriva de fallos técnicos al ejecutar el loop.

```text
PenalizaciónErrores = Sum(MissSeverityPorMúsico) * k
```

Ejemplo:

| Severidad | Efecto |
|-----------|--------|
| Pequeño error | -2 |
| Error notorio | -5 |
| Error grave (rompe sección) | -10 |

### 2.4 **PenalizaciónRepetición**

Evita spammear el mismo loop sin cambios.

Forma simple:

```text
PenalizaciónRepetición = 
    if(LoopMuySimilarAlAnterior) -5
    if(3 loops iguales) -15
```

---

## 🔁 3. Cálculo de SongHype

### **Actualización por loop**

```text
SongHype_next = clamp(SongHype_prev + ΔHypeLoop, 0, 100)
```

### **Relación con LoopScore**

```text
ΔHypeLoop = 
    if(LoopScore > 20) +15
    if(LoopScore > 10) +8
    if(LoopScore > 0) +3
    if(LoopScore == 0) 0
    if(LoopScore < 0) -5
    if(LoopScore < -10) -12
```

### Efectos secundarios

- SongHype alto → bonus creciente a VIBE.
- SongHype bajo → la audiencia se enfría.

---

## 🎚 4. Cálculo del VIBE basado en LoopScore + SongHype

VIBE se aplica **por espectador** usando afinidades.

```text
ΔVIBE_espectador = 
    LoopScore
  * AfinidadEspectador (0.5–1.5)
  * (1 + SongHype / 100)
```

Interpretación:

- Si la canción es buena pero el hype está bajo → efecto modesto.
- Si la canción está en el clímax (SongHype > 70) → multiplicador poderoso.
- Si el público no tiene afinidad → efecto bajo aunque la canción esté buena.

---

## 🔝 5. Incentivo a cortar la canción en el pico

El jugador puede terminar la canción manualmente.  
El **payout final** depende del **SongHype actual** y del promedio de LoopScores.

```text
PayoutFinalVIBE =
    (PromedioLoopScores * (1 + SongHype / 100))
```

Idea:
> Terminar la canción en el pico es óptimo.  
> Estirarla demasiado baja Hype → bajas ganancias.

---

## 🧩 6. Ejemplo numérico

Canción con dos loops, segundo es el pico:

| Loop | LoopScore | Hype antes | ΔHypeLoop | Hype Nuevo |
|------|-----------|------------|------------|------------|
| 1 | 12 | 0 | +8 | 8 |
| 2 | 25 | 8 | +15 | 23 |

Supongamos público amante del groove con afinidad 1.3:

```text
ΔVIBE = 25 * 1.3 * (1 + 23/100) ≈ 40.0
```

Si termina ahora el jugador recibe 40 de VIBE.  
Si hace un loop extra con baja variación:

- LoopScore ≈ 5
- ΔHypeLoop = +3 → Hype ≈ 26
- ΔVIBE sube muy poco por loop
- El promedio final baja

Resultado: **mejor terminar en el clímax**.

---

## 📌 7. Conclusión

- **LoopScore** es la métrica base del output emocional de cada loop.
- **SongHype** es un recurso dinámico que representa la conexión en tiempo real.
- **VIBE** es la traducción a impacto real en el público.
- Esto permite loops dinámicos, decisiones tácticas y picos climáticos diseñados por el jugador.

---

**Fin del documento.**
