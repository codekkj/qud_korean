/*
 * 파일명: Options_Patch.cs
 * 분류: [UI Patch] 설정 화면 패치
 * 역할: 설정(Options) 화면이 열리고 닫힐 때 번역 범위를 관리합니다.
 */

using HarmonyLib;
using Qud.UI;
using UnityEngine;
using QudKRTranslation.Data;

namespace QudKRTranslation.Patches
{
    [HarmonyPatch(typeof(OptionsScreen))]
    public static class Options_Patch
    {
        [HarmonyPatch(nameof(OptionsScreen.Show), new System.Type[0])]
        [HarmonyPrefix]
        static void Show_Prefix()
        {
            // 주의: CommonData를 같이 넣으면 옵션 UI 내부의 동적 텍스트 및 툴팁까지 과도하게 번역될 수 있음.
            // 따라서 옵션 화면에서는 OptionsData 고유 항목만 번역 대상으로 삼습니다.
            if (!ScopeManager.IsScopeActive(OptionsData.Translations))
            {
                ScopeManager.PushScope(OptionsData.Translations);
                Debug.Log("[Options_Patch] Scope activated (Options logic only)");
            }
        }

        [HarmonyPatch(nameof(OptionsScreen.Hide), new System.Type[0])]
        [HarmonyPostfix]
        static void Hide_Postfix()
        {
            // 현재 스코프가 OptionsData면 제거
            if (ScopeManager.IsScopeActive(OptionsData.Translations))
            {
                ScopeManager.PopScope();
                Debug.Log("[Options_Patch] Scope deactivated");
            }
        }
    }
}
