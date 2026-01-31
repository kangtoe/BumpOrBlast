# 기술 스펙

## 개발 규칙
- **스크립트 명명**: 모든 C# class 및 script 이름에 `kangtoe99_` 접두어 사용
  - 예: `kangtoe99_PlayerController.cs`, `kangtoe99_EnemyAI.cs`
- **아트 애셋**: Unity 기본 프리미티브(Primitives)만 사용
  - Sprite: Circle, Square, Triangle 등
  - 색상으로 시각적 구분

## Unity 설정
- Unity 2D 프로젝트
- Rigidbody2D 및 Collider2D 사용
- Physics2D Layer로 충돌 최적화

## 스크립트 구조

### Core (공통)
- `kangtoe99_Character.cs` - 기본 이동 및 체력 관리 (부모 클래스)
  - 이동 기능 (Rigidbody2D)
  - 체력 시스템 (현재/최대 체력, 데미지, 사망)
  - 공통 속성 (이동속도, 체력 등)

### Player
- `kangtoe99_Player.cs` - Character 상속, 플레이어 고유 기능
  - 방향키 입력 처리
  - 마우스 방향 추적
- `kangtoe99_PlayerShooting.cs` - 사격 시스템
  - 마우스 클릭 감지
  - 탄환 생성 및 발사
  - 탄창 관리

### Enemy
- `kangtoe99_Enemy.cs` - Character 상속, 적 고유 기능
  - 플레이어 추적 AI
  - 충돌 시 데미지 처리
- `kangtoe99_EnemySpawner.cs` - 스폰 시스템
- `kangtoe99_EnemyData.cs` - 적 데이터 (ScriptableObject)

### Systems
- `kangtoe99_GameManager.cs` - 게임 전체 관리
- `kangtoe99_LevelUpSystem.cs` - 레벨업 및 강화
- `kangtoe99_ScoreSystem.cs` - 점수 관리
- `kangtoe99_CameraShake.cs` - 화면 흔들림

### UI
- `kangtoe99_UIManager.cs` - UI 전체 관리
- `kangtoe99_HealthBar.cs` - 체력바
- `kangtoe99_AmmoDisplay.cs` - 탄창 UI
- `kangtoe99_FloatingText.cs` - 플로팅 텍스트
- `kangtoe99_LevelUpPanel.cs` - 레벨업 UI
- `kangtoe99_ShellCasing.cs` - 탄피 연출

### Utils
- `kangtoe99_Bullet.cs` - 탄환 동작
- `kangtoe99_ObjectPool.cs` - 오브젝트 풀링 (선택)

## 최적화
- 오브젝트 풀링 (탄환, 적, 이펙트)
- 거리 기반 적 삭제
- Layer Collision Matrix 설정
