namespace Application.Dtos.AppFile
{
    public class AppFileRequestDto
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool VersionControl { get; set; } = false;
        public bool Observer { get; set; } = false;
    }
}
