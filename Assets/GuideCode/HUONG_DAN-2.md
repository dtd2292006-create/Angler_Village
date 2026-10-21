# Sửa kháng cự theo khoảng cách, giữ phạt theo thời gian

## Cài đặt

Thay toàn bộ nội dung bốn script runtime hiện có bằng file tương ứng trong Scripts:
- FishingFightRules.cs
- FishResistanceController.cs
- FishDefinition.cs
- HookController.cs

Thay FishingFightSelfCheck.cs trong thư mục Editor bằng bản trong Editor của gói này. File này cần thay vì hàm khởi tạo lớp luật đã đổi theo mô hình mới.
Không thêm bản .cs trùng class và không xóa .meta. Lưu hết rồi cho Unity Refresh một lần.
Không cần gắn component mới hoặc chạy lại Setup. Giữ bản sửa CanvasRenderer trên Ring đang dùng.

## Dữ liệu cần kiểm tra

Trong asset FishDefinition, nhóm Fight - Resistance:

| Trường | Giá trị để thử | Ý nghĩa |
| --- | --- | --- |
| Initial Fight Resistance | 80 | Sức chống cự cơ sở ở giới hạn xa nhất của lượt quăng |
| Distance Resistance Exponent | 1 | Đường giảm theo khoảng cách: 1 tuyến tính; >1 giảm nhanh hơn khi tiến gần; <1 giữ cao lâu hơn |
| Idle Resistance Gain Per Second | 2 | Tăng sức chống cự còn lại mỗi giây không thực sự thu trong nước, kể cả lúc QTE |

Resistance Drop Per Unit cũ đã ẩn và không dùng trong mô hình mới. Giá trị cũ vẫn lưu để không phá asset hoặc API, nhưng không cần chỉnh nữa.
Các trường QTE Good/Great/Fail, nhịp QTE và dữ liệu bơi giữ nguyên giá trị đang có của bạn.

Trong asset dây, Progress Drain Multiplier tiếp tục làm dây mạnh giảm kháng cự nhanh hơn theo tiến độ: nó nhân vào Exponent. Ví dụ cá Exponent = 1, dây Multiplier = 1.3 → đường giảm thực tế có số mũ 1.3. Không cần thay file dữ liệu dây.

## Công thức

1. Tính khoảng cách thực tế Hook tới RodTip, không dùng số dây đang nhả.
2. Chuẩn hóa từ điểm bắt cá (CatchDistance) đến CastLineLimit cố định của lượt quăng, được giá trị t từ 0 đến 1.
3. Hệ số khoảng cách = t ^ (Exponent của cá × Progress Drain Multiplier của dây).
4. Kháng cự hiển thị = sức chống cự còn lại × hệ số khoảng cách.

Sức chống cự còn lại ban đầu bằng Initial Fight Resistance; tăng bởi thời gian không thu và QTE Fail; giảm bởi Good/Great. Không có giảm tự động chỉ vì đang giữ thu.

Ví dụ Exponent thực tế = 1, sức còn lại = 80:
- Ở giới hạn xa nhất: 80.
- Ở giữa đoạn từ điểm bắt tới giới hạn xa nhất: 40.
- Tại điểm bắt: 0, dù trước đó đã tích phạt.

Good/Great Resistance Drop và Fail Resistance Gain giờ thay đổi sức chống cự còn lại bên trong công thức. Vì vậy hiệu ứng trên thanh cũng được nhân hệ số khoảng cách. Ví dụ ở hệ số 0.5, Great trừ 28 sức thì thanh giảm trực tiếp 14; phần thu thưởng còn giúp giảm thêm do cá tiến gần. Hình phạt thời gian cũng nhân hệ số khoảng cách, nên cùng một giây không thu sẽ nguy hiểm hơn khi cá còn xa.

Khoảng cách xa nhất dùng CastLineLimit, không dùng CurrentLineLength hoặc vị trí cá lúc vừa ăn mồi. Nếu dùng chiều dài đang thu ngắn lại, tỷ lệ có thể luôn gần 1 khi dây căng và thanh sẽ không giảm đúng.
Nếu cá ăn mồi ở gần, thanh khởi đầu thấp hơn cùng loài ăn mồi ở xa. Để so sánh hai lần câu, hãy dùng sức quăng, vị trí cá và loại dây tương đương.

## Điều giữ nguyên

- Không thu: tăng sức còn lại theo thời gian, kể cả khi cá đã tới giới hạn dây và không thể ra xa thêm.
- Cá kéo ra xa: thanh tăng thêm do hệ số khoảng cách tăng.
- Giữ thu nhưng cá không tiến gần: thanh không tự giảm.
- QTE Good/Great/Fail và thu thưởng vẫn hoạt động.
- Các vùng xanh/trung bình/đỏ, nhịp thưởng/phạt ẩn, giới hạn không chồng QTE vẫn giữ.
- Dây đứt khi kháng cự vượt trần đủ lâu; không có đứt bắt buộc theo cấp cá.
- Pause không tăng phạt. Ngoài nước không cộng phạt không thu và thời gian quá tải được giảm.
- Animator, Catch, vòng QTE, input và tư thế cá không đổi.

## Kiểm tra

Trong Unity chạy Tools → Fishing → Run Fight Rule Checks. Bản mới có 14 tình huống, gồm giữ chuột tại chỗ không tự giảm, tới điểm bắt luôn về 0 dù có phạt, và giảm nhanh hơn với đường cong mạnh hơn.
Menu này được cung cấp để chạy trong Unity; tại môi trường tạo file chưa có Unity/C# compiler để thực thi. Đã kiểm tra cú pháp C# và đường tích hợp.

Trong Play Mode, mở Runtime Debug của FishResistanceController:
- Remaining Strength: sức chống cự còn lại, chỉ thay bởi phạt thời gian và kết quả QTE.
- Distance Factor: hệ số theo khoảng cách.
- Resistance: tích của hai số trên.

Thử giữ thu và so khoảng cách giảm với thanh; nhả để cá kéo lùi; xử lý Good/Great; để yên chịu phạt; câu tới cuối. Thanh phải về 0/ẩn khi bắt được cá.
