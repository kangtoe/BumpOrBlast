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
- 시각: 색상은 등급별, 크기는 scaleMul

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
- **드롭은 챔피언만** — 일반 적은 처치 점수만, 챔피언만 `DropSystem.TryDrop` 호출

## 드롭 (DropSystem 인스펙터)

| 값 | 현재 | 의미 |
|---|---|---|
| healthPackDropRate | 0.5 | HP팩 드롭 확률 |
| bombDropRate | 0.5 | 폭탄 드롭 확률 |
| minEnemiesForBomb | 15 | 폭탄 스폰 최소 적 수 |
| bombCooldown | 120 | 폭탄 쿨다운(초) |
| healthPackHealthThreshold | 0.6 | HP팩 스폰 체력 조건 (이하일 때) |
| healthPackCooldown | 90 | HP팩 쿨다운(초) |

- `TryDrop`: bomb → healthPack → (폴백) XP orb 순으로 roll. XP orb는 항상 폴백이라 챔피언은 최소 XP는 보장
- XP orb 가치는 처치한 챔피언의 (등급·챔피언 배율 적용된) scoreValue

## 미결정 / 검토 중

- **스폰 속도 게이팅**: 축 3이 t=0부터 항상 적용 — 축 1·2의 진행 중/후 분리 원칙과 어긋남. 게이팅하거나 스텝식으로 바꿀지 미결정
- **진행 후 HP 배율 방식**: 현재 선형(가산). 복리(`1.5^step`)로 바꿀지
- **진행 후 스케일 대상**: 현재 HP만. 공격력·속도·점수에도 적용할지
- **챔피언 드롭 풍성함**: 현재 기존 `TryDrop` roll 그대로. 챔피언이 유일한 드롭원이라 HP팩/폭탄이 너무 드물 수 있음 — 확정 드롭/다중 드롭 검토
- **Luck 스탯 연동** (R8b): 챔피언 출현률·드롭 확률에 반영할지
