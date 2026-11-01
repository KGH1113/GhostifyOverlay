# Third-party notices

| 구성 요소 | 사용 범위 | 저작권·라이선스 전문 |
|---|---|---|
| [JipperResourcePack](https://github.com/Jongye0l/JipperResourcePack) — Jongyeol | 수정한 키뷰어 코드 일부, 어템의 레벨 해시 판별 기준 참고 | [BSD-3-Clause](Licenses/JipperResourcePack-BSD-3-Clause.txt) |
| Gmarket Sans Light / Medium | UI·오버레이·설치 프로그램 글꼴 | [동봉된 OFL](Assets/GmarketSans-OFL.txt) |
| Noto Sans KR | 한글 등 누락 글리프 보완 | [SIL OFL 1.1](Assets/NotoSansKR-OFL.txt) |

키뷰어 기준 커밋은 `ac6d6410571ee7d585aedce7f5085de34f615af2`이며 [원본 파일 기록](Licenses/JipperResourcePack-source.json)을 제공합니다. Noto Sans KR의 변환·출처는 [폰트 기록](Assets/NotoSansKR-source.json)에 있습니다.

레벨 판별은 같은 커밋의 `OverlayContents/PlayCount.cs`에서 타일·주요 플레이 이벤트만 해시하는 기준을 참고했습니다. Ghostify는 별도의 버전이 지정된 직렬화와 SHA256을 사용하며, 원본 `Plays.dat` 형식이나 JALib 의존성을 포함하지 않습니다.

위 문서의 저작권·라이선스 조건·면책 전문은 소스와 EXE·ZIP 배포물에 함께 포함합니다.

[ADOFAI Tweaks](https://github.com/PizzaLovers007/AdofaiTweaks)의 기본 곡 정보 UI 숨김 동작을 참고하고, 함께 설치된 경우 설정을 읽어 호환합니다. 해당 프로젝트의 소스 코드·DLL은 포함하지 않습니다. 참고 기준은 `f1d0a1e2500004f8a92c73705af8b7dc6aca2839`입니다. 채터링 차단은 [얼불춤용 KeyboardChatterBlocker](https://github.com/fangshenghan/KeyboardChatterBlocker)의 같은 키 입력 간격 동작을 참고한 자체 구현입니다. 해당 프로젝트의 코드·DLL은 포함하지 않습니다.

## 적용 범위와 배포 조건

[라이선스 정책](LICENSE.md)을 따릅니다. 라이선스 전문을 보존한 것은 제3자 코드를 Ghostify 자체 코드로 재허락하거나 원저작자의 권리를 없애는 의미가 아닙니다.

JipperResourcePack 유래 부분의 원저작권은 Copyright (c) 2024, Jongyeol입니다. 소스에는 원저작권·BSD 세 조건·면책 전문을 유지하고, 바이너리 배포에는 해당 전문을 동봉합니다. 원저작자·기여자가 Ghostify를 공식 승인·추천한 것처럼 홍보하지 않습니다. 현재 수정 파일 목록은 `Licenses/JipperResourcePack-source.json`의 `derivedFiles`에 기록합니다.

Noto Sans KR은 기록된 가변 폰트에서 Regular 400으로 변환한 버전입니다. Reserved Font Name 'Source'를 사용하지 않으며 기존 저작권·OFL 전문을 보존합니다. Gmarket Sans는 제공된 Light/Medium 파일과 동봉된 공급자 라이선스를 그대로 유지합니다.

설치기의 `Mascot.png`와 변환 아이콘 `Ghostify.ico`는 사용자 제공 리소스이며, 프로젝트 소유자는 2026-11-02 직접 제작 또는 Ghostify 포함 배포 허락을 받았음을 확인했습니다. 이미지에 BSD·OFL이나 제3자 재사용 허락을 부여하지 않습니다.

게임·Unity·TextMeshPro·SkyHook·Newtonsoft.Json·UMM/Harmony 등 실행 환경의 DLL은 사용자 설치본을 참조하며 패키지에 동봉하지 않습니다. ADOFAI Tweaks·KeyboardChatterBlocker와의 동작 참고·호환 관계는 위 설명과 같으며, 공식 제휴·보증을 의미하지 않습니다.
