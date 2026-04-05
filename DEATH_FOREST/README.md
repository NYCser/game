# Death Forest: Evidence Run

## Mục tiêu
Đây là một project Unity 3D một màn, làm theo tinh thần **Sneaky Mansion** (lẻn trong biệt thự, né mối nguy, hoàn thành mục tiêu rồi thoát), nhưng mình đã đổi hẳn cốt truyện và logic chính thành **đột nhập lấy bằng chứng** thay vì chỉ chạy trốn khỏi thế lực siêu nhiên.

## Cốt truyện mới
Bạn vào vai **Mai**, cựu thanh tra xây dựng. Anh trai của Mai mất tích sau khi điều tra một chuỗi biến mất bí ẩn liên quan đến Death Forest. Nửa đêm, Mai lẻn vào biệt thự để khôi phục điện phụ, lấy 3 hồ sơ bằng chứng, cướp thẻ đỏ của trưởng ca và mở kho lưu trữ trước khi thoát bằng thang nâng.

## Những gì đã thay đổi
- Không còn logic "vào biệt thự rồi chỉ cần sống sót để thoát".
- Game được đổi thành tuyến mục tiêu rõ ràng:
  1. tìm cầu chì xanh,
  2. khôi phục điện phụ,
  3. lấy 3 hồ sơ,
  4. lấy thẻ đỏ,
  5. mở kho lưu trữ,
  6. thoát bằng thang nâng.
- Thêm cơ chế **trốn trong tủ**, **cúi**, **đèn pin**, **AI nghe tiếng chân**, **cửa khóa theo tiến trình**.
- Phần đồ họa ưu tiên nhẹ: dùng primitive low-poly, fog, ánh sáng điểm, vật phẩm phát sáng và layout 1 màn để dễ tối ưu.

## Cấu trúc chính
- `Assets/Scripts/LevelFactory.cs`: sinh toàn bộ map bằng code lúc bấm Play.
- `Assets/Scripts/PlayerMotor.cs`: di chuyển FPS, cúi, chạy, đèn pin.
- `Assets/Scripts/PatrolEnemy.cs`: tuần tra, nghe tiếng động, phát hiện và đuổi.
- `Assets/Scripts/GameManager.cs`: quản lý objective và trạng thái thắng/thua.
- `Assets/Scripts/Editor/SceneTools.cs`: menu tạo scene rỗng để build/play tiện hơn.

## Cách mở project
1. Mở folder này bằng **Unity 6000.3.11f1** (đúng bản bạn đang dùng) hoặc bản 6000.3 gần đó.
2. Nếu chưa có scene:
   - vào menu `Death Forest/Create Empty Play Scene`
   - Unity sẽ tạo `Assets/Scenes/DeathForest_Play.unity`
3. Mở scene đó và bấm **Play**.
4. Runtime bootstrap sẽ tự sinh player, UI, map, AI, vật phẩm và objective.

### Ghi chú tương thích Unity 6000.3.11f1
- Project đã đổi `ProjectVersion.txt` sang **6000.3.11f1** để Hub nhận đúng bản Editor.
- Giữ **Built-In Render Pipeline** cho nhẹ và dễ chạy; project này không cần URP/HDRP.
- Đã sửa logic **restart bằng phím R** để tránh lỗi singleton khi rebuild map trong runtime.
- Đã thay lời gọi tìm object runtime sang API mới trên Unity mới để giảm warning khi import.

## Vì sao làm kiểu runtime-generated
- Ít phụ thuộc prefab/scene reference nên giảm lỗi mất link.
- Bạn chỉnh nhanh layout/chơi thử ngay trong `LevelFactory.cs`.
- Rất dễ thay đổi số phòng, vị trí AI, vị trí item khi cần mở rộng.

## Tối ưu đã ưu tiên
- 1 màn, primitive mesh đơn giản.
- Ít số lượng đèn động.
- Shadow distance thấp.
- Không dùng hậu kỳ nặng, texture nặng, animation phức tạp.

## Việc bạn nên làm tiếp nếu muốn polish
- Thay primitive bằng asset low-poly.
- Thêm âm thanh bước chân, tiếng còi, ambience.
- Chỉnh lại AI path nếu muốn bản đồ phức tạp hơn.
- Tách `LevelFactory` thành prefab/scene thật khi design đã ổn.


## Sửa lỗi khi mở bằng Unity 6000.3.11f1
Nếu Console báo lỗi kiểu **CharacterController could not be found in the namespace UnityEngine** và Safe Mode bật lên, nguyên nhân thường là project đang thiếu các built-in module trong `Packages/manifest.json`, đặc biệt là **Physics**.

Bản zip đã vá sẵn phần này. Sau khi mở lại project:
1. Chờ Unity resolve package xong.
2. Hết Safe Mode rồi vào menu `Death Forest/Create Empty Play Scene`.
3. Mở scene vừa tạo, rồi bấm **Play**.

### Vì sao bạn thấy scene trống / "không sense đồ họa"
- Điều đó là bình thường với project này.
- Map **không được dựng sẵn trong scene editor**.
- Toàn bộ biệt thự, player, AI, item và HUD được **sinh bằng code khi bạn bấm Play**.
- Nếu chỉ mở một scene rỗng như `Untitled`, bạn sẽ chỉ thấy camera + directional light + bầu trời.
