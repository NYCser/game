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


## Tinh chỉnh giao diện và phản hồi
- Đổi HUD sang tông tối xanh-xám, chữ chính sáng hơn, chữ phụ dịu hơn để đọc dễ trên nền tối.
- Thêm bảng intro cốt truyện đầu màn để người chơi hiểu ngay bối cảnh.
- Thêm thanh **CẢNH BÁO** hiển thị mức bị phát hiện và trạng thái an toàn / bị kiểm tra / bị truy đuổi.
- Thêm âm nền hum nhẹ và tiếng bip cảnh báo tạo bằng code, không cần import asset âm thanh ngoài.
- Đổi hình kẻ gác sang dáng low-poly tối màu, có mắt đèn đỏ/cam để nhìn ra trạng thái AI.
