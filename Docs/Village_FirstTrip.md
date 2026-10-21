# Village · Chuyến câu đầu tiên

## Luồng đã nối

MainMenu → Chơi mới → Village, PlayerSpawn tại House. Hồ sơ mới chỉ có cần tre; 0 xu, chưa có dây hoặc mồi. Bảng đầu game hướng dẫn sang phải đến guild.

- Waiter / cô tiếp tân: tới gần rồi bấm E hoặc nút tương tác. Hội thoại giải thích nhận việc, cá mới/cũ, giao cá, thất bại và thưởng. Hoàn tất hội thoại cấp chứng nhận và tự nhận ủy thác đầu tiên. Nếu đóng giữa chừng, nói chuyện lại để tiếp tục từ đầu; chưa cấp chứng nhận/quà.
- Sau chứng nhận: góc trái có cấp và XP, nằm dưới cụm đồng hồ/túi/loadout; khung mục tiêu ở bên phải dưới nút menu.
- FishShopManager: chủ cửa hàng. Lần đầu sau chứng nhận sẽ giới thiệu mua/bán, cấp và trang bị 1 dây cơ bản + 1 mồi giun miễn phí. Cờ nhận bộ khởi đầu được lưu; không nhận lặp khi đổi scene hoặc mở lại game.
- Đi tiếp sang phải vào biển ở Sign: tự sang Lake. Trước khi nhận chứng nhận/bộ miễn phí, lối đi nhắc về guild/cửa hàng.
- Lake: hệ thống câu hiện có, thêm thông tin ủy thác/tiền/cấp/XP và sổ cá. Esc → Về làng để quay lại gần biển, đi sang trái gặp tiếp tân giao cá.
- Tiếp tục ở MainMenu mở scene đã lưu. Vị trí Village dùng marker House hoặc LakeReturnSpawn; chưa lưu tọa độ chính xác của mỗi bước đi.

## Ủy thác

Dữ liệu tại `Assets/Data/Village/`:

| Asset | Yêu cầu | Hạn game | Thưởng tạm |
|---|---|---|---|
| Commission_FirstCatch | 1 cá bất kỳ, tự câu sau khi nhận | 1.440 phút | 30 xu + 60 XP |
| Commission_LakeSupply | 2 cá bất kỳ, dùng được cá cũ | 720 phút | 35 xu + 30 XP |

Một ủy thác đang hoạt động tại một thời điểm. Giao đủ cá hợp lệ chỉ tiêu thụ đúng số cá yêu cầu; tiền, XP và việc kết thúc ủy thác lưu cùng snapshot. Cá mới xác định bằng CatchSequence lớn hơn mốc nhận việc. Bán cá dành cho ủy thác sẽ làm tiến độ thiếu lại.

Hết hạn hoặc xác nhận hủy: thất bại, không game over, giữ cá, có thể nhận lại. Nhận lại ủy thác cá mới sẽ đặt mốc nhận mới. Đồng hồ dừng trong hội thoại, shop, journal, inventory và pause; vẫn chạy khi QTE. Không tính thời gian offline.

Cấp 1 cần 60 XP để lên cấp 2; mỗi cấp tiếp theo cần thêm 30 XP. Thanh cấp chỉ xuất hiện sau khi có chứng nhận. Phần thưởng/cấp hiện là nền tảng, chưa khóa vùng hồ hoặc tạo hạng guild riêng.

## Cửa hàng

Chỉ có **Mua đồ / Bán cá / Bán đồ**. Không mua cá từ cửa hàng.

Giá ở `VillageContent_Main.asset`, không gắn cứng vào UI. Hiện có đúng các trang bị đã có art/data: cần tre, dây cơ bản, mồi giun. Cần tre 30 xu, mồi giun 5 xu; dây cơ bản cấp miễn phí khi đã hết loại đó. Giá cá bán mẫu 8/10/12 xu theo loài trong danh sách. Bán trang bị được nửa giá mua; giữ lại ít nhất một cần và một mồi để tránh mất khả năng tiếp tục câu. Đây là giá tạm để chỉnh cân bằng sau.

Món mua thêm nằm trong túi, người chơi chọn trang bị bằng I. Bộ miễn phí đầu game tự trang bị. Chưa bổ sung tiêu hao mồi theo lượt quăng hoặc trừ stack dây khi đứt trong đợt này; cơ chế đứt dây gameplay cũ vẫn hoạt động.

## Sổ cá kiểu Pokédex

J hoặc nút Sổ cá mở ở cả Village và Lake. Lấy danh sách loài từ FishCatalog_Main:

- Chưa tự câu: ???, chưa hiện hình và thông tin.
- Tự câu lần đầu: thông báo khám phá, mở hình/tên/độ sâu/mồi ưa thích/tập tính.
- Các lần sau: tăng tổng đã câu. Bán hoặc giao hết cá không xóa khám phá.
- Dữ liệu khám phá lưu riêng trong hồ sơ. Cá đã có trong save cũ được ghi nhận khi nâng cấp; không thể khôi phục lịch sử cá đã bán trước khi có sổ.

Refresh Fish List giữ ID và thứ tự cũ, nối loài mới ở cuối. Giữ definition cũ trong catalog nếu muốn duy trì mục đã khám phá. Không đổi Species ID hoặc xóa meta của nội dung đã lưu.

## Chỉnh trong Unity

VillageServices giữ tham chiếu tới PlayerSpawn, LakeReturnSpawn, Waiter, FishShopManager và Sign. Bạn có thể di chuyển marker/NPC bằng Inspector; logic không khóa tọa độ chúng. Player di chuyển dùng lại script và Animator hiện có; Village không có hành động quăng cần.

Prefab UI: VillageShellUI (pause/settings/HUD nền), VillagePanelUI (hội thoại/guild/shop), VillageInteractionUI (nút E), JourneyHudUI (cấp/tiền/mục tiêu), FishJournalUI (sổ cá), JourneyListRow (ô danh sách). Bố cục do RectTransform; chỉ nội dung danh sách dùng layout. Các canvas dùng tham chiếu 1920×1080.

Map, sprite và clip NPC giữ nguyên. Đã xử lý bốn đoạn conflict marker trong Village, chọn hai Animator Controller có GUID thực sự tồn tại trong repo. Main đã được kết hợp vào nhánh làm việc; không force-push main.

Save schema 3 đọc được schema 1/2, giữ cá và trang bị cũ. Quy tắc chỉ có cần áp dụng cho hồ sơ mới; không tự tước dây/mồi từ hồ sơ cũ. Chọn Chơi mới nếu muốn đi đúng luồng khởi đầu; hồ sơ hiện tại được cất thành bản sao trước.

Chưa chạy Unity import/compile/Play Mode/build cho đợt này, theo phạm vi đã thống nhất. Rà soát tĩnh không thay thế xác nhận trong Unity.

## Bổ sung khả năng đọc và đường về

HUD Lake có nút VỀ LÀNG ngay phía trên giữa màn hình, dùng cùng bước xác nhận/lưu/chuyển scene với mục Về làng trong Esc. Kết quả câu được hiển thị bằng FishingResultUI: thẻ nền đậm, ảnh cá hoặc ký hiệu, tiêu đề theo màu và mô tả trong 4,5 giây. Có thông báo riêng cho thành công, đứt dây và thu mồi chưa có cá; không chặn thao tác hoặc đóng băng thời gian.

Khoảng hở chân: đối chiếu Idle/Walk 96×120, PPU 64, hàng alpha cuối kết thúc tại y=113 (6 px trống phía dưới). Với Visual y=0,54, đáy pixel chân ở root y=0,63375. Đáy capsule chỉnh về 0,64375 để bù contact offset 0,01; không sửa clip, pivot, PPU hoặc Scale. Bỏ phần nâng thêm 0,01 khi đặt player ở Village. Chưa quan sát kết quả thực tế trong Unity.

Brief team art: [UI_Art_Brief.md](UI_Art_Brief.md).
