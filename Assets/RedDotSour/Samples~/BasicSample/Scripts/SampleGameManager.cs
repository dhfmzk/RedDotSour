using RedDotSour.Core;
using RedDotSour.Persistence;
using UnityEngine;

namespace RedDotSour.Samples
{
    /// <summary>
    /// RedDotSour 사용 예시. 트리 생성 + 경로 등록 + 영속화 설정.
    /// </summary>
    public class SampleGameManager : MonoBehaviour
    {
        public static SampleGameManager I { get; private set; }

        public RedDotSour<SampleCategory> RedDot { get; private set; }
        public RedDotContainer<int> Inventory { get; private set; }
        public RedDotContainer<int> Quest { get; private set; }
        public RedDotContainer<int> Mail { get; private set; }

        private void Awake()
        {
            if (I != null && I != this)
            {
                Destroy(this.gameObject);
                return;
            }

            I = this;
            DontDestroyOnLoad(this.gameObject);

            this.InitRedDot();
        }

        private void InitRedDot()
        {
            this.RedDot = new RedDotSour<SampleCategory>();

            // 영속화 설정 (Snapshot + Journal)
            this.RedDot.SetPersistence(new JournalFilePersistence("sample_reddot"));

            // 카테고리별 컨테이너 생성 (키 타입: int)
            this.Inventory = this.RedDot.Create<int>(
                SampleCategory.Inventory, k => k.ToString(), int.Parse);
            this.Quest = this.RedDot.Create<int>(
                SampleCategory.Quest, k => k.ToString(), int.Parse);
            this.Mail = this.RedDot.Create<int>(
                SampleCategory.Mail, k => k.ToString(), int.Parse);

            // 저장된 데이터 로드
            this.RedDot.Load();

            // 샘플 데이터 등록 (이미 로드된 키는 LoadRecord에서 덮어씀)
            for (var i = 1; i <= 5; i++) this.Inventory.Register(i);
            for (var i = 1; i <= 3; i++) this.Quest.Register(i);
            for (var i = 1; i <= 4; i++) this.Mail.Register(i);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                this.RedDot.Save();
            }
        }

        private void OnApplicationQuit()
        {
            // 종료 시 Compact로 깔끔하게 저장
            this.RedDot.Compact();
        }
    }
}
