# Túi đồ: cá, trang bị và mồi

Mở `Assets/_Recovery/Test.unity`, Play, nhấn I. Có ba tab Cá / Trang bị / Mồi. Cá vẫn là từng cá thể; cần và dây được liệt kê theo loại sở hữu, mồi có số lượng. Chọn một vật phẩm để xem mô tả, chỉ số hiện có và bấm Trang bị. Dấu ✓ cho biết món đang dùng. Không đổi trang bị giữa lượt quăng, kéo hoặc Catch.

## Bộ khởi đầu và save cũ

`Assets/Data/Equipment/EquipmentCatalog_Main.asset` chứa danh mục và Starting Items: cần tre, dây cước cơ bản, mồi giun hiện có; mặc định mỗi loại 1. Đây là cấu hình khởi đầu, chưa phải cân bằng kinh tế. Chỉnh Quantity trong catalog cho hồ sơ mới. Thay Starting Items không tự tặng lại đồ cho hồ sơ đã chơi.

Save profile nâng từ schema 1 lên 2, thêm equipment vào cùng `player.json`. Save cá cũ được đọc nguyên trạng; chỉ hồ sơ chưa có phần equipment mới được nhận đồ khởi đầu một lần. Cá và trang bị cùng dùng ghi tạm + backup/replace hiện có. Không cần xóa save. Bản code schema 1 cũ sẽ từ chối đọc schema 2 để tránh ghi đè mất dữ liệu mới.

Catalog giữ ID cố định (rod.bamboo, line.basic, bait.worm). Không đổi ID sau khi phát hành save; tên hiển thị và tên asset đổi được. Vật phẩm có trong catalog không đồng nghĩa người chơi sở hữu nó. Đồ chưa tìm thấy định nghĩa trong catalog vẫn được giữ trong dữ liệu sở hữu; không tự xóa.

## Chỉ số và hình ảnh

- Rod_Bamboo.asset tham chiếu đúng sprite cần hiện tại và giữ min/max vận tốc quăng 3/7, giới hạn dây 2/7 như scene đang dùng. Các thông số khác của Hook không thay đổi.
- Line_Basic.asset tham chiếu FishingLineDefinition BasicLine hiện có; không sao chép chỉ số kháng cự/QTE.
- Bait_Worm.asset tham chiếu BaitDefinition Worm_Bait hiện có; đổi mồi qua BaitVisualController, đổi sprite và màu vòng trong, vòng ngoài vẫn trắng. FishBaitSensor tiếp tục dùng chính BaitDefinition đó.
- Chưa đổi texture/hình học dây trong bước này. Chưa bổ sung một cây cần khác chỉ vì có ảnh rod_flex: ảnh đó là biến thể cần cong, không mặc định coi là một cần mới.
- Thêm cần có hình học khác về sau cần chuẩn hóa điểm cầm/đầu cần và biến thể animation trước; đổi sprite không đủ để bảo đảm dây luôn nối đúng. Cần mới trong bước này phải dùng cùng rig/pivot của cần tre.

## Trách nhiệm code

- EquipmentInventory: sở hữu, số lượng, loadout và kiểm tra dữ liệu thuần.
- PlayerEquipment: API trang bị và sự kiện Changed, đặt trên PlayerData.
- FishingEquipmentDefinition/Catalog: định nghĩa và danh mục, tách khỏi dữ liệu người chơi.
- FishingEquipmentRig: cầu nối loadout đã lưu với Hook, kháng cự và hình mồi trong scene.
- EquipmentInventoryView/EquipmentSlotView/InventoryTabs: hiển thị và tab; không ghi file.
- PlayerSaveController/PlayerSaveFileStore: lưu toàn bộ profile; FishInventory vẫn chỉ quản lý cá.

Prefab chính và scene đã nối sẵn. Trong Prefab Mode, bố cục mới ở WindowLayer/InventoryPanel/FishPage, EquipmentPage và Tabs. RectTransform vẫn chỉnh tự do; GridLayoutGroup/ContentSizeFitter chỉ quản lý xếp ô/chiều cao nội dung.

## Phạm vi bước này

Hoàn thiện sở hữu, lựa chọn và lưu trang bị. Chưa trừ số lượng mồi khi cá ăn, chưa trừ một dây sở hữu khi dây đứt, chưa có cửa hàng/nhận lại dây miễn phí. Các phần đó sẽ được nối thành một bước tiêu hao riêng; không coi số lượng hiển thị hiện tại là đã có kinh tế hoàn chỉnh.

## Kiểm tra

Chạy Tools/Fishing/Validate Fish Inventory UI và Tools/Fishing/Validate Equipment Inventory. Công cụ thứ hai tạo save thử trong thư mục tạm riêng, kiểm tra migration v1/v2, không tặng lại đồ, loadout, dữ liệu sai và phục hồi backup; không dùng save thật.

Play: kiểm tra cá cũ vẫn còn; cả ba tab đúng nội dung; sprite không méo; danh sách trang bị báo đang dùng; không mở túi khi mồi đang thả; đóng túi vẫn di chuyển/câu được. Dừng rồi Play lại: số lượng không tự tăng. Mồi giun vẫn thu hút đúng loài như trước. Nếu đã có tùy chỉnh prefab tại máy, commit trước khi pull và giữ phần chỉnh của bạn khi giải quyết xung đột.

Chưa chạy Unity Editor/Play Mode tại môi trường thực thi. Kiểm tra cú pháp, YAML và liên kết được thực hiện riêng; cần kiểm tra compile/runtime trong Unity 6000.3.7f1.
