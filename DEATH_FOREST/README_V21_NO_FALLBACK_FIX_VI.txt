Death Forest v21 - bo procedural fallback cho landmark + sua loi compile Editor

Da sua:
1. Bo runtime procedural fallback trong LevelFactory.BuildLandmarks().
   - Runtime gio CHI nap landmark authored prefab tai:
     Resources/Generated/Landmarks/DeathForestLandmarkSet
   - Neu prefab thieu, game se log error ro rang thay vi tu dung landmark procedural.

2. Sua loi compile Editor do LevelFactory de internal.
   - Cac helper authoring sau da doi sang public:
     - AuthoredLandmarkSetResourcePath
     - AuthoredLandmarkSetPrefabAssetPath
     - GetLegacyLandmarkIdsForAuthoring()
     - CreateLegacyLandmarkSetAuthoringRoot()
     - CreateLegacyLandmarkAuthoringRoot(string)
   - Ly do: script trong Assets/Scripts/Editor compile o assembly rieng, nen internal khong truy cap duoc.

3. Them Editor bootstrap tu tao landmark authored prefab mac dinh neu file prefab chua ton tai.
   - File moi: Assets/Scripts/Editor/LandmarkPrefabBootstrap.cs
   - Menu moi: Death Forest/Landmarks/Rebuild Default Authored Landmark Prefab

4. Bo catalog legacy fallback trong ExternalAssetCatalog.
   - Neu chua co catalog external asset, editor se khong nhay ve cac prefab legacy hard-code nua.

5. Bo primitive/light fallback trong RuntimeObjectEditor.
   - Runtime object editor gio chi tao object tu ExternalAssetCatalog.

Cach dung:
- Mo project trong Unity.
- Cho Unity compile xong.
- Neu can, chay menu: Death Forest/Landmarks/Rebuild Default Authored Landmark Prefab
- Import prefab ngoai vao Assets/Resources/ExternalAssets/
- Chay menu: Death Forest/Rebuild External Asset Catalog

Neu van con Safe Mode, can chup man hinh Console de xem loi compile moi phat sinh, vi minh khong the chay Unity Editor truc tiep trong moi truong nay.
