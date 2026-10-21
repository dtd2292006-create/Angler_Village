# Gắn âm thanh

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
