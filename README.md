# Red Dot Sour

Unity용 레드닷(알림 뱃지) 시스템 라이브러리

---

## Overview

게임 UI에서 미확인 항목을 표시하는 레드닷(빨간 점) 시스템입니다.

- **카테고리**(enum)로 도메인 분리 — 인벤토리, 퀘스트, 메일 등
- **키 타입을 동적 결정** — 카테고리별로 `int`, `enum`, 커스텀 struct 등 자유 선택
- **`DateTime?` 단일 값** — `null`이면 미확인(빨콩), 값이 있으면 확인 완료
- **UI 미포함** — 데이터 컨테이너만 제공, UI 구현은 사용자 영역

## Quick Start

```csharp
// 1. 카테고리 정의
public enum MyCategory
{
    Inventory,
    Quest,
    Mail,
}

// 2. 인스턴스 생성
var redDot = new RedDotSour<MyCategory>();

// 3. 카테고리별 컨테이너 생성 (키 타입은 이 시점에 결정)
var inventory = redDot.Create<int>(MyCategory.Inventory);

// 4. 아이템 등록 — 미확인(null) 상태로 추가, 빨콩 ON
inventory.Register(1001);
inventory.Register(1002);
inventory.Register(1003);

// 5. 확인 처리 — 현재 시점으로 마킹, 빨콩 OFF
inventory.Mark(1001);

// 6. 조회
inventory.IsOn(1001);    // false (확인됨)
inventory.IsOn(1002);    // true  (미확인)
inventory.IsOnAny();     // true  (1002, 1003 미확인)
inventory.CountOn();     // 2

// 7. 카테고리 단위 조회
redDot.IsOnAny(MyCategory.Inventory);  // true
redDot.CountOn(MyCategory.Inventory);  // 2

// 8. 미확인으로 되돌리기
inventory.Unmark(1001);  // 빨콩 다시 ON
```

## Architecture

```
RedDotSour<TCategory>
├── [Inventory] → RedDotContainer<int>
│   ├── 1001 → 2026-04-11 14:30   (확인됨)
│   ├── 1002 → null                (미확인 → 빨콩)
│   └── 1003 → null                (미확인 → 빨콩)
├── [Quest] → RedDotContainer<QuestId>
│   └── ...
└── [Mail] → RedDotContainer<int>
    └── ...
```

### Core API

| 클래스 | 역할 |
|---|---|
| `RedDotSour<TCategory>` | 메인 진입점. 카테고리별 컨테이너 관리 |
| `RedDotContainer<TKey>` | 카테고리 내 키-값 저장소 |
| `IRedDotContainer` | 비제네릭 인터페이스 (카테고리 단위 조회용) |

### RedDotContainer&lt;TKey&gt; API

| 메서드 | 설명 |
|---|---|
| `Register(key)` | 키 등록. 미확인(null) 상태, 빨콩 ON |
| `Mark(key)` | 확인 처리 (DateTime.Now), 빨콩 OFF |
| `Mark(key, at)` | 특정 시점으로 확인 처리 |
| `Unmark(key)` | 미확인으로 되돌림, 빨콩 ON |
| `Remove(key)` | 키 제거 |
| `IsOn(key)` | 해당 키가 빨콩인지 |
| `TryGet(key, out value)` | 마지막 확인 시간 조회 |
| `IsOnAny()` | 컨테이너 내 빨콩 존재 여부 |
| `CountOn()` | 빨콩 개수 |
| `ClearAll()` | 전체 제거 |

## Requirements

- Unity 2022.3+

## License

MIT License - see [LICENSE](./LICENSE) for details.
