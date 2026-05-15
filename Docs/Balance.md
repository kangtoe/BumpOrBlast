# BumpOrBlast - 밸런스 문서

게임 밸런스 튜닝 값과 난이도 설계를 모은 문서. 실제 수치는 인스펙터/SO에서 조정 —
이 문서는 "무엇이 어디에 있고 어떻게 상호작용하는지"의 진리원천.
관련: [SystemsRework.md](SystemsRework.md) (시스템 설계·진행), [GameDesign.md](GameDesign.md)

표의 "현재" 값은 2026-05-15 기준 씬/자산 값 — 변경 시 이 문서도 갱신할 것.

## 난이도 축 (현재 3개)

플레이가 진행되며 적이 강해지는 경로:

| 축 | 적용 시점 | 단위 | 위치 |
|---|---|---|---|
| 1. 등급 escalation | 등급 진행 중 (0 ~ ProgressionDuration) | solid→blend 스텝 | `EnemyTierData` |
| 2. 진행 후 HP 스텝 배율 | 등급 진행 완료 후 | 시간 스텝 | `EnemySpawner` |
| 3. 스폰 속도 증가 | **항상 (t=0부터)** | 스폰당 누적 | `EnemySpawner` |

- 축 1·2는 **순차** — 진행 중엔 1, 진행 완료(마지막 등급 solid 도달) 후엔 2. 동시 누적 아님
- 축 1은 HP·공격력·속도·점수 전부, 축 2는 HP만
- 축 3은 게이팅 없이 t=0부터 항상 적용 — 1·2의 "진행 중/후 분리" 원칙과 어긋남 (→ 미결정)

## 적 등급 (EnemyTierData.asset)

`Assets/Data/Enemy/EnemyTierData.asset` 인스펙터. 등급별 배율은 프리팹 기본 수치 위에 곱해짐.

| 등급 | statMul (HP·Dmg·Speed) | scoreMul (점수·XP) | scaleMul | solidDuration | blendDuration |
|---|---|---|---|---|---|
| Gray | 1 | 1 | 1 | 30 | 20 |
| Green | 1.5 | 1.5 | 1.1 | 30 | 20 |
| Blue | 2.5 | 3 | 1.2 | 30 | 20 |
| Purple | 4 | 6 | 1.35 | 30 | 20 |
| Orange | 8 | 10 | 1.6 | (무시) | (무시) |

- **solid** = 그 등급만 스폰. **blend** = 인접 두 등급을 비율(0→1)로 추첨하며 전환
- `ProgressionDuration` = Σ(solid+blend) of Gray~Purple = **현재 200초**
- 마지막 등급(Orange)의 solid/blend는 무시 — 도달 후 영구 고정
- 시각: 색상은 공용 팔레트(`Assets/Data/TierColorPalette.asset`)에서, 크기는 scaleMul
- 등급 색상은 적·아이템이 공유 — `kangtoe99_TierColorPalette` SO 한 곳에서 관리

## 스폰 / 진행 후 (EnemySpawner 인스펙터)

| 값 | 현재 | 의미 |
|---|---|---|
| initialSpawnInterval | 5 | 시작 스폰 간격(초) |
| minSpawnInterval | 1.5 | 최소 간격 (하한) |
| intervalDecreaseRate | 0.02 | 스폰당 간격 감소량 (t=0부터 항상) |
| postStepDuration | 30 | 진행 후 HP 스텝 길이(초) |
| postStepIncrement | 0.5 | 스텝당 HP 배율 가산 (**선형, 복리 아님**) |
| postMaxMultiplier | 4 | 진행 후 HP 배율 상한 |

- 진행 후 HP 배율 = `min(1 + floor(over / postStepDuration) × postStepIncrement, postMaxMultiplier)`,
  `over = elapsed - ProgressionDuration`
- `postStepIncrement 0.5` → 스텝마다 기준 HP의 +50%p (1.0 → 1.5 → 2.0 …), 6스텝째 ×4 상한 도달

## 챔피언 (EnemySpawner 인스펙터)

| 값 | 현재 | 의미 |
|---|---|---|
| championCheckInterval | 30 | 챔피언 출현 시도 주기(초) |
| championChance | 0.5 | 주기마다 실제 출현 확률 |
| championStatMultiplier | 3 | 챔피언 수치 배율 (등급 배율 위에 추가) |
| championScaleMultiplier | 1.8 | 챔피언 스케일 배율 |

- 등급과 **별개 축**. 새 적이 아니라 기존 적의 강화판
- **모든 적이 XP orb 드롭** — 점수는 orb 픽업 시점에서만 가산. 챔피언만 보너스(폭탄/회복) 추가

## 드롭 (DropSystem 인스펙터)

| 값 | 현재 | 의미 |
|---|---|---|
| championBombWeightAtFullHP | 1 | HP 가득(=1) 시 폭탄 선택 가중 — hpRatio에 곱해진다 |
| championHealWeightAtZeroHP | 1 | HP 0(=1) 시 회복 선택 가중 — (1-hpRatio)에 곱해진다 |

### 일반 적 처치
- `DropSystem.DropEnemy(pos, dir, scoreValue)` → XP orb 1개
- XP orb 가치는 처치한 적의 (등급 배율 적용된) scoreValue
- XP orb는 `Magnet` 스탯 반경 내에서 플레이어 쪽으로 인력, lifetime 종료 직전 깜빡임

### 챔피언 처치
- `DropSystem.DropChampion(pos, dir, scoreValue)` → XP orb 1개 + 보너스 1종
- 보너스 선택: `bombWeight = championBombWeightAtFullHP × hpRatio`, `healWeight = championHealWeightAtZeroHP × (1 - hpRatio)` 정규화 후 추첨
  - HP 만땅(1.0) → 100% 폭탄
  - HP 빈사(0.0) → 100% 회복
  - HP 50% → 50:50
- 쿨다운/최소 적 수/체력 임계치는 모두 제거(챔피언 스폰 빈도가 게이트)

## XP Orb (DropXPOrb 프리팹 인스펙터)

| 값 | 의미 |
|---|---|
| xpLifetime | XP orb 수명(초). 끝나면 자동 사라짐 |
| magnetForce | Magnet 범위 내에서 플레이어 쪽으로 가하는 힘 |
| magnetMinDistance | 이 거리 이내에선 인력 생략(떨림 방지) |
| blinkStartBeforeEnd | 사라지기 N초 전부터 깜빡임 시작 |
| blinkSpeed | 깜빡임 주파수(Hz) |
| blinkMinAlpha | 깜빡임 최저 알파 |

## 미결정 / 검토 중

- **스폰 속도 게이팅**: 축 3이 t=0부터 항상 적용 — 축 1·2의 진행 중/후 분리 원칙과 어긋남. 게이팅하거나 스텝식으로 바꿀지 미결정
- **진행 후 HP 배율 방식**: 현재 선형(가산). 복리(`1.5^step`)로 바꿀지
- **진행 후 스케일 대상**: 현재 HP만. 공격력·속도·점수에도 적용할지
- **Luck 스탯 연동** (R8b): 챔피언 출현률·드롭 확률에 반영할지
