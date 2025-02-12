namespace FitMind_API.Models.Enums
{
    public class Enum
    {
        public enum StatusEnum
        {
            Pending = 1,   // User has registered but not yet verified
            Active = 2,    // User is active and can use the application
            Inactive = 3,  // User is inactive (e.g., manually deactivated by admin)
            Suspended = 4, // User is temporarily suspended (e.g., due to policy violation)
            Deleted = 5    // User account is deleted (soft delete)
        }
    }
}
