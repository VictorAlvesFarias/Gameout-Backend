namespace Domain.Entitites.ApplicationContextDb
{
    public class AppFile : BaseUserOwnedEntity
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool VersionControl { get; set; }
        public bool Observer { get; set; }
        public bool AutoValidateSync { get; set; }
        public int Status { get; set; }
        public string StatusDetails { get; set; }
        public string StatusMessage { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public void Update(string? name = null, string? path = null, bool? versionControl = null, bool? observer = null, bool? autoValidateSync = null, int? status = null, string? statusDetails = null, string? statusMessage = null)
        {
            Name = name ?? Name;
            Path = path ?? Path;
            VersionControl = versionControl ?? VersionControl;
            Observer = observer ?? Observer;
            UpdateDate = DateTime.Now;
            AutoValidateSync = autoValidateSync ?? AutoValidateSync;
            Status = status ?? Status;
            StatusDetails = statusDetails ?? StatusDetails;
            StatusMessage = statusMessage ?? StatusMessage;
        }
    }
}
