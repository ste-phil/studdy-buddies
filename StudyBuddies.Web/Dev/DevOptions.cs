namespace StudyBuddies.Web.Dev;

public class DevOptions
{
    public const string SectionName = "Dev";

    public bool Enabled { get; set; }

    public string DefaultUserEmail { get; set; } = "philipp@dev.local";
}
