namespace TelcobrightMediation
{
    public interface IConfigurationSection
    {
        string SectionName { get; set; }
        int SectionOrder { get; set; }
    }
}
