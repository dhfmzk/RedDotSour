namespace RedDotSour.Persistence
{
    public interface IRedDotPersistence
    {
        /// <summary>
        /// dirty records만 저장한다.
        /// </summary>
        void Save(RedDotSaveData data);

        /// <summary>
        /// 전체 records를 저장한다. Compact용.
        /// </summary>
        void SaveAll(RedDotSaveData fullData);

        /// <summary>
        /// 저장된 데이터를 로드한다.
        /// </summary>
        RedDotSaveData Load();

        /// <summary>
        /// 저장된 데이터를 전부 삭제한다.
        /// </summary>
        void Clear();
    }
}
