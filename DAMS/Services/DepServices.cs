using DAMS.DBAccess;
using DAMS.Models;

namespace DAMS.Services
{
    public class DepServices
    {
        private readonly DepartmentRep _departmentRep;

        public DepServices(DepartmentRep departmentRep)
        {
            _departmentRep = departmentRep;
        }
        public void AddDepartment(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("Department name cannot be empty.");
            }
            if (_departmentRep.ExistByName(name)) {
                throw new Exception("Department already exists.");
            }
            _departmentRep.AddDepartment(new Department { DepartmentName = name });
        }
        public void UpdateDepartment(int id, string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
                throw new Exception("Department name cannot be empty");

            var existing = _departmentRep.GetDepartmentById(id);
            if (existing == null)
                throw new Exception("Department not found");

            // allow same name for same record, but block duplicates
            if (_departmentRep.ExistByName(newName))
                throw new Exception("Department name already exists");

            _departmentRep.UpdateDepartment(new Department
            {
                DepartmentId = id,
                DepartmentName = newName
            });
        }
        public void DeleteDepartment(int id)
        {
            var existing = _departmentRep.GetDepartmentById(id);
            if (existing == null)
                throw new Exception("Department not found");

            _departmentRep.DeleteDepartment(new Department { DepartmentId = id });
        }
        public List<Department> GetAll()
        {
            return _departmentRep.GetAllDepartments();
        }

    }
}
