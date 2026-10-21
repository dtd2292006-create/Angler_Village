# UI túi cá — bước đầu

## Chạy thử

Mở `Assets/_Recovery/Test.unity`, bấm Play. UI đã có trong scene dưới dạng instance của `Assets/Prefabs/UI/FishInventoryUI.prefab`; không cần chạy công cụ dựng scene.

- `I` hoặc nút **TÚI CÁ [I]**: mở/đóng túi.
- `Esc` hoặc nút đóng: đóng túi.
- Cuộn chuột hoặc kéo thanh cuộn: xem danh sách.
- Nhấp một ô: chọn cá, xem tên và thông tin.
- Túi không mở khi đang quăng, mồi còn được triển khai hoặc animation Catch đang chạy. Thu mồi và đợi xong động tác trước khi mở.

Túi chặn input di chuyển/quăng cần, không đổi khóa FishingSpot và không đổi Time.timeScale. Nếu đóng bằng chuột đang giữ, phải nhả chuột trước khi quăng tiếp. Đây chưa phải hệ thống đồng hồ/ngày đêm; việc đóng băng đồng hồ game sẽ được nối riêng khi có đồng hồ.

## Dữ liệu và phạm vi

Mỗi con cá chiếm một mục riêng theo InstanceId, mới nhất ở đầu. UI chỉ đọc PlayerInventory và đăng ký sự kiện Changed khi mở; không viết lại save, không gộp loài, không thêm sức nặng/giá bán giả. Cá thiếu dữ liệu loài vẫn hiện trong túi với nhãn dự phòng. Không bán/xóa cá trong bước này.

Khung Panel/Content, viền ItemSlot, dấu góc Selected, màu Scroll/Track và Scroll/Thumb được chuyển từ shape Figma sang uGUI native (FigmaPanelGraphic); không dùng ảnh chụp màn hình làm asset. Sprite cá lấy từ FishDefinition hiện có, giữ tỉ lệ. Đây là bố cục khởi đầu được dựng lại cho túi cá, không sao chép toàn màn hình mẫu Figma.

Font hiện dùng font built-in của Unity để hoạt động ngay; file font pixel trên Figma chưa nằm trong repository. Có thể thay Font trên các Text sau khi nhập font có đủ dấu tiếng Việt. Không xuất chữ động thành ảnh.

## Chỉnh bố cục

- Mở `Assets/Prefabs/UI/FishInventoryUI.prefab` trong Prefab Mode.
- Chọn `WindowLayer/InventoryPanel` để đổi kích thước/vị trí bằng RectTransform.
- Chỉnh `FishScroll`, `FishDetail`, tiêu đề và nút độc lập. Code không đặt lại RectTransform mỗi frame.
- `FishScroll/Viewport/Content` dùng GridLayoutGroup + ContentSizeFitter để tự xếp ô và tăng chiều cao. Chỉnh Cell Size/Spacing tại đây; các ô bên trong do layout điều khiển là có chủ đích.
- Chỉnh hình thức từng ô tại `Assets/Prefabs/UI/FishInventorySlot.prefab`. Nếu đổi cỡ ô, chỉnh Cell Size trên Content cho khớp.
- FigmaPanelGraphic giữ viền 3 px và góc 6 px theo đơn vị thiết kế của Canvas; Color đổi nền, Border/Light/Shade chỉnh viền.
- Instance trong scene có tham chiếu Hook / PlayerFishingController / PlayerAnimationController. Nếu đưa prefab vào scene khác, gắn ba tham chiếu tương ứng trên FishInventoryView.

## Cấu trúc code

- FishInventoryView: nối dữ liệu, chọn cá, hiển thị cửa sổ.
- FishInventorySlotView: hiển thị và nhận nhấp một con cá.
- InventoryWindowInput: phím I/Esc.
- GameInputBlock: quyền chặn input theo chủ sở hữu; không chứa state câu cá.
- FigmaPanelGraphic: mesh khung pixel theo component Figma.
- PlayerInputReader: chỉ bổ sung kiểm tra input UI và yêu cầu nhả chuột sau khi đóng.

Không sửa Animator, script di chuyển, dữ liệu cá hoặc định dạng save.

## Kiểm tra trong Unity

Chạy **Tools → Fishing → Validate Fish Inventory UI** để kiểm tra prefab, tham chiếu, CanvasRenderer, font, navigation và quyền chặn input. Công cụ không sửa scene.

Sau đó kiểm tra:
1. Túi rỗng mở được, thông báo rõ, đóng bằng I/Esc/X.
2. Cá đã lưu từ lần trước hiện đúng khi mở; mỗi cá là một mục riêng.
3. Chọn cá hiển thị đúng sprite/tên; cá có hình khác nhau không bị kéo méo.
4. Câu thêm cá, mở lại túi: số lượng tăng đúng một; khởi động lại vẫn còn cá.
5. Mở túi, giữ A/D: nhân vật không trượt. Đóng túi: di chuyển lại được.
6. Nút mở/đóng không quăng cần; giữ chuột khi đóng rồi nhả không làm quăng ngoài ý muốn.
7. Khi mồi đang ở ngoài hoặc QTE đang chạy, I không mở túi; câu/QTE/Catch vẫn hoạt động.
8. Danh sách nhiều hàng cuộn được, không lộ ô ra ngoài viewport; thử ở 1920×1080 và 1280×720.

Giới hạn kiểm chứng: thay đổi được chuẩn bị bên ngoài Unity Editor. Kiểm tra cú pháp C# và liên kết YAML được chạy ở môi trường làm việc; chưa xác nhận compile, Play Mode hoặc render thực tế trong Unity 6000.3.7f1.
