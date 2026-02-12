# ALWTTTT · Diseño de performance en vivo y sistema de combate (Resumen)

**Fecha:** 18 Nov 2025  
**Autor:** Claudio + ChatGPT  
**Ámbito:** Portar el sistema de composición por cartas (funcional en *Ship Interior*) a *Gigs* con resolución de combate/vibe.

---

## 1) Objetivo
Convertir la composición por cartas en **performance-combate** durante conciertos, manteniendo:
- La **improvisación** y variación en cada show.
- La existencia de **canciones icónicas** (lo que la gente reconoce/canta).
- Un **pacing** claro por secciones (turnos) y feedback legible (vibe, efectos, estrés).

---

## 2) Núcleo del sistema de resolución por sección
Cada sección (Intro, Verso, Estribillo, Solo, Clímax, Outro) funciona como **un turno**:

1. **Cartas → Intenciones Escénicas**  
   Cada carta aporta *IntentTags* y un *AudienceImpactProfile* (Vibe base, reparto frente/fondo, riesgo de estrés, penalidad de fallo).  
   **IntentTags sugeridos:** `Power`, `Groove`, `Emotion`, `Virtuosity`, `Rebellion`, `CohesionBoost`.

2. **Fusión de cartas en la sección** → *PlannedSectionProfile*  
   - `IntentIntensity[IntentTag]` (fuerza de cada intención)  
   - `TargetBaseVibe` (suma ponderada de las cartas)  
   - `TargetRisk` (promedio ponderado de riesgo)  
   - Spotlight opcional (rol expuesto).

3. **Ejecución por músico** (stats mínimos):  
   - `InstrumentSkill[style/role]`, `StagePresence`, `CohesionWithBand`, `Stress/Fatigue`, `SpotlightAffinity`.  
   - Para cada músico: `PerformanceScore = Ability - Demand`, con modificadores por **estrés** y **mismatch** de spotlight.  
   - Posible `Mistake` si Score < umbral y el riesgo de la sección es alto.

4. **Resultado de la sección** → *LiveSectionResult*:  
   - `DeliveredVibeFront` y `DeliveredVibeBack` (lane natural: cerca/lejos del escenario).  
   - `CrowdHype` (medidor persistente, multiplica impacto futuro).  
   - `PerformerBreakdown` (scores por músico, stress delta).  
   - `SideEffects` (buffs/debuffs narrativos, p.ej., *Mosh Pit*, *Crowd Chant*, *Awkward Silence*, *StunCrowdFront*, *ShieldBandStress*).

**Fórmula guía:**  
`finalVibe = TargetBaseVibe × ExecutionFactor × CohesionFactor × MistakeFactor`  
Distribución: frente/fondo según *IntentIntensity* y *AudienceImpactProfile*.

---

## 3) Canciones icónicas vs improvisación constante
Para reconciliar identidad y variación, se separan **tres niveles**:

- **(A) Hook / Motivo Identitario (SignatureHook):**  
  Motivo recordable (riff/coro/letanía). Es un *preset de varias cartas* bloqueado.  
  El público recuerda **el hook**, no la toma entera.

- **(B) Arreglo de una noche (Live Arrangement):**  
  Cómo ese hook se interpretó hoy (tempo, estilo, solos, dinámicas). Es efímero/combate.  
  Puede **guardarse como “bootleg”** para reusarse tácticamente (más fiable, menos frescura).

- **(C) Canon de la banda:**  
  Hooks que han pegado repetidamente quedan **canonizados** (tienen nombre y fanservice).  
  Beneficios al ejecutarlos: +Vibe inmediato, menor StressRisk, coros del público, etc.

**Conclusión:** la banda **improvisa siempre**, pero su **mitología** se construye con **hooks** (lo que el público canta y espera).

---

## 4) Dos estilos de performance y un híbrido recomendado

### Opción 1 · Composición previa (pre-armada)
- Antes del show eliges cartas para toda la estructura.  
- En vivo, la banda ejecuta lo planificado, resolviendo por secciones.  
- + Claridad y “show pro”; – Menos agencia en tiempo real.

### Opción 2 · Composición dinámica en vivo (looping por vueltas)
- Empiezas con un loop (p.ej., batería+bajo+acordes).  
- Mientras suena, juegas más cartas para **extender/modificar** la sección.  
- Cierras la sección para **resolver** Vibe/Efectos/Stress.  
- + Máxima fantasía de improvisación; – Exige UI y pacing finos, evitar loops infinitos.

### Opción 3 · **Híbrido (recomendado)**
- **Macroestructura predefinida** (Intro → Verso → Estribillo con Hook → Puente → Clímax → Outro).  
- **Dentro de cada bloque**, el loop arranca con lo planificado y **tú improvisas** jugando cartas durante algunas vueltas.  
- Al **cerrar el bloque**, se resuelve el turno (LiveSectionResult) y pasas al siguiente bloque.  
- Mantiene identidad de canción **y** decisiones tácticas vivas por sección.

---

## 5) Reglas de pacing y riesgo (para el modo dinámico/híbrido)
- Cada vuelta **aumenta StressRisk** y probabilidad de `Mistake`.  
- *Hand Economy:* robos limitados por sección; cartas de *Close/Break/Transition* para cortar/encadenar.  
- Spotlight y *Demand* del bloque modifican los pesos de cada músico.  
- **Fatiga persistente** entre bloques para evitar “stall”.

---

## 6) UI/UX (feedback por turno)
- Panel de fin de sección:  
  - 🔥 **Frente**: `+X Vibe` (texto de flavor “Fans gritan TU NOMBRE”)  
  - 🌊 **Fondo**: `+Y Vibe` (texto “La gente atrás empieza a moverse”)  
  - Destacados/errores por músico (Stress ±, SideEffects).  
- Marcadores persistentes: `CrowdHype`, `Cohesion`, `Stress` por músico.  
- Mención especial si tocaste un **Hook Canónico** (+bonus y coreo del público).

---

## 7) Datos y clases sugeridas (resumen)
**IntentTags:** `Power`, `Groove`, `Emotion`, `Virtuosity`, `Rebellion`, `CohesionBoost`

**AudienceImpactProfile:** `BaseVibe`, `CrowdSplitFront`, `CrowdSplitBack`, `StressRisk`, `MissPenalty`

**PlannedSectionProfile:** `IntentIntensity`, `TargetBaseVibe`, `TargetRisk`, `SpotlightRole`

**Músico (stats mínimos):** `InstrumentSkill[style/role]`, `StagePresence`, `CohesionWithBand`, `Stress`, `SpotlightAffinity`

**MusicianPerformanceResult:** `PerformanceScore`, `Mistake`, `StressDelta`, `SpotlightBonus`

**LiveSectionResult:** `DeliveredVibeFront`, `DeliveredVibeBack`, `CrowdHype`, `PerformerBreakdown`, `SideEffects`

**SideEffects (ejemplos):** `StunCrowdFront`, `ShieldBandStress`, `TauntHeckler`, `LoseCredibilityBackRow`, `IgniteMoshPit`, `CrowdChant`, `AwkwardSilence`

**Fórmula guía:**  
`finalVibe = TargetBaseVibe × ExecutionFactor (roles clave) × CohesionFactor (banda) × MistakeFactor`

---

## 8) Implementación inmediata (checklist)
- [ ] Añadir a cartas: `intentTags`, `impactProfile`, `SpotlightRole` (sin romper lo musical).  
- [ ] Construir `PlannedSectionProfile` por bloque con las cartas jugadas.  
- [ ] Resolver `MusicianPerformanceResult` por músico (Ability vs Demand, estrés y spotlight).  
- [ ] Calcular `LiveSectionResult` y aplicarlo al combate (daño lanes, hype, side effects, stress).  
- [ ] Integrar **HookCard / SignatureHook** y marcar hooks **canonizados** (bonos al usarlos).  
- [ ] Implementar **modo híbrido**: macroestructura planificada + improvisación por bloque con cierre para resolver.  
- [ ] Añadir reglas de **fatiga por vuelta** y cartas de **Close/Break/Transition**.  
- [ ] UI de fin de sección + marcadores persistentes (Hype, Cohesion, Stress).

---

## 9) Notas de diseño
- El público “recuerda” **hooks**, no tomas completas. La mitología se construye con motivos.  
- **Live Arrangements** guardados son útiles tácticamente (fiables, menos frescos).  
- Repetir un hook en otro show **no** garantiza el mismo resultado: estado emocional, local, enemigos y side effects cambian.  
- El sistema refuerza tu tema central: **conflictos y química de la banda** influyen directamente en la eficacia del show.

---

## 10) Próximos pasos sugeridos
1. Prototipo del **híbrido**: un concierto corto con 3 bloques (Intro, Estribillo con Hook, Clímax).  
2. Implementar **Close/Break** y **fatiga por vuelta**.  
3. Primer pase de **SideEffects** (3 positivos, 3 negativos) con textos de flavor.  
4. UI de **fin de sección** con breakdown de músicos y lanes (frente/fondo).  
5. Marcar un **Hook canónico** y reproducir su bonus en un segundo show para validar la “fama”.

---

**Fin del resumen.**  
Este documento condensa las decisiones de diseño y su traducción a datos/código para portar el sistema de composición a combate en gigs, manteniendo identidad musical e improvisación viva.
