DEATH FOREST - HUONG DAN GAN ASSET NGOAI

Project nay da duoc mo san cac hook runtime cho asset ngoai. Ban chi can import prefab/audio vao Assets/Resources va dat dung ten file.

1) PREFAB CO THE GAN NGAY
- ForestTreePrefab.prefab
  Dung cho cay lon / cum cay. Runtime se them collider than cay neu prefab khong co.

- GhostModel.prefab
  Dung cho nhan vat ma. Runtime se xoa collider thua va can chinh model cho hop kich thuoc truy duoi.

- BrokenCarPrefab.prefab
  Dung cho xe bi nan. Runtime se them collider than xe neu prefab khong co.

- ForestCabinPrefab.prefab hoac RangerCabinPrefab.prefab
  Dung cho nha/cabin o khu kiem lam. Runtime se tu sinh wall collider khoang rong cua truoc neu prefab khong co collider hop le.

- ForestShackPrefab.prefab
  Dung cho choi/goi nha nho gan khu suoi.

2) AUDIO CO THE GAN NGAY
Dat clip .wav/.mp3 vao Assets/Resources voi dung ten:
- ForestAmbientLoop
- ForestTensionLoop
- GhostWhisperSfx
- GhostStingerSfx
- CarRepairSfx
- CarStartSfx
- EscapeWinSfx
- DeathLoseSfx
- FootstepGrass1
- FootstepGrass2
- FootstepGrass3
- FootstepGrassCrouch

Neu khong co clip ngoai, game se tu tao am thanh mac dinh bang code de van choi duoc.

3) CAC GOI Y ASSET NGOAI HOP DEATH FOREST
- Trees/foliage: Kenney Nature Kit, Kenney Graveyard Kit
- Ghost/monster: Quaternius ghost (CC0), hoac prefab ma low-poly tu nguon ban co
- Cabin/house: prefab cabin/goi nha low-poly; nho de pivot nam gan mat dat

4) LUU Y VE COLLIDER
- Nha/cabin ngoai thuong gay loi xuyen tuong vi khong co collider. Project da them bo adapter ExternalAssetPrefabAdapter de tao va/hoac bo sung collider runtime.
- Neu prefab cua ban da co MeshCollider/BoxCollider dep thi adapter se giu nguyen collider hien co.

5) MENU CHAY NHANH
- Vao menu: Death Forest/Create Empty Play Scene
- Mo scene DeathForest_Play.unity
- Bam Play

6) GOI Y IMPORT
- Chon Scale Factor phu hop khi import FBX
- De Read/Write mesh off neu khong can
- Bat Generate Colliders neu asset da co collider don gian; neu khong co thi runtime adapter van hoat dong
