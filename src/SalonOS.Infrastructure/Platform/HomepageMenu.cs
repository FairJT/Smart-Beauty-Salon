namespace SalonOS.Infrastructure;

public enum MenuLocation { Header = 1, Footer = 2 }

public class HomepageMenu
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public MenuLocation Location { get; set; } = MenuLocation.Header;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}