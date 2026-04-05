V19 - THEM OBJECT / ASSET EDITOR RUNTIME
=======================================

Da them che do tao/chinh object ngay trong luc Play.

Cach mo:
- Nhan F2 de mo / dong Asset Editor.

Chuc nang:
- Hien danh sach asset ben trai de chon va them vao map.
- Co asset built-in: Cube, Capsule, Cylinder, Sphere, Rock, Tree, Crate, Point Light.
- Neu trong Assets/Resources co prefab dat dung ten, editor se tu hien them:
  - ForestTreePrefab
  - BrokenCarPrefab
  - ForestCabinPrefab
  - RangerCabinPrefab
  - ForestShackPrefab
  - GhostModel

Cach dung:
- Click trai: dat object tai diem dang tro.
- Click phai: chon object da dat.
- Mui ten: di chuyen X/Z.
- PageUp / PageDown: di chuyen Y.
- Q / E: xoay object.
- Z / X: scale nho / to.
- Delete: xoa object dang chon.
- Ctrl + D: duplicate object dang chon.
- F5: save layout.
- F9: reload layout da save.

Luu y quan trong:
- Editor chi cho chon/chinh cac object duoc them boi editor runtime.
- Layout duoc luu ra file JSON trong Application.persistentDataPath, ten:
  death_forest_runtime_object_layout.json
- Moi lan game rebuild lai map, editor se tu load file layout nay neu co.

File code moi:
- Assets/Scripts/RuntimeObjectEditor.cs
- Assets/Scripts/RuntimeEditorPlacedObject.cs

File da sua:
- Assets/Scripts/LevelFactory.cs
- Assets/Scripts/PlayerMotor.cs
