# RedDotSour v2.0 - Rebuild TODO

> 설계단부터 리빌딩. 오픈소스 UPM 패키지로 공개 목표.

---

## 아키텍처

### 카테고리 + 제네릭 키 + DateTime? 단일 타입

```
RedDotSour<TCategory>
├── [Inventory] → RedDotContainer<int>
│   ├── 1001 → null          (미확인 → 빨콩)
│   ├── 1002 → 2026-04-11   (확인됨)
│   └── 1003 → null          (미확인 → 빨콩)
└── [Quest] → RedDotContainer<QuestId>
    └── ...
```

- **카테고리**(enum)로 도메인 분리
- **키 타입을 동적 결정** — `Create<TKey>(category)`로 카테고리별 키 타입 주입
- **값**(`DateTime?`) 단일 타입
  - `null` → 미확인 → 빨콩 ON
  - 값 있음 → 확인됨 → 빨콩 OFF
- **저장소**: `Dictionary<TKey, DateTime?>` + `HashSet<TKey>` dirty 추적
- **RedDotRecord**: `readonly struct`, 조회 전용 반환형 (스택 할당)
- **외부 의존성 없음** (TMP 포함 제거 완료)
- UI 컴포넌트는 라이브러리에 포함하지 않음

---

## 핵심 API

### RedDotSour&lt;TCategory&gt;

```csharp
public class RedDotSour<TCategory> where TCategory : Enum
{
    RedDotContainer<TKey> Create<TKey>(TCategory category);   // 컨테이너 생성, 키 타입 결정
    RedDotContainer<TKey> Get<TKey>(TCategory category);      // 타입 불일치 시 InvalidCastException

    bool IsOn<TKey>(TCategory category, TKey key);
    bool IsOnAny(TCategory category);
    int CountOn(TCategory category);
}
```

### RedDotContainer&lt;TKey&gt;

```csharp
public class RedDotContainer<TKey> : IRedDotContainer
    where TKey : struct, IEquatable<TKey>
{
    // 등록/조작
    void Register(TKey key);                    // 미확인(null)으로 추가, 빨콩 ON
    void Mark(TKey key);                        // 확인 처리 (DateTime.Now), 빨콩 OFF
    void Mark(TKey key, DateTime at);           // 특정 시점으로 확인
    void Unmark(TKey key);                      // 미확인으로 되돌림, 빨콩 ON
    bool Remove(TKey key);

    // 조회 — O(1)
    bool IsOn(TKey key);
    bool TryGet(TKey key, out RedDotRecord<TKey> record);
    bool IsOnAny();                             // _onCount > 0
    int CountOn();                              // _onCount

    // 이벤트
    event Action OnChanged;                     // 상태 변경 시 발화, 구독자별 try-catch

    // dirty 추적 (persistence용)
    int DirtyCount;
    IEnumerable<RedDotRecord<TKey>> GetDirtyRecords();
    IEnumerable<RedDotRecord<TKey>> GetAllRecords();
    void LoadRecord(TKey key, DateTime? checkedAt);   // DB 로드, dirty 안 함
    void ClearDirty();                                // O(1), HashSet.Clear()
}
```

### RedDotRecord&lt;TKey&gt;

```csharp
public readonly struct RedDotRecord<TKey>
    where TKey : struct, IEquatable<TKey>
{
    public readonly TKey Key;
    public readonly DateTime? CheckedAt;
    public bool IsOn => CheckedAt == null;
}
```

---

## 파일 구조

```
Assets/RedDotSour/
├── Core/
│   ├── RedDotSour.cs            메인 API
│   ├── RedDotContainer.cs       카테고리별 컨테이너
│   ├── RedDotRecord.cs          readonly struct 조회 반환형
│   └── IRedDotContainer.cs      비제네릭 인터페이스
├── Persistence/
│   ├── IRedDotPersistence.cs    영속화 인터페이스
│   ├── RedDotSaveData.cs        직렬화 중간 형태
│   ├── JournalEntry.cs          JSONL 한 줄 단위
│   ├── JournalFilePersistence.cs  Snapshot+Journal+Compact (기본)
│   └── PlayerPrefsPersistence.cs  소규모 전용
├── (추후) Editor/
└── Tests/Editor/
    ├── RedDotContainerTests.cs  (33 tests)
    ├── RedDotSourTests.cs       (12 tests)
    └── RedDotRecordTests.cs     (5 tests)
```

---

## 구현 순서

### Phase 1: Core
- [x] 1.1 `IRedDotContainer` 인터페이스 (OnChanged, DirtyCount, ClearDirty)
- [x] 1.2 `RedDotRecord<TKey>` readonly struct
- [x] 1.3 `RedDotContainer<TKey>` (Dict+HashSet, 이벤트, O(1) 캐싱, dirty 추적)
- [x] 1.4 `RedDotSour<TCategory>` (Create/Get 동적 키 타입, 타입 안전성)
- [x] 1.5 Mark/Unmark 미등록 키 방어 (KeyNotFoundException)
- [x] 1.6 Get<TKey> 타입 불일치 방어 (InvalidCastException)
- [x] 1.7 TMP 의존성 제거
- [x] 1.8 Core 유닛 테스트 (50 tests)

### Phase 2: Persistence

**설계 완료. Snapshot + Journal + Compaction 패턴 채택.**

구조:
```
reddotsour.snapshot.json   ← Compact 시점의 전체 상태
reddotsour.journal         ← dirty만 JSONL append
```
- `Save()` — dirty records만 journal에 append (전체 직렬화 없음)
- `Compact()` — 메모리 상태 → snapshot 재작성 + journal 삭제
- `Load()` — snapshot + journal 재생
- 크래시 안전: temp → atomic rename, 깨진 줄 skip

PlayerPrefs — 소규모(~100건) 전용, 단일 키 JSON 직렬화. 한계 문서화 필수.
SQLite — 코어에 미포함. README에 gilzoide/unity-sqlite-net 통합 가이드 제공.

상세 설계: `.claude/plans/compressed-fluttering-planet.md`

- [x] 2.1 `RedDotSaveData` + `JournalEntry` 직렬화 모델
- [x] 2.2 `IRedDotPersistence` 인터페이스 (Save/SaveAll/Load/Clear)
- [x] 2.3 `RedDotContainer` 수정 — Create에 키 시리얼라이저 추가, Export/Import
- [x] 2.4 `RedDotSour` 수정 — SetPersistence, Save/Load/Compact
- [x] 2.5 `JournalFilePersistence` 구현
- [x] 2.6 `PlayerPrefsPersistence` 구현
- [x] 2.7 Persistence 테스트 (16 tests)

### Phase 3: Editor Tooling
- [x] 3.1 `RedDotSourDebugWindow` (EditorWindow)
- [x] 3.2 `RedDotSourRegistry` (정적 접근자)

### Phase 4: Sample / Demo
- [x] 4.1 `SampleGameManager`
- [x] 4.2 `SampleControlPanel`
- [x] 4.3 `SampleRedDotBadge` (이벤트 구독 → UI 반영 참고 예시)
- [x] 4.4 `SampleCategory`
- [ ] 4.5 샘플 씬 (Unity 에디터에서 수동 생성 필요)

### Phase 5: 패키지화 + Documentation
- [x] 5.1 asmdef 파일 (Runtime, Editor, Tests)
- [x] 5.2 package.json (UPM)
- [x] 5.3 CHANGELOG.md
- [x] 5.4 루트 README 업데이트 (Persistence, Installation 섹션)
- [ ] 5.5 최종 정리 + v2.0.0 태그
