using RedDotSour.Core;
using UnityEngine;

namespace RedDotSour.Samples
{
    /// <summary>
    /// OnChanged 이벤트 구독 → UI 반영 참고 예시.
    /// 라이브러리는 UI를 제공하지 않으므로, 사용자가 이런 식으로 구현한다.
    /// </summary>
    public class SampleRedDotBadge : MonoBehaviour
    {
        [SerializeField] private SampleCategory _category;
        [SerializeField] private GameObject _badgeObject;

        private IRedDotContainer _container;

        private void Start()
        {
            if (SampleGameManager.I == null) return;

            this._container = this._category switch
            {
                SampleCategory.Inventory => SampleGameManager.I.Inventory,
                SampleCategory.Quest => SampleGameManager.I.Quest,
                SampleCategory.Mail => SampleGameManager.I.Mail,
                _ => null,
            };

            if (this._container == null) return;

            // 이벤트 구독 — 상태 변경 시 UI 자동 갱신
            this._container.OnChanged += this.UpdateBadge;

            // 초기 상태 반영
            this.UpdateBadge();
        }

        private void OnDestroy()
        {
            if (this._container != null)
            {
                this._container.OnChanged -= this.UpdateBadge;
            }
        }

        private void UpdateBadge()
        {
            if (this._badgeObject != null)
            {
                this._badgeObject.SetActive(this._container.IsOnAny());
            }
        }
    }
}
