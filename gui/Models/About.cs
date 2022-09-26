namespace gui.Models;

internal class About
{
    public string Title => AppInfo.Name;
    public string Version => AppInfo.VersionString;
    public string MoreInfoUrl => "https://karlokoelewijn.nl/";
    public string Message => "message string...";
}