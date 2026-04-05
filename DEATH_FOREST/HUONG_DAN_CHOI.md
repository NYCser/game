Lưu ý bản Unity: project này đã được chỉnh để mở bằng **Unity 6000.3.11f1**.

# Hướng dẫn chơi Death Forest: Evidence Run

## Điều khiển
- `WASD`: di chuyển
- `Shift`: chạy
- `Ctrl` hoặc `C`: cúi
- `F`: bật/tắt đèn pin
- `E`: tương tác / nhặt / mở cửa / trốn / thoát chỗ ẩn
- `Esc`: nhả hoặc khóa chuột
- `R`: chơi lại sau khi thắng/thua

## Luồng chơi chuẩn
1. Vào khu tiện ích bên phải để lấy **cầu chì xanh**.
2. Lắp cầu chì vào **hộp điện phụ**.
3. Khi điện phụ mở, khu văn phòng phía bắc có thể vào được.
4. Thu đủ **3 hồ sơ bằng chứng**.
5. Lấy **thẻ đỏ** trong khu văn phòng.
6. Quay lại **cửa kho lưu trữ** ở phía bắc để mở khóa.
7. Vào kho và kích hoạt **thang nâng** để kết thúc màn.

## Cách tránh bị bắt
- Đừng chạy vô tội vạ vì AI nghe được tiếng chân.
- Bật đèn pin chỉ khi cần quan sát, vì đang sáng sẽ dễ lộ hơn.
- Nếu thấy địch áp sát, tìm **tủ ẩn** và bấm `E` để chui vào.
- Cúi người giúp giảm tầm phát hiện.
- Hạn chế đứng lâu ở hành lang giữa vì đây là tuyến tuần tra chính.

## Mẹo chỉnh gameplay
- Vị trí item, AI, cửa, ánh sáng nằm trong `Assets/Scripts/LevelFactory.cs`.
- Thứ tự nhiệm vụ và điều kiện thắng/thua nằm trong `Assets/Scripts/GameManager.cs`.
- Hành vi player nằm trong `PlayerMotor.cs`, AI nằm trong `PatrolEnemy.cs`.


## Bản tinh chỉnh mới
- HUD đã đổi sang panel tối, chữ dễ đọc hơn và có thanh **CẢNH BÁO** ở giữa màn hình.
- Có lớp phủ đỏ nhẹ khi bị phát hiện hoặc bị truy đuổi.
- Âm thanh hiện tại là **synth procedural** tạo bằng code: nền hum nhẹ, tiếng bip cảnh báo khi bị soi/truy đuổi, âm báo thắng/thua.
- Kẻ đuổi theo vẫn dùng primitive để nhẹ máy, nhưng đã đổi thành dáng lính canh tối màu với mắt phát sáng, không còn kiểu khối vàng placeholder.
- Game vẫn là kiểu **bị bắt một lần là thua**, nên không có thanh máu; thay vào đó là thanh cảnh báo để người chơi biết mức độ nguy hiểm.
