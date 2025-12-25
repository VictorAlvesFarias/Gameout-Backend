using System.ComponentModel;

namespace Application.Types
{
    public enum AppStoredFileStatusTypes
    {
        [Description("Processing")]
        Processing = 1,

        [Description("Error")]
        Error = 2,

        [Description("Complete")]
        Complete = 3,

        [Description("Pending with Error")]
        PendingWithError = 4
    }
}
