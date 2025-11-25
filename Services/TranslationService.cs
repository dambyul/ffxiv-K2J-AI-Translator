using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DambyulK2J.Services
{
    public class TranslationService : IDisposable
    {
        private readonly HttpClient _httpClient;
        private Dictionary<string, string> _glossary = new();
        
        private readonly Regex _koreanRegex = new Regex(@"[가-힣ㄱ-ㅎㅏ-ㅣ]");

        public TranslationService()
        {
            _httpClient = new HttpClient();
        }
    public async Task LoadDictionaryAsync()
        {
            // 추후 Firebase 등 외부 DB에서 로드하는 로직으로 대체 예정
            await Task.Delay(100); 
            
            _glossary = new Dictionary<string, string>
            {
                // [기초 용어 / 파티 상태]
                { "파티", "パーティ" },
                { "견학", "見学" },
                { "초행", "初見" },
                { "완료", "済" },         // 예: 완료 1명 -> 済1
                { "클자", "済み" },       // 클리어 한 사람
                { "미클자", "未クリア" },
                { "미클", "未コン" },     // 미컴플리트
                { "모집", "募集" },
                { "예습", "予習" },
                { "미예습", "未予習" },
                { "복습", "復習" },
                { "연습", "練習" },
                { "전반", "前半" },
                { "후반", "後半" },
                { "처음부터", "最初から" },
                { "트라이", "トライ" },
                { "클리어", "クリア" },
                { "가능", "可" },
                { "불가", "不可" },
                { "환영", "歓迎" },
                { "빈집", "空き" },       // 자리 있음

                // [전투 진행 / 룰]
                { "레디체크", "レディチェ" },
                { "레디 체크", "レディチェ" },
                { "RC", "RC" },
                { "1밥", "1飯" },         // 1탐
                { "1릴", "1セット" },
                { "무관", "不問" },
                { "조기해산", "早期解散" },
                { "매크로", "マクロ" },
                { "마커", "マカ" },
                { "조정", "調整" },
                { "침묵", "無言" },
                { "부담없이", "お気軽に" },
                { "상자", "箱" },
                { "입찰", "NEED" },
                { "선입", "〆" },         // 예: 용기사〆 (용기사 자리 없음/마감)
                { "프리로", "フリロ" },   // Free Roll
                { "주사위", "ダイス" },
                { "왼쪽", "左" },
                { "오른쪽", "右" },
                { "본업", "本職" },
                { "부직업", "サブジョブ" },
                { "직변", "着替え" },     // 직업 변경

                // [목적 / 목표]
                { "목적", "目的" },
                { "클목", "クリ目" },     // 클리어 목적
                { "파밍", "周回" },       // 뺑뺑이
                { "소화", "消化" },       // 주간 숙제(파밍)
                { "전멸기", "時間切れ" },
                { "기믹", "ギミック" },
                { "안정", "安定" },
                { "도우미", "お手伝い" },
                { "용병", "傭兵" },
                { "사장", "出荷" },       // 출하(버스)
                { "갱신", "詰め" },       // 프프로그 갱신
                { "로그작", "詰め" },
                { "뇌사", "脳死" },       // 뇌사법
                { "자살", "自滅" },       // 자멸식(청마 등)

                // [공략법 / 국룰]
                { "게임8", "game8" },
                { "누케마루", "ぬけまる" },
                { "햄카츠", "ハムカツ" },
                { "이누마루", "犬丸" },
                { "하루우라라", "はるうらら" },
                { "시노쇼", "しのしょ" },
                { "민토", "みんと" },

                // [직업 / 역할]
                { "탱커", "タンク" },
                { "탱", "T" },
                { "멘탱", "MT" },
                { "섭탱", "ST" },
                { "힐러", "ヒーラー" },
                { "힐", "H" },
                { "퓨어힐", "PH" },
                { "배리어힐", "BH" },
                { "섭힐", "BH" },
                { "딜러", "DPS" },
                { "딜", "D" },
                { "근딜", "メレー" },
                { "원딜", "レンジ" },
                { "캐스터", "キャス" },
                { "파티장", "主" },       // 주인(누시)

                // [아이템 / 재화]
                { "밥", "飯" },
                { "약", "薬" },
                { "석판", "トークン" },   // 토큰
                { "낱장", "断章" },
                { "무기", "武器" },
                { "장비", "装備" },
                { "길", "ギル" },
                { "만길", "万" },         // 예: 100만 -> 100万

                // [인스턴스 / 특수 컨텐츠]
                { "지도", "地図" },
                { "심층", "深層" },
                { "제단", "祭殿" },
                { "업적", "アチブ" },
                { "마물", "モブ" },
                { "마물런", "モブハン" }, // 모브헌트
                { "돌발", "FATE" },
                { "탈것", "マウント" },
                { "피리", "笛" },
                { "꼬친", "ミニオン" },
                { "악보", "譜面" },
                { "룩템", "ミラプリ" },
                { "청마", "青魔" },
                { "로그", "ログ" },
                { "몰볼", "モルボル" },
                { "쿠로", "クロ" },       // 쿠로 수첩
                { "망궁", "死者" },       // 사자의 궁전
                { "천궁", "ミハシラ" },   // 천의 궁전
                { "오르토", "オルト" },   // 오르토 에우레카
                { "조율해제", "制限解除" },
                { "조율 해제", "制限解除" },
                { "초월", "超える力" },
                { "최저", "下限" },
                { "하한", "下限" },

                // [지역]
                { "림사", "リムサ" },
                { "그리다니아", "グリダニア" },
                { "울다하", "ウルダハ" },
                { "크리스타리움", "クリスタリウム" },
                { "율모어", "ユールモア" },
                { "올드샬레이안", "オールド・シャーレアン" },
                { "라자한", "ラザハン" },
                { "툴리요라", "トライヨラ" }, // 황금의 유산
                { "솔루션나인", "ソリューション・ナイン" },
                { "에테", "エーテ" },     // 에테라이트
                { "하우징", "ハウジング" },

                // [기타 회화]
                { "수고하셨습니다", "お疲れ様でした" },
                { "수고", "おつ" },
                { "잘부탁드립니다", "よろしくお願いします" },
                { "잘부탁", "よろ" },
                { "죄송합니다", "すみません" },
                { "감사합니다", "ありがとうございます" },
                { "잠시만요", "少々お待ちください" },
                { "화이팅", "頑張りましょう" },
                { "동영상", "動画" },
                // [기본 인사 / 대답 / 사과]
                { "잘부탁드립니다", "よろしくお願いします" },
                { "잘부탁해요", "よろしくです" },
                { "잘부탁해", "よろしく" },
                { "네", "はい" },
                { "아니오", "いいえ" },
                { "죄송합니다", "すみません" },
                { "미안합니다", "ごめんなさい" },
                { "수고하셨습니다", "お疲れ様でした" },
                { "수고", "おつ" },
                { "감사합니다", "ありがとうございます" },
                { "감사", "あり" },
                { "천만에요", "どういたしまして" },
                { "괜찮아요", "大丈夫です" },
                { "괜찮아", "大丈夫" },
                { "축하합니다", "おめでとうございます" },
                { "축하해", "おめでとう" },
                { "실수", "ミス" },
                { "오폭", "誤爆" },         // 삑사리, 채팅 실수 등
                { "조심하겠습니다", "気を付けます" },

                // [파티 진행 회화]
                { "초행입니다", "初見です" },
                { "익숙하지 않지만", "不馴れですが" },
                { "텔포 감사합니다", "テレポありがとうございました" },
                { "파티 감사합니다", "PTありがとうございました" },
                { "빠지겠습니다", "抜けます" },
                { "저도 빠지겠습니다", "私も抜けます" },
                { "화장실", "トイレ" },
                { "다녀오겠습니다", "行ってきます" },
                { "다녀오세요", "いってらっしゃい" },
                { "어서오세요", "おかえりなさい" },
                { "돌아왔습니다", "戻りました" },
                { "잠수", "離席" },       // 자리비움
                { "자리비움", "離席" },
                { "곧 돌아옵니다", "すぐ戻ります" },
                { "준비 완료", "準備完了" },
                { "출발", "出発" },
                { "해산", "解散" },
                { "재모집", "再募集" },
                { "보충", "補充" },       // 인원 보충
                { "리트", "ワイプ" },     // 전멸 후 재시작
                { "막트", "ラスト" },
                { "라스트", "ラスト" },
                { "수리", "修理" },
                { "음식", "飯" },         // 밥
                { "탕약", "薬" },

                // [전투 관련]
                { "무적기", "無敵" },
                { "앙갚음", "リプ" },     // 리프라이절
                { "견제", "牽制" },
                { "철벽", "鉄壁" },
                { "중재", "インタベ" },   // 인터벤션
                { "날개", "パッセ" },     // 패시지 오브 암즈
                { "신보", "シェイク" },   // 셰이크 오프
                { "베일", "ヴェール" },   // 디바인 베일
                { "교대", "スイッチ" },   // 탱커 스위치
                { "탱버", "強攻撃" },
                { "산개", "散開" },
                { "쉐어", "頭割り" },
                { "넉백", "ノックバック" },
                { "낙사", "落下死" },
                { "부활", "蘇生" },
                { "리밋", "LB" },

                // [특수 상황]
                { "팅겼어요", "落ちました" },
                { "인터넷", "回線" },
                { "렉", "ラグ" },
                { "버그", "バグ" },
                { "블루스크린", "ブルスク" },
                { "재접", "リログ" },     // 재로그인

                // [마물 / 돌발 / 청마]
                { "소환작", "沸かし" },
                { "전파", "拡散" },       // 외치기 전파
                { "외치기", "シャウト" },
                { "위치", "場所" },
                { "좌표", "座標" },
                { "교환", "交換" },
                { "배웠습니다", "覚えました" }, // 청마 스킬 획득

                // [기타]
                { "한국인", "韓国人" },
                { "외국인", "外人" },
                { "일본어 서툼", "日本語不自由" },
                { "번역기", "翻訳機" }
            };
        }

    public async Task<string> TranslateAsync(string text, string apiKey, string partyInfo)
        {
            if (string.IsNullOrEmpty(apiKey)) return "Error: API Key is missing.";

            // [핵심 수정] 입력 텍스트에 포함된 단어만 골라냅니다 (AI 집중력 향상)
            var relevantGlossary = _glossary
                .Where(kvp => text.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            // 필터링된 단어장만 넘겨줍니다.
            string systemPrompt = SystemPrompts.GetBasePrompt(partyInfo, relevantGlossary);

            var requestBody = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] {
                            new { text = $"{systemPrompt}\n\n<input_text>\n{text}\n</input_text>" }
                        }
                    }
                }
            };

            try 
            {
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}", jsonContent);

                if (!response.IsSuccessStatusCode) return $"Error: API Status {response.StatusCode}";

                var responseString = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(responseString);
                string translatedText = json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString().Trim() ?? "Translation Failed";

                if (_koreanRegex.IsMatch(translatedText))
                {
                    return "Error: Output contained Korean characters (Blocked for safety).";
                }

                return translatedText;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        public async Task SendReportAsync(string original, string translated, string reason)
        {
            await Task.Delay(100); 
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}