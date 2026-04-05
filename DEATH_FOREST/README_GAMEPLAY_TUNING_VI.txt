BẢN TINH CHỈNH GAMEPLAY - DEATH FOREST

Mục tiêu của bản chỉnh này là làm game "đã tay" hơn mà không phá scope hiện tại.

Đã thay đổi:
1. AI công bằng hơn
- Guard không còn biết vị trí thật của người chơi sau khi mất tầm nhìn.
- Guard sẽ đuổi tới vị trí nhìn thấy cuối cùng rồi kiểm tra khu vực đó.
- Sửa công thức góc nhìn để nhìn thẳng đáng sợ hơn nhìn lệch mép FOV.
- Bắt người chơi chỉ xảy ra khi có đường nhìn/catch line rõ, giảm cảm giác bị bắt xuyên góc tường.

2. Thêm công cụ đánh lạc hướng
- Người chơi có 1 mồi nhử tiếng động từ đầu trận.
- Có thêm 2 pickup "mồi nhử tiếng động" đặt ngẫu nhiên trên map.
- Nhấn Q để ném mồi nhử theo tâm màn hình.
- Guard trong bán kính nghe thấy sẽ chuyển sang state Investigate.

3. Tăng risk/reward cho tương tác
- Khởi động hộp điện tạo tiếng động lớn.
- Mở/đóng cửa an ninh tạo tiếng động vừa.
- Điều này làm route planning thú vị hơn thay vì chỉ bấm E vô hại.

4. HUD và onboarding
- Inventory hiển thị số mồi nhử còn lại.
- Hint/story đã nhắc Q để người chơi biết mechanic mới.

File C# đã chỉnh:
- Assets/Scripts/GameTypes.cs
- Assets/Scripts/GameManager.cs
- Assets/Scripts/PlayerMotor.cs
- Assets/Scripts/PatrolEnemy.cs
- Assets/Scripts/LevelFactory.cs
- Assets/Scripts/FuseBoxInteractable.cs
- Assets/Scripts/SlidingDoorInteractable.cs
- Assets/Scripts/ExitInteractable.cs

Gợi ý test nhanh trong Unity:
- Chạy vòng đầu, thử ném mồi nhử vào hành lang rồi cắt qua lối khác.
- Mở cửa khi guard đang ở gần để kiểm tra phản ứng Investigate.
- Bị chase rồi vòng qua góc/tủ để xem guard có tìm ở vị trí cuối cùng thay vì ESP.

Nếu muốn nâng thêm 1 nấc sau bản này, ưu tiên tốt nhất là:
- thêm camera báo động,
- thêm 1 guard chỉ xuất hiện sau khi khôi phục điện,
- thêm 1 tuyến phụ/shortcut để mở kho lưu trữ theo 2 cách.
