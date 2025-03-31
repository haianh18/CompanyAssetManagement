namespace FinalProject.Models.ViewModels
{
    public class DepartmentManagementViewModel
    {
        public List<Department> Departments { get; set; } = new List<Department>();
        public bool ShowInactive { get; set; } = false;
        public string SearchString { get; set; } = string.Empty;
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}
