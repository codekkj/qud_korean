using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    // 유틸: 태그(색상/마크업/플레이스홀더)를 보존하면서 번역 시도
    public static class TranslationUtils
    {
        // 태그 패턴: <...> (HTML-like), {{...}} (커스텀 마크업), color tags 등
        private static readonly Regex TagRegex = new Regex(@"(<[^>]+>)|(\{\{[^}]+\}\})", RegexOptions.Compiled);

        // scopeName: e.g. Data.MainMenuData.Translations or Data.OptionsData.Translations (string key or whatever TranslationEngine expects)
        public static string TranslatePreserveTags(string input, string[] scope = null)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // 1) 토큰화: 태그들을 플레이스홀더로 대체
            var tags = new Dictionary<string, string>();
            int idx = 0;
            string tokenized = TagRegex.Replace(input, match =>
            {
                string key = $"§TAG{idx++}§";
                tags[key] = match.Value;
                return key;
            });

            // 2) 번역 시도 (TranslationEngine API에 맞춰 호출)
            string translated = null;
            try
            {
                // TranslationEngine.TryTranslate(string text, out string translated, string[] scope)
                bool ok = TranslationEngine.TryTranslate(tokenized, out translated, scope);
                if (!ok || translated == null)
                {
                    // 실패하면 원문(태그 복원) 반환
                    return ReinsertTags(tokenized, tags);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Qud-KR] TranslatePreserveTags - TranslationEngine threw: " + ex);
                return ReinsertTags(tokenized, tags);
            }

            // 3) 플레이스홀더를 원래 태그로 되돌림
            return ReinsertTags(translated, tags);
        }

        private static string ReinsertTags(string text, Dictionary<string, string> tags)
        {
            if (tags == null || tags.Count == 0) return text;
            foreach (var kv in tags)
            {
                text = text.Replace(kv.Key, kv.Value);
            }
            return text;
        }

        // 체크박스/값/단위(숫자) 등 '번역하지 말아야 할 패턴'을 검사
        // 필요시 확장해서 특정 패턴은 번역하지 않도록 필터링
        public static bool SeemsLikeControlValue(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            // 예: "On", "Off", "Enabled", "Disabled", "True", "False", 숫자만 등
            var lower = input.Trim().ToLowerInvariant();
            if (lower == "on" || lower == "off" || lower == "yes" || lower == "no" || lower == "true" || lower == "false")
                return true;
            if (Regex.IsMatch(input, @"^\d+(\.\d+)?$")) return true;
            return false;
        }
    }
}