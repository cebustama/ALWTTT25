# Repo_Tree_Index — índice de rutas del repositorio ALWTTT

**Snapshot: 2026-08-29 19:30.** Generado por `make-tree-unity.ps1`. **No es un documento gobernado** — es operativa del PK (Capa 2), igual que `PK_Manifest.md`.

**Para qué sirve.** El PK es plano: los ficheros adjuntos pierden su carpeta. Este índice devuelve la ruta real de cualquier fichero por su nombre, sin inferirla del `namespace`. Sirve para rellenar la columna *Ruta en repo* de `PK_Manifest.md` y para pedir ficheros por ruta exacta en el File Request Protocol.

**Caducidad.** Es una foto. Regenerar cuando un lote cree, borre o mueva ficheros, y anotar la regeneración en `PK_Manifest.md` sección C. Un índice de rutas viejo hace pedir ficheros que ya no existen.

**Alcance.** Código y documentación: `Assets\Scripts`, `Assets\Editor`, `Assets\PinkTrombonePOC`, `Docs`, `Packages`, más los documentos sueltos de la raíz. Extensiones: `.cs` `.shader` `.asmdef` `.compute` `.hlsl` `.cginc` `.md` `.yaml` `.yml` `.json` `.txt`.

**Fuera: código de terceros** (`MidiPlayer`, `com.merry-yellow.code-assist`). Existe en el repo y sale en `tree.txt`, pero no es código de ALWTTT; indexarlo solo haría que compita en la búsqueda con el código propio.

Assets de Unity (`.asset` / `.prefab` / `.unity`) **fuera**; regenerar con `-IncludeAssets` si un lote de contenido los necesita.

**Total indexado: 446 ficheros.**

| Carpeta | Ficheros | KB |
|---|---:|---:|
| `(raiz)` | 5 | 124 |
| `Assets/PinkTrombonePOC` | 4 | 71 |
| `Assets/PinkTrombonePOC/PinkTromboneSrc` | 13 | 54 |
| `Assets/Scripts` | 1 | 7 |
| `Assets/Scripts/Backgrounds` | 4 | 11 |
| `Assets/Scripts/Cards` | 78 | 770 |
| `Assets/Scripts/Characters` | 19 | 140 |
| `Assets/Scripts/Controllers` | 2 | 52 |
| `Assets/Scripts/Data` | 50 | 228 |
| `Assets/Scripts/DevMode` | 8 | 163 |
| `Assets/Scripts/Editor` | 1 | 7 |
| `Assets/Scripts/Encounters` | 2 | 2 |
| `Assets/Scripts/Enums` | 22 | 12 |
| `Assets/Scripts/Extensions` | 1 | 1 |
| `Assets/Scripts/Generation` | 13 | 29 |
| `Assets/Scripts/Interfaces` | 7 | 3 |
| `Assets/Scripts/Managers` | 13 | 442 |
| `Assets/Scripts/Map` | 11 | 25 |
| `Assets/Scripts/Music` | 16 | 288 |
| `Assets/Scripts/Sensory` | 21 | 81 |
| `Assets/Scripts/Status` | 9 | 84 |
| `Assets/Scripts/Tutorial` | 14 | 131 |
| `Assets/Scripts/UI` | 44 | 286 |
| `Assets/Scripts/Utils` | 5 | 8 |
| `Docs` | 9 | 581 |
| `Docs/archive` | 8 | 660 |
| `Docs/archive/absorbed` | 2 | 5 |
| `Docs/archive/snapshots` | 1 | 0 |
| `Docs/audits` | 2 | 107 |
| `Docs/integrations` | 1 | 1 |
| `Docs/integrations/midigenplay` | 5 | 96 |
| `Docs/planning` | 11 | 122 |
| `Docs/planning/active` | 16 | 437 |
| `Docs/planning/archive` | 6 | 58 |
| `Docs/reference` | 5 | 31 |
| `Docs/runtime` | 3 | 66 |
| `Docs/systems` | 12 | 454 |
| `Packages` | 2 | 19 |

## Nombres duplicados (atención)

El PK es plano: estos nombres no identifican un fichero de forma unívoca. Al pedirlos, usar la ruta completa.

- **README.md** — `Docs/archive/absorbed/README.md` / `Docs/archive/README.md` / `Docs/archive/snapshots/README.md` / `Docs/integrations/README.md` / `Docs/planning/archive/README.md` / `Docs/README.md` / `Docs/reference/README.md` / `Docs/runtime/README.md` / `Docs/systems/README.md`

## Rutas

### (raiz)

```
AGENTS.md
ALWTTT Claude API.txt
CONTRIBUTING.md
files.txt
F-R5e-3_payment_gate.txt
```

### Assets/PinkTrombonePOC

```
Assets/PinkTrombonePOC/PinkTromboneBackingPlayer.cs
Assets/PinkTrombonePOC/PinkTrombonePlayground.cs
Assets/PinkTrombonePOC/PinkTrombonePOC.asmdef
Assets/PinkTrombonePOC/PinkTromboneSinger.cs
```

### Assets/PinkTrombonePOC/PinkTromboneSrc

```
Assets/PinkTrombonePOC/PinkTromboneSrc/Arg.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/FORK_NOTES.md
Assets/PinkTrombonePOC/PinkTromboneSrc/Glottis.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/LICENSE.txt
Assets/PinkTrombonePOC/PinkTromboneSrc/MathX.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/Noise.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/NoiseGenerator.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/PinkTrombone.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/RandomSource.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/Tract.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/TractShaper.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/Transient.cs
Assets/PinkTrombonePOC/PinkTromboneSrc/TurbulencePoint.cs
```

### Assets/Scripts

```
Assets/Scripts/file_tree.txt
```

### Assets/Scripts/Backgrounds

```
Assets/Scripts/Backgrounds/BackgroundContainer.cs
Assets/Scripts/Backgrounds/BackgroundRoot.cs
Assets/Scripts/Backgrounds/ForegroundAnimator.cs
Assets/Scripts/Backgrounds/StageLightAnimator.cs
```

### Assets/Scripts/Cards

```
Assets/Scripts/Cards/Card3D.cs
Assets/Scripts/Cards/CardActions/AddStressAction.cs
Assets/Scripts/Cards/CardActions/AddVibeAction.cs
Assets/Scripts/Cards/CardActions/ApplyStatusEffectAction.cs
Assets/Scripts/Cards/CardActions/AudienceMoveToFrontAction.cs
Assets/Scripts/Cards/CardActions/BlockStressAction.cs
Assets/Scripts/Cards/CardActions/BlockVibeAction.cs
Assets/Scripts/Cards/CardActions/HealStressAction.cs
Assets/Scripts/Cards/CardActions/RemoveVibeAction.cs
Assets/Scripts/Cards/CardAnimationData.cs
Assets/Scripts/Cards/CardBase.cs
Assets/Scripts/Cards/CardConditionData.cs
Assets/Scripts/Cards/CardDefinition.cs
Assets/Scripts/Cards/CardTypeDB.cs
Assets/Scripts/Cards/ChoiceCard.cs
Assets/Scripts/Cards/Composition/CompositionCardClassifier.cs
Assets/Scripts/Cards/Composition/PartActionDescriptor.cs
Assets/Scripts/Cards/Composition/TrackActionDescriptor.cs
Assets/Scripts/Cards/Editor/CardAssetFactory.cs
Assets/Scripts/Cards/Editor/CardAuthoringNav.cs
Assets/Scripts/Cards/Editor/CardEditorWindow.cs
Assets/Scripts/Cards/Editor/CardEditorWindow.JsonImport.cs
Assets/Scripts/Cards/Editor/CardEditorWindow.LLM.cs
Assets/Scripts/Cards/Editor/CardInventoryWindow.cs
Assets/Scripts/Cards/Editor/ChordProgressionCatalogueWizard.cs
Assets/Scripts/Cards/Editor/CompositionInventoryWindow.Cards.cs
Assets/Scripts/Cards/Editor/CompositionInventoryWindow.cs
Assets/Scripts/Cards/Editor/DeckAssetSaveService.cs
Assets/Scripts/Cards/Editor/DeckCardCreationService.cs
Assets/Scripts/Cards/Editor/DeckEditorDtos.cs
Assets/Scripts/Cards/Editor/DeckEditorWindow.cs
Assets/Scripts/Cards/Editor/DeckJsonImportService.cs
Assets/Scripts/Cards/Editor/DeckValidationService.cs
Assets/Scripts/Cards/Editor/InstrumentEffectEditor.cs
Assets/Scripts/Cards/Editor/LLM/CardLLMVocabularyBuilder.cs
Assets/Scripts/Cards/Editor/MusicianCatalogService.cs
Assets/Scripts/Cards/Editor/PartEffectEditorWindow.cs
Assets/Scripts/Cards/Effects/AddInspirationPerLoopSpec.cs
Assets/Scripts/Cards/Effects/ApplyStatusEffectSpec.cs
Assets/Scripts/Cards/Effects/CardEffectDescriptionBuilder.cs
Assets/Scripts/Cards/Effects/CardEffectSpec.cs
Assets/Scripts/Cards/Effects/DrawCardsSpec.cs
Assets/Scripts/Cards/Effects/GrantBonusLoopSpec.cs
Assets/Scripts/Cards/Effects/ModifyStressSpec.cs
Assets/Scripts/Cards/Effects/ModifyVibeSpec.cs
Assets/Scripts/Cards/Effects/RevealPreferencesSpec.cs
Assets/Scripts/Cards/Enums/CardAcquisitionFlags.cs
Assets/Scripts/Cards/Enums/CardActionTiming.cs
Assets/Scripts/Cards/Enums/CardDomain.cs
Assets/Scripts/Cards/Enums/CardPerformerRule.cs
Assets/Scripts/Cards/Enums/CardPrimaryKind.cs
Assets/Scripts/Cards/Enums/PartActionKind.cs
Assets/Scripts/Cards/Extensions/CardDefinitionDescriptionExtensions.cs
Assets/Scripts/Cards/GenericCardCatalogSO.cs
Assets/Scripts/Cards/LLMAuthoring/ALWTTT.Cards.LLMAuthoring.asmdef
Assets/Scripts/Cards/LLMAuthoring/AssemblyInfo.cs
Assets/Scripts/Cards/LLMAuthoring/CardImportDtoParser.cs
Assets/Scripts/Cards/LLMAuthoring/CardImportDtos.cs
Assets/Scripts/Cards/LLMAuthoring/CardLLMFieldPlan.cs
Assets/Scripts/Cards/LLMAuthoring/CardLLMGenerator.cs
Assets/Scripts/Cards/LLMAuthoring/CardLLMPromptBuilder.cs
Assets/Scripts/Cards/LLMAuthoring/CardLLMResponseHandler.cs
Assets/Scripts/Cards/LLMAuthoring/CardLLMVocabulary.cs
Assets/Scripts/Cards/LLMAuthoring/CardPaletteDescriptorScanner.cs
Assets/Scripts/Cards/LLMAuthoring/CardPaletteIntentResolver.cs
Assets/Scripts/Cards/LLMAuthoring/Tests/ALWTTT.Cards.LLMAuthoring.Tests.asmdef
Assets/Scripts/Cards/LLMAuthoring/Tests/CardImportDtoParserTests.cs
Assets/Scripts/Cards/LLMAuthoring/Tests/CardLLMFieldPlanTests.cs
Assets/Scripts/Cards/LLMAuthoring/Tests/CardLLMGeneratorTests.cs
Assets/Scripts/Cards/LLMAuthoring/Tests/CardLLMPromptBuilderTests.cs
Assets/Scripts/Cards/LLMAuthoring/Tests/CardLLMResponseHandlerTests.cs
Assets/Scripts/Cards/LLMAuthoring/Tests/CardPaletteIntentResolverTests.cs
Assets/Scripts/Cards/LLMAuthoring/Tests/FakeLLMClient.cs
Assets/Scripts/Cards/MusicianCardCatalogData.cs
Assets/Scripts/Cards/MusicianCardEntry.cs
Assets/Scripts/Cards/Payloads/ActionCardPayload.cs
Assets/Scripts/Cards/Payloads/CardPayload.cs
Assets/Scripts/Cards/Payloads/CompositionCardPayload.cs
```

### Assets/Scripts/Characters

```
Assets/Scripts/Characters/Actions/CharacterActionBase.cs
Assets/Scripts/Characters/Actions/CharacterActionData.cs
Assets/Scripts/Characters/Actions/CharacterActionParameters.cs
Assets/Scripts/Characters/Actions/CharacterActionProcessor.cs
Assets/Scripts/Characters/Audience/AudienceCharacterSimple.cs
Assets/Scripts/Characters/AudienceCharacterBase.cs
Assets/Scripts/Characters/AudienceCharacterCanvas.cs
Assets/Scripts/Characters/AudienceCharacterStats.cs
Assets/Scripts/Characters/BandCharacterCanvas.cs
Assets/Scripts/Characters/BandCharacterStats.cs
Assets/Scripts/Characters/CharacterAnimator.cs
Assets/Scripts/Characters/CharacterBase.cs
Assets/Scripts/Characters/CharacterCanvas.cs
Assets/Scripts/Characters/CharacterStats.cs
Assets/Scripts/Characters/MusicianBase.cs
Assets/Scripts/Characters/Musicians/MusicianCharacterSimple.cs
Assets/Scripts/Characters/SpriteOutlineController.cs
Assets/Scripts/Characters/StatusStats.cs
Assets/Scripts/Characters/VibeEffectiveness.cs
```

### Assets/Scripts/Controllers

```
Assets/Scripts/Controllers/HandController.cs
Assets/Scripts/Controllers/MidiGenPlayPanelController.cs
```

### Assets/Scripts/Data

```
Assets/Scripts/Data/AlwtttLogSetup.cs
Assets/Scripts/Data/ALWTTTProjectRegistriesSO.cs
Assets/Scripts/Data/Audio/AudioMixSettingsSO.cs
Assets/Scripts/Data/Audio/CharacterSfxProfileSO.cs
Assets/Scripts/Data/Audio/MixGainProfileSO.cs
Assets/Scripts/Data/Audio/OstCatalogSO.cs
Assets/Scripts/Data/Audio/SongData.cs
Assets/Scripts/Data/Audio/SoundBankSO.cs
Assets/Scripts/Data/Audio/SoundProfileData.cs
Assets/Scripts/Data/Audio/VoiceProfileSO.cs
Assets/Scripts/Data/Cards/Configs/BandDeckData.cs
Assets/Scripts/Data/Cards/Configs/BandDeckEntry.cs
Assets/Scripts/Data/Cards/Configs/CardData.cs
Assets/Scripts/Data/Cards/Configs/CardRewardData.cs
Assets/Scripts/Data/Cards/Configs/CardTypeData.cs
Assets/Scripts/Data/Cards/Configs/DeckData.cs
Assets/Scripts/Data/Cards/Configs/RewardContainerData.cs
Assets/Scripts/Data/Cards/Databases/RewardDatabase.cs
Assets/Scripts/Data/Cards/Part Effects/InstrumentEffect.cs
Assets/Scripts/Data/Cards/Part Effects/MeterEffect.cs
Assets/Scripts/Data/Cards/Part Effects/ModulationEffect.cs
Assets/Scripts/Data/Cards/Part Effects/PartEffect.cs
Assets/Scripts/Data/Cards/Part Effects/TempoEffect.cs
Assets/Scripts/Data/Cards/Part Effects/TonalityEffect.cs
Assets/Scripts/Data/Characters/Audience/AudienceCharacterData.cs
Assets/Scripts/Data/Characters/Audience/AudienceIntentionData.cs
Assets/Scripts/Data/Characters/Musicians/MusicianCharacterData.cs
Assets/Scripts/Data/Characters/Musicians/MusicianProfileData.cs
Assets/Scripts/Data/CompositionFxConfigSO.cs
Assets/Scripts/Data/Core/GameplayData.cs
Assets/Scripts/Data/Core/PersistentGameplayData.cs
Assets/Scripts/Data/Core/SceneData.cs
Assets/Scripts/Data/Core/SpecialKeywordData.cs
Assets/Scripts/Data/Encounters/EncounterData.cs
Assets/Scripts/Data/Encounters/GigEncounterSO.cs
Assets/Scripts/Data/Events/RandomEventData.cs
Assets/Scripts/Data/Gig/DemoLaunchConfigSO.cs
Assets/Scripts/Data/Gig/GigDevSettingsSO.cs
Assets/Scripts/Data/Gig/GigFlowSettingsSO.cs
Assets/Scripts/Data/Gig/GigPresentationSO.cs
Assets/Scripts/Data/Gig/GigSetupRosterSO.cs
Assets/Scripts/Data/Gig/MeterTuningSO.cs
Assets/Scripts/Data/RhythmFxConfigSO.cs
Assets/Scripts/Data/SectorMap/Configs/NodeTypeData.cs
Assets/Scripts/Data/SectorMap/Configs/NodeTypeDatabase.cs
Assets/Scripts/Data/SectorMap/Configs/SectorMapData.cs
Assets/Scripts/Data/SectorMap/State/SectorMapState.cs
Assets/Scripts/Data/SectorMap/State/SectorNodeState.cs
Assets/Scripts/Data/Serializable Wrappers/SerializableCardInventory.cs
Assets/Scripts/Data/Serializable Wrappers/SerializableStringIntDictionary.cs
```

### Assets/Scripts/DevMode

```
Assets/Scripts/DevMode/DevAudioMixTab.cs
Assets/Scripts/DevMode/DevCardCatalogueTab.cs
Assets/Scripts/DevMode/DevCompositionDebugTab.cs
Assets/Scripts/DevMode/DevGigOutcomeTracker.cs
Assets/Scripts/DevMode/DevModeController.cs
Assets/Scripts/DevMode/DevRunTelemetryLogger.cs
Assets/Scripts/DevMode/DevStatsTab.cs
Assets/Scripts/DevMode/GenerationDebugFormatter.cs
```

### Assets/Scripts/Editor

```
Assets/Scripts/Editor/S5e_InspirationEconomyTool.cs
```

### Assets/Scripts/Encounters

```
Assets/Scripts/Encounters/EncounterBase.cs
Assets/Scripts/Encounters/GigEncounter.cs
```

### Assets/Scripts/Enums

```
Assets/Scripts/Enums/ActionTargetType.cs
Assets/Scripts/Enums/AudienceIntentionType.cs
Assets/Scripts/Enums/AudioActionType.cs
Assets/Scripts/Enums/CardConditionType.cs
Assets/Scripts/Enums/CardType.cs
Assets/Scripts/Enums/CharacterActionType.cs
Assets/Scripts/Enums/CharacterType.cs
Assets/Scripts/Enums/FxType.cs
Assets/Scripts/Enums/GigPhase.cs
Assets/Scripts/Enums/InventoryType.cs
Assets/Scripts/Enums/MoodTag.cs
Assets/Scripts/Enums/MusicianCharacterType.cs
Assets/Scripts/Enums/MusicianStat.cs
Assets/Scripts/Enums/NodeType.cs
Assets/Scripts/Enums/OstTrackId.cs
Assets/Scripts/Enums/RandomEventEffectType.cs
Assets/Scripts/Enums/RarityType.cs
Assets/Scripts/Enums/RewardType.cs
Assets/Scripts/Enums/RhythmLane.cs
Assets/Scripts/Enums/SensorySfxType.cs
Assets/Scripts/Enums/SpecialKeywords.cs
Assets/Scripts/Enums/VenueType.cs
```

### Assets/Scripts/Extensions

```
Assets/Scripts/Extensions/ListExtensions.cs
```

### Assets/Scripts/Generation

```
Assets/Scripts/Generation/Map/ISectorGenStage.cs
Assets/Scripts/Generation/Map/SectorGenUtils.cs
Assets/Scripts/Generation/Map/SectorGraphGenerator.cs
Assets/Scripts/Generation/Map/SectorGraphStepper.cs
Assets/Scripts/Generation/Map/Stages/AssignTypesStage.cs
Assets/Scripts/Generation/Map/Stages/BuildSpinesStage.cs
Assets/Scripts/Generation/Map/Stages/ConnectivityStage.cs
Assets/Scripts/Generation/Map/Stages/CrossLinksStage.cs
Assets/Scripts/Generation/Map/Stages/EnforceSemanticsStage.cs
Assets/Scripts/Generation/Map/Stages/LayoutAndCreateNodesStage.cs
Assets/Scripts/Generation/Map/Stages/PickStartExitStage.cs
Assets/Scripts/Generation/Map/Stages/RepairGapsStage.cs
Assets/Scripts/Generation/Music/MidiToolkitAdapter.cs
```

### Assets/Scripts/Interfaces

```
Assets/Scripts/Interfaces/IAudienceMember.cs
Assets/Scripts/Interfaces/IAudienceStats.cs
Assets/Scripts/Interfaces/ICharacter.cs
Assets/Scripts/Interfaces/ICharacterStats.cs
Assets/Scripts/Interfaces/IMusician.cs
Assets/Scripts/Interfaces/IMusicianStats.cs
Assets/Scripts/Interfaces/INodeResolver.cs
```

### Assets/Scripts/Managers

```
Assets/Scripts/Managers/AudioManager.cs
Assets/Scripts/Managers/BandSetupManager.cs
Assets/Scripts/Managers/DeckManager.cs
Assets/Scripts/Managers/FxManager.cs
Assets/Scripts/Managers/GameManager.cs
Assets/Scripts/Managers/GigLauncher.cs
Assets/Scripts/Managers/GigManager.cs
Assets/Scripts/Managers/GigRunContext.cs
Assets/Scripts/Managers/MidiMusicManager.cs
Assets/Scripts/Managers/MusicDirector.cs
Assets/Scripts/Managers/SectorMapManager.cs
Assets/Scripts/Managers/ShipInteriorManager.cs
Assets/Scripts/Managers/UIManager.cs
```

### Assets/Scripts/Map

```
Assets/Scripts/Map/NodeResolveContext.cs
Assets/Scripts/Map/NodeResolverProcessor.cs
Assets/Scripts/Map/Resolvers/BossNodeResolver.cs
Assets/Scripts/Map/Resolvers/GigNodeResolver.cs
Assets/Scripts/Map/Resolvers/RandomEventNodeResolver.cs
Assets/Scripts/Map/Resolvers/RecruitNodeResolver.cs
Assets/Scripts/Map/Resolvers/RehearsalNodeResolver.cs
Assets/Scripts/Map/SectorLinkVisual.cs
Assets/Scripts/Map/SectorMapVisual.cs
Assets/Scripts/Map/SectorNodeVisual.cs
Assets/Scripts/Map/ShipController.cs
```

### Assets/Scripts/Music

```
Assets/Scripts/Music/CompositionSession.cs
Assets/Scripts/Music/Context Data/LoopFeedbackContext.cs
Assets/Scripts/Music/Context Data/PartFeedbackContext.cs
Assets/Scripts/Music/Context Data/SongFeedbackContext.cs
Assets/Scripts/Music/FloatingTextMidiListener.cs
Assets/Scripts/Music/InstrumentRules.cs
Assets/Scripts/Music/Interfaces/ICompositionContext.cs
Assets/Scripts/Music/LoopScoreCalculator.cs
Assets/Scripts/Music/MidiEventInterfaces.cs
Assets/Scripts/Music/MusicianMidiResponder.cs
Assets/Scripts/Music/RhythmFxTester.cs
Assets/Scripts/Music/RhythmParticleEmitter.cs
Assets/Scripts/Music/RhythmParticleMidiListener.cs
Assets/Scripts/Music/SongConfigBuilder.cs
Assets/Scripts/Music/Voice/SingerVoice.cs
Assets/Scripts/Music/Voice/SingerVoiceDirector.cs
```

### Assets/Scripts/Sensory

```
Assets/Scripts/Sensory/AudienceBlockedEvent.cs
Assets/Scripts/Sensory/AudienceTurnStartedEvent.cs
Assets/Scripts/Sensory/CardPlayedEvent.cs
Assets/Scripts/Sensory/Events/AudienceReactionEvent.cs
Assets/Scripts/Sensory/Events/AudienceVibeImpactEvent.cs
Assets/Scripts/Sensory/Events/SfxStageCrossedEvent.cs
Assets/Scripts/Sensory/Events/SongEndVibeEvent.cs
Assets/Scripts/Sensory/Events/SpotlightRedirectEvent.cs
Assets/Scripts/Sensory/GigOutcomeEvent.cs
Assets/Scripts/Sensory/GigStartedEvent.cs
Assets/Scripts/Sensory/ISensoryEvent.cs
Assets/Scripts/Sensory/LoopResolvedEvent.cs
Assets/Scripts/Sensory/MusicianStressHitEvent.cs
Assets/Scripts/Sensory/PsychicWaveOverlayController.cs
Assets/Scripts/Sensory/RewardChoiceOpenedEvent.cs
Assets/Scripts/Sensory/SensoryAudioAdapter.cs
Assets/Scripts/Sensory/SensoryEventBus.cs
Assets/Scripts/Sensory/SensoryFtPresentation.cs
Assets/Scripts/Sensory/SensoryFxAdapter.cs
Assets/Scripts/Sensory/SensorySfxPresentation.cs
Assets/Scripts/Sensory/StatusAppliedEvent.cs
```

### Assets/Scripts/Status

```
Assets/Scripts/Status/CharacterStatusId.cs
Assets/Scripts/Status/CharacterStatusPrimitiveDatabaseSO.cs
Assets/Scripts/Status/Editor/StatusEffectWizardWindow.cs
Assets/Scripts/Status/Runtime/StatusEffectContainer.cs
Assets/Scripts/Status/Runtime/StatusEffectInstance.cs
Assets/Scripts/Status/StatusEffectActionData.cs
Assets/Scripts/Status/StatusEffectCatalogueSO.cs
Assets/Scripts/Status/StatusEffectSO.cs
Assets/Scripts/Status/StatusType.cs
```

### Assets/Scripts/Tutorial

```
Assets/Scripts/Tutorial/TutorialController.cs
Assets/Scripts/Tutorial/TutorialDialogCatalogSO.cs
Assets/Scripts/Tutorial/TutorialDialogSO.cs
Assets/Scripts/Tutorial/TutorialGuidedDriver.cs
Assets/Scripts/Tutorial/TutorialHighlightSpawnHook.cs
Assets/Scripts/Tutorial/TutorialHighlightTarget.cs
Assets/Scripts/Tutorial/TutorialInputGate.cs
Assets/Scripts/Tutorial/TutorialLoopHoldGate.cs
Assets/Scripts/Tutorial/TutorialModalGate.cs
Assets/Scripts/Tutorial/TutorialOptInPrompt.cs
Assets/Scripts/Tutorial/TutorialOverlayView.cs
Assets/Scripts/Tutorial/TutorialRevisitPanel.cs
Assets/Scripts/Tutorial/TutorialScriptedDrawQueue.cs
Assets/Scripts/Tutorial/TutorialTokenResolver.cs
```

### Assets/Scripts/UI

```
Assets/Scripts/UI/AudiencePickerRow.cs
Assets/Scripts/UI/BeatPulseIndicator.cs
Assets/Scripts/UI/Canvases/BandSetupCanvas.cs
Assets/Scripts/UI/Canvases/CanvasBase.cs
Assets/Scripts/UI/Canvases/GameOverCanvas.cs
Assets/Scripts/UI/Canvases/InventoryCanvas.cs
Assets/Scripts/UI/Canvases/MidiListenerCanvasBase.cs
Assets/Scripts/UI/Canvases/RandomEventCanvas.cs
Assets/Scripts/UI/Canvases/RecruitCanvas.cs
Assets/Scripts/UI/Canvases/RewardCanvas.cs
Assets/Scripts/UI/Canvases/ShipInteriorCanvas.cs
Assets/Scripts/UI/CardDetailViewController.cs
Assets/Scripts/UI/CardUI.cs
Assets/Scripts/UI/ChoicePanel.cs
Assets/Scripts/UI/CompositionStripThemeSO.cs
Assets/Scripts/UI/ConflictPanelUI.cs
Assets/Scripts/UI/FloatingText.cs
Assets/Scripts/UI/GigCanvas.cs
Assets/Scripts/UI/GigSetupController.cs
Assets/Scripts/UI/HealthBarController.cs
Assets/Scripts/UI/MainMenuController.cs
Assets/Scripts/UI/MinicardTooltipController.cs
Assets/Scripts/UI/MusicianMapStatusUI.cs
Assets/Scripts/UI/MusicianPickerRow.cs
Assets/Scripts/UI/MusicianUI.cs
Assets/Scripts/UI/NewSongPanelUI.cs
Assets/Scripts/UI/RewardContainer.cs
Assets/Scripts/UI/Song Composition/CompositionContextRowUI.cs
Assets/Scripts/UI/Song Composition/CompositionStripDriver.cs
Assets/Scripts/UI/Song Composition/LoopsTimerUI.cs
Assets/Scripts/UI/Song Composition/SongCompositionUI.cs
Assets/Scripts/UI/Song Composition/SongPartElementUI.cs
Assets/Scripts/UI/Song Composition/SongPartsLayoutUI.cs
Assets/Scripts/UI/Song Composition/SongTrackElementUI.cs
Assets/Scripts/UI/Song Composition/TrackHoverPanel.cs
Assets/Scripts/UI/StatusIconBase.cs
Assets/Scripts/UI/TempoTextIndicator.cs
Assets/Scripts/UI/Tooltips/EconPipTooltipTarget.cs
Assets/Scripts/UI/Tooltips/I2DTooltipTarget.cs
Assets/Scripts/UI/Tooltips/ITooltipTargetBase.cs
Assets/Scripts/UI/Tooltips/TooltipController.cs
Assets/Scripts/UI/Tooltips/TooltipManager.cs
Assets/Scripts/UI/Tooltips/TooltipText.cs
Assets/Scripts/UI/UIPulseAnimator.cs
```

### Assets/Scripts/Utils

```
Assets/Scripts/Utils/ButtonSoundPlayer.cs
Assets/Scripts/Utils/CoreLoader.cs
Assets/Scripts/Utils/HighscoreStore.cs
Assets/Scripts/Utils/InventoryHelper.cs
Assets/Scripts/Utils/SceneChanger.cs
```

### Docs

```
Docs/changelog-ssot.md
Docs/coverage-matrix.md
Docs/CURRENT_STATE.md
Docs/MGP_Boundary_Index.md
Docs/PK_Manifest.md
Docs/README.md
Docs/SSoT_CONTRACTS.md
Docs/SSoT_INDEX.md
Docs/ssot_manifest.yaml
```

### Docs/archive

```
Docs/archive/Application_Ledger_2026-08-21.md
Docs/archive/changelog-ssot_2026-03-18_to_2026-06-22.md
Docs/archive/CURRENT_STATE_pre-prune_2026-06-16.md
Docs/archive/Design_Starter_Deck_v1.md
Docs/archive/DOC-APPLY-1_Application_Report_2026-07-31.md
Docs/archive/README.md
Docs/archive/Runbook_ST_R5a_Voltage.md
Docs/archive/SNAPSHOT_RETENTION_POLICY.md
```

### Docs/archive/absorbed

```
Docs/archive/absorbed/README.md
Docs/archive/absorbed/Source_Docs_Supersession_Map.md
```

### Docs/archive/snapshots

```
Docs/archive/snapshots/README.md
```

### Docs/audits

```
Docs/audits/ALWTTT_Combat_MVP_Audit_Final.md
Docs/audits/PK_Audit_Report_2026-08-26.md
```

### Docs/integrations

```
Docs/integrations/README.md
```

### Docs/integrations/midigenplay

```
Docs/integrations/midigenplay/ALWTTT_Uses_MidiGenPlay_Quick_Path.md
Docs/integrations/midigenplay/integrations_midigenplay_README.md
Docs/integrations/midigenplay/MidiGenPlay_Expressive_Surface_for_ALWTTT_Cards.md
Docs/integrations/midigenplay/Palette_Card_Identity_Design.md
Docs/integrations/midigenplay/SSoT_ALWTTT_MidiGenPlay_Boundary.md
```

### Docs/planning

```
Docs/planning/Design_Action_Economy_v1.md
Docs/planning/Design_Fill_Window_v0_1.md
Docs/planning/Design_Game_And_Card_Maxims_v0_1.md
Docs/planning/Design_Pending_Effects_v1.md
Docs/planning/Design_Project_Directives_v0_1.md
Docs/planning/Design_Singer_Expression_Input_v0_1.md
Docs/planning/Design_Song_Parts_Library_v0_1.md
Docs/planning/Design_Tempo_Identity_v1.md
Docs/planning/Design_Tutorial_System_v0_2.md
Docs/planning/Design_Vibe_Telegraph_v0_1.md
Docs/planning/planning_README.md
```

### Docs/planning/active

```
Docs/planning/active/CSV_Composition_Validation_Sub_Roadmap.md
Docs/planning/active/Design_Audience_Status_v1.md
Docs/planning/active/Design_Composition_Debug_Tab_v0_1.md
Docs/planning/active/Design_Composition_Variations_v0_1.md
Docs/planning/active/Design_Demo_Cut_v1.md
Docs/planning/active/Design_Sensory_Contract_v0_1.md
Docs/planning/active/Design_Starter_Deck_v2_DRAFT.md
Docs/planning/active/Design_Track_Card_Levels_v0_1.md
Docs/planning/active/Design_Tutorial_System_v0_1.md
Docs/planning/active/Design_Vertical_Slice_v0_1.md
Docs/planning/active/planning_active_README.md
Docs/planning/active/Roadmap_ALWTTT.md
Docs/planning/active/Roadmap_Audio.md
Docs/planning/active/RosterExpansion_Sub_Roadmap.md
Docs/planning/active/S5_DemoCutClose_Sub_Roadmap.md
Docs/planning/active/TUT-REBUILD_Sub_Roadmap.md
```

### Docs/planning/archive

```
Docs/planning/archive/ALWTTT_DeckEditorWindow_Roadmap_Proposal.md
Docs/planning/archive/Combat_MVP_Roadmap.md
Docs/planning/archive/M1_5_Dev_Mode_Sub_Roadmap.md
Docs/planning/archive/README.md
Docs/planning/archive/Roadmap_Combat_MVP.md
Docs/planning/archive/Roadmap_Combat_MVP_Closure_Actionable.md
```

### Docs/reference

```
Docs/reference/CSO_Primitives_Catalog.md
Docs/reference/Design_Asset_Naming_v0_1.md
Docs/reference/PinkTrombone_Voice_Levers.md
Docs/reference/README.md
Docs/reference/Report_CardLLM_Pipeline.md
```

### Docs/runtime

```
Docs/runtime/README.md
Docs/runtime/SSoT_Runtime_CompositionSession_Integration.md
Docs/runtime/SSoT_Runtime_Flow.md
```

### Docs/systems

```
Docs/systems/README.md
Docs/systems/SSoT_Audience_and_Reactions.md
Docs/systems/SSoT_Audio.md
Docs/systems/SSoT_Card_Authoring_Contracts.md
Docs/systems/SSoT_Card_System.md
Docs/systems/SSoT_Dev_Mode.md
Docs/systems/SSoT_Editor_Authoring_Tools.md
Docs/systems/SSoT_Gig_Combat_Core.md
Docs/systems/SSoT_Gig_Encounter.md
Docs/systems/SSoT_Scoring_and_Meters.md
Docs/systems/SSoT_Singer_Voice.md
Docs/systems/SSoT_Status_Effects.md
```

### Packages

```
Packages/manifest.json
Packages/packages-lock.json
```

