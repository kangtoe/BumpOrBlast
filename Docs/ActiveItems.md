# 활성 아이템 목록

`Assets/Resources/Items/Tier{N}_*/` 에 있는 35개. 미사용 22개는 `Assets/Resources/UnusedItems/Tier{N}_*/`.

작성: 2026-05-20. 각 티어 7개씩 정선 (컨셉 명확성 + 스텟 커버리지 기준).
모디파이어 표기: `Stat ±value` (Multiplicative는 %로, Additive는 flat). kind는 `StatKindRegistry`에서 도출.

---

## Tier 0 Gray (7개)

| 아이템 | 컨셉 | 모디파이어 |
|---|---|---|
| **BoneBroken** | 회복 트레이드 | HPRegen +30%, MaxHP -15 |
| **Dice5** | 다탄 (속도 희생) | ProjectileCount +2, ProjectileSpeed -15% |
| **Flag** | 단순 HP | MaxHP +20% |
| **Glass** | 발사체 속도 | ProjectileSpeed +20% |
| **HeartEmpty** | 탱커 (데미지 희생) | MaxHP +15%, Damage -5% |
| **Hourglass** | 연사형 | FireRate +20%, Damage -5% |
| **Leaf** | 가벼움 (마찰↓ + 자석↑) | Friction -15%, Magnet +30% |

## Tier 1 Green (7개)

| 아이템 | 컨셉 | 모디파이어 |
|---|---|---|
| **Coffee** | 각성 (속도+데미지) | ProjectileSpeed +20%, Damage +5% |
| **Compass** | 단순 연사 | FireRate +15% |
| **Crosshairs** | 정확도 (각도 좁힘) | ProjectileSpread -40 |
| **Lighting** | 큰 한방 | Damage +15%, FireRate -8% |
| **Paperplane** | 행운형 (느림) | Luck +30, FireRate -10% |
| **Scope** | 속도+조준 | ProjectileSpeed +20%, FireRate -10% |
| **Tower** | 탱키 (비용↑) | MaxHP +25%, EnergyCost +15% |

## Tier 2 Blue (7개)

| 아이템 | 컨셉 | 모디파이어 |
|---|---|---|
| **Battery** | 다탄 안정 | ProjectileCount +1, ProjectileSpread -10, HPRegen -8 |
| **Cutlery** | 큰 탄 + 데미지 | ProjectileScale +20%, Damage +15% |
| **GameController** | 균형형 (DPS 트레이드) | MaxHP +10%, Damage +10%, FireRate -15% |
| **Gear** | 이동 강화 | MoveForce +25% |
| **Shield** | 탱키 (에너지 희생) | MaxHP +20%, EnergyMax -15% |
| **SpadeAce** | 에너지 빌드 | EnergyMax +30%, EnergyCost +15% |
| **Wallet** | 픽업·운 | Luck +5, Magnet +25% |

## Tier 3 Purple (7개)

| 아이템 | 컨셉 | 모디파이어 |
|---|---|---|
| **Chain** | 단순 강화 | Damage +8, MaxHP +5 |
| **CreditCard** | 난사형 (전반 트레이드) | ProjectileCount +2, ProjectileSpread +20, Damage -20%, ProjectileScale -10%, ProjectileSpeed -10% |
| **Device** | 강력한 무기 업글 | Damage +30%, ProjectileScale +12%, FireRate -12% |
| **Flame** | 거대 탱커 (몸 밀어붙임) | MaxHP +25%, Friction +15%, BodyScale +20%, CollisionKnockback +40% |
| **Gem** | 작고 빠름 (Flame 반대) | MoveForce +25%, MaxHP -10%, BodyScale -10%, CollisionKnockback -20% |
| **Star** | 운형 (자석 희생) | Luck +30, Magnet -25% |
| **Suitcase** | 행상인 (HP↑/이동↓) | MaxHP +30%, MoveForce -15% |

## Tier 4 Orange (7개)

| 아이템 | 컨셉 | 모디파이어 |
|---|---|---|
| **Crown** | 슈퍼 난사 | ProjectileCount +6, Damage -15%, ProjectileScale -10, FireRate -10, ProjectileSpread +15 |
| **Stopwatch** | 시간정지 = 연사 | FireRate +35%, MoveForce -15% |
| **Sword** | 균형 | MaxHP +5%, Damage +8%, FireRate +5%, Luck +5, MoveForce +5 |
| **Thermo** | 거대 HP (회복 0) | MaxHP +100%, HPRegen -50% |
| **Trophy** | 빠른 난사 (관통 -1) | FireRate +30%, ProjectileSpread +15, Pierce -1 |
| **Umbrella** | 만능 탱커 | MaxHP +15%, BodyScale +20%, Friction +15%, FireRate -10%, Damage +15% |
| **Yinyang** | 음양 트레이드 | EnergyMax +30%, EnergyRegen +30%, MaxHP -15%, HPRegen -15% |

---

## 미사용 아이템 (22개)

`Assets/Resources/UnusedItems/Tier{N}_*/`에 보관. 컨셉 흐림 / 중복 / 약함 이유로 격리.

- **T0 (11)**: Bag, Bow, Gift, Graduation, Hammer, InvBelt, InvPotion, Key, Ribbon, Scissors, Wrench
- **T1 (4)**: Mail (Lighting과 중복), Map (잡탕), Palette (약함), Pin (Compass와 중복·더 약함)
- **T2 (4)**: Clip (count 망가짐), Pen (Gear와 겹침), Puzzle (잡탕), Skull (잡탕)
- **T3 (2)**: BrokenHeart (약함), Phone (Device와 중복)
- **T4 (1)**: Laurel Wreath (효과 미미)
