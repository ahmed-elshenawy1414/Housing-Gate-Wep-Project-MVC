using StudentHousing.Resources;

// Root-level marker file to match Training Platform solution structure.
// Re-exports the shared resource type so Views can reference SharedResource directly.
namespace Housing_Gate_Wep_Project_MVC
{
    /// <summary>
    /// Alias for StudentHousing.Resources.SharedResource - kept at project root
    /// to mirror the Training Platform layout where SharedResource.cs lives
    /// alongside Program.cs and GlobalUsings.cs.
    /// </summary>
    public class SharedResource : StudentHousing.Resources.SharedResource { }
}
