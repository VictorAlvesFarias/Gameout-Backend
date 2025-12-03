namespace Application.Dtos.AppFile
{
    public class AppFileRequestDto
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool VersionControl { get; set; }
        public bool Observer { get; set; }
        public bool AutoValidateSync { get; set; }
    }
}
