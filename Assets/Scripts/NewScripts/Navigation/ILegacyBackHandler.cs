namespace SaySuomi.Navigation
{
    /// <summary>
    /// Marks a screen that still reads Escape in its own Update, so one press does not also
    /// reach Navigator. Temporary: remove it from each screen as it migrates, and delete this
    /// when none are left.
    /// </summary>
    public interface ILegacyBackHandler { }
}
