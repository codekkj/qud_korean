using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace QudKRTranslation.Patches
{
    /// <summary>
    /// UI 레벨에서 옵션 항목의 레이블/설명(HelpText)만 번역.
    /// 전역 Options.GameOption 객체 자체를 변경하지 않음.
    /// 동작 방식:
    /// - 옵션 항목을 렌더링/바인딩하는 메서드(예: OptionRow.SetData, OptionRow.setData 등)를 탐색해서 postfix로 번역 로직을 넣음.
    /// - 다양한 게임/버전의 메서드명을 시도하도록 작성되어 있음.
    /// </summary>
    [HarmonyPatch]
    public static class Options_UI_Patch
    {
        // 동적 대상 찾기: OptionRow 타입의 SetData 혹은 setData 메서드
        static MethodBase TargetMethod()
        {
            string[] candidateTypes = { "Qud.UI.OptionRow", "Qud.UI.OptionsRow", "Qud.UI.OptionEntry", "XRL.UI.OptionRow", "Game.OptionRow", "OptionRow" };
            string[] candidateMethodNames = { "SetData", "setData", "Bind", "Initialize" };

            foreach (var tname in candidateTypes)
            {
                var t = AccessTools.TypeByName(tname);
                if (t == null) continue;
                foreach (var mname in candidateMethodNames)
                {
                    var m = AccessTools.Method(t, mname);
                    if (m != null) return m;
                }
            }
            Debug.Log("[Qud-KR] Options_UI_Patch: No OptionRow.SetData target found - skipping UI-level options patch.");
            return null;
        }

        // Postfix: UI가 항목을 바인딩한 직후에 레이블만 번역
        [HarmonyPostfix]
        static void Postfix(object __instance)
        {
            try
            {
                if (__instance == null) return;

                // 가능한 필드/속성 이름 후보
                string[] labelFieldNames = { "Label", "m_Label", "labelText", "titleText", "Text" };
                string[] helpFieldNames = { "HelpText", "help", "description", "Intro", "Subtitle" };

                // 레이블 텍스트 가져오기/설정 유틸
                Func<string> getLabel = null;
                Action<string> setLabel = null;
                Func<string> getHelp = null;
                Action<string> setHelp = null;

                var t = __instance.GetType();

                // 레이블 추출 시도
                foreach (var fname in labelFieldNames)
                {
                    var f = AccessTools.Field(t, fname);
                    if (f != null && f.FieldType == typeof(string))
                    {
                        getLabel = () => (string)f.GetValue(__instance);
                        setLabel = v => f.SetValue(__instance, v);
                        break;
                    }
                    var p = AccessTools.Property(t, fname);
                    if (p != null && p.PropertyType == typeof(string))
                    {
                        getLabel = () => (string)p.GetValue(__instance);
                        setLabel = v => p.SetValue(__instance, v);
                        break;
                    }
                    // UnityEngine.UI.Text, TMPro 등일 경우 Text 컴포넌트의 text 속성 접근
                    var compField = AccessTools.Field(t, fname);
                    if (compField != null)
                    {
                        var comp = compField.GetValue(__instance);
                        if (comp != null)
                        {
                            var compType = comp.GetType();
                            var textProp = AccessTools.Property(compType, "text");
                            if (textProp != null && textProp.PropertyType == typeof(string))
                            {
                                getLabel = () => (string)textProp.GetValue(comp);
                                setLabel = v => textProp.SetValue(comp, v);
                                break;
                            }
                        }
                    }
                }

                // 설명(Help) 추출 시도
                foreach (var fname in helpFieldNames)
                {
                    var f = AccessTools.Field(t, fname);
                    if (f != null && f.FieldType == typeof(string))
                    {
                        getHelp = () => (string)f.GetValue(__instance);
                        setHelp = v => f.SetValue(__instance, v);
                        break;
                    }
                    var p = AccessTools.Property(t, fname);
                    if (p != null && p.PropertyType == typeof(string))
                    {
                        getHelp = () => (string)p.GetValue(__instance);
                        setHelp = v => p.SetValue(__instance, v);
                        break;
                    }
                    var compField = AccessTools.Field(t, fname);
                    if (compField != null)
                    {
                        var comp = compField.GetValue(__instance);
                        if (comp != null)
                        {
                            var compType = comp.GetType();
                            var textProp = AccessTools.Property(compType, "text");
                            if (textProp != null && textProp.PropertyType == typeof(string))
                            {
                                getHelp = () => (string)textProp.GetValue(comp);
                                setHelp = v => textProp.SetValue(comp, v);
                                break;
                            }
                        }
                    }
                }

                // Label 번역
                if (getLabel != null && setLabel != null)
                {
                    string orig = getLabel();
                    if (!string.IsNullOrEmpty(orig) && !TranslationUtils.SeemsLikeControlValue(orig))
                    {
                        // scope: only Options UI scope
                        string[] scope = { Data.OptionsData.Translations }; // Data.OptionsData.Translations는 예시; 실제 scope key 맞춰 변경
                        string translated = TranslationUtils.TranslatePreserveTags(orig, scope);
                        if (!string.IsNullOrEmpty(translated) && translated != orig)
                        {
                            setLabel(translated);
                        }
                    }
                }

                // Help 번역
                if (getHelp != null && setHelp != null)
                {
                    string origHelp = getHelp();
                    if (!string.IsNullOrEmpty(origHelp))
                    {
                        string[] scope = { Data.OptionsData.Translations };
                        string translatedHelp = TranslationUtils.TranslatePreserveTags(origHelp, scope);
                        if (!string.IsNullOrEmpty(translatedHelp) && translatedHelp != origHelp)
                        {
                            setHelp(translatedHelp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Qud-KR] Options_UI_Patch postfix exception: " + ex);
            }
        }
    }
}