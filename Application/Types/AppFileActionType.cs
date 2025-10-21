namespace Domain.Entitites.Shared
{
    public enum AppFileActionType
    {
        InsertFile,
        UpdateFile,
        DeleteFile,
        DeleteStoredFile,
        DownloadFile,
        RequestSync,
        SingleSync,
        StreamAssigned,
        ProcessingCompleted,
        ProcessingFailed,
        ProcessingError
    }
}
