# Kháng cự cá + QTE — thay cơ chế độ căng trước đó

## 1. Cài file một lần

Gói tiếp nối hệ cá ăn mồi/bám Hook/Catch và bước giằng đã chạy của bạn. Mỗi file được cung cấp đầy đủ.

Thay nội dung file đang có, giữ file .meta:
- FishDefinition.cs
- FishingLineDefinition.cs
- HookController.cs
- FishStruggleController.cs
- IHookLoadProvider.cs
- FishingTensionView.cs

Thêm các file mới trong Scripts:
- FishingFightRules.cs
- FishResistanceController.cs
- FishingQteController.cs
- FishingQteInput.cs
- FishingQteRingGraphic.cs
- FishingQteView.cs

Đặt hai file trong Editor vào một thư mục có tên chính xác Editor trong Assets, ví dụ Assets/Game/Editor/Fishing/. Không đặt Editor script cạnh script runtime ngoài thư mục Editor.

Đừng chép thêm file trùng class ở thư mục khác. Lưu hết rồi Refresh một lần.
Giữ HookCatchController phiên bản có AttachedFishChanged từ bước giằng trước. PlayerFishingController, FishCatchable, FishHookPose, camera và Animator không cần thay.

**LineTensionController cũ phải ngừng chạy. Công cụ setup ở bước 3 sẽ tự tắt component này trên Hook bạn chọn.** Không chạy song song với FishResistanceController vì nó còn cơ chế đứt bắt buộc đã bỏ. Có thể gỡ component cũ sau khi setup; file nguồn cũ để lại vẫn biên dịch được nhờ giữ API tương thích.

## 2. Chỉnh asset dây

Chọn asset Line_Basic hiện có. Nếu chưa có, Project → Create → Fishing → Line Definition.
Các trường mới hoạt động độc lập với Strength cũ (trường cũ đã ẩn để giữ tương thích asset).

| Trường | Dây cơ bản để test | Ý nghĩa |
| --- | --- | --- |
| Resistance Capacity | 100 | Trần chịu kháng cự; vượt trần lâu mới đứt |
| Progress Drain Multiplier | 1 | Hệ số giảm kháng cự theo tiến độ kéo cá |
| Qte Window Multiplier | 1 | Hệ số mở rộng Good/Great |

Ví dụ dây tốt hơn: Capacity = 150, Drain Multiplier = 1.3, Window Multiplier = 1.1.
Không thay texture dây trong bước này. Chưa nối kho đồ/shop nên vẫn dùng một asset dây được gán để thử.

## 3. Tự gắn component và HUD

Sau khi Unity biên dịch:

1. Mở menu **Tools → Fishing → Setup Resistance and QTE**.
2. Kéo object Hook trong scene vào **Scene Hook**.
3. Kéo Canvas HUD đang dùng vào **HUD Canvas**. Canvas nên là Screen Space - Overlay hoặc Screen Space - Camera đã thiết lập.
4. Kéo asset Line_Basic vào **Line Asset**.
5. Bấm **Set up Hook and HUD** một lần.
6. Lưu scene bằng Ctrl+S.

Công cụ có Undo, chỉ chạy ngoài Play Mode. Nó:
- Tắt component LineTensionController cũ trên Hook.
- Gắn FishResistanceController, FishingQteController và FishingQteInput.
- Gán asset dây cho FishResistanceController.
- Gán lại Source cho thanh FishingTensionView đã có trong Canvas. Nếu chưa có thanh, tạo thanh mới phía trên giữa màn hình.
- Tạo FishingQtePanel với vòng QTE ở giữa, hơi thấp hơn tâm màn hình.
- Không đổi Animator, animation, camera, movement hoặc hình cần.

Nếu Canvas có nhiều FishingTensionView, công cụ dùng cái tìm thấy đầu tiên. Tự kiểm tra Source của thanh bạn thực sự muốn dùng.
HookController → Load Provider Component vẫn gán FishStruggleController, không gán FishResistanceController vào đó.

## 4. Chỉnh dữ liệu từng loài cá

Chọn asset FishDefinition. Các nhóm mới:

| Nhóm/trường | Giá trị đầu để test | Ý nghĩa |
| --- | --- | --- |
| Initial Fight Resistance | 80 | Kháng cự khi cá ăn mồi |
| Resistance Drop Per Unit | 8 | Mỗi unit đưa cá gần cần giảm bao nhiêu; nhân thêm hệ số dây |
| Idle Resistance Gain Per Second | 2 | Kháng cự tăng mỗi giây không thực sự thu trong nước, kể cả đang QTE |
| Good Resistance Drop | 12 | Giảm kháng cự khi Good |
| Great Resistance Drop | 28 | Giảm kháng cự khi Great |
| Fail Resistance Gain | 18 | Tăng kháng cự khi sai hoặc hết giờ |
| Good Bonus Reel | 0.15 | Unit dây thu thưởng khi Good |
| Great Bonus Reel | 0.65 | Unit dây thu thưởng khi Great |
| Qte Good Width | 0.2 | Độ rộng vùng Good cơ sở, theo phần của một vòng |
| Qte Great Width | 0.06 | Độ rộng vùng Great riêng của loài |
| Qte Sweep Seconds | 1.8 | Thời gian cơ sở chạy hết một vòng |
| Medium Qte Interval | 2.5–4 | Khoảng chờ sau QTE ở mức trung bình |
| Red Qte Interval | 1–1.8 | Khoảng chờ ở mức đỏ, nên ngắn hơn mức trung bình |

Đây là thông số kiểm tra, chưa phải cân bằng cuối cùng. Chọn cá ở đủ sâu để quan sát được nhiều nhịp.
Các giá trị mới cần kiểm tra trong Inspector của từng asset sau khi thay code, không chỉ dựa vào số mặc định trong file.

Các trường Hooked - Struggle trước đó vẫn điều khiển sức cản chuyển động/nhịp giằng; khi kháng cự giảm, ảnh hưởng này cũng giảm. Các trường Line Pull Strength/Multiplier cũ đã ẩn và không dùng để quyết định độ khó/đứt trong hệ mới.

## 5. Cách chơi thử

1. Giữ/nhả để quăng như cũ, cá ăn mồi như cũ.
2. Thanh kháng cự xuất hiện ngay khi cá mắc câu, không đợi giữ chuột.
3. Giữ chuột để thu. Khi cá tiến gần cần, thanh giảm.
4. QTE xuất hiện: việc thu tạm bị chặn dù bạn vẫn giữ chuột. **Nhả nút trái, rồi nhấp đúng lúc vạch vàng tới vùng trắng hoặc xanh.**
5. Trắng = Good; xanh = Great. Bấm ngoài vùng hoặc để hết vòng = Fail.
6. Sau một lần nhấp thành công hoặc sai, bạn có thể tiếp tục giữ chính nút trái để thu. Nếu không giữ thì dây tiếp tục nhả.

Lần giữ chuột có từ trước lúc QTE xuất hiện không được tính là đáp án. Chỉ một lần nhấp mới được chấm cho mỗi vòng. Game vẫn chạy trong QTE.

Trong QTE, cá có thể kéo dài dây trở lại, tối đa tới CastLineLimit của lượt quăng như hệ cũ. Tới giới hạn đó, dây không dài vô hạn, nhưng kháng cự vẫn tăng theo thời gian không thu. Good/Great thu thưởng vào bước vật lý kế tiếp; nếu dây đang chùng thì phần thưởng lấy bớt dây chùng trước.

## 6. Ngưỡng màu, nhịp QTE và đứt dây

Giá trị tương đối = kháng cự hiện tại / Resistance Capacity của dây.

| Tỷ lệ mặc định | Trạng thái |
| --- | --- |
| ≤ 35% | Xanh, không sinh QTE |
| > 35% và < 75% | Trung bình |
| ≥ 75% | Đỏ, QTE nhanh hơn, vùng bấm hẹp hơn và nhịp dày hơn |
| ≥ 100% | Tích thời gian quá tải, không phụ thuộc giữ hay nhả chuột |

QTE đang chạy giữ nguyên vùng bấm đã tạo; không thu hẹp vùng giữa chừng khi thanh tăng.
Độ rộng Good/Great tính từ giá trị riêng của loài, tương quan Initial Resistance với Capacity, hệ số vùng của dây và mức đỏ/trung bình. Vùng Great luôn nằm bên trong Good.
Nếu kháng cự khởi đầu của loài ≤ 35% Capacity, loài đó không sinh QTE với dây này; thả lâu vẫn có thể làm kháng cự tăng và gây đứt, nên không phải bất tử.

Great tăng khoảng chờ ẩn, có giới hạn. Fail rút ngắn khoảng chờ nếu sau Fail cá vẫn ở vùng đỏ. Khoảng chờ tối thiểu 0.6 giây, chỉ bắt đầu sau khi QTE hiện tại kết thúc. Không có QTE chồng nhau.

Trong FishResistanceController:
- Opening Grace Seconds = 3: khoảng đầu để người chơi có cơ hội gặp QTE cứu tình hình.
- Overload Duration = 2.5: tổng thời gian quá tải cần đạt để đứt.
- Overload Recovery Per Second = 1: xuống dưới trần thì thời gian quá tải giảm dần.

Không còn ngưỡng đứt bắt buộc theo cấp/sức cơ bản của cá. Dây yếu khiến tỷ lệ cao, cửa sổ nhỏ và thoát vùng đỏ khó hơn; Great tốt vẫn có thể cứu và kéo vượt cấp. Không cam kết mọi chênh lệch tùy ý đều có thể thắng: thông số vẫn cần cân bằng.
Thanh UI đầy ở 100%; kháng cự thực tế có thể cao hơn trần. Khi thời gian quá tải đang tích, thanh nhấp nháy cảnh báo. Khi ra khỏi nước, thời gian quá tải giảm và không sinh thêm QTE bơi dưới nước.

## 7. Kiểm tra

Có sẵn menu **Tools → Fishing → Run Fight Rule Checks** để chạy 10 kiểm tra trên lớp luật thực tế, không cần NUnit/Test Framework. Các kiểm tra không thay scene hoặc asset.
Tôi đã kiểm tra cú pháp bằng parser C#, nhưng chưa có Unity/trình biên dịch C# để chạy menu này tại đây.

Sau khi chạy menu, Play Mode cần kiểm tra:
- Không có cá: thu/thả như trước, không QTE hoặc thanh.
- Cá vừa mắc: thanh có giá trị ngay.
- Giữ thu: cá tiến gần, thanh giảm; nhả: dây dài trở lại và thanh tăng.
- Khi QTE hiện: giữ chuột không tiếp tục thu; phải nhả rồi nhấp mới chấm.
- Good/Great/Fail thay thanh đúng, Great thu thưởng rõ hơn và giãn nhịp.
- Mức xanh không xuất hiện thêm QTE; trở lại trung bình/đỏ có thể xuất hiện lại.
- Đổi sang dây Capacity = 250 với cùng cá 80: không QTE; đổi Drain/Window cũng có tác dụng riêng.
- Để kháng cự vượt trần đủ lâu: dây đứt, cá thoát, không Catch, không kẹt khóa di chuyển.
- Catch thành công: QTE hủy, thanh ẩn, tư thế Catch giữ nguyên.
- Hủy, tắt/bật Hook, pause/resume và câu lượt sau: không giữ khóa thu, kháng cự hoặc nhịp thưởng cũ.

Kháng cự từ tiến độ dùng khoảng cách thực tế tới đầu cần. Kéo lên rồi nhả xuống cùng khoảng sẽ hoàn lại phần thay đổi theo tiến độ, không tích giảm miễn phí.

## 8. Cấu trúc và giới hạn

| File | Trách nhiệm |
| --- | --- |
| FishingFightRules | Luật kháng cự/quá tải và chấm một vòng QTE, độc lập Unity |
| FishResistanceController | Phiên giằng, dữ liệu loài/dây, thưởng/phạt và kết quả đứt |
| FishingQteController | Lịch xuất hiện, tạo vòng, khóa thu tạm thời, nhịp thưởng/phạt |
| FishingQteInput | Đọc nút trái mới sau khi đã nhả |
| HookController | Duy nhất điều khiển vị trí, vận tốc, chiều dài và thu thưởng |
| FishStruggleController | Nhịp sức cản và gia tốc bơi khi mắc câu |
| FishingTensionView | Tên component cũ được giữ, nguồn mới là FishResistanceController |
| FishingQteRingGraphic / View | Vẽ vòng, vạch và phản hồi; không quyết định kết quả |
| Hai Editor script | Setup scene/HUD và kiểm tra luật, không vào bản build |

Vòng QTE hiện là đồ họa dựng bằng mesh UI để test, chưa dùng bộ art pixel. Có thể thay phần hiển thị sau mà không đổi luật.
Chưa nối kho đồ/shop hoặc trừ dây/mồi khi đứt; chưa làm âm thanh hay thông báo mất đồ. Đứt đi qua nhánh hủy, không gọi FishLanded. Các asset dữ liệu không bị sửa khi chơi.

Không thay Animator bằng code, không ẩn cần Idle/Walk, không sửa hoạt ảnh Catch.
