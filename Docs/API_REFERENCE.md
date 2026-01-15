# Caves of Qud 한글화 API 참조 가이드

이 문서는 한글화 패치 작성 시 참조할 수 있는 주요 클래스, 네임스페이스 및 메서드 목록을 정리합니다.

---

## 1. UI 핵심 시스템

### [ConsoleLib.Console] ScreenBuffer
**용도:** 전역 텍스트 렌더링 시스템 (클래식 UI 및 배경 텍스트)
*   `void Write(string text)` : 텍스트를 화면에 씁니다. (가장 많이 패치됨)
*   `ScreenBuffer WriteAt(int x, int y, string s, bool processMarkup)` : 특정 위치에 텍스트를 씁니다.
*   `int WriteBlockWithNewlines(string s, ...)` : 여러 줄의 텍스트 블록을 씁니다.

### [XRL.UI] Popup
**용도:** 게임 내 모든 팝업 알림 및 대화 상자
*   `static void Show(string Text, ...)` : 메시지 팝업을 표시합니다.
*   `static string AskString(string Prompt, ...)` : 문자열 입력을 받습니다.
*   `static int AskNumber(string Prompt, ...)` : 숫자 입력을 받습니다.
*   `static void ShowOptionPicker(string Title, string[] Options, ...)` : 선택지 팝업을 표시합니다.

### [XRL.UI] UITextSkin
**용도:** 모던 UI (TextMeshPro 기반) 텍스트 렌더링 관리
*   `void Apply()` : 설정된 텍스트를 UI 요소에 적용합니다. (번역 시점)
*   `bool SetText(string text)` : UI의 텍스트를 변경합니다.

---

## 2. 주요 게임 화면 (Qud.UI)

### MainMenu / MainMenuScreen
**용도:** 게임 시작 화면
*   `void Show()` : 메인 메뉴를 표시합니다. (Scope 설정 시점)
*   `void UpdateMenuBars()` : 하단 버튼 바 등을 업데이트합니다.

### InventoryScreen / InventoryAndEquipmentStatusScreen
**용도:** 인벤토리 및 장비 관리 화면
*   `void Show()` : 인벤토리 화면을 엽니다.
*   `void RebuildLists(GameObject GO)` : 아이템 목록을 다시 생성합니다.
*   `void UpdateViewFromData()` : 데이터를 기반으로 화면을 갱신합니다.

### CharacterStatusScreen / StatusScreensScreen
**용도:** 캐릭터 정보, 속성, 변이 화면
*   `string GetTabString()` : 탭 이름(속성, 권능 등)을 반환합니다.
*   `void UpdateViewFromData()` : 속성치 및 포인트 현황을 갱신합니다.
*   `void HandleBuyMutation()` : 변이 구매 버튼 처리.

### TradeScreen
**용도:** 상점 거래 화면
*   `void BeforeShow()` : 화면 표시 전 준비.
*   `void UpdateTitleBars()` : 상단 텍스트(상인 이름 등) 업데이트.
*   `void UpdateTotals()` : 총 가치 및 무게 합계 업데이트.

---

## 3. 정보 제공 화면 (Qud.UI)

### JournalStatusScreen / QuestsStatusScreen
**용도:** 일지 및 퀘스트 추적
*   `string GetTabString()` : "Journal", "Quests" 등의 탭 이름.
*   `void UpdateViewFromData()` : 목록 갱신.

### FactionsStatusScreen / ReputationStatusScreen
**용도:** 세력 관계 및 평판 확인
*   `string GetTabString()` : "Reputation" 탭 이름.

### TinkeringStatusScreen / TinkeringScreen
**용도:** 아이템 제작 및 개조(팅커링)
*   `string GetTabString()` : "Tinkering" 탭 이름.

---

## 4. 기타 유틸리티

### [XRL.World] GameObject
**용도:** 플레이어, NPC, 아이템 등 모든 게임 객체
*   `string DisplayName` : 객체의 표시 이름.
*   `int Stat(string Name)` : 특정 능력치 값 확인.
*   `int GetStat(string Name).Value` : 속성 상세 값 확인.

### [Qud.UI] PlayerStatusBar
**용도:** 게임 화면 상단 HUD (HP, LVL, EXP 등)
*   `void Update()` : 매 프레임 상태바 정보 갱신.
*   `void UpdateString(StringDataType type, ...)` : 특정 데이터(HPBar, XPBar 등) 텍스트 갱신.

---

## 🛠️ 패치 작성 팁

1.  **Scope 설정 (Prefix/Postfix):**
    대부분의 Screen(`Show()`, `SetPage()`) 클래스에서 `Prefix`로 `ScopeManager.PushScope()`를 호출하고, `Postfix`로 `ScopeManager.PopScope()`를 호출하여 번역 범위를 제한하세요.

2.  **직접 텍스트 수정 (Postfix):**
    `UpdateViewFromData()`와 같이 UI 요소를 직접 건드리는 메서드에서는 `Postfix`에서 `instance.textElement.SetText()` 등을 사용하여 한글로 덮어쓰세요.

3.  **동적 대상 찾기 (TargetMethod):**
    `MainMenu`와 같이 클래스명이 변경될 가능성이 있는 경우, `AccessTools.TypeByName()`을 사용하는 `TargetMethod`를 정의하여 안전하게 패치하세요.

---

## 🔍 실제 메서드 탐색 방법

특정 클래스의 모든 메서드를 확인하려면 터미널에서 다음 명령을 사용하세요:

```bash
# 특정 클래스의 public 메서드 목록 보기
grep "public.*void" /Users/ben/Desktop/qud_korean/Assets/core_source/Path/To/Class.cs
```
