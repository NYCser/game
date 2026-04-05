DEATH FOREST - HUONG DAN IMPORT ASSET NGOAI MOI
==============================================

Muc tieu moi:
- Khong hard-code tung prefab trong RuntimeObjectEditor nua.
- Khong phai sua code moi khi them asset moi.
- Co the bake layout runtime thanh prefab that de dung lai o lan Play sau.

QUY TRINH MOI
1) Import prefab vao:
   Assets/Resources/ExternalAssets/
   (co the tao them subfolder ben trong)

2) Trong Unity, chay menu:
   Death Forest/Rebuild External Asset Catalog

3) Vao Play, nhan F2 mo editor.
   Asset editor se tu hien tat ca prefab da quet tu catalog.

4) Dat asset, Save Layout (F5).

5) Neu muon bien layout nay thanh prefab that, chay menu:
   Death Forest/Bake Runtime Layout To Prefab

6) Tu lan Play sau, neu co prefab:
   Assets/Resources/Generated/BakedExternalEnvironment.prefab
   LevelFactory se tu nap prefab nay vao map.

Luu y:
- Catalog runtime duoc tao tai:
  Assets/Resources/Generated/ExternalAssetCatalog.json
- Prefab bake duoc tao tai:
  Assets/Resources/Generated/BakedExternalEnvironment.prefab
- Dat ten prefab co chua cac tu khoa nhu tree / bush / rock / cabin / shack / car / ghost
  thi LevelFactory se uu tien dung chung cho cac cho do hoa procedural tuong ung.
- Neu chua co asset ngoai phu hop, game van fallback ve cach cu de khong bi hong gameplay.
