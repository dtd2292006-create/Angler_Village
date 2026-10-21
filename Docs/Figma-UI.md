# Figma UI integration

Source: https://www.figma.com/design/HkksYQAnBBVOy5em7s2tE4

The UI uses native uGUI controls, the existing stepped `FigmaPanelGraphic`, and original exported component artwork. No complete screen screenshot is used as an interactive UI. Runtime scripts update data and visibility; positions and sizes remain editable in the prefab RectTransforms.

| UI | Prefab | Figma reference |
| --- | --- | --- |
| Main menu | MainMenuUI | 109:1222, logo 155:7187 |
| Settings and pause | MainMenuUI, LakeShellUI, VillageShellUI | 109:1322, 109:1532 |
| HUD and equipped items | LakeShellUI, JourneyHudUI | 156:7461, 153:5797 |
| Inventory | InventoryUI | 107:30 |
| Guild and shop | VillagePanelUI | 108:1102, 108:828 |
| NPC dialogue | VillagePanelUI | 154:6172 |
| First certification | VillagePanelUI / CertificateSheet | 154:7101 |
| Fish journal | FishJournalUI, FishJournalSlot | 154:6424 |
| Catch results and discovery | FishingResultUI, FishJournalUI | 155:6955 |

Art is in `Assets/Art/UI/Figma`; `SourceManifest.json` records source node IDs. Sprites use Point filtering with mipmaps and compression disabled. Fonts are VT323 for headings/buttons and Be Vietnam Pro for body copy, with OFL licenses included. Both fonts contain the Vietnamese characters used by these screens.

`EquipmentLoadoutView` displays equipped data only. `FishJournalSlotView` owns square journal selection only; it is independent of shop/guild list rows. The journal keeps unknown species hidden until discovered. NPC portraits follow the current conversation. The certificate is shown only after successful initial enrollment and uses the same modal owner as the guild panel.

The reference QTE rim decorates the existing dynamic ring. Good/Great windows, difficulty, cooldowns and resistance logic are unchanged. Video settings remain functional, even though the Figma display section was a placeholder. Shop actions remain single-item purchases/sales; the layout does not include a decorative quantity selector with no effect.

Figma's Settings, Close and NewCatch components exported empty. They are excluded rather than imported as blank sprites. The HUD uses an **Esc** keycap for pause/settings; modal close controls retain their text glyphs.

Lake travel remains a world trigger at the left edge of Lake, returning beside the right-hand sign in Village. There is no return-to-village UI button. An unresolved merge conflict in LakeShellUI was removed while preserving this route.

Static verification covers C# syntax, Unity YAML, local and known cross-asset references, sprite validity, Vietnamese font coverage, and offline layout previews. Unity import, Play mode and builds have not been run in this environment. Final rendering and gameplay must be checked in Unity when the project reaches the testing stage.
