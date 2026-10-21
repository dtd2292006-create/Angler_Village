# Brief UI · Cần thủ làng quê

## Mục tiêu và phong cách

Thiết kế bộ UI pixel art cho game câu cá PC, không khí làng quê Việt Nam ấm áp: gỗ tre, giấy ngà, vải túi đồ, kim loại nhỏ và màu nước hồ. Đồng bộ nhân vật/environment hiện có; tránh lấy quá nhiều họa tiết khiến chữ và icon khó đọc. UI phải có phân cấp: nội dung chính sáng rõ, trang trí lùi về nền.

Thiết kế tại khung tham chiếu 1920×1080, kiểm tra cả khi thu xuống 1280×720. Vẽ riêng component và một bản bố cục minh họa cho từng màn hình. Không gộp giao diện thành một PNG đầy đủ; tiền, tên cá, giá, cấp và các dòng nhiệm vụ sẽ do Unity hiển thị.

## P0 · Bộ thành phần dùng chung — làm trước

| Bộ | Cần bàn giao | Gợi ý kích thước PNG nguồn |
|---|---|---|
| Khung cửa sổ | Khung lớn, khung nhỏ, khung nội dung, khung hội thoại. Viền và giữa hỗ trợ 9-slice; ghi rõ border | 384×240, 240×144 |
| Nút | Chính/phụ/đóng, mỗi loại Normal, Hover, Pressed, Disabled, Focus | 192×60; đóng 48×48 |
| Tab | Normal, Hover, Selected, Disabled; Selected phải khác Hover rõ ràng | 192×60 |
| Ô vật phẩm | Nền ô, viền Hover/Selected, dấu Equipped, khóa, số lượng đặt riêng | 96×96 |
| Thanh | Track, Fill, Thumb; XP, lực quăng, kháng cự, âm lượng. Hai đầu/cap tách nếu cần | Track 240×24; thumb 24×36 |
| Icon chức năng | Túi, sổ cá, ủy thác, cửa hàng, cài đặt, đóng, quay lại/về làng, xu, XP/cấp, đồng hồ | 48×48 hoặc 96×96 |
| Trạng thái | Dấu tick, khóa, mới khám phá, thành công, cảnh báo, thất bại | 48×48 |
| Công tắc | On/Off; nhạc, hiệu ứng và trạng thái mute riêng | 72×36; icon 48×48 |

Kích thước trên là gợi ý cho asset nguồn, không phải toàn bộ kích thước cửa sổ trong game. Khung và nút sẽ giãn phần giữa bằng 9-slice. Ghi rõ vùng an toàn cho nội dung để chữ không chạm viền/trang trí.

## P1 · Các màn hình và thành phần gameplay

| Màn hình | Cần vẽ mới | Nội dung dùng lại / do code quản lý |
|---|---|---|
| HUD Lake/Village | Các bảng nhỏ cho giờ/ngày, xu, cấp/XP; thẻ mục tiêu; nút về làng; nền nhãn phím | Chữ giờ, tiền, cấp, mục tiêu và fill thanh |
| Kết quả câu | Khung thẻ thành công, đứt dây/cá thoát, thu mồi chưa có cá; vùng icon cá rõ; icon cảnh báo | Tên/hình loài cá và kết quả được thay động. Không vẽ tên vào ảnh |
| Khám phá mới | Banner riêng nhẹ, huy hiệu NEW và hiệu ứng điểm sáng nhỏ | Tên loài mới và sprite cá có sẵn |
| Inventory | Khung túi đồ, 3 tab cá/trang bị/mồi, bảng chi tiết, nút trang bị, dấu đang dùng | Sprite cá/cần/dây/mồi hiện có; số lượng và thông tin động |
| Guild | Nền bảng ủy thác, thẻ nhiệm vụ, huy hiệu mới/đang nhận/đủ cá/hết hạn, bảng thưởng xu/XP | Điều kiện, thời hạn, tiến độ, phần thưởng và tên nhiệm vụ |
| Chứng nhận | Tấm chứng nhận trống, con dấu guild và huy hiệu cần thủ | Tên người chơi, ngày/cấp nếu có. Có thể tái dùng khung hội thoại |
| Cửa hàng | 3 tab Mua đồ / Bán cá / Bán đồ; thẻ hàng, nhãn giá, trạng thái không đủ tiền; khung bộ miễn phí | Không có tab mua cá. Dùng icon trang bị và cá sẵn có |
| Hội thoại NPC | Bảng thoại, bảng tên, dấu tiếp tục, chỗ portrait | Portrait tiếp tân và chủ tiệm nên vẽ riêng 192×192 hoặc dùng phần đầu sprite hiện có; không cần vẽ lại animation |
| Sổ cá | Khung sách/sổ, tab hoặc trang, ô loài, nền ảnh lớn, dấu ?, khóa, đã khám phá, tổng số loài | Cá đã có sprite; chưa khám phá có thể dùng silhouette do Unity tạo, không cần vẽ lại mỗi silhouette |
| Pause/Settings | Khung bảng, hàng cài đặt, slider/toggle/button theo bộ P0 | Chữ và giá trị do UI tạo; ưu tiên tái dùng component |
| Main Menu | Logo tên game tách nền; bố cục menu và một nền dùng art làng/hồ hiện có | Nút Chơi mới/Tiếp tục/Cài đặt/Thoát dùng bộ chung |

Ưu tiên thực hiện: **P0 → HUD + thông báo kết quả → hội thoại/guild/shop → sổ cá/inventory → Main Menu và trang trí thêm.** Không cần vẽ lại sprite cá/cần/mồi hiện có chỉ để làm icon; chỉ bổ sung những icon thực sự thiếu.

## QTE và màu trạng thái

Vòng QTE cần tách nền vòng, viền, kim/con trỏ, tâm và feedback Good/Great/Fail. Giữ vùng hợp lệ màu trắng và vùng Great màu xanh theo cơ chế đã thống nhất. Không vẽ cố định các cung Good/Great lên một ảnh vòng hoàn chỉnh: góc/độ rộng thay đổi theo cá và dây, Unity phải tạo được các vùng này động.

Kháng cự giữ hướng xanh → vàng → đỏ; fill và khung tách. Không dùng riêng màu để phân biệt: kèm icon hoặc hình dáng/nhãn. Thành công có dấu tick/cá; thất bại có dấu cảnh báo. Banner phải đọc được trên cả nền trời sáng và nước tối.

## Quy chuẩn bàn giao

- PNG RGBA nền trong suốt; không viền trắng quanh alpha, không blur/anti-alias; kiểm tra trên cả nền sáng và tối.
- Thống nhất lưới pixel và độ dày viền. Gợi ý nhịp 3 px ở bản xuất 1920×1080, viền chính 6 px hoặc 9 px; không trộn các mức phóng pixel khác nhau.
- Với ảnh UI, Unity sẽ dùng Point filtering, không mipmap và cấu hình import UI riêng. Không bắt art environment đổi PPU chỉ vì UI; môi trường hiện dùng PPU 64, UI có thể dùng PPU 100 cùng Canvas reference 100.
- Với mỗi ảnh 9-slice: kèm số border Left/Right/Top/Bottom theo pixel PNG và vùng nội dung an toàn. Góc/ốc vít/họa tiết không nằm ở vùng sẽ bị kéo giãn.
- Không bake chữ, giá, số lượng hoặc trạng thái đang chọn vào nền panel. Logo game là ngoại lệ. Font cần đầy đủ tiếng Việt và giấy phép sử dụng; gửi file font kèm thông tin license.
- Nếu có animation UI, tách frame cùng kích thước/pivot, cung cấp FPS và loại loop. Chỉ thêm khi component tĩnh đã hoàn tất.
- Bàn giao file nguồn có layer, PNG tách, bảng màu, font/license, bảng 9-slice và một bảng tổng hợp component/state. Tên file gợi ý: UI_Button_Primary_Normal.png, UI_Panel_Guild.png, UI_Toast_Catch.png.
- Gửi bản mockup 1920×1080 cho Lake HUD, Village hội thoại/guild/shop, inventory và sổ cá; mockup chỉ là hướng dẫn bố cục, không thay các ảnh component tách.

## Bố cục cần giữ rõ

Trái trên: giờ/ngày và thông tin người chơi; cấp/XP có vùng riêng bên dưới. Phải trên: túi/menu, mục tiêu và nút sổ cá không chồng nhau. Trung tâm ưu tiên nhìn cá/QTE; thẻ kết quả nằm cao hơn tâm và banner khám phá ở vùng riêng. Dưới màn hình dành cho lời nhắc thao tác/lực quăng. Luôn có đường về làng dễ nhận ra tại Lake.
