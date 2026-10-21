# Gắn âm thanh

## Bộ file cần chuẩn bị

Hiện có 19 cue đã được nối với gameplay. Tên file dưới đây là quy ước bàn giao, không phải tên bắt buộc của Unity. Chỉ cần một clip cho mỗi cue để có bộ âm thanh đầu tiên hoàn chỉnh.

| Tên file gợi ý | Nội dung cần nghe | Độ dài gợi ý | Lặp |
| --- | --- | --- | --- |
| BGM_MainMenu.wav | Nhạc mở đầu nhẹ, mộc mạc, không lời | 60–120 giây | Có |
| BGM_Village.wav | Nhạc làng quê ấm áp, tiết tấu vừa phải | 60–180 giây | Có |
| BGM_Lake.wav | Nhạc hồ yên bình, thưa nhạc cụ để nghe rõ QTE | 60–180 giây | Có |
| AMB_Village.wav | Gió, chim và sinh hoạt xa; không chứa nhạc | 20–60 giây | Có |
| AMB_Lake.wav | Gió nhẹ, nước ven bờ, chim hoặc côn trùng; không chứa nhạc | 20–60 giây | Có |
| SFX_Step.wav | Một bước chân nhẹ trên đất/gỗ | 0.1–0.3 giây | Không |
| SFX_Cast.wav | Tiếng vút của cần và dây lúc quăng | 0.2–0.7 giây | Không |
| SFX_Splash.wav | Mồi chạm nước, bắn nước nhỏ | 0.2–0.8 giây | Không |
| SFX_Reel_Loop.wav | Cuộn dây chạy đều khi giữ thu dây | 1–3 giây | Có |
| SFX_Bite.wav | Dấu hiệu cá mắc câu, rõ và ngắn | 0.2–0.6 giây | Không |
| SFX_QTE_Appear.wav | Tín hiệu cần chú ý khi QTE xuất hiện | 0.1–0.4 giây | Không |
| SFX_QTE_Good.wav | Thành công vừa, nghe dễ chịu | 0.15–0.5 giây | Không |
| SFX_QTE_Great.wav | Thành công nổi bật hơn Good, không quá chói | 0.2–0.7 giây | Không |
| SFX_QTE_Fail.wav | Lỗi ngắn, phân biệt rõ với Good/Great | 0.15–0.6 giây | Không |
| SFX_LineBreak.wav | Dây bật/đứt sắc, ngắn | 0.2–0.7 giây | Không |
| SFX_FishLanded.wav | Bắt cá thành công; một đoạn thưởng ngắn | 0.5–1.5 giây | Không |
| UI_Click.wav | Nhấn nút, không quá nặng khi nghe nhiều lần | 0.05–0.2 giây | Không |
| UI_Open.wav | Mở túi/cửa sổ, gợi chất giấy hoặc vải | 0.1–0.4 giây | Không |
| UI_Close.wav | Đóng cửa sổ, cùng chất âm với Open | 0.1–0.4 giây | Không |

Ưu tiên file gốc WAV PCM 16-bit hoặc 24-bit, 44.1 hoặc 48 kHz. Nhạc và môi trường có thể stereo; hiệu ứng ngắn có thể mono. Nếu chỉ có OGG vẫn có thể nhập vào Unity. Không cần tự chuyển file sang MP3.

Các file lặp phải nối đầu/cuối liền mạch, không có khoảng lặng ở đầu/cuối. Riêng Reel cần chạy đều, không kèm tiếng bắt đầu/kết thúc dài vì người chơi nhấp/thả thu dây thường xuyên. File một lần nên được cắt sát nội dung, không rè hoặc vỡ tiếng. Mỗi hiệu ứng là một file riêng; không trộn QTE, nhạc hoặc môi trường vào hiệu ứng câu.

Đặt file vào `Assets/Audio/Clips/Music`, `Assets/Audio/Clips/Ambience`, `Assets/Audio/Clips/SFX` rồi để Unity tạo `.meta` và push cả clip lẫn `.meta` lên cùng nhánh. Sau đó có thể gắn clip vào prefab, đặt cách import và cân âm lượng qua mixer. Có thể dùng chung một clip cho nhiều cue lúc đầu nếu chưa đủ.

Âm thanh bổ sung sau, hiện chưa có cue riêng: nhận/hoàn thành/thất bại ủy thác, giao dịch mua/bán, tăng cấp, khám phá loài mới và tiếng hội thoại NPC. Không cần thu thoại đầy đủ cho giai đoạn này. Nếu có nhiều biến thể bước chân hoặc tiếng nước, giữ thành các file riêng để bổ sung cơ chế chọn biến thể sau.

## Các nguồn đã nối

Mở prefab `Assets/Prefabs/GameAudio.prefab` ngoài Play Mode. Mỗi object con đã có AudioSource và đăng ký với GameAudio; thả clip vào ô AudioClip của nguồn tương ứng. Không thêm AudioListener vào prefab. Mỗi scene có một listener trên camera.

Mixer: `Assets/Audio/GameAudio.mixer`, gồm Master → Music và SFX. AudioSource đã được route; settings điều khiển ba volume qua exposed parameters. Không đổi tên MasterVolume, MusicVolume, SfxVolume nếu chưa cập nhật code.

| Nguồn | Thời điểm | Loop / bus |
|---|---|---|
| MenuMusic | MainMenu | Có / Music |
| LakeMusic | Lake | Có / Music |
| VillageMusic | Village | Có / Music |
| VillageAmbience | Không gian làng | Có / SFX |
| Ambience | Không gian hồ | Có / SFX |
| Step | Đi bộ | Không / SFX |
| Cast | Hook được phóng | Không / SFX |
| Splash | Hook chạm nước | Không / SFX |
| Reel | Giữ thu dây hợp lệ | Có / SFX |
| Bite | Cá mắc câu | Không / SFX |
| QteAppear | Bắt đầu vòng QTE | Không / SFX |
| Good / Great / Fail | Kết quả QTE | Không / SFX |
| LineBreak | Đứt dây | Không / SFX |
| FishLanded | Đưa cá lên bờ | Không / SFX |
| UiClick / UiOpen / UiClose | Tương tác UI | Không / SFX |

Nguồn không có clip sẽ im lặng, không báo lỗi và không cản gameplay. Âm thanh môi trường/gameplay dừng cùng pause; nhạc và UI vẫn nghe được. GameAudio tồn tại xuyên scene và loại bản trùng. Gắn clip vào prefab asset, không gắn vào instance trong Play Mode vì thay đổi đó không được giữ sau khi dừng.

Hiện chưa có clip, chưa cân âm lượng hoặc nghe thử trong Unity. AudioMixer và nguồn native cần được Unity import ở lần mở dự án tiếp theo.
