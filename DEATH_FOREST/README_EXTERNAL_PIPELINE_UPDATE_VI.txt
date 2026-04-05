CAP NHAT PIPELINE ASSET NGOAI (KHONG CON PHU THUOC HARD-CODE RUNTIME)
===================================================================

Da thay doi:
1) Them ExternalAssetCatalog.cs
   - Runtime doc catalog asset ngoai.
   - Khong can hard-code tung ten prefab trong code nua.

2) Them menu editor Rebuild External Asset Catalog
   - Quet tat ca prefab trong Assets/Resources/ExternalAssets
   - Tao file catalog runtime tai Assets/Resources/Generated/ExternalAssetCatalog.json

3) Sua RuntimeObjectEditor
   - Asset list bay gio doc tu catalog.
   - Them nut Refresh Catalog.
   - Khong con phu thuoc vao danh sach prefab hard-code cu.

4) Sua LevelFactory
   - Uu tien dung prefab ngoai theo role/keyword cho tree, ghost, car, cabin, shack, bush, rock, crate,
     torii, altar, stream, barrier, table, shelf, sign.
   - Neu co prefab bake san tai Assets/Resources/Generated/BakedExternalEnvironment.prefab
     thi game tu nap them vao map luc Play.
   - Neu khong co asset ngoai hop le, game van fallback ve procedural cu.

5) Them menu Bake Runtime Layout To Prefab
   - Lay layout da save trong runtime editor
   - Sinh prefab that de lan sau game nap truc tiep prefab, khong can dat lai bang runtime editor nua.

CACH DUNG NHANH
- Import prefab -> Assets/Resources/ExternalAssets/
- Death Forest/Rebuild External Asset Catalog
- Play -> F2 -> dat asset -> F5 Save Layout
- Death Forest/Bake Runtime Layout To Prefab

GIOI HAN HIEN TAI
- Baker chi bake asset ngoai co trong catalog; cac primitive legacy nhu light_point se khong duoc bake.
- Nhung phan gameplay can trigger/component dac thu van do LevelFactory quan ly.
