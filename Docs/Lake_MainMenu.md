# Lake và Main Menu

Unity 6000.3.7f1 · Universal 2D · PC.

## Scene và luồng chơi

Luồng Village mới được mô tả ở [Village_FirstTrip.md](Village_FirstTrip.md). Chơi mới hiện vào Village, Tiếp tục dùng scene đã lưu.

Mở `Assets/Scenes/MainMenu.unity`. Build Settings đã liệt kê MainMenu trước Lake để chuyển scene bằng tên. Scene gốc `Assets/_Recovery/Test.unity` được giữ làm bản tham chiếu, không bị thay thế.

- Chơi mới: xác nhận trước; chuyển thư mục save hiện tại sang bản sao có dấu thời gian, rồi tạo hồ sơ mới.
- Tiếp tục: chỉ bật khi đọc được hồ sơ chính hoặc bản dự phòng.
- Lake: giữ nguyên map, Animator, hệ thống câu/QTE/cá hiện tại. Thêm HUD, pause, hướng dẫn, settings và âm thanh.
- Về menu/thoát: lưu trước; nếu ghi thất bại thì hiện thông báo và không tiếp tục thoát.

## Điều khiển

| Phím | Hành động |
|---|---|
| A / D | Di chuyển |
| Giữ / thả chuột trái tại Fishing Spot | Lấy đà / quăng cần |
| Giữ chuột trái dưới nước | Thu dây |
| Nhấp chuột trái khi QTE xuất hiện | Thực hiện QTE |
| I hoặc nút túi đồ | Mở / đóng inventory |
| Esc | Đóng lớp UI trên cùng hoặc mở / đóng pause |

Inventory và pause đóng băng thế giới. QTE vẫn để thế giới chạy. Mất focus cửa sổ sẽ mở pause. Khóa nhập liệu và khóa thời gian có chủ sở hữu riêng để các cửa sổ không tự mở khóa lẫn nhau.

## UI và cài đặt

Prefab `Assets/Prefabs/UI/LakeShellUI.prefab` và `MainMenuUI.prefab` dùng RectTransform để chỉnh bố cục trong Inspector. HUD được ẩn khi mở inventory để tránh che nhau. Vị trí UI không bị script đặt lại từng frame.

Settings: âm lượng tổng/nhạc/hiệu ứng, tắt riêng nhạc hoặc hiệu ứng, cửa sổ/toàn màn hình, VSync và ba độ phân giải. Âm lượng áp dụng ngay; thiết lập video áp dụng khi bấm nút. Lưu bằng PlayerPrefs, độc lập với hồ sơ chơi.

## Thời gian và quần thể cá

`PlayerWorldClock` trên PlayerData bắt đầu 08:00 ngày 1, mặc định 2 phút game / giây thật. Ngừng khi pause hoặc mở inventory; được lưu cùng hồ sơ, không tính thời gian offline. `LakeLighting` điều chỉnh Global Light 2D theo giờ. Đây là nền tảng chu kỳ ngày đêm, chưa có lịch NPC hoặc nhiệm vụ.

`LakeFishRespawn` trên LakeServices giữ danh sách Starting Fish. Cá đã bắt tái xuất hiện từ bản mẫu ban đầu sau 8–15 giây; bản ghi cá trong túi đồ độc lập. Thêm cá vào scene cần bổ sung nó vào mảng này nếu muốn hồi sinh. Camera dư trên CameraBound đã được tắt riêng component Camera; collider giới hạn vẫn giữ.

## Lưu và phạm vi

Save: `Application.persistentDataPath/Saves/slot_01/player.json`, có bản dự phòng. Cá, trang bị và đồng hồ được lưu; vị trí player, lượt câu đang diễn ra và cá đang bơi không được khôi phục. Hồ sơ cũ chưa có đồng hồ bắt đầu 08:00, giữ dữ liệu cá/trang bị cũ.

Village, NPC, cửa hàng, guild và ủy thác cơ bản đã được bổ sung; xem Village_FirstTrip.md. Chưa tiêu hao mồi hoặc trừ kho dây khi đứt để tránh làm người chơi hết trang bị trước khi có cửa hàng.

Chưa chạy Unity import, Play Mode hoặc build cho đợt thay đổi này. Rà soát tĩnh không thay thế việc kiểm tra trong Unity.
