namespace Domain.Entitites.ApplicationContextDb
{
    public class AppStoredFile : BaseUserOwnedEntity
    {
        public int AppFileId { get; set; }
        public AppFile AppFile { get; set; }
        public int? StoredFileId { get; set; }
        public StoredFile? StoredFile { get; set; }
        
        public void Update(int? appFileId = null, int? storedFileId = null)
        {
            AppFileId = appFileId ?? AppFileId;
            StoredFileId = storedFileId ?? StoredFileId;
        }
    }
}
