using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    /// <summary>
    /// 대안 방식: Options 화면이 열릴 때만 GameOption.DisplayText / HelpText 를 임시로 번역하고,
    /// 화면이 닫힐 때 원래 문자열로 복원 (전역 변경 방지).
    /// 대상 메서드: Options.Show() 와 Options.Hide() 를 동적으로 찾아 후킹함.
    /// </summary>
    [HarmonyPatch]
    public static class Options_TempTranslate_Patch
    {
        private static Dictionary<string, (string display, string help)> backup = new Dictionary<string, (string, string)>();

        static MethodBase TargetMethod()
        {
            // 이 클래스는 개별 메서드를 패치하기 위한 placeholder입니다.
            return null;
        }

        // Show() 를 찾아 Prefix 로 원문 백업 및 번역 적용
        [HarmonyPatch]
        public static class ShowHook
        {
            static MethodBase TargetMethod()
            {
                var t = AccessTools.TypeByName("Qud.UI.OptionsScreen") ?? AccessTools.TypeByName("XRL.UI.OptionsScreen") ?? AccessTools.TypeByName("Options");
                if (t == null) return null;
                var m = AccessTools.Method(t, "Show") ?? AccessTools.Method(t, "show") ?? AccessTools.Method(t, "OnEnable");
                return m;
            }

            [HarmonyPrefix]
            static void Prefix()
            {
                try
                {
                    var optionsType = AccessTools.TypeByName("Options");
                    if (optionsType == null) return;
                    var dictField = AccessTools.Field(optionsType, "OptionsByID");
                    if (dictField == null) return;
                    var dict = dictField.GetValue(null) as System.Collections.IDictionary;
                    if (dict == null) return;

                    backup.Clear();
                    foreach (System.Collections.DictionaryEntry kv in dict)
                    {
                        var id = kv.Key as string;
                        var go = kv.Value;
                        if (go == null) continue;
                        var t = go.GetType();
                        var displayF = AccessTools.Field(t, "DisplayText");
                        var helpF = AccessTools.Field(t, "HelpText");
                        string display = null, help = null;
                        if (displayF != null) display = displayF.GetValue(go) as string;
                        if (helpF != null) help = helpF.GetValue(go) as string;
                        if (id != null) backup[id] = (display, help);

                        // 임시 번역 (Options scope)
                        if (!string.IsNullOrEmpty(display) && !TranslationUtils.SeemsLikeControlValue(display))
                        {
                            var translated = TranslationUtils.TranslatePreserveTags(display, new[] { Data.OptionsData.Translations });
                            if (!string.IsNullOrEmpty(translated) && translated != display && displayF != null)
                                displayF.SetValue(go, translated);
                        }
                        if (!string.IsNullOrEmpty(help))
                        {
                            var translated = TranslationUtils.TranslatePreserveTags(help, new[] { Data.OptionsData.Translations });
                            if (!string.IsNullOrEmpty(translated) && translated != help && helpF != null)
                                helpF.SetValue(go, translated);
                        }
                    }
                    Debug.Log("[Qud-KR] Options_TempTranslate_Patch: Applied temporary translations for options.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Qud-KR] Options_TempTranslate_Patch Prefix exception: " + ex);
                }
            }
        }

        // Hide() 를 찾아 Postfix 로 복원
        [HarmonyPatch]
        public static class HideHook
        {
            static MethodBase TargetMethod()
            {
                var t = AccessTools.TypeByName("Qud.UI.OptionsScreen") ?? AccessTools.TypeByName("XRL.UI.OptionsScreen") ?? AccessTools.TypeByName("Options");
                if (t == null) return null;
                var m = AccessTools.Method(t, "Hide") ?? AccessTools.Method(t, "hide") ?? AccessTools.Method(t, "OnDisable");
                return m;
            }

            [HarmonyPostfix]
            static void Postfix()
            {
                try
                {
                    if (backup == null || backup.Count == 0) return;
                    var optionsType = AccessTools.TypeByName("Options");
                    if (optionsType == null) return;
                    var dictField = AccessTools.Field(optionsType, "OptionsByID");
                    if (dictField == null) return;
                    var dict = dictField.GetValue(null) as System.Collections.IDictionary;
                    if (dict == null) return;

                    foreach (System.Collections.DictionaryEntry kv in dict)
                    {
                        var id = kv.Key as string;
                        var go = kv.Value;
                        if (go == null) continue;
                        if (!backup.ContainsKey(id)) continue;
                        var t = go.GetType();
                        var displayF = AccessTools.Field(t, "DisplayText");
                        var helpF = AccessTools.Field(t, "HelpText");
                        var orig = backup[id];
                        if (displayF != null) displayF.SetValue(go, orig.display);
                        if (helpF != null) helpF.SetValue(go, orig.help);
                    }
                    backup.Clear();
                    Debug.Log("[Qud-KR] Options_TempTranslate_Patch: Restored original option texts.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[Qud-KR] Options_TempTranslate_Patch Postfix exception: " + ex);
                }
            }
        }
    }
}