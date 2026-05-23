# Balancing TODO (임시)

밸런싱 이슈를 하나씩 체크하면서 지운다. 모든 항목 클리어되면 이 파일 삭제.

작성 시점: 2026-05-20. 시스템 가정:
- Multiplicative는 모디파이어 간 퍼센트 합산 후 한 번에 곱 (복리 X)
- 인벤토리 티어별 스택 상한: Gray 12 / Green 8 / Blue 5 / Purple 3 / Orange 1
- ItemData.maxStack은 티어에서 파생, 인스펙터 비노출
- kind는 스텟별로 고정 (StatKindRegistry) — 모디파이어에 따로 저장하지 않음
- 티어는 Resources/Items/Tier{N}_*/ 폴더 위치가 진리원천 (ItemData에 tier 필드 없음)

---

## 테스트 워크플로 (티어별)

전체 한 번에 잡지 말고 한 티어씩 격리해서 점검:

1. **테스트하지 않을 티어 아이템을 옮긴다** — `Assets/Resources/Items/Tier{N}_*/` → `Assets/Resources/UnusedItems/Tier{N}_*/`
2. Resources/Items/Tier{N}_*/ 폴더가 비면 해당 티어는 등장 후보에서 자동 제외 (LevelUpSystem이 Resources에서만 로드)
3. 단일 티어 집중 테스트 → 밸런스 만족 시 다음 티어로
4. 모든 티어 통과 후 통합 테스트

> **빠른 대안**: TierDropTable_Default.asset의 다른 티어 곡선을 잠시 0으로 깔아도 됨 (자산 이동 없이 가중치만 0). 하지만 ItemTierRegistry가 비어 있는 티어 폴더는 등록 자체를 안 하므로 깔끔히 격리하려면 이동 권장.

---

## 시스템 이슈

- [ ] **음수 multiplicative 누적 → 클램프 의존**
  - Dice -15% ProjectileSpeed × 12 Gray = -180% → 1로 클램프
  - Bag+Leaf -15% Friction 누적 → 0 클램프
  - Scissors/Mail/CreditCard FireRate 음수 누적 → 0.02 클램프
  - 대응 후보: 개별 음수 크기 축소 / 음수 합산 하한(예: -50%) 신설 / 음수 아이템 maxStack 별도 캡

- [ ] **CollisionKnockback 옵션 2개뿐** — Flame +40%, Gem -20%. 콘텐츠 추가 또는 스텟 자체 재검토

- [ ] **EnergyRegen / EnergyMax 옵션 폭 좁음** — 각 4~5개, 변화 폭 작음

## Tier 4 (Orange) 이슈

- [ ] **Thermo 일강** — 단일 +100% MaxHP가 1슬롯 차지 대비 압도. 너프 또는 동급 신규 T4 추가
- [ ] **Laurel Wreath** — +1 Pierce, -10% Damage. 효과 작고 트레이드오프 미미
- [ ] **Sword** — 5종 모디파이어가 5/5/5/5/5%로 분산, 임팩트 약함
- [ ] **Crown** — 1슬롯 차지 대비 트레이드오프 무거움(-Damage, -Scale, -FireRate)

## 약한 아이템 (티어 안에서 거의 안 뽑힐 후보)

- [ ] T0 Wrench
- [ ] T0 Key
- [ ] T1 Compass
- [ ] T1 Pin
- [ ] T1 Palette
- [ ] T1 Crosshairs(Scope)
- [ ] T2 Battery
- [ ] T2 Wallet
- [ ] T3 BrokenHeart

## 중복 아이템 (차별화 필요)

- [ ] **GameController(T2) ≡ InputController(T2)** — 모디파이어 완전 동일
- [ ] **Phone(T3) vs Device(T3)** — Damage 중심 겹침. Phone +25 단일 vs Device +30/-12/+12

## kind 정책 변경 잔여 (자산 값 재조정 필요)

새 정책에서 자산의 기존 kind가 안 맞아 의미가 바뀐 항목. 자산엔 더 이상 `kind:` 필드가 없으니, value만 새 정책 기준으로 보정.

- [ ] **Clip.ProjectileCount** — value -15가 옛 Mul -15% 의도 → 새 Add -15 flat → base 2 → -73 → 1로 클램프. **즉시 수정 필요** (예: value -1 또는 +1로 의도 재정의)
- [ ] **Chain.Damage** — value +8이 옛 Add (+8 flat) → 새 Mul +8%. Base 10 기준 +0.8/스택으로 약화. 의도가 "데미지 보강"이면 value를 큰 값으로 (예: +20%)
- [ ] **Key.Damage** — value -5가 옛 Add (-5 flat) → 새 Mul -5%. 패널티가 1/10로 약화. 의도에 맞춰 조정
- [ ] **Puzzle / Battery.ProjectileSpread** — base 0이라 새 Add(원래 의도)는 OK, 변경 없음. 확인만
- [ ] **BrokenHeart.EnergyRegen** — value +15가 옛 Add (+15 flat) → 새 Mul +15%. Base 20 기준 +3/스택. 의도가 회복 보강이면 +30~40% 수준으로

> MaxHP는 base 100이라 Add ↔ Mul이 numerically 동일(+10 = +10%). BoneBroken/Hammer/Chain의 MaxHP 변화 없음.

## 완료된 항목

- [x] kind 인코딩 모순 (raw value vs 퍼센트 해석) — 공식 재정의로 해결 (Multiplicative value는 퍼센트 단위)
- [x] MaxHP / FireRate / Damage 폭주 (×110k / ×750 / ×197) — 합산 + 티어 한도로 해결 (×4.7 / ×5.5 / ×2.9)
- [x] ProjectileScale / HPRegen 음수 폭주 — 위와 동일 + StatBoundsRegistry 클램핑
- [x] RotationSpeed / SpeedCapOvershoot 모디파이어 — 고정값으로 동결 (의도)
- [x] 티어 평균 파워 역전 (T4 < T3) — per-pick 기준으로 재해석 시 정상 (level-up 당 1 선택지 기준)
- [x] 스텟별 적용 방식 단일 출처 — StatKindRegistry 도입, StatModifierData에서 kind 제거
- [x] 티어 자산 폴더 분리 — Resources/Items/Tier{N}_*/ 구조 + ItemTierRegistry로 ItemData.tier 필드 제거
