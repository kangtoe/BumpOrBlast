# 시스템 리워크 작업 문서

## 개요

BumpOrBlast의 **2D 탑다운 슈터 정체성은 유지**한 채, 그 위에 **로그라이크 성장·진행 시스템 계층**을 추가하고 기존 시스템을 심화·확장한다.

- 슈팅의 핵심(WASD 이동 + 마우스 회전·사격, Bump 충돌)은 그대로
- 탄창 → **에너지** / 분산된 수치 → **중앙 스탯** / 일회성 드롭 → **영구 아이템**으로 확장
- 적 5등급 분류, 오픈 필드, 카메라 추적 등 **로그라이크 장르(특히 뱀파이어 서바이버)에서 영감 받은 요소** 일부 도입

> 참고: 이 문서는 원래 "VampireSurvivorsRework.md"라는 이름으로 시작했고 초기에는 장르 전환을 목표로 했으나, 2026-05-12 사용자 재검토로 **자동 전진·자동 사격을 폐기**하면서 방향성이 "전환"에서 "시스템 심화"로 재정립되었다.

- **작성일**: 2026-04-21
- **최근 갱신**: 2026-05-12 — 방향성 재정립 + 자동전진·자동사격 폐기, 에너지·스탯·아이템·적 5등급 결정 추가
- **상태**: Phase R1~R3 코드 반영 (회전 입력 추상화, 카메라 추적, EnemyRegistry, 탄창 제거, PlayerStats)
- **관련 문서**: [GameDesign.md](GameDesign.md), [TechnicalSpec.md](TechnicalSpec.md), [DevelopmentGuide.md](DevelopmentGuide.md)

## 방향성

### 유지하는 것 (슈터 정체성)
- WASD 4방향 자유 이동 + 마우스 회전·좌클릭 사격
- 발사체 기반 공격 (자동 발사 X)
- 적 추적 AI + Bump 충돌 데미지
- 기존 LevelUpSystem, ScoreSystem, GameManager 흐름

### 확장·대체하는 것 (시스템 심화)
- 탄창·재장전 → 에너지 시스템 (시간 자동 회복)
- 분산된 수치(Player·PlayerShooting SerializeField) → 중앙 PlayerStats + Modifier
- 일회성 드롭(XP/회복/폭탄)만 있던 진행 → 영구 효과 **아이템** 추가
- LevelUpSystem 선택지 2종 → 다중 카테고리 풀 (스탯·아이템·Bump·회복)
- 단일 적 타입 → **5등급 색상 분류** (Gray/Green/Blue/Purple/Orange)
- 화면 wrap-around → 오픈 필드 + 카메라 추적 + 적 리사이클

### 로그라이크 장르에서 차용한 요소
- 영구 성장 아이템 (뱀서·노이타 등의 패시브 빌드)
- 오픈 필드 + 무한 적 리사이클
- 5등급 희귀도 색상 (디아블로 계열)

## 변경 이력

### 2026-05-12 결정
1. **방향성 재정립** — "장르 전환"에서 "시스템 심화"로 (문서 이름도 SystemsRework로 변경)
2. **자동 전진 컨셉 폐기** → 기존 WASD 4방향 자유 이동 유지
3. **자동 사격 컨셉 폐기** → 기존 마우스 입력 사격 유지 (탄창은 폐기, fireRate 연사 유지)
4. **에너지 시스템 신설** (탄창 대체, 시간 자동 회복)
5. **중앙 PlayerStats + Modifier 구조 신설**
6. **아이템 시스템 신설** (영구 효과, 레벨업 선택지로 획득)
7. **드롭(Drop)/아이템(Item) 명명 분리** (기존 `ItemDropSystem` → `DropSystem` 리네임)
8. **적 5등급 색상 분류** (Gray/Green/Blue/Purple/Orange)
9. **아이템 5등급 색상 분류** (적과 동일 색계, 풀 구분 의미)

## 설계 확정 사항

### 1. 조작 체계
- **WASD/방향키 4방향 자유 이동** (월드 좌표 기준)
- 물리 기반 이동: `AddForce(moveDirection * moveForce)` + maxSpeed 클램프 + LinearDamping 마찰 (`kangtoe99_Character.Move`)
- **회전은 마우스 보간 기반**: `IRotationInput`이 목표 방향(Vector2) 제공 → `Mathf.MoveTowardsAngle`로 `rotationSpeed(도/초)` 만큼 보간
- 회전 속도 초기 튜닝 범위: **180~360°/sec**, 스탯 시스템으로 업그레이드 가능
- **사격은 마우스 좌클릭 누름 감지 + fireRate 쿨다운** (자동 연사). 단 에너지 부족 시 발사 불가

### 2. 카메라 & 필드
- **카메라 플레이어 추적** ([kangtoe99_CameraFollow.cs](../Assets/Scripts/Systems/kangtoe99_CameraFollow.cs))
- **오픈 필드**: 화면 wrap-around 폐기. 적이 cullRadius 초과 시 플레이어 기준 원주에 재배치 ([kangtoe99_EnemySpawner.cs](../Assets/Scripts/Enemy/kangtoe99_EnemySpawner.cs))
- **완전 무한감 지향**: 안개·장벽 없음. 배경은 [kangtoe99_GridBackground.cs](../Assets/Scripts/Systems/kangtoe99_GridBackground.cs)로 카메라 위치 기반 무한 타일링

### 3. 무기 체계
- **기본 무기**: 마우스 방향 발사 (현재 `kangtoe99_PlayerShooting` → 추후 `ForwardWeapon`으로 리팩터)
- **추가 무기**: 업그레이드로 해금 — 자동 조준, 공전 드론, 장판 등
- 모든 무기는 사격 시 **에너지 소모**, 에너지 부족 시 발사 불가

### 4. Bump 데미지 (양방향 교환형)
- 플레이어-적 충돌 시 양쪽 모두 데미지
- 업그레이드로 `BumpDamageDealtMul`(주는 배율) / `BumpDamageTakenMul`(받는 배율) 독립 조정
- Bump 빌드 = 무기 빌드와 별도 축

### 5. 플랫폼 & 입력
- **타겟**: PC 우선 출시, 모바일(세로) 대응 준비
- **입력 추상화**:
  - 회전: `IRotationInput` (PC=마우스, 모바일=조이스틱)
  - 이동: WASD/방향키 (모바일은 화면 좌측 가상 조이스틱)
  - 사격: 마우스 좌클릭 (모바일은 화면 우측 사격 버튼)

## 스탯 시스템

### 구조
중앙 `kangtoe99_PlayerStats` 컴포넌트가 모든 스탯의 단일 진리원천. 스탯별 base값을 보유하고 외부 modifier 리스트로 동적 보정.

```
kangtoe99_PlayerStats (MonoBehaviour)
  ├─ Dictionary<StatType, float> baseValues
  ├─ List<IStatModifier> modifiers
  ├─ float GetFinal(StatType) — base + Σ(가산) → ×Π(배율)
  ├─ void AddModifier(IStatModifier)
  └─ void RemoveModifier(IStatModifier)

IStatModifier
  ├─ StatType Stat
  ├─ ModifierKind Kind (Additive | Multiplicative)
  ├─ float Value
  └─ object Source (디버깅/제거 용도 — 아이템/레벨업/일시버프 식별)
```

### 스탯 목록
| 카테고리 | 스탯 | 비고 |
|---|---|---|
| 발사체 | ProjectileCount | 동시 발사 개수 |
| 발사체 | ProjectileSpeed | 탄속 |
| 발사체 | ProjectileScale | 크기 배율 |
| 발사체 | ProjectileSpread | 산탄 각도 (도) — Count≥2일 때 유효 |
| 발사체 | Pierce | 관통 횟수 |
| 무기 | Damage | 기본 데미지 |
| 무기 | FireRate | 발사 간격(초) — 작을수록 빠름 |
| 무기 | EnergyCostPerShot | 발당 에너지 소모 |
| 에너지 | EnergyMax | 최대 에너지 |
| 에너지 | EnergyRegen | 초당 회복량 |
| 기체 | MaxHP (Durability) | 최대 체력 |
| 기체 | HPRegen (Repair) | 초당 자연 회복 |
| 기체 | BodyScale | 기체 크기 (충돌 판정 영향) |
| 이동 | MoveSpeed | 최대 이동 속도 |
| 이동 | RotationSpeed | 초당 회전 각도 |
| 이동 | Friction | LinearDamping 값 (높을수록 빨리 정지) |
| Bump | BumpDamageDealtMul | 충돌로 주는 데미지 배율 |
| Bump | BumpDamageTakenMul | 충돌로 받는 데미지 배율 |
| 메타 | Luck | 드롭 확률 + 고급 선택지 등장률 |
| 메타 | PickupRange | 드롭 자동 흡수 반경 |

### 적용 흐름
- `PlayerShooting` 등 소비자는 `stats.GetFinal(StatType.Damage)` 형태로 조회 (캐싱 가능)
- 아이템 획득 시 해당 아이템의 modifier들을 `AddModifier`로 등록
- 일시 버프(예: 픽업 시 5초 데미지↑)도 같은 인터페이스, Source로 만료 시 제거

## 에너지 시스템

탄창·재장전을 대체. 발사 시 소모, **시간 자동 회복만** (Bump 회복·적 처치 회복 없음).

```
kangtoe99_EnergySystem (MonoBehaviour, Player 부착)
  ├─ float current
  ├─ float Max => stats.GetFinal(EnergyMax)
  ├─ float Regen => stats.GetFinal(EnergyRegen)
  ├─ bool TryConsume(float amount) — 충분하면 차감 후 true
  └─ Update(): current = Mathf.Min(Max, current + Regen * dt)
```

- `PlayerShooting.Shoot()` 안에서 `if (!energy.TryConsume(stats.GetFinal(EnergyCostPerShot))) return;`
- UI: 기존 탄창 UI를 에너지 게이지로 대체 (단일 바 또는 세그먼트)

## 아이템 / 드롭 구분

### 명명
- **Drop (드롭)**: 적이 떨어뜨리는 일시 픽업. XP, HP 회복, 폭탄 등. 기존 [kangtoe99_ItemDropSystem.cs](../Assets/Scripts/Item/kangtoe99_ItemDropSystem.cs) → `kangtoe99_DropSystem`으로 리네임 권장
- **Item (아이템)**: 레벨업 선택지로 획득하는 영구 효과. 스탯 modifier + 트리거 효과 가능

### 아이템 데이터 (ScriptableObject)
```
kangtoe99_ItemData : ScriptableObject
  ├─ string displayName
  ├─ Sprite icon
  ├─ string description
  ├─ ItemTier tier (Gray/Green/Blue/Purple/Orange)
  ├─ int maxStack — 이 아이템의 최대 중첩 보유 개수
  ├─ List<StatModifierData> modifiers — 스탯 보너스
  └─ List<TriggerEffectData> triggers — 특수 효과 (고등급 풀에 주로)
```

### 등급 = 풀 구분
- Gray/Green/Blue/Purple/Orange 각각 별개의 아이템 풀
- 고등급 풀일수록 더 강한 modifier 수치 + 트리거 효과(예: "HP 50% 이하 시 데미지+30%", "Bump 시 폭발")
- 상위 등급 등장 확률은 행운 스탯과 레벨에 비례

### 보유 제한
- 종류별(스탯·트리거) 보유 수 제한 없음
- 단 같은 아이템(동일 ScriptableObject)은 `maxStack` 까지만 중첩

### 트리거 효과 (확장형)
```
TriggerEffectData (abstract ScriptableObject)
  ├─ Subscribe(player) — 이벤트 후킹
  └─ Unsubscribe(player)

예시 구현체:
  - OnLowHpDamageBoost — HP가 threshold 이하일 때 Damage modifier 추가
  - OnBumpExplode — 적과 충돌 시 폭발 데미지
  - OnKillSpawnProjectile — 적 처치 시 추가 투사체
```

## 적 5등급 시스템

### 등급 정의
| 등급 | 색상 | HP/Damage/Speed | 드롭 |
|---|---|---|---|
| Gray | 회색 | 기본 1배 | XP 소량, 일반 풀 |
| Green | 초록 | 1.5배 | XP 1.5배, Green+ 풀 확률 |
| Blue | 파랑 | 2.5배 | XP 3배, Blue+ 풀 확률 |
| Purple | 보라 | 4배 | XP 6배, Purple+ 풀 확률 |
| Orange | 주황 | 8배 (미니보스급) | XP 10배 + 확정 고등급 드롭 |

### 구조
- `kangtoe99_EnemyData` ScriptableObject에 `EnemyTier tier` 필드 추가
- 등급별 수치 스케일은 코드 상수 또는 별도 `EnemyTierCurve` ScriptableObject로 관리
- 행동 패턴은 등급과 독립 (기존 Normal/Fast/Heavy 행동 타입 유지 가능)
- 스폰 확률은 시간(난이도 곡선)에 따라 상위 등급 비율 증가

### 시각
- 스프라이트는 동일, 색상만 등급별 머티리얼 또는 SpriteRenderer.color로 차등
- 크기는 등급별 약간 차등 가능 (Orange는 명확히 크게)

## 선택지 풀 확장안 (LevelUpSystem)

| 카테고리 | 가중치 비고 |
|---|---|
| 스탯 강화 (직접 modifier) | 무난한 풀 채움. 발사체·무기·이동·에너지 등 |
| 아이템 획득 (영구) | maxStack 미만인 풀에서 추첨. Gray~Orange 풀별 가중치 |
| Bump 빌드 | 충돌 데미지·받는 데미지·돌진 부스트 등 |
| 회복 옵션 | HP 즉시 회복 (가끔 등장) |

**규칙**:
- 4지선다 유지
- 같은 선택지 연속 등장 방지
- 행운 스탯이 높을수록 Blue+ 풀 등장률 상승
- 보유 중인 아이템이 maxStack 도달 시 풀에서 제외

## 기존 시스템 활용 계획

### 그대로 재사용
| 파일 | 역할 |
|---|---|
| [kangtoe99_GameManager.cs](../Assets/Scripts/Systems/kangtoe99_GameManager.cs) | 게임 상태 |
| [kangtoe99_ScoreSystem.cs](../Assets/Scripts/Systems/kangtoe99_ScoreSystem.cs) | 점수 |
| [kangtoe99_EnemyData.cs](../Assets/Scripts/Enemy/kangtoe99_EnemyData.cs) | 적 데이터 SO (tier 필드만 추가) |

### 부분 수정
| 파일 | 변경 내용 |
|---|---|
| [kangtoe99_Character.cs](../Assets/Scripts/Core/kangtoe99_Character.cs) | Bump 데미지 배율 필드 추가 (받는/주는) |
| [kangtoe99_Player.cs](../Assets/Scripts/Player/kangtoe99_Player.cs) | PlayerStats 컴포넌트 의존 추가. 이동·회전 파라미터는 stats에서 조회 |
| [kangtoe99_PlayerShooting.cs](../Assets/Scripts/Player/kangtoe99_PlayerShooting.cs) | 스탯·에너지 의존으로 전환. 발사 전 에너지 체크 |
| [kangtoe99_LevelUpSystem.cs](../Assets/Scripts/Systems/kangtoe99_LevelUpSystem.cs) | 선택지 풀을 동적 풀로 확장. 스탯·아이템·Bump 카테고리 추가 |
| [kangtoe99_ItemDropSystem.cs](../Assets/Scripts/Item/kangtoe99_ItemDropSystem.cs) | `kangtoe99_DropSystem`으로 리네임. 행운 스탯 반영 |
| [kangtoe99_EnemySpawner.cs](../Assets/Scripts/Enemy/kangtoe99_EnemySpawner.cs) | 등급 스폰 확률 곡선 도입 |

### 신규 추가
| 파일 | 역할 |
|---|---|
| `kangtoe99_PlayerStats.cs` | 중앙 스탯 + Modifier 합산 |
| `kangtoe99_IStatModifier.cs` | 스탯 보정 인터페이스 |
| `kangtoe99_EnergySystem.cs` | 에너지 자원 관리 |
| `kangtoe99_ItemData.cs` (SO) | 아이템 정의 |
| `kangtoe99_ItemInventory.cs` | 보유 아이템 + 스택 추적 |
| `kangtoe99_TriggerEffectData.cs` (SO 추상) | 트리거 효과 베이스 |
| `kangtoe99_EnemyTier.cs` (enum + curve) | 5등급 정의 |

### 폐기 또는 대체
| 파일 | 사유 |
|---|---|
| 탄창/재장전 UI | 에너지 게이지로 대체 |
| `kangtoe99_AmmoUIManager` 등 | 에너지 UI 컴포넌트로 교체 |

## 단계별 구현 계획

### Phase R1: 조작·카메라 기초 (완료)
- IRotationInput + MouseRotationInput, CameraFollow, GridBackground, 화면 wrap-around 제거

### Phase R2: 오픈 필드 + 적 리사이클 (완료)
- EnemyRegistry, EnemySpawner 원주 재배치

### Phase R3: 스탯 시스템 (다음 작업)
1. `IStatModifier` 인터페이스 + `StatType` enum 정의
2. `kangtoe99_PlayerStats` 컴포넌트 작성 (base값 + modifier 합산)
3. Player·PlayerShooting을 PlayerStats 조회 방식으로 리팩터
4. **수용 기준**: 외부에서 modifier를 추가/제거하면 발사 데미지·이동 속도 등이 즉시 반영됨

### Phase R4: 에너지 시스템
1. `kangtoe99_EnergySystem` 컴포넌트 작성
2. PlayerShooting 발사 전 에너지 체크 + 소모
3. UI: 탄창 UI 폐기 → 에너지 게이지
4. **수용 기준**: 연사 시 에너지가 줄고, 멈추면 회복되며, 0일 때 발사 불가

### Phase R5: Bump 데미지 시스템
1. Character에 incoming/outgoing 배율 추가
2. 양방향 데미지 적용 + 무적 프레임
3. **수용 기준**: 적과 접촉 시 양쪽 HP 감소, 배율 modifier로 조정 가능

### Phase R6: 아이템 시스템
1. `kangtoe99_ItemData` SO + 등급 enum
2. `kangtoe99_ItemInventory` 컴포넌트 (Player 부착)
3. 트리거 효과 베이스 클래스 + 기본 구현체 2~3종
4. **수용 기준**: ItemData 에셋을 인벤토리에 추가하면 스탯 modifier·트리거가 발동

### Phase R7: 적 5등급 시스템
1. `EnemyTier` enum + 등급 스케일 데이터
2. EnemyData에 tier 필드 추가, 시각·수치 적용
3. EnemySpawner 등급 확률 곡선
4. 등급별 드롭 확률 차등 (행운 스탯 반영)
5. **수용 기준**: 시간이 지날수록 고등급 적 출현 비율 증가, 처치 시 등급에 맞는 드롭

### Phase R8: LevelUpSystem 풀 확장
1. 동적 선택지 풀 (카테고리별 가중치)
2. 스탯 강화 / 아이템 / Bump / 회복 카테고리 통합
3. 행운 스탯이 고등급 아이템 풀 확률에 반영
4. 4지선다 UI 유지, 아이콘·등급 색상 표시
5. **수용 기준**: 매 레벨업마다 다양한 카테고리가 등장, 빌드 다양성 체감

### Phase R9: 무기 프레임워크 (확장)
1. `WeaponBase` 추상 + ScriptableObject `WeaponData`
2. `ForwardWeapon` / `AutoAimNearestWeapon` 구현
3. 아이템에 "무기 슬롯 추가" 카테고리 통합
4. **수용 기준**: 새 무기를 획득하면 동시 발사, 같은 무기 재선택 시 레벨업

### Phase R10: 밸런싱 & 폴리싱 (PC 완성)
- 추진력/마찰/회전속도 튜닝, 등급 스폰 곡선, 아이템 가중치 조정

### Phase R11: 모바일 대응
- `JoystickRotationInput` + 이동/사격 가상 컨트롤
- 세로 UI 재배치

## 미결정 사항

- **에너지 0일 때 빈클릭 사운드 유지 여부**
- **아이템 트리거 효과 첫 풀에 어떤 효과들을 넣을지** (R6 구현 시 결정)
- **Bump 무적 프레임 길이** (0.3s? 0.5s?)
- **등급별 스폰 곡선 곡률** (선형/지수형)
- **행운 스탯 효과 곱**: 1포인트당 드롭률 +X%, 고등급 가중치 +Y배 — 수치 미정
- **카메라 부드러운 추적**: 현재 직접 lock, 추후 Cinemachine 검토
- **구 문서 처리**: 리워크 완료 후 [GameDesign.md](GameDesign.md) / [TechnicalSpec.md](TechnicalSpec.md) 갱신

### 폴더 구조 마이그레이션 (DevelopmentGuide 가이드 준수)

[DevelopmentGuide.md](DevelopmentGuide.md)의 폴더 규약은 신규 작업부터 적용하되, 기존 평면 구조의 마이그레이션은 추후 검토:

- `Assets/Editor/` 평면 → `Assets/Editor/Tools/` + `Assets/Editor/Validators/` 분리
- `Assets/Prefabs/` 평면 → `Assets/Prefabs/<Category>/` (Player, Enemies, Items, Weapons, World, VFXs, Bosses)
- `Assets/Data/<Category>/` 신설 (현재 SO 자산 인스턴스 없음, 클래스만 존재)

**마이그레이션 시 주의**: `.meta` 파일 함께 이동해야 GUID 보존(인스펙터 참조 깨짐 방지). 시점은 Phase R6(아이템 SO 도입)~R7(적 5등급) 무렵 자산이 늘어나기 시작할 때가 자연스러움.

## 스크립트 명명 규칙 (유지)

- 모든 신규 C# 클래스 파일에 `kangtoe99_` 접두어
- 아트는 Unity 기본 프리미티브 + 색상 구분 (`Tilesheet`, `Simple Vector Icons` 임포트됨)
