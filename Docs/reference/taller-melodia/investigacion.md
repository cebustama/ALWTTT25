# Investigación: Pink Trombone y la expresividad de una voz (2026-11-13)

Hecha por un subagente con búsqueda web para el Taller de melodías. Fuentes leídas directamente: el HTML original de https://dood.al/pinktrombone/, zakaton/Pink-Trombone, zakaton/pink-trombone-midi y npm; el resto, de búsquedas, y marcado cuando solo se vio citado. **Sin verificar** = no se pudo comprobar en la fuente.

## 1. Origen
- Neil Thapen, «version 1.1, March 2017», «Bare-handed procedural speech synthesis», licencia MIT (cabecera del HTML).
- Bibliografía que cita la cabecera: J. O. Smith III, *Physical Audio Signal Processing* (CCRMA); B. H. Story, «A parametric model of the vocal tract area function for vowel and consonant simulation», JASA 117(5), 2005; H.-L. Lu y J. O. Smith, «Glottal source modeling for singing voice synthesis», ICMC 2000; J. Mullen, *Physical modelling of the vocal tract with the 2D digital waveguide mesh*, tesis, York, 2006.
- Kelly-Lochbaum y Liljencrants-Fant no se nombran en el archivo: es inferencia del código (tubo de 44 secciones con ondas R/L; glotis LF con `Rd = 3·(1−tenseness)`, 0,5–2,7, y las fórmulas `Ra`, `Rk` de Fant 1995).
- Cadena de nuestro fork: Thapen (web) → chdh/pink-trombone-mod (modular, MIT) → lostmsu/pink-trombone-mod (C#) → ALWTTT POC-FORK 1–7.

## 2. Lo que el original web hace y el fork no expone
- Constricciones libres por toque (`TractUI.handleTouches`): ventana coseno de ancho 10 atrás, 5 en punta y labios; solo estrecha. Portado como X1.
- Velo: 0,01 por defecto, 0,4 al tocar la cavidad nasal.
- Oclusivas: transitorio al soltar una obstrucción con la nariz cerrada (el original usa `noseA[0] < 0.05`; el fork, `noseDiameter[0] < 0.223`).
- Fricativas: ruido bandpass a 1 kHz (Q 0,5) inyectado si el diámetro está entre ~0,3 y 0,7; ataque de 0,1 s; modulado por la fase glótica.
- Etiquetas de consonantes de la interfaz (índice): g/k/ŋ 22, d/t/n 36, b/p/m 41, ʒ/ʃ 31,5, z/s 36, v/f 41, l 38, w 41, h 4,5.

## 3. Forks
- zakaton/Pink-Trombone (GPL-3.0): AudioWorklet, hasta 4 constricciones, **`tractLength`** (44 por defecto, 15–88).
- npm `pink-trombone` 1.0.2 (MIT, asmatzaile): polifónico, sin cabeza.
- zakaton/pink-trombone-midi: tabla de fonemas (lengua para vocales, constricción frontal para consonantes; diámetro −1 = nariz). Consonantes: r 27,6/0,6; ʃ 31/0,6; s 36/0,6; f 41/0,5; g 20/0; d 36/0; b 41/0; ŋ 20/−1; n 36/−1; m 41/−1; w 41/0,7; l 38/1,2.

## 4. Fisiología de los estilos
- **CVT (Sadolin):** Neutral, Curbing, Overdrive, Edge, por cantidad de «metal»; efectos por nivel: pliegues vocales (creak, aire), ventriculares (distorsión), aritenoides/epiglotis/ariepiglóticos (growl, rattle). Es descripción del propio método. https://completevocalinstitute.com/levels-of-the-vocal-tract/
- **Growl:** Sakakibara, Fuks, Imagawa y Tayama, «Growl voice in ethnic and pop styles», ISMA 2004: los ariepiglóticos vibran con los vocales, subarmónicos (visto citado). Aaen, McGlashan y Sadolin, J. Voice 34(1), 2020, doi 10.1016/j.jvoice.2017.12.020: cada efecto rugoso tiene su patrón supraglótico; sin patología. Guzman et al., «Aerodynamic characteristics of growl voice and reinforced falsetto in metal singing», J. Voice 33(5), 2019: más flujo, presión y SPL con menos aducción.
- **Rock:** Guzman et al. 2013 (Folia Phoniatr.): laringe alta y compresión supraglótica en cantantes de rock. **Punk: sin estudio específico (sin verificar).**
- **Fry:** Hollien et al. 1966 (JSHR 9); F0 < 100 Hz, pulsos irregulares (revisión de Davidson 2021).
- **Rd:** bajo = presionada, alto = soplada; ~0,8 / 1,1 / 2,9 en un barítono (fuente secundaria). Fant 1995 **sin verificar** en la primaria.
- **Laringe baja → tracto largo → formantes más bajos** (Lasarcyk 2007, ICPhS). Formante del cantante: Sundberg 1974, JASA 55:838. Sundberg y Nordström 1976 **sin verificar**.
- **Vibrato:** Prame 1994, JASA 96: 5,5–6,7 Hz, ±60 cents típicos. Ópera y jazz, misma tasa, más extensión en ópera (J. Voice 2015). Freddie Mercury: 7,0 Hz irregular (Herbst et al. 2016).

## 5. Contorno melódico
- Huron, «The melodic arch in Western folksongs», *Computing in Musicology* 10, 1996 (6.251 melodías del corpus Essen).
- Huron, *Sweet Anticipation*, MIT Press, 2006 (proximidad, declinación por pasos, regresión, arco).
- von Hippel y Huron, «Why do skips precede reversals?», *Music Perception* 18(1), 2000: la «vuelta tras salto» se explica por regresión a la media de la tesitura.
- Gap-fill (Meyer 1956): **sin verificar con URL**.
