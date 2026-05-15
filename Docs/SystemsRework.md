# 시스템 리워크 작업 문서

## 개요

BumpOrBlast의 **2D 탑다운 슈터 정체성은 유지**한 채, 그 위에 **로그라이크 성장·진행 시스템 계층**을 추가하고 기존 시스템을 심화·확장한다.

- 슈팅의 핵심(WASD 이동 + 마우스 회전·사격, Bump 충돌)은 그대로
- 탄창 → **에너지** / 분산된 수치 → **중앙 스탯** / 일회성 드롭 → **영구 아이템**으로 확장
- 적 5등급 분류 등 **로그라이크 장르(특히 뱀파이어 서바이버)에서 영감 받은 요소** 일부 도입
- 카메라는 고정, 화면 wrap-around 유지 (오픈 필드·카메라 추적 컨셉은 폐기 — 아래 변경 이력 참조)

> 이 문서는 원래 "VampireSurvivorsRework.md"로 시작해 장르 전환을 목표로 했으나, 2026-05-12 재검토로 자동 전진·자동 사격을 폐기하면서 방향성이 "전환"에서 "심화"로 재정립되었다.

- **작성일**: 2026-04-21
- **최근 갱신**: 2026-05-15 — R7 적 5등급 + 챔피언 몹 + 드롭 재구성
- **상태**: Phase R1~R4 + R6a + R7 + R8a 완료. R5 폐기. R6b 트리거 · R8b(고급 풀 가중치) · R9~R11 미착수.
- **관련 문서**: [Balance.md](Balance.md), [GameDesign.md](GameDesign.md), [TechnicalSpec.md](TechnicalSpec.md), [DevelopmentGuide.md](DevelopmentGuide.md)

## 변경 이력 (핵심 결정만)

### 2026-05-15
- **R7 적 5등급** → `kangtoe99_Tier` enum(Gray~Orange, 적·아이템 공용) + `kangtoe99_EnemyTierData` SO(등급별 statMultiplier/scoreMultiplier/scaleMultiplier). 스폰 시 배율 레이어로 적용 — `EnemyData` 데이터화는 안 함(사용자 결정). `EnemySpawner`가 `Enemy.ApplyTier(entry, color)`로 HP/Damage/moveForce/score·색·스케일 적용
- **등급 색상 공용 팔레트** (사용자 결정) → `kangtoe99_TierColorPalette` SO(`Assets/Data/TierColorPalette.asset`)가 등급별 색을 한 곳에서 관리. 적 틴트(`EnemyTierData.colorPalette`)·아이템 배경 UI가 공유. `kangtoe99_EnemyTier`/`kangtoe99_ItemTier` 별개 enum을 `kangtoe99_Tier` 하나로 통합
- **난이도 = 등급 → 스텝 배율 순차** (사용자 결정) → 등급 진행 중엔 시간 배율 OFF(1배), **마지막 등급(Orange) solid 도달 후부터** 적 HP가 **스텝마다** 강해짐(스폰당 누적 아님). `EnemySpawner.GetPostProgressionMultiplier()` — `over = elapsed - tierData.ProgressionDuration`, `1 + floor(over/postStepDuration)×postStepIncrement`, `postMaxMultiplier` 상한. 진행 후 스케일은 선형이라 SO 아닌 스포너 인스펙터 필드(사용자 결정). HP에만 적용 — 공격력·속도는 등급 배율로만
- **스폰 진행: solid → blend → solid** (사용자 결정) → `EnemyTierData.PickTier(elapsedTime)`. solid 구간은 단일 등급, blend 구간은 인접 두 등급을 비율(0→1) 추첨, 최고 등급 도달 후 고정. **solid/blend 시간은 스텝별** — 각 `TierEntry`가 자기 `solidDuration`/`blendDuration` 보유(인스펙터 설정)
- **챔피언 몹** (사용자 결정) → 5등급과 **별개 축**. 새 적이 아니라 기존 적의 강화판. `EnemySpawner`가 `championCheckInterval`마다 `championChance` 확률로 추가 스폰. `Enemy.ApplyChampion(statMul, scaleMul)`이 등급 배율 위에 추가 강화 + `isChampion` 플래그
- **드롭 재구성** (사용자 결정) → 일반 적은 드롭 없음(처치 점수만). 기존 `DropSystem.TryDrop`(XP/HP/Bomb 로직)은 그대로 두되 **호출 지점을 챔피언 처치로만 이동** — `Enemy.Die()`에서 `isChampion` 게이트. 등급별 드롭 풀 차등은 폐기(드롭은 챔피언 전용)
- 신규 자산: `Assets/Data/Enemy/EnemyTierData.asset` — `EnemySpawner.tierData`에 배선

### 2026-05-14
- **종합 정보 패널(RunSummary) 도입** → `kangtoe99_RunStats`(한 판 통계 누적 싱글톤: 가한/받은 피해·처치·생존시간) + `kangtoe99_RunSummaryUI`(레벨/점수/생존시간/처치/피해 + 빌드 표시). 스탯은 **항목당 Text 1개**(`levelText`/`scoreText`/… 6개 필드, VerticalLayoutGroup 행 배치) — 한 덩어리 문자열 아님
- **공용 정보 패널** → 일시정지(`PauseSystem`)와 게임오버 1단계(`GameManager`)가 **같은 RunSummaryUI 패널 인스턴스 공유**(기존 PausePanel 재사용 — 별도 패널 안 만듦). 표시 내용은 동일, **제목(`titleText`)·안내 문구(`hintText`)만 맥락별로 다름**(일시정지 "Paused" / 게임오버 "Game Over"). 인게임 표기는 영어로 통일(사용자 방침). `RunSummaryUI.Show(title, hint)`/`Hide()`가 진입점 — `panelRoot`를 켜고 끔. 두 시스템 모두 `infoPanel` 필드(타입 `kangtoe99_RunSummaryUI`)로 같은 인스턴스 참조
- **게임오버 2단계 흐름** → 사망 시 ① 공용 정보 창 먼저 표시 → **Enter/Space 입력** → ② 랭킹(OverPanel) 표시. 정보 창을 읽는 동안 랭킹 서버 요청은 백그라운드로 미리 시작(`gameOverUI.ShowGameOver`를 1단계에서 호출). GameManager가 `GameOverSequence` 코루틴의 `WaitUntil`로 전환 제어. 정보 창은 사라지고 랭킹만 표시. 일시정지/게임오버는 실질 상호배타라 패널 공유 안전
- **사망 시 플레이어 `Destroy` → `SetActive(false)`** → 게임오버 정보 패널의 빌드 표시(`BuildDisplayUI`)가 플레이어의 `ItemInventory`를 계속 참조해야 하므로, `Player.Die()`에서 파괴 대신 비활성화. 비활성 GameObject는 Update/물리가 멈추지만 컴포넌트 참조는 유효. 씬 재시작에서 정리됨
- **일반 플레이 HUD 빌드 표시 제거** → 상시 HUD 빌드 표시(`HUD_BuildContainer`) 삭제. 빌드/아이템 목록은 **공용 정보 패널(일시정지·게임오버)에서만** 표시. `BuildDisplayUI` 스크립트 자체는 유지(공용 패널·게임오버 랭킹 영역이 사용)
- **자동 셋업 도구 재작성** → `kangtoe99_SceneSetup` (메뉴 `Tools > BumpOrBlast > Setup RunSummary`). RunStats 싱글톤 보장 + 씬의 RunSummaryUI를 찾아 `panelRoot`/`titleText`/`hintText`/`buildDisplay` 배선 + `PauseSystem.infoPanel`·`GameManager.infoPanel` 배선 + 정리(구버전 SummaryPanel · `HUD_BuildContainer` 제거). 일회성 — 셋업 후 폐기
- **플레이어 이름 입력 이전** → 시작 화면 이름 입력 폐기. 이름은 RunSummaryUI 패널에서 Edit 버튼 → 입력 패널로 수정. `PlayerPrefs` 저장(기본값 "player name"), 수정 안 하면 계속 유지. 입력 패널 열린 동안 Enter/Space/ESC는 게임 진행(랭킹 전환·일시정지 해제)에 안 쓰이도록 가드(`RunSummaryUI.IsNameEditOpen`)
- **게임오버 리더보드 재설계** → 페이지네이션·`myRankEntry` 폐기. 최상위 5 + 내 주변 5(중복 허용)를 한 명단으로 `entryContainer`에 `LeaderboardEntry` 프리팹 런타임 생성. 내 항목 이름은 항상 `GameManager.PlayerName` 기준 렌더(서버 PATCH 타이밍과 무관). `RankApi.UpdateRankName`(이름만 PATCH) 추가 — 이름 수정 시 서버 반영(best-effort)
- **전체 TMP 전환** → `UnityEngine.UI.Text` → `TextMeshProUGUI` 전 프로젝트 전환(씬 36개 + 프리팹 11개 + 스크립트 6개). 씬/프리팹은 fileID 유지로 참조 보존. `InputField` 1개는 레거시 uGUI로 남김. `Assets/TextMesh Pro/`는 .gitignore 대상(표준 관행 — 폰트 GUID가 결정적이라 참조 유지됨) → 클론 후 `Window > TextMeshPro > Import TMP Essential Resources` 필요
- **씬 타이틀 영어화** → 인스펙터에 한글로 직렬화돼 있던 `pauseTitle`/`summaryTitle` → "Paused"/"Game Over" (인게임 텍스트 영어 통일 방침)

### 2026-05-12 (방향성 재정립)
- "장르 전환" → "시스템 심화"로 재정립 (문서 이름 SystemsRework)
- 자동 전진/자동 사격 컨셉 폐기, WASD + 마우스 사격 유지
- 에너지 시스템 신설(탄창 대체, 시간 자동 회복만), 중앙 PlayerStats + Modifier, 영구 아이템, 적/아이템 5등급(Gray/Green/Blue/Purple/Orange) 도입 결정
- 오픈 필드 + 카메라 추적 + 적 원주 리사이클 폐기 → 카메라 고정 + 화면 wrap-around + 거리 초과 적 삭제(이전 동작) 복원. `kangtoe99_EnemyRegistry`는 자동 조준 무기용으로 유지

### 2026-05-13
- **Bump 양방향 데미지 폐기**(Phase R5 자체 폐기) → 플레이어만 충돌 데미지를 받음(기존 동작). `BumpDamageDealtMul`/`BumpDamageTakenMul` 스탯 enum에서 완전 제거. 추후 무적 프레임 등 필요 시 다시 도입 검토
- **R6 분할** → R6a(스탯 modifier 기반 아이템) 완료, R6b(트리거 효과)는 분리해서 추후
- **Drop/Item 명명 분리 적용** → 일시 픽업 = `kangtoe99_Drop*`, 영구 효과 = `kangtoe99_ItemData/Inventory`. `Scripts/Item/`→`Scripts/Drop/`, `Prefabs/Items/`→`Prefabs/Drops/`. 메서드 `TryDropItem` → `TryDrop`
- **maxSpeed 클램프 제거** → Character.Move 및 Enemy.ChasePlayer의 속도 클램프 제거. drag(linearDamping)로만 평형 속도 결정. 플레이 테스트 후 어색하면 부활 검토
- **R8a LevelUpSystem 전면 재작성** → 기존 Damage/FireRate 하드코드 2지선다 폐기. 동적 4지선다 슬롯. `kangtoe99_ILevelUpChoice` 인터페이스(`ItemData` + `InstantDropItemData`가 구현). **풀 고갈 조건 강화**: ItemData가 1개라도 사용 가능하면 ItemData만 노출(1~4개). 완전 고갈(0개) 시에만 `InstantDropItemData`(드롭 3종 즉시 발동)로 슬롯 채움
- **R8a Build UI 추가** → HUD 좌상단 상시 빌드 표시 + `kangtoe99_PauseSystem`(ESC 토글) + GameOverUI에 별도 빌드 영역. `kangtoe99_BuildDisplayUI` + `kangtoe99_BuildEntrySlot`이 세 영역 모두에서 재사용
- **자동 셋업 도구** → `kangtoe99_SceneSetup` (메뉴 `Tools > BumpOrBlast > Setup Scene`). 한 번 실행으로 ItemInventory + ItemDisplay.prefab + LevelUpChoiceSlot.prefab + ChoiceContainer + LevelUpSystem 배선 + HUD/Pause/GameOver 빌드 영역 + DebugPanel 전부 자동 생성·배선. **prefab은 현재 코드 필드 시그니처 검증 → 누락 시 삭제 후 재생성**. itemPool/instantDropPool은 프로젝트 전체 자산을 타입 검색해 채움. **일회성 도구 — Scene 셋업 완료 후 폐기**(2026-05-14 사용 후 삭제 완료). 다음 UI 구조 변경 시 필요하면 재작성
- **R8a DebugPanel** → 백틱(`) 키 토글. **예외적으로 IMGUI(OnGUI) 사용** — Canvas/prefab/Button 위젯 없이 GUILayout으로 즉시 그림. 게임 상태 정보(레벨/점수/HP/에너지/적 수/보유 아이템 수) + 액션 4종(Force Level Up · Kill All Enemies · Heal Full · Add Random Item). Time.timeScale 건드리지 않음. 자동 셋업은 `DebugPanel` GameObject 1개만 추가

## 설계 확정 사항

### 조작 체계
- WASD/방향키 4방향 자유 이동 (월드 좌표 기준), 물리 기반 `AddForce` + linearDamping
- 회전은 마우스 보간 기반: `IRotationInput` → `Mathf.MoveTowardsAngle`로 `RotationSpeed(도/초)` 보간 (초기 튜닝 범위 180~360°/sec)
- 사격은 마우스 좌클릭 + fireRate 쿨다운 자동 연사, 에너지 부족 시 발사 불가

### 카메라 & 필드
- 카메라 고정, 화면 wrap-around. 적은 cullRadius 초과 시 삭제 (이전 동작)
- 향후 부드러운 추적이 필요하면 Cinemachine 검토 (미결정)

### 무기 체계
- 기본 무기: 마우스 방향 발사 (`kangtoe99_PlayerShooting`, 추후 `ForwardWeapon`으로 리팩터 — Phase R9)
- 추가 무기는 업그레이드로 해금 (자동 조준, 공전 드론, 장판 등 — Phase R9)
- 모든 무기는 사격 시 에너지 소모

### Bump 충돌
- 플레이어만 데미지를 받음(기존 동작). 양방향 교환·관련 스탯 모두 폐기(2026-05-13)

### 플랫폼 & 입력
- PC 우선, 모바일(세로) 대응 준비
- 입력 추상화: 회전(`IRotationInput`) / 이동(WASD or 가상 조이스틱) / 사격(좌클릭 or 사격 버튼)

## 살아있는 청사진

### 스탯 시스템 (구현 완료)
- 중앙 `kangtoe99_PlayerStats`가 모든 스탯의 단일 진리원천
- base는 `kangtoe99_PlayerStatsData` SO에서 로드(자산이 진리원천, 코드 기본값 없음)
- 외부 modifier 리스트로 동적 보정. 공식: `(base + Σ가산) × Π(1 + 배율)`
- `OnStatChanged` 이벤트로 의존 컴포넌트 반응(MaxHP/BodyScale/Friction)
- 스탯 카테고리 — 발사체(Count/Speed/Scale/Spread/Pierce), 무기(Damage/FireRate/EnergyCost), 에너지(EnergyMax/EnergyRegen), 기체(MaxHP/HPRegen/BodyScale), 이동(MoveForce/RotationSpeed/Friction), 메타(Luck/Magnet). enum 본체는 [kangtoe99_StatType.cs](../Assets/Scripts/Stats/kangtoe99_StatType.cs)
- **메타 스탯 적용 상황**: Luck/Magnet은 아직 미적용(Drop·LevelUp 풀과 연동 시 wire-up)

### 에너지 시스템 (구현 완료)
- 탄창·재장전 대체. 사격 시 `EnergyCost` 소모, 시간 자동 회복만 (`EnergyRegen`/sec)
- 사격 시 회복 50% 배율(연사 중 패널티)
- UI: 눈금 세그먼트 게이지 (`EnergyBarUI`, `pointsPerTick`)

### 아이템 / 드롭 명명
- **Drop (드롭)**: 적이 떨어뜨리는 일시 픽업. XP/HP회복/폭탄. → [kangtoe99_DropSystem](../Assets/Scripts/Drop/kangtoe99_DropSystem.cs)
- **Item (아이템)**: 레벨업 선택지로 획득하는 영구 효과. 스탯 modifier(+R6b 트리거 효과 예정) → [Scripts/Item/](../Assets/Scripts/Item/)

### 아이템 데이터 (R6a 구현 완료)
- `kangtoe99_ItemData` SO: displayName / icon / tier(5단계) / maxStack / List<StatModifierData>
- `Description` 프로퍼티는 modifiers를 자동 조립 (예: "Damage +20%\nFireRate -10%"). 수동 입력 없음. R6b 진입 시 trigger 효과 텍스트도 같은 자리에서 합칠 예정
- 공개 포맷 API: `ItemData.FormatModifier(m)` = "Damage +20%" (이름+값) / `ItemData.FormatValue(m)` = "+20%" (값만). UI는 stat 아이콘과 결합 시 FormatValue 사용
- **Stat 아이콘**: `kangtoe99_StatIconRegistry` SO가 StatType→Sprite 매핑 보유 (단일 자산 진리원천, EnumMap 패턴). 실제 UI 표시 방식(TMP 인라인 vs 행 단위 Image+Text)은 R8 LevelUp 풀 확장 시 결정
- 등급 = **풀 구분 의미**: Gray/Green/Blue/Purple/Orange 각각 별개 풀. 고등급 풀일수록 더 강한 modifier + 트리거 효과(R6b)
- 같은 ItemData는 maxStack까지만 중첩, 종류는 무제한
- `kangtoe99_ItemInventory`가 Player에 부착되어 스택·modifier 등록 관리. modifier source는 Entry 객체(스택 단위 추적)

### 트리거 효과 (R6b 미착수)
```
TriggerEffectData (abstract ScriptableObject)
  ├─ Subscribe(player) — 이벤트 후킹
  └─ Unsubscribe(player)
```
첫 풀 후보: OnLowHpDamageBoost / OnKillSpawnProjectile / OnEnergyFullCritical 등. 구체 선정은 R6b 진입 시 결정. (OnBumpExplode는 Bump 양방향 폐기로 컨셉 부적합 — 제외)

### 적 5등급 + 챔피언 (R7 완료)
등급별 배율 (`EnemyTierData.asset` 초기값 — 인스펙터에서 튜닝):
| 등급 | statMultiplier | scoreMultiplier | scaleMultiplier |
|---|---|---|---|
| Gray | 1 | 1 | 1 |
| Green | 1.5 | 1.5 | 1.1 |
| Blue | 2.5 | 3 | 1.2 |
| Purple | 4 | 6 | 1.35 |
| Orange | 8 | 10 | 1.6 |

- 등급은 **스폰 시 배율 레이어** — `EnemyData` 데이터화 안 함. 행동 패턴(프리팹)은 등급과 독립
- 스폰 진행은 **solid → blend → solid**: solid(단일 등급) ↔ blend(인접 두 등급 비율 추첨) 반복, 최고 등급 도달 후 고정. solid/blend 시간은 등급별로 각각 설정(`TierEntry.solidDuration`/`blendDuration`)
- 등급 진행 완료(마지막 등급 solid 도달) 후엔 적 HP가 스텝마다 강해짐 — `EnemySpawner`의 `postStepDuration`/`postStepIncrement`/`postMaxMultiplier`(선형, 인스펙터). 진행 중엔 등급 escalation이, 그 후엔 스텝 배율이 난이도를 이어감
- **챔피언**: 등급과 별개 축. 주기적 확률 스폰되는 강화 변종(`championStatMultiplier`/`championScaleMultiplier`). **드롭은 챔피언만** — 일반 적은 처치 점수만, 챔피언만 `DropSystem.TryDrop`(XP/HP/Bomb) 호출
- 시각: 색상은 등급별, 크기는 등급·챔피언 배율 누적

### LevelUpSystem (R8a 완료, R8b 미착수)

**R8a 완료**: 동적 4지선다. `ILevelUpChoice` 통일 인터페이스. `LevelUpSystem`이 itemPool에서 IsAvailable한 ItemData를 우선 노출. **완전 고갈(0개) 시에만** instantDropPool로 슬롯 채움.

**ILevelUpChoice 구현체**
- `kangtoe99_ItemData` — 영구 modifier. IsAvailable = `!Inventory.IsFull(this)`. Apply = `Inventory.TryAdd(this)`
- `kangtoe99_InstantDropItemData` — Drop prefab 1개 참조. Apply = 플레이어 위치에 비활성 prefab을 인스턴스화한 뒤 `Drop.TriggerPickup(player)` 호출. 즉시 효과 발동 후 Destroy

**Drop 변경**: `Drop.OnTriggerEnter2D` 내부 픽업 로직을 `public TriggerPickup(player)`로 추출. 콜라이더 충돌과 코드 직접 호출 모두에서 동일 경로

### Build UI (R8a 완료, R8b 가중치 미착수)

세 영역에서 동일한 `kangtoe99_BuildDisplayUI` 재사용. 각 영역의 slotPrefab은 **공통 ItemDisplay.prefab(아이템 UI 틀)** 직접 사용. 중간 BuildEntrySlot 레이어 폐기:
- **HUD 좌상단** — 상시 표시
- **PausePanel** (`kangtoe99_PauseSystem`, ESC 토글) — GridLayout
- **GameOverPanel** — 리더보드와 별도 영역

`ItemInventory.GetBuildEntries()` 노출 + `OnItemAdded` 이벤트로 BuildDisplayUI가 자동 갱신.

### UI 역할 분리 (R8a, 사용자 결정)

**아이템 UI** = `kangtoe99_ItemDisplayView` (`Assets/Prefabs/UIs/ItemDisplay.prefab`)
- 빌드 화면 슬롯으로 배치 (HUD/Pause/GameOver). BuildDisplayUI가 직접 인스턴스화
- 평소 표시: 아이콘 + 중복 수(xN, 우상단 anchor)
- 마우스 호버 시: 자식 tooltipRoot 활성화 → 이름/설명 표시 (`IPointerEnterHandler`/`IPointerExitHandler`)
- raycastTarget Image가 root에 있어 슬롯 전체에서 호버 감지

**선택지 UI** = `kangtoe99_LevelUpChoiceSlot` (`Assets/Prefabs/UIs/LevelUpChoiceSlot.prefab`)
- LevelUp 패널에 인스턴스화. 자체적으로 Icon + Name + Description 직접 표시 (ItemDisplayView 의존 폐기)
- 클릭 시 `ILevelUpChoice.Apply` 호출

**ILevelUpChoice/ItemData/InstantDropItemData에 시각 prefab 필드 없음** — UI 표시는 슬롯이 SO 데이터(Icon/이름/설명)를 받아 알아서 채움

**자동 셋업 도구** (`kangtoe99_SceneSetup`, 일회성 — 사용 후 폐기): ItemDisplay.prefab(tooltip 자식 포함) + LevelUpChoiceSlot.prefab(자체 표시) 자동 생성. 현재 코드 필드(`IsSet` 검증)와 prefab 직렬화 mismatch 감지 시 삭제 후 재생성 — prefab v3↔v4 직렬화 불일치로 Bind가 null 스킵하던 문제의 항구 대책. Scene 셋업 완료 후 도구는 폐기됨

**R8b (미착수)**: 풀 가중치 / Luck 스탯 → 고등급 ItemData 풀 확률 상승 / 같은 선택지 연속 등장 방지 / 회복 카테고리 / 아이템 아이콘 UI(StatIconRegistry 활용)

## 미착수 Phase 세부

### R6b: 아이템 트리거 효과
1. `TriggerEffectData` 추상 SO + Subscribe/Unsubscribe
2. 구현체 2~3종 선정·구현
3. `ItemData`에 `List<TriggerEffectData> triggers` 필드 추가, `ItemInventory.TryAdd`/제거에서 Subscribe/Unsubscribe 연결
4. **수용 기준**: 트리거 효과를 가진 ItemData 획득 시 해당 이벤트에서 효과 발동

### R8b: LevelUpSystem 풀 확장 (R8a 완료 후)
1. 풀 가중치 / Luck 스탯이 고등급 ItemData 풀 확률에 반영
2. 같은 선택지 연속 등장 방지 (직전 LevelUp 기억)
3. 회복 카테고리 (별도 ILevelUpChoice 구현체 — HpRestoreChoice 등)
4. 아이콘·등급 색상 표시 (StatIconRegistry + Tier 컬러)
5. (완료) ItemInventory의 임시 디버그 핫키 제거 — DebugPanel(`)로 흡수
6. **수용 기준**: 매 레벨업마다 다양한 카테고리·등급 등장, 빌드 다양성 체감

### R9: 무기 프레임워크 (확장)
1. `WeaponBase` 추상 + `WeaponData` SO
2. `ForwardWeapon` / `AutoAimNearestWeapon` 구현
3. 아이템에 "무기 슬롯 추가" 카테고리 통합
4. **수용 기준**: 새 무기 획득 시 동시 발사, 같은 무기 재선택 시 레벨업

### R10: 밸런싱 & 폴리싱 (PC 완성)
추진력/마찰/회전속도 튜닝, 등급 스폰 곡선, 아이템 가중치 조정

### R11: 모바일 대응
`JoystickRotationInput` + 이동/사격 가상 컨트롤, 세로 UI 재배치

## 미결정 사항

- maxSpeed 클램프 제거 후 플레이 감각 (어색하면 부활 검토)
- 에너지 0일 때 빈클릭 사운드 유지 여부
- R6b 트리거 효과 첫 풀 구체 선정
- 등급별 스폰 곡선 곡률 (선형/지수형)
- Luck 효과 곱: 1포인트당 드롭률 +X%, 고등급 가중치 +Y배 — 수치 미정
- 카메라 부드러운 추적: 현재 직접 고정, 추후 Cinemachine 검토
- 구 문서 처리: 리워크 완료 후 [GameDesign.md](GameDesign.md) / [TechnicalSpec.md](TechnicalSpec.md) 갱신

## 폴더 구조 (DevelopmentGuide 가이드 준수)

- `Assets/Scripts/<Category>/` — 카테고리 폴더 (Player/Enemy/Drop/Item/Stats/Systems/UI/Utils/Core/Network)
- `Assets/Prefabs/<Category>/` — Drops/UIs/(Enemies/Player 등은 평면)
- `Assets/Data/<Category>/` — Players(StatsData), Items(ItemData) 등 SO 자산
- `Assets/Editor/Drawers/` — 영구 PropertyDrawer
- `Assets/Editor/Tools/` — 메뉴 도구 (필요 시 일회성 도구는 사용 후 폐기)

> **자산 이동 시**: `.meta` 파일을 함께 옮겨야 GUID 보존(인스펙터 참조 깨짐 방지). `git mv`가 안전.

## 스크립트 명명 규칙

- 모든 신규 C# 클래스 파일에 `kangtoe99_` 접두어
- 아트는 Unity 기본 프리미티브 + 색상 구분 (`Tilesheet`, `Simple Vector Icons` 임포트됨)
