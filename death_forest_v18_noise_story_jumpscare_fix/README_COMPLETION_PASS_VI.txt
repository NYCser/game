BAN CAP NHAT HOAN THIEN NHANH - DEATH FOREST

Nhung gi da duoc va ngay trong project nay:
- Fix loi xuyen tuong o cabin/shack bang cau truc nha rong co wall collider that.
- Fix logic ma bi ket khi gap cay/da/nha:
  + them obstacle avoidance ngay trong PatrolEnemy
  + them co che tu doi huong va sidestep khi bi canh collider chan
  + neu bi ket lau, ma se bo waypoint do hoac doi diem dieu tra de thoat khoi vat can
- Them ho tro prefab ngoai cho:
  + ForestTreePrefab
  + GhostModel
  + BrokenCarPrefab
  + ForestCabinPrefab / RangerCabinPrefab
  + ForestShackPrefab
- Them script ExternalAssetPrefabAdapter de tu sinh collider runtime cho asset import bi thieu.
- Them sound runtime:
  + ambience rung
  + tension/drone
  + whisper/stinger khi bi de doa
  + footstep co tieng rieng cho player
  + metal repair / car start / win / lose
- Doi menu tao scene thanh Death Forest/Create Empty Play Scene
- Doi ten runtime bootstrap sang Death Forest

De them 1 asset hop canh:
- Nen them 1 pack Graveyard / shrine props quanh khu mo va cong do de map day hon, thay vi chi de block don.

Luu y:
- Chua the bundle truc tiep asset web vao file zip nay vi moi truong hien tai khong import duoc package/FBX tu Internet vao Unity thay ban.
- Nhung project da san sang de ban chi can tha prefab/audio vao Assets/Resources voi dung ten la game se tu nhan.
