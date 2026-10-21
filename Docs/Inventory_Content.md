# Thêm, bớt cá và trang bị

Có ba phần độc lập: định nghĩa món đồ, danh mục tra cứu và số lượng người chơi sở hữu. Thêm định nghĩa vào catalog không tự tặng món đó vào túi đồ.

## Cần / dây / mồi

1. Ngoài Play Mode, Create → Fishing → Inventory → Equipment Definition.
2. Chọn Kind, đặt Display Name, icon và mô tả.
3. Rod: gắn sprite đúng pivot/tay cầm và chỉnh tốc độ quăng, chiều dài dây. Line: gắn FishingLineDefinition. Bait: gắn BaitDefinition (sprite và màu vòng trong; vòng ngoài trắng giữ nguyên).
4. Chọn FishingEquipmentCatalog đang được PlayerEquipment dùng. Bấm **Refresh Equipment List (keep existing IDs)**. Định nghĩa mới chưa có ID sẽ nhận ID từ GUID asset; ID đã có không đổi. Tránh duplicate asset rồi giữ nguyên Item ID.
5. Muốn hồ sơ mới có sẵn món nào: chỉnh Starting Items trong catalog. Hiện mặc định chỉ cấp cần tre; dây/mồi đầu tiên được ShopService cấp một lần theo tiến trình Village. Điều này không sửa hồ sơ đã chơi.
6. Muốn cấp/trừ cho hồ sơ hiện tại: vào Play Mode → **Tools → Fishing → Inventory Content Tools**, chọn definition và Quantity rồi Grant / Remove. Đây là thao tác thật, được lưu vào hồ sơ. Chỉ đổi trang bị khi rig cho phép, không giữa lượt câu.

Khi số lượng về 0, món đang trang bị sẽ được bỏ trang bị; thiếu cần/dây/mồi thì không quăng được. Công cụ không tự tìm món thay thế. Cấp lại và trang bị lại từ inventory khi cần.

## Cá

Tạo/chỉnh FishDefinition, rồi đưa nó vào danh sách Fish Definitions của FishCatalog mà PlayerInventory sử dụng. Giữ Species ID duy nhất, ổn định. Muốn loài cá thực sự xuất hiện ở hồ: nhân bản actor cá trong Lake, gắn definition, chỉnh Visual/MouthPoint/kích thước theo hình cá, giữ tham chiếu vùng bơi chung. Thêm FishCatchable mới vào LakeServices → LakeFishRespawn → Starting Fish để hồi sinh sau khi bị bắt.

Inventory Content Tools cho phép thêm một bản ghi cá thử hoặc xóa chính xác một con đã bắt. Thêm bản ghi vào túi không sinh actor trong hồ; sinh actor trong hồ không tự thêm vào túi.

## Ngừng dùng nội dung an toàn

Muốn ngừng xuất hiện: bỏ cá khỏi scene / nguồn spawn, bỏ trang bị khỏi Starting Items và nguồn cấp sau này. Nên giữ definition và ID cũ trong catalog để hồ sơ đã lưu vẫn tra được tên, hình và chỉ số.

Không đổi Item ID / Species ID của nội dung đã phát hành, không xóa tùy tiện file `.meta`. Xóa hẳn definition cần migration hồ sơ riêng; công cụ refresh không tự chuyển món cũ thành món mới. Những món chưa từng xuất hiện trong hồ sơ có thể xóa rồi cập nhật catalog.

Sổ cá [J] lấy dữ liệu từ cùng FishCatalog_Main, lưu khám phá độc lập với số cá trong túi. Thêm loài vào catalog chưa mở khám phá cho người chơi; chỉ tự câu mới mở. Xem Docs/Village_FirstTrip.md.
