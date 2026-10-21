# Build bản Windows và ảnh minh chứng

## 1. Nhận bản project

Thoát Play Mode, lưu các thay đổi đang làm. Trong GitHub Desktop, chọn nhánh `codex/fish-inventory-ui`, Fetch origin rồi Pull origin. Mở Unity 6000.3.7f1 và đợi import/compile xong. Console không được còn lỗi đỏ trước khi build.

Chọn `Tools > Fishing > Validate Consumables` ngoài Play Mode. Công cụ dùng dữ liệu thử riêng, kiểm tra trừ đồ, không âm số lượng, bỏ trang bị khi cạn và lưu/tải trạng thái hết đồ; không sửa hồ sơ chơi. Kết quả PASS chỉ áp dụng các kiểm tra này, không thay thế chơi thử toàn bộ.

## 2. Kiểm tra tiêu hao trong Play Mode

- Ghi số mồi/dây trong túi trước mỗi lượt. Chỉ khi cá cắn và mắc câu thành công mới trừ đúng 1 mồi; quăng/thu khi chưa có cá cắn, hủy lấy đà hoặc quăng ngoài giới hạn không mất mồi.
- Thu mồi về khi chưa có cá cắn không mất mồi. Cá đã cắn rồi thoát thì mồi vẫn đã mất. Thu dây bình thường không mất dây.
- Dùng mồi cuối vẫn được cá cắn, QTE và nhận cá. Sau khi kết thúc lượt, phải mua/trang bị thêm mồi trước lượt tiếp.
- Khi đứt dây, chỉ mất 1 dây đang dùng; không trừ thêm mồi lần thứ hai. Nếu còn dây cùng loại, vẫn giữ trang bị đó; nếu hết thì bỏ trang bị.
- Thoát rồi Tiếp tục: số lượng đã tiêu hao giữ nguyên. Không cấp lại bộ khởi đầu.
- Hết dây cơ bản: trở về cửa hàng và mua mục miễn phí. Hết mọi loại mồi và không đủ xu mua mồi cơ bản: mục mồi có giá cứu trợ 0 xu; chỉ nhận 1 mỗi lần khi đang hết, không nhận chồng. Mua/nhận xong mở túi I để trang bị.
- Thử một lượt thành công, một lượt thất bại, giao cá, bán cá và nhận lại ủy thác.

## 3. Tạo bản Windows

1. Mở `File > Build Profiles`.
2. Chọn nền tảng Windows; nếu chưa có, `Add Build Profile > Windows > Add Build Profile`, rồi Switch Profile nếu cần. Nếu Unity báo thiếu module, cài Windows Build Support cho đúng bản Editor từ Unity Hub.
3. Mở Scene List. Giữ đúng thứ tự và bật cả ba scene:

| Thứ tự | Scene |
| ---: | --- |
| 0 | Assets/Scenes/MainMenu.unity |
| 1 | Assets/Scenes/Village.unity |
| 2 | Assets/Scenes/Lake.unity |

Không đưa scene `_Recovery/Test` hoặc SampleScene vào bản nộp. Nếu profile bật Override Global Scene List, kiểm tra danh sách riêng của profile cũng đúng ba scene trên.

4. Chọn Architecture Intel 64-bit/x86_64 tùy nhãn hiển thị trong Editor.
5. Trong Player Settings, kiểm tra Product Name là Angler_Village và Default Screen Width/Height là 1920/1080. Có thể dùng Windowed hoặc Fullscreen Window; thử thực tế để UI không bị cắt. Giữ scripting backend đang hoạt động của project; không cần chuyển sang IL2CPP chỉ để nộp bài.
6. Bản thử có thể bật Development Build để đọc lỗi. Bản nộp cuối tắt Development Build, Script Debugging và Autoconnect Profiler.
7. Bấm Build; chọn thư mục riêng như `Builds/Angler_Village_Windows`, đặt tên `Angler_Village.exe`. Không đặt đầu ra trong Assets.
8. Mở exe ngoài Unity và kiểm tra lại menu, chuyển scene, âm thanh, tiêu hao, giao/bán cá, thoát và Tiếp tục.
9. Nén toàn bộ thư mục đầu ra thành `Angler_Village_Windows.zip`; giữ exe, UnityPlayer.dll, thư mục `_Data` và mọi file/thư mục phụ thuộc Unity tạo. Không gửi riêng exe.

Nếu cần nộp source project riêng, nén Assets (kèm .meta), Packages, ProjectSettings và README. Không cần Library, Temp, Logs, Obj. Bản source và bản chạy là hai gói khác nhau. Đừng xóa save để chụp ảnh; sao lưu cả thư mục slot_01 trước nếu cần thử Chơi mới. Build dùng cùng Company/Product có thể đọc cùng save trên máy phát triển; build không tự bảo đảm một hồ sơ mới.

## 4. Các ảnh nên chụp

Chụp từ Game View hoặc bản exe, ưu tiên cùng kích thước 1920×1080; tránh Scene View có Gizmos. Một scene có nhiều trạng thái nên cần nhiều ảnh.

| Tên ảnh gợi ý | Scene | Nội dung và mục đích |
| --- | --- | --- |
| 01_MainMenu | MainMenu | Chơi mới, Tiếp tục, Cài đặt, Thoát |
| 02_Settings | MainMenu hoặc Lake | Volume/mute nhạc và SFX, thiết lập hiển thị |
| 03_Village | Village | Nhân vật, guild, cửa hàng và biển ra hồ |
| 04_Guild | Village | Hội thoại/chứng nhận hoặc danh sách ủy thác |
| 05_Commission | Village | Mục tiêu, thời hạn và phần thưởng một ủy thác |
| 06_Shop | Village | Trang bị mua, giá và mục thu mua cá |
| 07_Lake | Lake | Toàn cảnh hồ và HUD với thông tin thực tế |
| 08_FishingQTE | Lake | Cá đang mắc câu, vòng QTE và vùng mục tiêu |
| 09_CatchSuccess | Lake | Kết quả bắt cá thành công, tên/hình cá |
| 10_FishingFailure | Lake | Thông báo thất bại hoặc đứt dây |
| 11_Inventory | Lake hoặc Village | Cá sở hữu, trang bị và số mồi/dây |
| 12_FishJournal | Lake hoặc Village | Loài đã khám phá và ô chưa khám phá |
| 13_CommissionCompleted | Village | Giao cá thành công, nhận xu/XP |
| 14_CommissionFailed | Village hoặc Lake | Ủy thác hết hạn/thất bại, cá còn giữ |

Có thể thêm hai ảnh trước/sau cùng một lượt để minh chứng mồi giảm đúng 1; hai ảnh trước/sau đứt dây để minh chứng dây giảm. Ghi số lượng rõ ràng, không chỉnh số bằng ảnh. Chỉ mô tả nâng cấp nhiều cấp và mở khu vực mới là hướng phát triển khi chưa triển khai.

## 5. Tài liệu tham khảo

- Scene List: https://docs.unity.com/en-us/engine/6000.0/manual/building-and-publishing/build-settings/build-profile-scene-list
- File đầu ra Windows: https://docs.unity.com/en-us/engine/6000.0/manual/platform-specific/windows/standalone-binaries

Hướng dẫn áp dụng cho Unity 6; nhãn giao diện có thể khác nhẹ giữa các bản vá. Chỉ ghi build/kiểm thử thành công khi đã thực hiện và có kết quả trên máy chạy Unity.
