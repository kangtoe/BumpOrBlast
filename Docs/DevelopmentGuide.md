# Bump or Blast — Development Guide

이 프로젝트에서 "어떻게 일하는가"를 모은 인덱스. 세부 규약은 각 하위 문서 참조.

## 0. 원칙

- **기획 먼저, 개발 나중** — 변경 사항은 문서에 먼저 반영한 뒤 구현 진입
- **수동 에디터 작업 최소화** — 반복 세팅은 에디터 스크립트로 자동화
- **임시값으로 시작, 튜닝은 나중** — 스탯/수치는 ScriptableObject에 두고 플레이테스트로 조정
- **논리 단위로 쪼개 커밋** — 한 커밋엔 한 주제

## 1. 문서 (`Docs/`)

| 파일 | 역할 |
|------|------|
| [`GameDesign.md`](./GameDesign.md) | 게임 규칙, 전투, 진행, 레벨업 |
| [`TechnicalSpec.md`](./TechnicalSpec.md) | 기술 구조, 스크립트 구성, 최적화 |
| [`DevelopmentPlan.md`](./DevelopmentPlan.md) | 개발 단계별 계획 |
| [`SystemsRework.md`](./SystemsRework.md) | 시스템 심화 설계(에너지·스탯·아이템·적 등급·오픈필드) + 단계별 로드맵 |
| [`CommitGuide.md`](./CommitGuide.md) | 커밋 메시지 규약 |
| [`DevelopmentGuide.md`](./DevelopmentGuide.md) | 본 문서 — 개발 방법론 인덱스 |

**향후 추가 예정 (TBD)**:
- `Upgrades.md` — 무기/패시브 종류와 레벨업 효과 (현재는 SystemsRework에 포함)
- `Items.md` — 아이템 시스템 (현재는 SystemsRework에 포함)
- `Enemy.md` — 적 시스템 (현재는 SystemsRework에 포함)
- `EditorAutomation.md` — 에디터 자동화 카테고리
- `Roadmap.md` — 전체 Phase 체크리스트 (현재는 SystemsRework "단계별 구현 계획" 참조)

**기획 변경 시**: 관련 문서부터 업데이트 → 커밋 → 구현.

## 2. Git / 커밋

- 세부 규약: [`CommitGuide.md`](./CommitGuide.md)
- 한글 커밋 메시지 인코딩 회피: **임시 파일 + `git commit -F <file>`** 을 디폴트로
- 가이드 도입(2026-05-12) 이전 커밋들은 prefix 없는 한글 제목 사용. 이후 커밋부터 `feat/fix/refactor/chore/docs/perf` prefix 적용

## 3. 에디터 자동화

- **원칙**: 에디터 툴 스크립트는 **한 덩어리의 변경을 만드는 일회성 패치**로 취급.
  결과물(프리팹 / SO / ProjectSettings 등)을 커밋한 뒤 **툴 자체도 삭제**.
  리포에는 "진행 중인 작업" 툴만 남김.
- **유지 대상 (예외)**:
  - 순수 유틸리티 (여러 툴에서 재사용되는 헬퍼)
  - 반복 사용 편집 툴 (밸런싱 SO 에디터 등)
  - 검증(Validators) 훅
- 새로운 변경이 필요할 때는 **작은 새 툴을 따로** 만들어 실행 → 삭제. 기존 툴에 기능 누적 지양.
- **폴더 구조 (신규 도구부터 적용)**
  ```
  Assets/Editor/
  ├── Tools/       # 진행 중 작업 툴 + 영구 유틸
  └── Validators/  # (필요 시) 씬/에셋 검증
  ```
  현재 `Assets/Editor/` 평면 구조 (`kangtoe99_VSReworkSceneSetup.cs`, `kangtoe99_VSReworkBalanceTuning.cs`)는 추후 마이그레이션 검토 — [`SystemsRework.md`](./SystemsRework.md) 미결정 사항 참조
- 일회성 툴 파일 상단에 `// one-shot: 작업 완료 후 삭제` 주석 표기

## 4. UI

- **현재 상태**: uGUI(Image, Text, Button, Canvas 기반) 사용 — 기존 자산 모두 uGUI
- **방침**: 당분간 uGUI 유지. UI Toolkit 전환은 [TBD] (필요성·이득 검증 후 결정)
- **월드 스페이스 UI**: FloatingText 등은 현재 uGUI 월드 Canvas 사용. TextMeshPro 도입 검토 가능

## 5. 데이터 / 밸런싱

- 스탯·수치는 **ScriptableObject**로 분리 (`WeaponData`, `PassiveData`, `EnemyData` 등)
- 프로토타입에선 임시값으로 시작, **플레이테스트로 튜닝**
- SO 자산 편집이 자주 반복되면 `Tools` 카테고리에 **전용 에디터** 제작 고려

### 자산 명명 규약

- **SO 클래스명 = 카테고리 prefix**. 자산명 = `<Class>_<Variant>.asset`
  - 카테고리: `WeaponData`, `PassiveData`, `EnemyData`, `ItemData`, `GemData`, `BossCombineData`, `BossMasterData`, `BossProjectileData`, ...
  - Variant: `Basic`, `Heal`, `Default`, 무기 종류명 등
- 보스 자산은 클래스명에 **`Boss` prefix** 명시 — 일반 적/엘리트 데이터와 구분 (예: `BossCombineData_Default.asset`, `BossMasterData_Default.asset`)
- 폴더는 **카테고리별로 분리** — `Assets/Data/<Category>/` 하위에 배치 (신규부터 적용)
  ```
  Assets/Data/
  ├── Weapons/   # WeaponData_*
  ├── Passives/  # PassiveData_*
  ├── Items/     # ItemData_*
  ├── Enemies/   # EnemyData_*
  ├── Bosses/    # Boss*Data_* (Master / Combine / Projectile / Horde)
  ├── Waves/     # WaveSchedule_*
  └── Gems/      # GemData_*
  ```
  새 카테고리 추가 시 동일 패턴으로 폴더 신설.
- 기존 SO 자산이 다른 위치에 있다면 추후 마이그레이션 검토 (현재는 SO 자산 인스턴스 없음, 클래스만 존재)

## 6. 프리팹

- 인스펙터 인스턴스가 source of truth — 런타임 코드는 데이터로 색/스케일/회전 등을
  덮어쓰지 않음 (예: 발사체 색은 prefab 의 SpriteRenderer.Color 가 기준).
  레벨/배율 계산은 prefab 값을 베이스로 곱셈 적용.
- 폴더는 **카테고리별로 분리** — `Assets/Prefabs/<Category>/` (신규부터 적용)
  ```
  Assets/Prefabs/
  ├── Bosses/    # 보스 unit / 발사체
  ├── Enemies/   # 일반 적 + EnemyProjectile + EnemySpawner
  ├── Items/     # Item 베이스 + 종류별 variant + ItemDropEnemy
  ├── Player/    # Player
  ├── VFXs/      # 시각 이펙트 prefab
  ├── Weapons/   # 무기 발사체/유닛 (Blade, Boomerang, Drone, Flame, Laser, Mine, Orbital, Projectile_Basic ...)
  └── World/     # 월드 엔티티 (Gem 등)
  ```
  새 카테고리 추가 시 동일 패턴으로 폴더 신설.
- 현재 `Assets/Prefabs/` 평면 구조 (Capsule.prefab, enemy_*.prefab 등)는 추후 마이그레이션 검토
- 코드의 prefab 참조는 **인스펙터 SerializedField** 우선 — 경로 문자열
  (`Assets/Prefabs/...`) 하드코딩 지양. 런타임 자동 생성 컴포넌트도 prefab 인스턴스에서
  참조 받도록 설계.

## 7. Phase 진행

- Phase별 체크리스트: [`SystemsRework.md`](./SystemsRework.md) "단계별 구현 계획" 섹션 (Roadmap.md 분리는 TBD)
- 각 Phase 종료 시 **간단 회고** 후 다음 진입
  - 완성된 것 / 미룬 것 / 기획 변경 필요한 것 정리

## 8. 코드 컨벤션

- 모든 신규 C# 클래스 파일에 `kangtoe99_` 접두어 사용 ([`TechnicalSpec.md`](./TechnicalSpec.md) 참조)
- 폴더는 `Assets/Scripts/<Category>/` (Core, Player, Enemy, Systems, UI, Utils, Item, Network, Stats 등)
- 그 외 세부 규약은 _TBD — 프로토타입 진행 중 필요해지는 시점에 본 섹션 또는 별도 문서로 확장._
