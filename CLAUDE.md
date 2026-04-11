# CLAUDE.md

## 프로젝트 개요

**RedDotSour**는 Unity용 레드닷(알림 뱃지/红点) 시스템 라이브러리다.
게임 UI에서 "읽지 않은 메일", "새 상품", "받지 않은 보상" 등을 빨간 점으로 표시하는 시스템을 제공한다.

오픈소스 UPM 패키지로 공개 예정이며, 현재 v2.0 리빌딩 진행 중이다.

## 기술 스택

- **Unity 2022.3** (LTS)
- **C#** (.NET Standard 2.1)
- **Built-in Render Pipeline**
- **TextMeshPro** (UI 텍스트)

## v2.0 아키텍처 (목표)

트리 기반 + 이벤트 기반 + 카운트 시스템. **데이터 컨테이너만 제공하며 UI는 포함하지 않는다.**

- 경로 키 (`"Mail/System"`)로 부모-자식 계층 표현
- 리프 노드만 직접 count 설정, 부모는 자식 합계 자동 계산
- `Action<RedDotNode> OnChanged` 이벤트를 통해 사용자가 자신의 UI에 연결
- `IRedDotPersistence` 전략 패턴으로 영속화 (PlayerPrefs 기본, SQLite는 나중에)
- UI 컴포넌트(뱃지, 텍스트 등)는 라이브러리가 제공하지 않음 — 사용자가 구현

상세 계획은 `TODO.md` 참조.

## 패키지 구조

```
Assets/RedDotSour/           ← UPM 패키지 루트
├── Runtime/
│   ├── Core/                ← RedDotSour, RedDotNode, PathUtility
│   └── Persistence/         ← IRedDotPersistence, PlayerPrefs 구현
├── Editor/                  ← 디버그 윈도우
├── Tests/                   ← NUnit 기반 유닛 테스트
└── Samples~/                ← 사용 예제 (UI 연동 예시 포함)
```

> UI 컴포넌트는 제공하지 않는다. 샘플에서 이벤트 구독 → UI 반영 패턴을 보여줄 뿐이다.

## 네임스페이스 규칙

- Core: `RedDotSour`
- Persistence: `RedDotSour.Persistence`
- Editor: `RedDotSour.Editor`
- Tests: `RedDotSour.Tests`

## 코딩 컨벤션

- private 필드: `_camelCase` (언더스코어 프리픽스)
- 프로퍼티/메서드: `PascalCase`
- `this.` 접근자 사용
- 라이브러리 코드에 `MonoBehaviour` 의존 최소화 (Core/Persistence는 순수 C#)
- 외부 의존성 금지 (Unity 내장 + TMP만 허용)

## 빌드 & 테스트

```bash
# Unity 에디터에서 테스트 실행
# Window > General > Test Runner > EditMode/PlayMode
```

테스트 프레임워크: `com.unity.test-framework` (NUnit)

## 주의사항

- `Samples~/` 디렉토리는 Unity가 컴파일하지 않음 (UPM 샘플 규약)
- `.meta` 파일은 항상 함께 관리할 것
- ScriptableObject 싱글톤 패턴은 v2.0에서 제거됨 — `RedDotSourRegistry` 정적 클래스로 대체
- SQLite 영속화는 stub만 유지, 구현은 나중에
- UI 컴포넌트(RedDotBadge 등)는 라이브러리에 포함하지 않음 — 데이터 컨테이너 + 이벤트만 제공
