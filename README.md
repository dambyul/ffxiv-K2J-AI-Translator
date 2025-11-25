# ffxiv-K2J-AI-Translator

**파이널판타지14 글로벌 서버(Global Server) 한국인 유저를 위한 실시간 한일 AI 번역 플러그인**

**ffxiv-K2J-AI-Translator**는 Google Gemini AI를 활용하여, 게임 내 채팅창에 입력한 한국어를 문맥에 맞는 자연스러운 일본어(경어체)로 즉시 번역해 주는 Dalamud 플러그인입니다.

단순 번역을 넘어, 파티원의 닉네임을 인식하여 오역을 방지하고, 글로벌 서버 환경에 맞춰 한글 출력을 원천 차단하는 등 철저한 안전 장치가 마련되어 있습니다.

---

## 주요 기능 (Key Features)

### 1. AI 기반 실시간 번역
* **Google Gemini 2.5 Flash** 모델을 사용하여 빠르고 정확하게 번역합니다.
* 게임 분위기에 맞는 **정중한 일본어(Desu/Masu)**체를 기본으로 사용합니다.
* 영어로 작성된 시스템 프롬프트를 통해 AI의 지시 이행 능력을 극대화했습니다.

### 2. 스마트 문맥 인식 (Context Awareness)
* **파티원 닉네임 보호:** 현재 파티원의 닉네임 리스트를 실시간으로 참조하여, 닉네임을 단어 뜻으로 오역하는 참사를 방지합니다.
    * 예: `Summer` (닉네임) → `여름(夏)` (X) / `サマーさん` (O)
* **자동 음차 변환:** 발음 가능한 영어 닉네임은 가타카나로, 불가능한 문자열은 알파벳 그대로 유지하며 자동으로 경칭(`さん`)을 붙입니다.

### 3. 강력한 안전 장치 (Safety First)
* **한글 출력 차단 (Anti-Mojibake):** 글로벌 클라이언트에서 텍스트 깨짐을 방지하기 위해, 번역 결과에 **한글이 단 한 글자라도 포함되면 전송을 차단**합니다.
* **프롬프트 인젝션 방어:** 사용자의 입력값을 태그로 격리하여, 번역 외의 명령(예: "레시피 알려줘")을 수행하지 않도록 합니다.
* **무한 루프 방지:** 설정 UI에서 입력 채널(Input)과 출력 채널(Output)을 동일하게 설정할 수 없도록 강제합니다.
* **충돌 방지 (Crash Free):** 최신 `FFXIVClientStructs`와 `Utf8String` 마샬링을 사용하여 게임 클라이언트 튕김 현상을 방지했습니다.

### 4. 사용자 편의성
* **BYOK (Bring Your Own Key):** 사용자가 본인의 Google API Key를 직접 입력하여 사용하므로 무료/유료 티어를 자유롭게 선택 가능합니다.
* **안전한 테스트 모드:** 기본 출력은 `Echo`(나에게만 보임)로 설정되어 있어 충분히 검증 후 파티 채팅으로 전환할 수 있습니다.
* **직관적인 UI:** `/byul` 명령어로 설정창을 열어 API 키와 채널을 간편하게 관리할 수 있습니다.

---

## 향후 계획 (Roadmap)

이 프로젝트는 지속적으로 기능을 확장할 예정이며, 다음과 같은 업데이트가 준비 중입니다.

* **Multi-LLM 지원:** 현재 지원하는 Google Gemini 외에 **OpenAI (ChatGPT)** API 지원을 추가하여 사용자가 선호하는 AI 모델을 선택할 수 있도록 할 예정입니다.
* **Firebase 기반 동적 사전:** 현재 코드 내에 하드코딩된 고유명사 사전(예: 리즈 -> リズ, 산개 -> 散開)을 **Firebase 클라우드 데이터베이스**로 이관하여 관리할 예정입니다. 이를 통해 플러그인 업데이트 없이도 최신 용어가 실시간으로 반영됩니다.
* **커뮤니티 기여형 번역 개선:** 사용자가 번역 기록 창에서 '오번역 신고'를 할 경우, 해당 데이터가 Firebase로 전송되어 공용 번역 사전을 업데이트하는 데 활용되도록 기능을 확장할 예정입니다.

---

## 설치 및 사용 방법 (Getting Started)

이 플러그인은 현재 **개발자 플러그인(Dev Plugin)** 형태로 로드해야 합니다.

### 1. 사전 준비
1.  [Google AI Studio](https://aistudio.google.com/)에서 **API Key**를 발급받습니다. (무료)
2.  **FFXIVQuickLauncher** 설정에서 `Enable Dalamud staging` (또는 Dev plugins) 옵션을 켭니다.

### 2. 빌드 (Build)
1.  이 저장소를 클론(Clone)합니다.
2.  터미널에서 프로젝트 폴더로 이동하여 다음 명령어를 실행합니다.
    ```powershell
    dotnet clean
    dotnet build DambyulK2J.csproj --configuration Release
    ```
3.  빌드가 성공하면 `bin/Release/net9.0-windows/` 폴더에 `DambyulK2J.dll`과 `plugin.json`이 생성됩니다.

### 3. 게임 내 로드
1.  게임 접속 후 `/xlsettings` 입력 → **Experimental** 탭 클릭.
2.  **Dev Plugin Locations**에 빌드된 결과물 폴더 경로를 추가하고 저장합니다.
3.  `/xlplugins` 입력 → **ffxiv-K2J-AI-Translator**를 찾아 설치/로드합니다.

### 4. 사용하기
1.  채팅창에 **/byul** 입력 → 설정창 오픈.
2.  발급받은 **API Key** 입력 및 저장.
3.  **입력 채널(Input)**과 **출력 채널(Output)** 선택.
4.  지정된 채널에 한국어로 채팅을 입력하면 일본어로 번역되어 전송됩니다.

---

## 주의사항 (Disclaimer)

* **제재 위험:** 이 플러그인은 외부 프로그램(3rd Party Tool)으로 분류되며, 게임사의 약관에 위배될 소지가 있습니다. 사용으로 인한 모든 책임(계정 제재 등)은 사용자 본인에게 있습니다.
* **자동 전송 주의:** 파티 채팅으로 자동 전송하는 기능은 파티원에게 혼란을 줄 수 있으므로, 반드시 사전에 양해를 구하거나 `Echo` 모드로 충분히 테스트한 후 사용하시기 바랍니다.
* **오번역 가능성:** AI 번역은 완벽하지 않으며, 특히 게임 고유 명사나 줄임말에서 오역이 발생할 수 있습니다.

---

## 기술 스택 (Tech Stack)

* **Language:** C# (.NET 9.0)
* **Framework:** Dalamud (API 13)
* **Libraries:**
    * `Google.GenerativeAI` (Rest API via HttpClient)
    * `FFXIVClientStructs` (Game Function Hooking)
    * `Dalamud.Bindings.ImGui` (UI)