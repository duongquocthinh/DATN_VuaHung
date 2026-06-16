# DATN_VuaHung - Development Notes

## Project
- Name: DATN_VuaHung
- Unity version: 2022.3.62f3
- GitHub: https://github.com/duongquocthinh/DATN_VuaHung.git
- Branch: main
- Uses Git LFS.

## Local Setup
- Current machine path: D:\UnityProjects\DATN_VuaHung
- Main scene: Assets/_Project/Scenes/MainScene.unity
- Keep project on D: because C: has very little free space.

## Git Workflow When Switching Machines
Before working:
```powershell
git pull origin main
git lfs pull
```

After working:
```powershell
git status
git add Assets Packages ProjectSettings .gitignore .gitattributes .vsconfig
git commit -m "Update project"
git push origin main
```

Do not add these to Git:
- Library
- Temp
- Obj
- Logs
- Build or Builds
- UserSettings
- .vs
- .rar or .zip backups

If `git status` says `nothing to commit, working tree clean`, there is nothing new to commit.

## Game Context
The game is about the Banh Chung Banh Giay legend.
The player collects ingredients:
- Dong leaves
- Rice
- Mung beans
- Meat

Then the player crafts banh chung / banh giay and submits the cakes to King Hung.

The map should feel like an ancient Vietnamese village, not an empty survival map. Important areas:
- Village
- Fence around the village
- Rice field
- Dong leaf bushes
- Forest
- Cooking fire
- Houses
- King Hung area or King Hung house

Story scene idea:
- King Hung has a house or important area.
- King Hung and a messenger / scholar / command reader stand near the village.
- Villagers gather to listen to the command.
- After the command, villagers spread out to work: cooking, lighting fire, wrapping cakes, pounding rice, chatting, sitting, or standing.

## Current State
- Terrain map exists.
- Fence has been placed manually with Fence prefabs.
- Dong_Bush / LaDong_Bush exists.
- Rice_Field exists.
- Cooking fire and some houses exist.
- Terrain color is being changed away from bright survival green toward ancient Vietnamese earth / moss tones.

Terrain target colors:
- Moss/yellow grass: #6F7F32 or #7F8F3A
- Village dirt road: #9A7A3D
- Village yard dirt: #B09155

Terrain painting guidance:
- Use Terrain > Paint Texture.
- Do not repaint everything from scratch.
- Use brush size around 10-25.
- Use opacity around 10-25.
- Paint lightly, avoid opacity 100.
- Village, road, and fence areas should use brown/yellow dirt.
- Forest and outside village can stay moss green.

## Performance Notes
Target hardware can be weak, so optimize carefully.
- Avoid very high triangle counts.
- Do not use Terrain Paint Details for heavy mesh objects such as fences.
- Fence should be placed manually or with few objects.
- Mesh Compression does not significantly reduce triangles.
- Prefer low-poly / game-ready assets.
- When using Meshy, request low-poly / game-ready output.
- Use Blender Decimate if needed.
- Turn off Cast Shadows for many small secondary objects.
- Use BoxCollider instead of MeshCollider when possible.
- Avoid too many heavy assets in the scene.

## Interaction Notes
SelectionManager was adjusted from the tutorial for more reliable raycast behavior.
Recommended approach:
- Ray from screen center with `ViewportPointToRay(0.5, 0.5)`.
- Use `Physics.RaycastAll`.
- Use `QueryTriggerInteraction.Collide`.
- Use `GetComponentInParent<InteractableObject>()`.

Interactable objects need:
- Collider, usually BoxCollider
- InteractableObject script
- ItemName, for example `La Dong`

For dong bushes:
- Each bush can keep its own Collider + InteractableObject.
- Empty parent `Dong_Bushes` is only for organizing the Hierarchy.
- Do not merge meshes if individual bushes should remain interactable.
- Do not put one shared Collider/InteractableObject on the parent if each bush should interact separately.

## NPC / AI Direction
The tutorial rabbit AI only moves randomly by transform.position and is not ideal for villagers.
Longer-term direction:
- Use NavMeshAgent for villagers.
- Bake NavMesh on walkable ground.
- Use points such as SpeechPoint, WorkPoint, IdlePoint.
- Start with Capsule NPCs first if needed.
- Later use Mixamo for villagers and animations.

Animator basics:
- Idle
- Walk
- Work
- Sit
- bool isWalking / isRunning
- trigger for work actions if needed

## Static / Fence Notes
- Fence has been placed manually.
- After placing, it can be marked Static.
- If Unity asks `Change Static Flags`, choose `Yes, change children`.
- If still editing positions, finish moving/rotating before marking Static.
- Turn off Cast Shadows on fence if performance is bad.

## Codex Continuation Instruction
When continuing this project on another machine, first read this file and then inspect the current Git status.
Use short, direct guidance because the developer is new to Unity and Git.
Prioritize safe Git workflow and performance-friendly Unity changes.
