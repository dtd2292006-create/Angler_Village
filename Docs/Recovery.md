# Khôi phục project và kiểm tra file

## Phiên bản đồng bộ

Đợt sửa này đồng bộ main với bộ project đầy đủ trên codex/fish-inventory-ui. Không force push và không xóa lịch sử. Nhánh backup/main-before-repair-20261022 giữ main trước đồng bộ. Nhánh recovery/before-spawn-fixes giữ commit 718f3e2 trước hai đợt sửa điểm về làng, gồm UI, âm thanh và tiêu hao mồi khi cá cắn.

## Kết quả rà soát GitHub

Đã đối chiếu 1.833 file Git theo cây repository, kiểm tra 915 GUID asset, dấu conflict trong các file văn bản đọc được và cú pháp 105 file C# trước khi bổ sung công cụ kiểm tra. Sáu prefab trong log lỗi Village đều tồn tại trong bộ project đầy đủ với đúng meta GUID: VillagePanelUI, VillageInteractionUI, JourneyHudUI, FishJournalUI, VillageShellUI và GameAudio. Không tìm thấy GUID asset trùng. Các GUID package Unity không được coi là asset bị thiếu trong Assets.

Commit 28eee99 chỉ sửa VillagePlayerEntry.cs và thêm hai trường serialized trong Village.unity; không xóa sáu prefab. Kết quả này không xác định được trạng thái merge/stash trên máy đã báo lỗi.

Ba file Apple resource fork và scene recovery cũ không phải UTF-8 không được kiểm tra dưới dạng văn bản. Đây là kiểm tra tĩnh, chưa phải Unity import/compile, Play Mode hoặc chạy bản build. Nhánh dự phòng chưa được xác nhận là bản chạy ổn trên máy người chơi.

## Nếu project trên máy đang conflict

1. Đóng Unity và sao lưu nguyên thư mục project, gồm lịch sử Git và thay đổi cục bộ. Không Discard, xóa stash hoặc chép đè bản duy nhất.
2. Sửa GitHub không tự giải quyết một merge đang dở trên máy. Không Pull thêm vào working tree đang conflict.
3. Để kiểm tra bộ nguồn GitHub độc lập: tải Code > Download ZIP của main, giải nén vào thư mục mới và mở bằng Unity 6000.3.7f1. Không chép riêng Village.unity vào một bộ source cũ.
4. Chọn Tools > Fishing > Validate Project Files ngoài Play Mode. Nếu có lỗi, giữ nguyên nội dung Console.
5. Nếu bản mới đủ file mà bản cũ vẫn thiếu prefab, cần đối chiếu Assets, Packages và ProjectSettings của chính bản cũ; các chỉnh sửa chưa push không thể phục hồi từ GitHub.
6. Khi bản mới đã chạy đúng, chuyển riêng những thay đổi cục bộ cần giữ từ bản sao lưu. Không chọn hàng loạt một phía cho các scene/prefab đang conflict.

GameBuild được thêm vào .gitignore cho các file chưa được Git theo dõi. Không xóa bản build đang có trên máy. Nếu file build đã được commit ở máy riêng, .gitignore không tự bỏ chúng khỏi lịch sử/index.
