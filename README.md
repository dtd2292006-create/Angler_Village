# Angler Village

Game câu cá 2D phong cách pixel cho PC, phát triển bằng Unity **6000.3.7f1** với Universal 2D. Người chơi nhận ủy thác tại làng, chuẩn bị cần/dây/mồi, ra hồ câu cá, rồi giao cá hoặc bán để nhận xu và kinh nghiệm. Sổ cá lưu các loài đã khám phá.

## Mở project

1. Trong Unity Hub, chọn **Add project from disk** và mở thư mục chứa `Assets`, `Packages`, `ProjectSettings`.
2. Dùng Unity **6000.3.7f1**, đợi quá trình import và biên dịch hoàn tất.
3. Mở `Assets/Scenes/MainMenu.unity`.
4. Chọn **Chơi mới** để bắt đầu tại làng hoặc **Tiếp tục** để tải hồ sơ đã lưu.

Editor mặc định bắt đầu Play từ Main Menu. Chọn `Tools > Fishing > Play Mode > Start From Current Scene` nếu cần thử scene đang mở. Danh sách scene của bản build nằm tại `ProjectSettings/EditorBuildSettings.asset` theo thứ tự MainMenu, Village, Lake.

## Điều khiển

| Thao tác | Phím hoặc chuột |
| --- | --- |
| Di chuyển | A / D |
| Lấy đà và quăng cần tại điểm câu | Giữ rồi thả chuột trái |
| Thu dây | Giữ chuột trái ngoài lượt QTE |
| Xác nhận QTE | Nhấp chuột trái khi kim tới vùng mục tiêu |
| Nói chuyện NPC | E khi đứng gần NPC |
| Túi đồ | I |
| Sổ cá | J |
| Tạm dừng hoặc đóng cửa sổ trên cùng | Esc |

Tại làng, tới guild nhận chứng nhận và ủy thác đầu tiên, sau đó nhận dây/mồi khởi đầu tại cửa hàng. Đi tới biển bên phải làng để ra hồ. Đi tới mép trái hồ để trở về cạnh biển đó. Cửa hàng mua trang bị và thu mua cá; cá dùng cho ủy thác phải được câu theo điều kiện của nhiệm vụ.

## Các scene

| File | Chức năng |
| --- | --- |
| `Assets/Scenes/MainMenu.unity` | Menu bắt đầu, tải hồ sơ, cài đặt và thoát |
| `Assets/Scenes/Village.unity` | Làng, NPC, guild, cửa hàng, hướng dẫn đầu game |
| `Assets/Scenes/Lake.unity` | Hồ câu, QTE, sức kháng cự, HUD và kết quả câu |
| `Assets/_Recovery/Test.unity` | Scene phát triển gốc, không nằm trong luồng chơi chính |

## Cấu trúc thư mục

| Thư mục | Nội dung |
| --- | --- |
| `Assets/Scripts/` | Điều khiển nhân vật, hook, dây câu và lưu dữ liệu |
| `Assets/Scripts/Fishing/` | Dữ liệu cá, mồi, sức kháng cự và QTE |
| `Assets/Scripts/Inventory/` | Kho trang bị, catalog và liên kết trang bị với nhân vật |
| `Assets/Scripts/Progression/` | Hồ sơ, kinh nghiệm, ủy thác và giao dịch |
| `Assets/Scripts/Game/` | Chuyển scene, thời gian, tạm dừng và cài đặt |
| `Assets/Scripts/UI/` | Túi đồ, sổ cá, menu, HUD và các cửa sổ |
| `Assets/Scripts/Audio/` | Nguồn âm, sự kiện âm thanh và bước chân |
| `Assets/Data/` | Catalog cá, trang bị và cấu hình ủy thác/cửa hàng |
| `Assets/Prefabs/` | Đối tượng và giao diện tái sử dụng |
| `Assets/Art/UI/Figma/` | Hình ảnh, font và thông tin nguồn giao diện |
| `Assets/Audio/` | AudioMixer, clip và giấy phép âm thanh |
| `Assets/Editor/` | Công cụ thiết lập và kiểm tra trong Unity Editor |
| `Packages/` | Danh sách package và phiên bản phụ thuộc |
| `ProjectSettings/` | Phiên bản Unity và cấu hình project |
| `Docs/` | Hướng dẫn cấu hình từng hệ thống |

## File cấu hình thường dùng

- Cá: `Assets/Data/Inventory/FishCatalog_Main.asset` và các `FishDefinition` mà catalog tham chiếu.
- Trang bị khởi đầu: `Assets/Data/Equipment/EquipmentCatalog_Main.asset`.
- Giá cửa hàng và danh sách ủy thác: `Assets/Data/Village/VillageContent_Main.asset`.
- Ủy thác: `Assets/Data/Village/Commission_FirstCatch.asset`, `Commission_LakeSupply.asset`.
- Giao diện: các prefab trong `Assets/Prefabs/UI/`; chỉnh vị trí/kích thước bằng RectTransform ngoài Play Mode.
- Âm thanh: `Assets/Prefabs/GameAudio.prefab`, `Assets/Audio/GameAudio.mixer`.

Giữ ID của loài/vật phẩm ổn định để hồ sơ đã lưu tiếp tục nhận diện dữ liệu. Khi chuyển asset giữa các máy, giữ kèm file `.meta` để tránh mất tham chiếu Unity.

## Lưu dữ liệu

Hồ sơ đặt tại `Application.persistentDataPath/Saves/slot_01/player.json`, gồm cá sở hữu, trang bị, tiền, kinh nghiệm, ủy thác, đồng hồ và tiến độ khám phá. Trên component `PlayerSaveController`, dùng **Log Save Location** để xem đường dẫn thực tế. Cài đặt âm thanh và hiển thị được lưu bằng PlayerPrefs, tách khỏi hồ sơ chơi.

## Âm thanh

Bản âm thanh gọn có nhạc cho Menu/Làng/Hồ, môi trường, bước chân, thao tác câu, QTE và UI. Ba bản nhạc dùng đoạn lặp 24 giây để giảm dung lượng. Mixer tách Master, Music và SFX; cài đặt có volume/mute riêng cho nhạc và hiệu ứng. Nguồn và giấy phép nằm trong `Assets/Audio/Licenses/`.

## Build và ảnh minh chứng

Xem [Hướng dẫn build bản Windows và chụp ảnh](Docs/Build_Submission.md).

## Tài liệu chi tiết

- [Luồng làng, guild và cửa hàng](Docs/Village_FirstTrip.md)
- [Scene hồ và Main Menu](Docs/Lake_MainMenu.md)
- [Catalog cá và trang bị](Docs/Inventory_Content.md)
- [Thiết lập âm thanh](Docs/Audio_Setup.md)
- [Nguồn giao diện Figma](Docs/Figma-UI.md)

## Phạm vi phiên bản

Đã có luồng Menu → Làng → Hồ → giao/bán cá, QTE, đứt dây, túi đồ, sổ cá, ủy thác và lưu hồ sơ. Chỉ khi cá cắn và mắc câu thành công mới trừ 1 mồi; quăng rồi thu về khi chưa có cá cắn không mất mồi. Sau khi cá đã cắn, dù bắt được cá hay cá thoát cũng không hoàn lại mồi và không trừ thêm lần nữa. Dây chỉ mất 1 đơn vị khi đứt, thu dây bình thường không mất dây. Lượt dùng mồi cuối vẫn hoàn thành bình thường; lần quăng sau cần thêm mồi. Cửa hàng cấp dây cơ bản miễn phí khi hết loại dây đó; nếu hết mọi loại mồi và không đủ xu mua mồi cơ bản, có thể nhận 1 mồi cứu trợ miễn phí. Nhận/mua xong mở túi [I] để trang bị. Mở thêm khu vực và nâng cấp trang bị đa cấp là hướng phát triển. Âm thanh phụ cho giao dịch/ủy thác chưa được nối trong bản gọn. Kiểm tra tĩnh không thay thế Unity import/compile, Play Mode hoặc kiểm thử bản build; chưa có kết quả xác nhận các bước này cho bản cập nhật hiện tại.
