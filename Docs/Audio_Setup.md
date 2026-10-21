# Thiết lập âm thanh

Bản gọn gắn đủ **19 clip cho 19 tín hiệu** sẵn có vào `Assets/Prefabs/GameAudio.prefab`: nhạc Menu/Làng/Hồ, môi trường, bước chân, câu cá, QTE và UI. Không cần kéo từng clip vào các scene.

## Clip đang dùng

| File | Giây | Lặp |
| --- | ---: | --- |
| BGM_MainMenu.ogg | 24 | Có |
| BGM_Lake.ogg | 24 | Có |
| AMB_Lake.ogg | 44.0 | Có |
| SFX_Step.wav | 0.249 | Không |
| SFX_Cast.wav | 0.432 | Không |
| SFX_Splash.wav | 0.915 | Không |
| SFX_Reel_Loop.ogg | 8.912 | Có |
| SFX_Bite.wav | 0.491 | Không |
| SFX_QTE_Appear.wav | 0.04 | Không |
| SFX_QTE_Good.wav | 0.29 | Không |
| SFX_QTE_Great.wav | 0.49 | Không |
| SFX_QTE_Fail.wav | 0.533 | Không |
| SFX_LineBreak.wav | 0.1 | Không |
| SFX_FishLanded.wav | 0.69 | Không |
| UI_Click.wav | 0.007 | Không |
| UI_Open.wav | 0.148 | Không |
| UI_Close.wav | 0.231 | Không |
| BGM_Village.ogg | 24 | Có |
| AMB_Village.ogg | 44.0 | Có |

Ba bản nhạc dùng đoạn trích 24 giây, mono 22.05 kHz, Vorbis 24 kbps để giảm dung lượng tải. Có fade ở hai đầu; khi chơi lâu sẽ nghe đoạn nhạc lặp lại. Có thể thay bằng bản đầy đủ sau.

## File và component

- `Assets/Prefabs/GameAudio.prefab`: mỗi object con giữ một AudioSource và AudioClip tương ứng. Âm thanh 2D, tắt Play On Awake.
- `Assets/Scripts/Audio/GameAudio.cs`: giữ một instance xuyên scene, chọn nhạc theo scene và nhận volume/mute từ GameSettings. Dùng cơ chế hiện có, không thêm lớp chuyển nhạc.
- `FishingAudioEvents.cs`: sự kiện quăng cần, chạm nước, thu dây, cá cắn, QTE, đứt dây, bắt cá và bước chân ở hồ.
- `VillageFootsteps.cs`: bước chân tại làng; `UIButtonAudio.cs`: phản hồi UI.
- `Assets/Audio/GameAudio.mixer`: Master → Music / SFX. Ba tham số là `MasterVolume`, `MusicVolume`, `SfxVolume`; nhạc qua Music, môi trường và hiệu ứng qua SFX.
- Nhạc và UI được phép phát khi AudioListener bị pause; các hiệu ứng gameplay chịu pause. Mức mute/volume vẫn theo cài đặt đã lưu.

## Thay clip

Mở prefab asset ngoài Play Mode, chọn object con tương ứng rồi thay AudioClip. Giữ nguyên AudioMixerGroup để thanh âm lượng tiếp tục điều khiển đúng nhóm. Nhạc và môi trường dùng Streaming; hiệu ứng ngắn dùng Decompress On Load. Camera scene giữ AudioListener; không thêm listener vào prefab âm thanh.

## Nguồn

Nhạc: HoliznaCC0, Quiet Village Collection. Môi trường: Thimras, Park ambiences. Tiếng câu: Memoraphile / You're Perfect Studio, Fisheefects. UI và foley: Kenney Interface Sounds, RPG Audio, Music Jingles. Lựa chọn giấy phép CC0 cùng nguồn và xử lý từng file nằm trong `Assets/Audio/Licenses/Audio-Credits.txt` và `AudioManifest.json`.

## Phạm vi kiểm tra

Đã kiểm tra dữ liệu file, clip không rỗng và liên kết clip/mixer trong prefab. Chưa chạy Unity import/compile, Play hoặc build cho bản này; cân âm lượng bằng nghe thực tế là bước tiếp theo. Bản gọn chưa nối thêm âm thanh giao dịch, ủy thác, tăng cấp hoặc khám phá loài.
