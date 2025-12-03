namespace Domain.Entitites.ApplicationContextDb
{
    public class AppStoredFile : BaseUserOwnedEntity
    {
        public int AppFileId { get; set; }
        public AppFile AppFile { get; set; }
        public int? StoredFileId { get; set; }
        public StoredFile? StoredFile { get; set; }
        public bool Versioned { get; set; }
        public int Status { get; set; }
        public string StatusDetails { get; set; }
        public string StatusMessage { get; set; }

        public void Update(int? appFileId = null, int? storedFileId = null, bool? versioned = null, int? status = null, string? statusDetails = null, string? statusMessage = null)
        {
            AppFileId = appFileId ?? AppFileId;
            StoredFileId = storedFileId ?? StoredFileId;
            Versioned = versioned ?? Versioned;
            UpdateDate = DateTime.Now;
            Status = status ?? Status;
            StatusDetails = statusDetails ?? StatusDetails;
            StatusMessage = statusMessage ?? StatusMessage;
        }
    }
}
