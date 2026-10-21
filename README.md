# Angler Village

Unity **6000.3.7f1**, Universal 2D, PC. Game câu cá pixel.

## Mở dự án

- Scene vào game: `Assets/Scenes/MainMenu.unity`.
- Scene hồ: `Assets/Scenes/Lake.unity`.
- Bản scene gốc giữ nguyên: `Assets/_Recovery/Test.unity`.
- Village sẽ được dựng riêng sau khi có map và NPC.

A/D di chuyển; giữ/thả chuột trái lấy đà và quăng cần; giữ chuột trái thu dây; nhấp chuột trái thực hiện QTE; I mở túi; Esc mở pause hoặc đóng lớp cửa sổ trên cùng.

Hiện có hệ thống câu, cá, kháng cự, QTE, đứt dây, Catch, inventory cá/trang bị/mồi và lưu hồ sơ. Đợt bổ sung Lake/MainMenu thêm HUD, pause, settings, đồng hồ/ngày đêm, hồi sinh cá và khung audio chưa gắn clip. Chưa chạy Unity/Play/build cho đợt bổ sung này. Chưa có kinh tế Village, shop/guild/ủy thác, tiêu hao mồi hoặc trừ kho dây.

## Hướng dẫn chỉnh nội dung

- [Lake, Main Menu và phạm vi](Docs/Lake_MainMenu.md)
- [Thêm/bớt cá, cần, dây, mồi](Docs/Inventory_Content.md)
- [Gắn clip và mixer](Docs/Audio_Setup.md)

Giữ script theo nhiệm vụ, giữ Animator transitions và cho phép chỉnh bố cục UI trong Inspector. Dữ liệu nội dung dùng ID ổn định; không đổi ID hoặc xóa meta của món đã lưu trong hồ sơ.
