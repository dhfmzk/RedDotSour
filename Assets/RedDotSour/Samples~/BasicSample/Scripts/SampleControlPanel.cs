using RedDotSour.Core;
using UnityEngine;

namespace RedDotSour.Samples
{
    /// <summary>
    /// 디버그용 컨트롤 패널. 런타임에서 레드닷 상태를 조작한다.
    /// </summary>
    public class SampleControlPanel : MonoBehaviour
    {
        private SampleCategory _selectedCategory = SampleCategory.Inventory;
        private string _keyInput = "1";

        private void OnGUI()
        {
            if (SampleGameManager.I == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 400));
            GUILayout.Label("RedDotSour Control Panel", GUI.skin.box);

            // 카테고리 선택
            GUILayout.Label("Category:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Inventory")) this._selectedCategory = SampleCategory.Inventory;
            if (GUILayout.Button("Quest")) this._selectedCategory = SampleCategory.Quest;
            if (GUILayout.Button("Mail")) this._selectedCategory = SampleCategory.Mail;
            GUILayout.EndHorizontal();

            GUILayout.Label($"Selected: {this._selectedCategory}");

            // 키 입력
            GUILayout.Label("Key (int):");
            this._keyInput = GUILayout.TextField(this._keyInput);

            if (!int.TryParse(this._keyInput, out var key))
            {
                GUILayout.Label("Invalid key");
                GUILayout.EndArea();
                return;
            }

            var container = this.GetContainer();

            // 상태 표시
            GUILayout.Space(10);
            GUILayout.Label($"IsOn({key}): {container.IsOn(key)}");
            GUILayout.Label($"IsOnAny: {container.IsOnAny()}");
            GUILayout.Label($"CountOn: {container.CountOn()}");
            GUILayout.Label($"DirtyCount: {container.DirtyCount}");

            // 조작 버튼
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Register"))
            {
                container.Register(key);
            }

            if (GUILayout.Button("Mark"))
            {
                try { container.Mark(key); }
                catch (System.Collections.Generic.KeyNotFoundException e)
                { Debug.LogWarning(e.Message); }
            }

            if (GUILayout.Button("Unmark"))
            {
                try { container.Unmark(key); }
                catch (System.Collections.Generic.KeyNotFoundException e)
                { Debug.LogWarning(e.Message); }
            }

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Remove")) container.Remove(key);
            if (GUILayout.Button("ClearAll")) container.ClearAll();
            GUILayout.EndHorizontal();

            // 영속화
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) SampleGameManager.I.RedDot.Save();
            if (GUILayout.Button("Compact")) SampleGameManager.I.RedDot.Compact();
            if (GUILayout.Button("Load")) SampleGameManager.I.RedDot.Load();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private RedDotContainer<int> GetContainer()
        {
            return this._selectedCategory switch
            {
                SampleCategory.Inventory => SampleGameManager.I.Inventory,
                SampleCategory.Quest => SampleGameManager.I.Quest,
                SampleCategory.Mail => SampleGameManager.I.Mail,
                _ => SampleGameManager.I.Inventory,
            };
        }
    }
}
