DEATH FOREST V20 - TACH LANDMARK PROCEDURAL SANG PREFAB / SCENE AUTHORING
=====================================================================

Muc tieu
--------
LevelFactory khong con coi landmark procedural la duong chinh nua.
Runtime se uu tien nap landmark authored prefab set tu:

  Resources/Generated/Landmarks/DeathForestLandmarkSet.prefab

Neu prefab authored chua ton tai, game van fallback sang legacy procedural de tranh vo map,
nhung do chi la duong lui de migrate.

File moi
-------
- Assets/Scripts/Editor/LandmarkPrefabAuthoringTools.cs

LevelFactory thay doi
---------------------
- Them AuthoredLandmarkSetResourcePath
- BuildLandmarks() gio uu tien prefab authored
- Them API migration/editor:
  - GetLegacyLandmarkIdsForAuthoring()
  - CreateLegacyLandmarkAuthoringRoot(string landmarkId)
  - CreateLegacyLandmarkSetAuthoringRoot()

Menu moi trong Unity
--------------------
1. Death Forest/Landmarks/Generate Authored Prefabs From Legacy Layout
   - Tu dong dung landmark tu legacy layout
   - Save tung landmark prefab rieng vao Assets/Resources/Generated/Landmarks/
   - Save 1 prefab tong hop DeathForestLandmarkSet.prefab

2. Death Forest/Landmarks/Spawn Authored Landmark Set In Current Scene
   - Dua prefab tong hop vao scene hien tai de ban chinh tay author

3. Death Forest/Landmarks/Save Selected Root As Authored Landmark Set
   - Save root dang chon trong Hierarchy thanh prefab runtime se nap

Quy trinh khuyen nghi
---------------------
1. Mo Unity
2. Chay Generate Authored Prefabs From Legacy Layout
3. Spawn Authored Landmark Set In Current Scene
4. Chinh sua landmark trong scene / prefab mode
5. Save Selected Root As Authored Landmark Set
6. Bam Play -> LevelFactory se nap prefab authored

Landmark da tach
----------------
- CrashSite
- RangerHut
- AbandonedCamp
- Shrine
- Creek
- AmbientClutter

Ghi chu
-------
- Runtime editor external asset va baked external environment van hoat dong doc lap.
- Landmark authored prefab la lop authoring rieng, de ve sau ban co the xoa hẳn legacy fallback neu muon.
