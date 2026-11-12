# Herramientas de testeo dentro de la build

Pedido de Claudio tras la reunión con Matías del 2026-11-12 (`reference/reuniones/2026-11-12_matias.md`,
punto 9). Estado: **idea, sin decidir**; el cuándo es D-PLAN-9 del plan de la demo. No es autoridad.

## Lo que se pide

Que en la build, sin Unity, un tester (Matías u otro) pueda **crear, editar y guardar**:
1. **Cartas**
2. **Enemigos** (miembros del público)
3. **Gigs** (encuentros: qué público, cuántas canciones)

Se guardan en local, dentro del juego, y se pueden **exportar a JSON** para pasárselas a Claudio, que las
importa al proyecto.

## Lo que ya existe (✦ IA)

- **El JSON del elenco** (`Assets/Settings/Audience/Elenco_S7.json` + `Characters/Editor/AudienceJson.cs`)
  ya describe miembros del público y encuentros: nombre, Vibe, gustos, habilidades, gritos y poses. Es el
  formato natural para (2) y (3). Hoy solo se lee **en el editor** (`AssetDatabase`, menú «ALWTTT ▸ S8 ▸
  Importar gritos»).
- **Cartas:** `CardEditorWindow` y `DeckCardCreationService`, también solo del editor. No hay formato
  JSON de cartas.
- **Dev Mode (F12)** ya corre en la build con `ALWTTT_DEV`: es el sitio natural para un panel de testeo.
- **TEST-SETUP-1** (S10 del plan) ya prevé lanzar un GigSetup desde el menú.

## Por qué no es pequeño (✦ IA)

- Los ScriptableObjects no se pueden crear como assets en una build. En runtime se crean **en memoria**
  (`ScriptableObject.CreateInstance`) desde el JSON, y se guarda el **JSON**, no el asset.
- Los sprites: un enemigo nuevo necesita imagen. En la build, o se elige entre los sprites que ya hay, o
  se carga un PNG de una carpeta (`Texture2D.LoadImage`).
- Las cartas tienen efectos compuestos (payload, primitivas, estados): un editor de cartas en el juego es
  la pieza más cara de las tres.
- Hace falta validación con mensajes claros: una carta o un enemigo malformado no puede romper el gig.

## Pasos posibles (✦ IA, por orden de coste)

1. **Leer JSON en la build** (gigs y enemigos): el juego carga los `.json` de una carpeta junto a los datos
   del jugador y los ofrece en el GigSetup de TEST-SETUP-1. El tester edita el JSON a mano. Reutiliza el
   formato del elenco.
2. **Exportar**: botón que guarda el gig o el enemigo actual a JSON y abre la carpeta.
3. **Editor en el juego** de enemigos y gigs (formularios en el panel de testeo).
4. **Cartas**: formato JSON de cartas y su editor en el juego.
