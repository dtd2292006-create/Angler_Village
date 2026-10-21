# Bước 1 — Cá vào túi và lưu/load

Mục tiêu: câu thành công → cộng một con cá riêng vào túi → lưu → dừng/mở lại game vẫn còn. Chưa làm giao diện túi, bán/giao cá, tiền, trang bị hoặc giới hạn sức chứa.

## 1. Đưa script vào project

Thay toàn bộ nội dung hai file hiện có, giữ file .meta:
- FishDefinition.cs
- HookCatchController.cs

Hai thay đổi chỉ thêm mã loài và mã giao dịch câu thành công. Không thay thông số cá, QTE, kháng cự, chuyển động hoặc Animator.

Thêm các file còn lại trong Scripts:
- FishInventoryData.cs
- FishInventory.cs
- FishCatalog.cs
- PlayerInventory.cs
- PlayerSaveFileStore.cs
- PlayerSaveController.cs
- FishCatchInventoryReceiver.cs

Đặt hai file trong Editor vào một thư mục có tên Editor bên trong Assets:
- FishInventoryTools.cs
- FishInventorySelfCheck.cs

Không để bản .cs trùng class ở thư mục khác. Lưu tất cả rồi cho Unity Refresh một lần.

## 2. Tạo danh mục cá

Trong Project, chuột phải → Create → Fishing → Fish Catalog.
Đặt tên asset FishCatalog_Main, có thể để trong Assets/Game/Data/.

Danh mục này giúp tìm lại tên và sprite từ mã loài được lưu. Nó dùng lại các asset FishDefinition hiện có, không tạo lại cá hay chỉnh thông số.
ID được lấy từ GUID của asset trong file .meta: đổi tên/di chuyển asset trong Unity không đổi ID. Đừng xóa .meta của dữ liệu cá.

## 3. Setup scene

Ngoài Play Mode mở Tools → Fishing → Setup Fish Inventory.

| Ô | Gán |
| --- | --- |
| Scene Hook | HookCatchController trên Hook đang dùng trong scene |
| Fish Catalog | Asset FishCatalog_Main vừa tạo |

Bấm Set up Fish Inventory, rồi Ctrl+S lưu scene.

Công cụ sẽ:
- Quét các FishDefinition trong Project, gán mã ổn định và điền danh mục cá.
- Tạo hoặc dùng lại một root riêng tên PlayerData, chứa PlayerInventory và PlayerSaveController.
- Gắn FishCatchInventoryReceiver lên Hook và nối về PlayerData.
- Không dựng/di chuyển UI hoặc thay Animator.

PlayerData phải là object root riêng, không đặt làm con của Player hoặc Hook. Nó tồn tại qua chuyển scene. Nếu các scene khác có một bản PlayerData mới, bản trùng bị loại khi chơi và receiver tìm bản chính còn tồn tại.

Khi thêm FishDefinition mới về sau, chọn FishCatalog_Main và bấm Refresh Fish List + Stable IDs trong Inspector trước khi Play. Không cần chạy lại toàn bộ setup nếu không đổi scene/Hook.

## 4. Kiểm tra lần đầu

1. Play. Console phải báo Player data ready và đường dẫn save.
2. Chọn PlayerData, mở PlayerInventory → Runtime Debug, xem Total Fish và Last Catch Sequence. Trong Play object này có thể nằm dưới scene DontDestroyOnLoad.
3. Câu thành công một con cá. Console phải báo Da them ca vao tui. Total Fish tăng đúng 1.
4. Câu thêm con cùng loài: tổng tăng thêm 1, không gộp mất thông tin từng con.
5. Dừng Play, Play lại: tổng cá phải được nạp lại từ file.

Để xem danh sách, bấm chuột phải lên tiêu đề component PlayerInventory → Log Fish Inventory. Console in tên loài, mã từng con, thứ tự và giờ bắt UTC.
Ngoài Play Mode các số Runtime Debug có thể về giá trị trong scene; hãy kiểm tra trong Play sau khi load, không dựa vào số ngoài Play.

Chỉ FishLanded thực tế mới cộng cá. Phát thử animation Catch bằng tay, thu mồi trống, hủy câu hoặc đứt dây không được cộng cá.

## 5. File lưu

PlayerSaveController có mục chuột phải Log Save Location để in đúng đường dẫn trên máy bạn.
Đường dẫn dùng Application.persistentDataPath/Saves/slot_01/player.json, không ghi vào Assets.

- player.json: bản lưu chính.
- player.json.bak: bản dự phòng, thường là trạng thái trước lần lưu gần nhất.
- player.json.tmp: file tạm trong lúc ghi.

Mỗi lần nhận cá hợp lệ, dữ liệu thay đổi và lưu ngay. Nếu ghi lỗi, cá vẫn trong RAM, Dirty = true và hệ thống thử lại sau 5 giây; cũng thử khi app pause/thoát. Vẫn có thể mất phần chưa ghi được nếu tiến trình bị tắt khi lỗi ghi chưa được giải quyết.
Khi đọc bản chính lỗi, hệ thống thử bản dự phòng; có thể quay về lần lưu trước và sẽ báo cảnh báo. Nếu cả hai lỗi, chỉ thử file tạm hoàn chỉnh nếu có. Không đọc được bản nào thì không tự tạo dữ liệu rỗng ghi đè file cũ.
Phiên bản save mới hơn code hiện tại bị từ chối, không âm thầm hạ xuống bản backup cũ.

## 6. Mỗi con cá lưu gì?

| Dữ liệu | Mục đích |
| --- | --- |
| InstanceId | Mã riêng của con cá, cũng là mã giao dịch nhận cá để tránh cộng trùng |
| SpeciesId | Mã loài ổn định, tra về FishDefinition qua FishCatalog |
| CatchSequence | Thứ tự bắt tăng dần và được lưu, phù hợp kiểm tra nhiệm vụ cần cá mới |
| CaughtAtUtcTicks | Giờ thực tế bắt cá để hiển thị/debug; chưa phải đồng hồ ngày/đêm trong game |

Sau này nhiệm vụ nhận cá mới có thể ghi nhận LastCatchSequence tại lúc nhận nhiệm vụ, chỉ chấp nhận cá có số lớn hơn. Không dùng đồng hồ máy tính làm thứ tự bắt vì giờ máy có thể thay đổi.
Chưa có cân nặng/kích thước/chất lượng ngẫu nhiên: hiện chưa có luật sinh các thuộc tính này nên không ghi số giả vào item. Có thể bổ sung dữ liệu theo luật sẽ chốt sau.
Cá của một loài không còn trong catalog vẫn giữ trong save, được log là Unknown species; không bị tự xóa.

## 7. Các kiểm tra sẵn có

Menu Tools → Fishing → Run Inventory Rule Checks chạy 10 tình huống trên model và file store thật: cộng trùng, nhiều cá cùng loài, thứ tự bắt, load lại, JSON giữ đúng long timestamp, backup, file hỏng, phiên bản mới và file tạm.
Các kiểm tra dùng thư mục tạm riêng có GUID và tự dọn; không đụng file lưu của người chơi.

Môi trường tạo gói đã kiểm tra cú pháp C# và thay đổi so với code đang có, nhưng không có Unity/C# compiler để chạy các kiểm tra tại đây. Cần chạy menu và test vòng câu trong project.

## 8. Trách nhiệm các phần

| Phần | Nhiệm vụ |
| --- | --- |
| FishDefinition | Dữ liệu loài, thêm SpeciesId |
| HookCatchController | Phát kết quả câu thành công, thêm mã giao dịch duy nhất |
| FishInventoryData / FishInventory | Bản ghi cá và luật thêm/load/không cộng trùng, không phụ thuộc scene |
| FishCatalog | Tra ID về asset tên/sprite |
| PlayerInventory | Sở hữu túi đang chạy và phát sự kiện Changed |
| PlayerSaveController | Load lúc khởi động, lưu khi thay đổi, thử lại khi lỗi |
| PlayerSaveFileStore | JSON, file tạm và backup |
| FishCatchInventoryReceiver | Nhận FishLanded rồi yêu cầu thêm cá vào túi |
| Editor tools | Setup/catalog và kiểm tra; không vào bản build |

Túi hiện chỉ chứa cá, không giới hạn số lượng, chưa có bán/xóa/giao cá hoặc UI inventory. Dây/mồi chưa bị trừ khi đứt. Bước tiếp theo sau khi xác nhận lưu/load đúng mới dựng giao diện đọc dữ liệu này.
