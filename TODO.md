# RedDotSour v2.0 - Rebuild TODO

> 설계단부터 리빌딩. 오픈소스 UPM 패키지로 공개 목표.

---

## 현재 코드 문제점 (v1.0, 삭제 완료)

| 문제 | 심각도 |
|---|---|
| 계층 구조 없음 — 부모-자식 전파 불가 | Critical |
| 폴링 기반 UI — 매 프레임 Update() 체크 | High |
| IConvertible 강제 — 키 하나에 15개 보일러플레이트 | High |
| CSV 직렬화 깨짐 — AppendLine + comma 혼합 | Medium |
| 생성자에서 Load() — 레코드 등록 전 로드 시도 | Medium |
| asmdef 없음 — UPM 패키지 불가 | High |
| 테스트 없음 | Medium |

---

## 새 아키텍처

### 핵심: 카테고리 + 플랫 키, DateTime? 단일 타입

```
RedDotSour<TCategory>
├── [Inventory] → RedDotContainer
│   ├── "item_001" → null          (미확인 → 빨콩)
│   ├── "item_002" → 2026-04-10   (확인됨)
│   └── "item_003" → null          (미확인 → 빨콩)
└── [Global] → RedDotContainer
    └── "event_01" → null          (미확인 → 빨콩)
```

- **카테고리**(enum)로 도메인 분리
- **키**(string)로 개별 항목 식별
- **값**(`DateTime?`) 단일 타입으로 통일
  - `null` → 미확인 → 빨콩 ON
  - 값 있음 → 확인됨 → 빨콩 OFF
- UI 컴포넌트는 라이브러리에 포함하지 않음

---

## 핵심 API 설계

### RedDotSour<TCategory>

```csharp
public class RedDotSour<TCategory> where TCategory : Enum
{
    // 조회
    bool IsOn(TCategory category, string key);
    bool TryGet(TCategory category, string key, out DateTime? value);

    // 조작
    void Mark(TCategory category, string key);              // 확인 처리 (DateTime.Now)
    void Mark(TCategory category, string key, DateTime at); // 특정 시점으로 확인
    void Unmark(TCategory category, string key);            // null로 되돌림

    // 카테고리 단위
    bool IsOnAny(TCategory category);                       // 카테고리 내 빨콩 하나라도?
    int CountOn(TCategory category);                        // 카테고리 내 빨콩 개수

    // 영속화
    void Save();
    void Load();
}
```

### RedDotContainer

```csharp
public class RedDotContainer
{
    Dictionary<string, DateTime?> table;

    bool IsOn(string key);              // table[key] == null
    bool TryGet(string key, out DateTime? value);
    void Mark(string key);
    void Mark(string key, DateTime at);
    void Unmark(string key);
    bool IsOnAny();
    int CountOn();
}
```

### RedDotRecord

```csharp
public class RedDotRecord
{
    string Key;
    DateTime? DateTime;
}
```

---

## 파일 구조

```
Assets/RedDotSour/                          (패키지 루트)
├── package.json
├── README.md / LICENSE.md / CHANGELOG.md
│
├── Runtime/
│   ├── RedDotSour.Runtime.asmdef
│   └── Core/
│       ├── RedDotSour.cs                   메인 API
│       ├── RedDotContainer.cs              카테고리별 컨테이너
│       └── RedDotRecord.cs                 개별 레코드
│
├── Editor/
│   ├── RedDotSour.Editor.asmdef
│   └── RedDotSourDebugWindow.cs
│
├── Tests/
│   └── Editor/
│       ├── RedDotSour.Tests.Editor.asmdef
│       ├── RedDotContainerTests.cs
│       └── RedDotSourTests.cs
│
└── Samples~/BasicSample/
    ├── Scripts/
    │   ├── SampleGameManager.cs
    │   ├── SampleControlPanel.cs
    │   └── SampleRedDotBadge.cs            이벤트 구독 → UI 반영 참고 예시
    └── Scenes/RedDotSample.unity
```

> UI 컴포넌트는 라이브러리에 포함하지 않음. 샘플의 `SampleRedDotBadge`는 참고용 예시.

---

## 구현 순서

### Phase 1: Core
- [ ] 1.1 패키지 구조 + asmdef
- [ ] 1.2 `RedDotRecord` 구현
- [ ] 1.3 `RedDotContainer` 구현
- [ ] 1.4 `RedDotSour<TCategory>` 구현
- [ ] 1.5 Core 유닛 테스트

### Phase 2: Persistence
- [ ] 2.1 영속화 전략 (PlayerPrefs + JSON)
- [ ] 2.2 Save/Load 연결
- [ ] 2.3 Persistence 테스트

### Phase 3: Editor Tooling
- [ ] 3.1 `RedDotSourDebugWindow` (EditorWindow)

### Phase 4: Sample / Demo
- [ ] 4.1 `SampleGameManager`
- [ ] 4.2 `SampleControlPanel`
- [ ] 4.3 `SampleRedDotBadge` (참고 예시)
- [ ] 4.4 샘플 씬
- [ ] 4.5 package.json에 sample 등록

### Phase 5: Documentation
- [ ] 5.1 패키지 README.md
- [ ] 5.2 CHANGELOG.md
- [ ] 5.3 루트 README 업데이트
- [ ] 5.4 최종 정리 + v2.0.0 태그
