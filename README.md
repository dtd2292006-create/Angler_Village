# Angler Village

Unity **6000.3.7f1**, Universal 2D, PC. Game câu cá pixel.

## Mở dự án

- Scene vào game: `Assets/Scenes/MainMenu.unity`.
- Scene hồ: `Assets/Scenes/Lake.unity`.
- Bản scene gốc giữ nguyên: `Assets/_Recovery/Test.unity`.
- Scene làng: `Assets/Scenes/Village.unity` (map và NPC do bạn dựng). Chơi mới bắt đầu tại House.

A/D di chuyển; giữ/thả chuột trái lấy đà và quăng cần; giữ chuột trái thu dây; nhấp chuột trái thực hiện QTE; E nói chuyện NPC; I mở túi; J mở sổ cá; Esc mở pause hoặc đóng lớp cửa sổ trên cùng.

Hiện có câu cá/QTE/đứt dây/Catch, inventory và lưu hồ sơ; MainMenu, Village và Lake đã nối scene. Village có hướng dẫn đầu game, chứng nhận/cấp/XP, ủy thác đầu tiên, cửa hàng trang bị + thu mua cá và bộ dây/mồi miễn phí một lần. Sổ cá kiểu Pokédex lưu khám phá riêng với cá đang sở hữu. Có ngày đêm và khung audio chưa có clip. Chưa thêm tiêu hao mồi hoặc trừ kho dây khi đứt.

Chưa chạy Unity import/compile/Play/build cho đợt Village. Dùng nhánh `codex/fish-inventory-ui` để tiếp tục làm chung; main và các asset Village mới đã được kết hợp vào nhánh này.

## Hướng dẫn chỉnh nội dung

- [Village, hướng dẫn đầu game, guild/shop/sổ cá](Docs/Village_FirstTrip.md)
- [Lake, Main Menu và phạm vi](Docs/Lake_MainMenu.md)
- [Thêm/bớt cá, cần, dây, mồi](Docs/Inventory_Content.md)
- [Brief bộ UI cho team art](Docs/UI_Art_Brief.md)
- [Gắn clip và mixer](Docs/Audio_Setup.md)

Giữ script theo nhiệm vụ, giữ Animator transitions và cho phép chỉnh bố cục UI trong Inspector. Dữ liệu nội dung dùng ID ổn định; không đổi ID hoặc xóa meta của món đã lưu trong hồ sơ.
