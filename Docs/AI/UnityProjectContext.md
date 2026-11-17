# Unity project context
Inspected 2026-11-11. Unity 6000.5.9f1; URP package 17.6.0, uGUI 2.5.0, Input System 1.20.0. No Unity MCP detected in manifest or exposed tools. Editor validation used the existing Unity window and project Logs/Editor.log.

GameCore is plain C# under Assets/Scripts; GameplayBootstrap builds uGUI and binds the game. Runtime canvases are transient; GameplayUiBaker.ShowUi builds the saved preview. Build Settings list SampleScene and Gameplay. No first-party test assemblies found; Unity Test Framework is installed.

Undo uses GameCore.UndosLeft, three successful uses per level. Hint usage is owned by GameplayBootstrap, three successful requests per level. NewLevel resets both. MessagePlate is right-anchored, hidden outside screen, and slides over 0.45 seconds using unscaled time. Existing HintDialogue character remains unchanged.

Button sprites: Assets/Resources/UI/StoneButtonStates.png, four top-to-bottom subassets StoneButtonState0..3. Resources.LoadAll plus ordinal name sorting loads persistent assets compatible with saved previews. Do not destroy these imported sprites. New Level was moved above Hint to retain both actions on the right.

Validation: manually exercised all three Undo and Hint states and level reset in Play Mode; verified saved Scene sprite GUID references. No C# errors or runtime exception entries found in Editor.log after changes. No standalone build performed.
