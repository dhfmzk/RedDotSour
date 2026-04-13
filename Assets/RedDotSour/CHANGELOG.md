# Changelog

## [0.0.1] - 2026-04-14

### Added
- `RedDotSour<TCategory>` — 카테고리 기반 메인 API
- `RedDotContainer<TKey>` — 제네릭 키 타입, 카테고리별 동적 결정
- `RedDotRecord<TKey>` — readonly struct 조회 반환형
- `IRedDotContainer` — 비제네릭 인터페이스 (이벤트, dirty 추적)
- `IRedDotPersistence` — 영속화 전략 인터페이스
- `JournalFilePersistence` — Snapshot + Journal + Compact 패턴
- `PlayerPrefsPersistence` — 소규모 전용
- `RedDotSourRegistry` — 에디터 디버그용 정적 레지스트리
- `RedDotSourDebugWindow` — 에디터 디버그 창
- `event OnChanged` — 구독자별 try-catch 격리
- O(1) `CountOn` / `IsOnAny` 캐싱
- `HashSet<TKey>` dirty 추적 (ClearDirty O(1))
- Mark/Unmark 미등록 키 방어 (KeyNotFoundException)
- Get<TKey> 타입 불일치 방어 (InvalidCastException)
- 유닛 테스트 66개 (Core 50 + Persistence 16)
