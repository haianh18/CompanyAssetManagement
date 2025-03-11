using Microsoft.AspNetCore.Identity;

namespace CompanyAssetManagement.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string FullName { get; set; }
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        public string status { get; set; }
    }
}
