# 뱀서 스타일 리워크 작업 문서

## 개요

BumpOrBlast를 현재의 **2D 탑다운 슈터**에서 **뱀파이어 서바이버 계열 로그라이크**로 전환한다. 기존 시스템 중 재사용 가능한 부분(레벨업 시스템, 적 스폰 난이도 스케일링, 아이템 드롭 등)을 최대한 살리고, 조작/카메라/무기 체계만 선택적으로 교체한다.

- **작성일**: 2026-04-21
- **상태**: 설계 확정, 구현 미착수
- **관련 문서**: [GameDesign.md](GameDesign.md) (현재 버전), [TechnicalSpec.md](TechnicalSpec.md)

## 설계 확정 사항

### 1. 조작 체계
- **자동 전진** + **회전만 플레이어 제어** (Luftrausers / Asteroids 계열)
- 물리 기반: 추진력(AddForce) + 드래그(linearDamping) 평형으로 일정 속도 *지향*
- 속도 직접 대입(`rb.linearVelocity = ...`) 금지 — 충돌 피드백 보존
- **회전은 보간 기반**: 입력이 "목표 방향(Vector2)"을 제공 → 플레이어 컴포넌트가 `Mathf.MoveTowardsAngle`로 `rotationSpeed(도/초)` 만큼 보간 회전. 스냅 회전 금지 (마우스 에임 슈터화 방지)
- `rotationSpeed` 초기 튜닝 범위: **180~360°/sec**
- 회전 보간 + 관성의 불일치로 자연스러운 드리프트 발생 → 의도된 재미 요소

### 2. 카메라 & 필드
- **카메라 플레이어 추적** (현재: 고정 + 화면 경계 wrap-around)
- **오픈 필드**: 맵 경계 없음. 적이 플레이어로부터 일정 거리 이상 멀어지면 **반대편 원주상에 재등장**
- **완전 무한감 지향**: 안개·장벽·비주얼 경계 힌트 없음. 플레이어가 "세상이 끝없이 이어진다"고 느끼도록 설계
- 배경은 반복 타일링 머티리얼 또는 그리드 단일 배경 사용 (무한 타일 맵 아님 — 배경만 반복)

### 3. 무기 체계
- **기본 무기**: 진행 방향 고정 자동 발사 (플레이어 스킬 축)
- **추가 무기**: 업그레이드로 해금 — 자동 조준, 공전, 장판 등 (빌드 다양성 축)
- 뱀서 원작처럼 **수동 조준 없음**, 모든 무기는 자동 발사

### 4. Bump 데미지 (양방향 교환형)
- 플레이어-적 충돌 시 **양쪽 모두 데미지**
- 업그레이드로 **받는 데미지 배율↓** / **주는 데미지 배율↑** 독립 조정 가능
- "Bump" 계열 빌드가 "Blast"(무기) 계열과 별도 축으로 성립

### 5. 로그라이크 선택지
- 기존 [kangtoe99_LevelUpSystem](../Assets/Scripts/Systems/kangtoe99_LevelUpSystem.cs) 구조 재사용
- 선택지 풀 확장 (현재: 4개 스탯 고정 → 변경 후: 하단 표 참고)

### 6. 플랫폼 & 입력
- **타겟**: PC 우선 출시, 모바일(세로) 대응 준비
- **모바일 방향**: 세로(Portrait) — 캐주얼 모바일 접근성 우선
- **회전 입력 추상화** (플랫폼 공통):
  - PC: 마우스 커서 월드 좌표 → 목표 방향
  - 모바일: 플로팅 조이스틱 드래그 벡터 → 목표 방향
  - 플레이어 컴포넌트는 `IRotationInput.GetTargetDirection(Vector2 playerPos)` 만 참조, 구현체를 모름
- 자동 전진·자동 발사이므로 추가 조작 입력 없음 → **한 손가락/마우스 한 개만으로 플레이 가능**

## 선택지 풀 확장안

| 카테고리 | 예시 | 비고 |
|---|---|---|
| 기본 무기 스탯 | 데미지, 연사, 관통, 탄속 | 기본 무기(ForwardWeapon)에만 적용 |
| 새 무기 획득 | 자동 조준 레이저, 공전 드론, 장판 등 | 소지하지 않은 WeaponData 중 랜덤 |
| 보유 무기 레벨업 | 해당 무기 스탯 개별 강화 | 최대 레벨 도달 시 풀에서 제외 |
| 패시브 (공통) | 최대 HP, 이동속도(평형속도), **회전 속도**, 경험치량, 픽업 범위 | 회전 속도는 양날의 검 — 빨라질수록 조향↑이지만 전진 방향도 쉽게 바뀜 |
| Bump 계열 | 충돌 데미지↑, 충돌 관통, 충돌 시 폭발, 돌진 부스트 | 게임 정체성 유지 |
| 방어 계열 | 받는 데미지↓, 무적 프레임↑, 마찰 저항(밀림↓) | 탱커 빌드 |

**가중치 규칙**: 무기 슬롯이 꽉 차면 "새 무기 획득" 확률 0. 레벨업 간 동일 선택지 연속 출현 방지.

## 기존 시스템 활용 계획

### 그대로 재사용
| 파일 | 역할 | 비고 |
|---|---|---|
| [kangtoe99_LevelUpSystem.cs](../Assets/Scripts/Systems/kangtoe99_LevelUpSystem.cs) | 로그라이크 선택지 | 선택지 종류만 확장 |
| [kangtoe99_GameManager.cs](../Assets/Scripts/Systems/kangtoe99_GameManager.cs) | 게임 상태 관리 | 그대로 |
| [kangtoe99_ScoreSystem.cs](../Assets/Scripts/Systems/kangtoe99_ScoreSystem.cs) | 점수 | 그대로 |
| [kangtoe99_ItemDropSystem.cs](../Assets/Scripts/Item/kangtoe99_ItemDropSystem.cs) | XP/회복/폭탄 드롭 | 그대로 |
| [kangtoe99_EnemyData.cs](../Assets/Scripts/Enemy/kangtoe99_EnemyData.cs) | 적 ScriptableObject | 그대로 |

### 부분 수정
| 파일 | 변경 내용 |
|---|---|
| [kangtoe99_Character.cs](../Assets/Scripts/Core/kangtoe99_Character.cs) | 이동 모델 유지 (AddForce 기반). Bump 데미지 배율 필드 추가 (받는/주는) |
| [kangtoe99_Player.cs](../Assets/Scripts/Player/kangtoe99_Player.cs) | 화면 wrap-around 제거, 방향키 입력 제거. 마우스 방향 회전만 유지. 자동 전진 로직 추가 |
| [kangtoe99_Enemy.cs](../Assets/Scripts/Enemy/kangtoe99_Enemy.cs) | EnemyRegistry 등록/해제 추가. 충돌 데미지 로직 양방향 확인 |
| [kangtoe99_EnemySpawner.cs](../Assets/Scripts/Enemy/kangtoe99_EnemySpawner.cs) | 스폰 위치: 화면 밖 4방향 → 플레이어 중심 원주 랜덤. 거리 초과 적 재배치(반대편 원주)로 변경 |
| [kangtoe99_AspectRatioController.cs](../Assets/Scripts/Systems/kangtoe99_AspectRatioController.cs) | 그대로 유지 가능, 카메라 추적 스크립트와 분리 |

### 폐기 또는 대체
| 파일 | 사유 |
|---|---|
| [kangtoe99_PlayerShooting.cs](../Assets/Scripts/Player/kangtoe99_PlayerShooting.cs) | 단일 무기 가정 + 수동 발사 구조 → 새 무기 프레임워크로 대체 |
| 탄창/재장전 관련 UI 및 로직 | 뱀서 체계에는 탄창/재장전 개념 없음 |

## 새 아키텍처

### 무기 시스템 (ScriptableObject 기반)

```
kangtoe99_WeaponBase (MonoBehaviour, abstract)
  ├─ WeaponData data (SO 참조)
  ├─ int level
  ├─ float cooldownTimer
  ├─ abstract Vector2 GetFireDirection()
  └─ protected Fire() — 투사체 생성, 쿨다운 리셋

kangtoe99_ForwardWeapon       : 기본 무기, GetFireDirection = transform.up
kangtoe99_AutoAimNearestWeapon: 가장 가까운 적 방향
kangtoe99_OrbitWeapon         : 플레이어 주위 공전 (타겟팅 무관, 별도 프리팹 스폰 루틴)
kangtoe99_AreaWeapon          : 주기적 장판 (방향 개념 없음)
kangtoe99_MultiShotWeapon     : 가장 가까운 N명 동시
```

**WeaponData (ScriptableObject)**:
- `damage`, `fireRate`, `projectileSpeed`, `projectilePrefab`
- `pierceCount`, `lifetime`
- `maxLevel`, `levelStatCurve` — 레벨별 스탯 배율
- `displayName`, `icon`, `description`

**플레이어는 `List<kangtoe99_WeaponBase>` 보유**. 레벨업 시스템이 이 리스트에 새 컴포넌트를 AddComponent하거나 기존 컴포넌트의 level을 증가.

### 회전 입력 추상화

```
kangtoe99_IRotationInput (interface)
  └─ Vector2 GetTargetDirection(Vector2 playerWorldPos)

kangtoe99_MouseRotationInput    : 마우스 월드 좌표 - 플레이어 좌표
kangtoe99_JoystickRotationInput : 플로팅 조이스틱 드래그 벡터
```

- 플레이어는 `IRotationInput` 만 보유 — 입력 소스를 모름
- 플랫폼 감지(`Application.isMobilePlatform` 또는 `#if UNITY_ANDROID || UNITY_IOS`)로 구현체 자동 장착
- 에디터 테스트 편의를 위해 강제 전환 토글 제공
- 실제 회전 로직은 플레이어 컴포넌트에서 `Mathf.MoveTowardsAngle(current, target, rotationSpeed * dt)` 로 보간

### EnemyRegistry (성능 핵심)

```
kangtoe99_EnemyRegistry (static)
  ├─ static List<kangtoe99_Enemy> ActiveEnemies
  ├─ Register(Enemy) / Unregister(Enemy)
  └─ FindNearest(Vector2 pos, float maxDist) — O(n) 단순 순회, 필요 시 공간분할로 확장
```

- Enemy.OnEnable에서 Register, OnDisable에서 Unregister
- 자동 조준 무기는 매 프레임이 아닌 **0.1~0.2초 주기**로만 타겟 캐싱
- 적 수 100~300 예상 범위에서 O(n) 순회로도 충분, 필요 시 QuadTree 도입 검토

### 카메라 추적
- 별도 `kangtoe99_CameraFollow.cs` 컴포넌트 신설 (또는 Cinemachine 도입)
- AspectRatioController는 레터박스 비율 유지 역할만 담당 (추적과 직교)

### 필드 리사이클 로직 (오픈 필드)
- EnemySpawner가 매 프레임 ActiveEnemies 중 `distance > cullRadius` 인 적 검색
- 해당 적을 삭제하지 않고, 플레이어 기준 반대편 원주 (스폰 반경 내 랜덤 위치)로 **재배치**
- 결과: "세상이 무한"한 것처럼 보이지만 적 인스턴스는 재활용됨

## 단계별 구현 계획

각 단계는 독립적으로 테스트 가능한 단위로 설계. 단계 완료마다 플레이 테스트 후 다음 단계 진행.

### Phase R1: 조작·카메라 기초
**목표**: 자동 전진 + 보간 회전 + 카메라 추적이 동작하는 상태 (PC 기준).
1. [kangtoe99_Player.cs](../Assets/Scripts/Player/kangtoe99_Player.cs)에서 화면 wrap-around 로직 제거
2. 방향키 입력 제거, 자동 전진 로직 추가 (`AddForce(transform.up * thrust)`, linearDamping 튜닝)
3. `kangtoe99_IRotationInput` 인터페이스 + `kangtoe99_MouseRotationInput` 구현체 추가 (PC 기본 장착)
4. 플레이어 회전 로직: 목표 방향 받아 `Mathf.MoveTowardsAngle` 보간, `rotationSpeed` 파라미터화
5. `kangtoe99_CameraFollow.cs` 신설, 카메라를 플레이어 추적
6. AspectRatioController는 그대로 유지
7. **수용 기준**: 조작 시 관성·미끄러짐이 느껴지고, 회전이 부드럽게 보간되며, 카메라가 플레이어를 따라 이동. 화면 경계에서 튀지 않음.

### Phase R2: 오픈 필드 + 적 리사이클
**목표**: 적이 무한히 몰려오는 느낌.
1. `kangtoe99_EnemyRegistry.cs` 신설, Enemy에 등록/해제 훅 추가
2. [kangtoe99_EnemySpawner.cs](../Assets/Scripts/Enemy/kangtoe99_EnemySpawner.cs) 스폰 위치를 플레이어 기준 원주 랜덤으로 변경
3. "거리 초과 적 삭제" → "거리 초과 적 반대편 원주 재배치"로 로직 교체
4. 배경을 타일링 머티리얼 또는 그리드로 교체 (무한 감각)
5. **수용 기준**: 플레이어가 한 방향으로 계속 전진해도 적이 사방에서 계속 등장하며, 멀리 떨어진 적이 사라지지 않고 재사용됨.

### Phase R3: Bump 데미지 시스템
**목표**: 충돌이 양방향 전투로 성립.
1. [kangtoe99_Character.cs](../Assets/Scripts/Core/kangtoe99_Character.cs)에 `incomingDamageMultiplier`, `outgoingDamageMultiplier` 필드 추가
2. 충돌 시 양쪽에 데미지 적용, 플레이어에 짧은 무적 프레임(0.3~0.5s)
3. TakeDamage 계산 시 배율 반영
4. **수용 기준**: 적 무리에 돌진하면 적을 밀치며 데미지를 주고, 플레이어도 HP가 깎임.

### Phase R4: 무기 프레임워크 (기본 무기만)
**목표**: 새 무기 시스템 토대 완성, 기본 무기로 게임 성립.
1. `WeaponData` SO 클래스 정의
2. `kangtoe99_WeaponBase` 추상 컴포넌트 작성
3. `kangtoe99_ForwardWeapon` 구현 (진행 방향 자동 발사)
4. [kangtoe99_PlayerShooting.cs](../Assets/Scripts/Player/kangtoe99_PlayerShooting.cs) 제거 또는 비활성화
5. 기본 무기 1종 데이터 에셋 생성, 플레이어에 장착
6. 탄창/재장전 UI 제거
7. **수용 기준**: 마우스 없이도 진행 방향으로 자동 발사되며, 기존처럼 적을 처치할 수 있음.

### Phase R5: 레벨업 선택지 풀 확장
**목표**: 로그라이크 루프가 뱀서 형태로 성립.
1. LevelUpSystem의 선택지 구조 확장 (기존 고정 4종 → 동적 풀)
2. 선택지 카테고리별 로직:
   - 기본 무기 스탯 증가
   - 패시브 (HP, 이동속도=평형속도, 경험치량, 픽업 범위)
   - Bump 계열 (충돌 데미지↑, 받는 데미지↓)
3. LevelUpPanel UI 기존 구조 유지 (4지선다)
4. **수용 기준**: 레벨업 시마다 다양한 선택지가 등장하며, 스탯이 정상 반영됨.

### Phase R6: 추가 무기 (자동 조준)
**목표**: 빌드 다양성 확보.
1. `kangtoe99_AutoAimNearestWeapon` 구현 (EnemyRegistry 사용, 타겟 캐싱)
2. `kangtoe99_OrbitWeapon` 구현 (공전 드론 프리팹)
3. 각 무기 WeaponData SO 에셋 생성
4. 레벨업 선택지 풀에 "새 무기 획득" 카테고리 추가
5. 보유 무기 레벨업 선택지 추가
6. **수용 기준**: 레벨업에서 새 무기를 획득하면 실제 전투에 투입되고, 같은 무기가 다시 선택지로 등장 시 레벨업됨.

### Phase R7: 밸런싱 & 폴리싱 (PC 완성)
**목표**: PC 플레이 가능한 완성도.
1. 추진력/드래그/질량 튜닝 (조작감)
2. 회전 속도(rotationSpeed) 튜닝
3. 각 무기 데미지·쿨다운 밸런싱
4. 레벨업 선택지 가중치 조정
5. 적 스폰 곡선 재조정
6. **수용 기준**: 5~10분 1회차 플레이가 지루하지 않고, 다양한 빌드 시도가 가능.

### Phase R8: 모바일 대응 (세로)
**목표**: Android/iOS 빌드에서 플레이 가능.
1. `kangtoe99_JoystickRotationInput` 구현 (플로팅 조이스틱 — 화면 터치 시 그 자리에 조이스틱 출현)
2. Joystick UI 프리팹 (Canvas ScreenSpace-Overlay, 데드존 0.1~0.2)
3. 플랫폼 감지로 `IRotationInput` 구현체 자동 선택 + 에디터 강제 전환 토글
4. AspectRatioController 세로(Portrait) 대응 확장
5. UI 재배치: HP바/스코어 상단, 레벨업 패널 세로 레이아웃
6. 세로 시야 차이 보정: 스폰 반경·적 속도 재튜닝 (가로 대비 좌우 시야 좁아짐)
7. 터치 디바이스 해상도별 UI 스케일 보정
8. **수용 기준**: 한 손가락 플로팅 조이스틱으로 회전 제어, 세로 화면에서 UI·게임플레이가 정상 동작.

## 미결정 사항 / 추후 결정

- **카메라 부드러운 추적 (보간)**: 현재는 직접 lock (target 위치에 즉시 붙음) 사용. SmoothDamp/Lerp 모두 jitter 발생, Rigidbody2D kinematic + FixedUpdate 방식도 해결 안 됨. 핵심 시스템 구현 후 Cinemachine 도입 또는 원인 재조사 필요
- **기본 무기 조작 강도**: 항상 진행 방향 발사 vs 마우스 조준 유지 병행 (현재는 전자로 확정, 플레이 테스트 후 재검토 여지)
- **브레이크/부스트 버튼**: Phase R1에서 기본 없이 구현 → 플레이 테스트 후 필요 시 추가
- **적 타입 확장**: 현재 3종(일반/빠른/탱크). 자동 전진 + 회전만 체계에서는 **돌진형/포위형/저격형** 추가가 재미를 좌우할 수 있음 (Phase R7 이후 확장 콘텐츠)
- **조이스틱 활성 영역**: 화면 전체 어디든 플로팅 vs 좌하단 영역 제한 (모바일 왼손/오른손잡이 대응)
- **모바일 입력 에디터 테스트**: Unity Remote 활용 vs 에디터에서 조이스틱 강제 활성화 토글 (또는 둘 다)
- **랜드스케이프 모바일 지원**: 향후 태블릿 유저 대응 여부 (AspectRatioController 확장 범위 결정)
- **구 문서 처리**: 리워크 완료 후 [GameDesign.md](GameDesign.md) / [DevelopmentPlan.md](DevelopmentPlan.md) / [TechnicalSpec.md](TechnicalSpec.md) 및 [README.md](../README.md) (현재 "PC (Unity)" 명시) 업데이트 또는 deprecated 처리

## 스크립트 명명 규칙 (기존 유지)

- 모든 신규 C# 클래스 파일에 `kangtoe99_` 접두어 사용 ([TechnicalSpec.md](TechnicalSpec.md))
- 아트 애셋은 Unity 기본 프리미티브만 사용, 색상으로 구분
