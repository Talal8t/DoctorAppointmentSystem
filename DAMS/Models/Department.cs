using System.Numerics;

namespace DAMS.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; }
        //Nav
        public List<Doctor> Doctors { get; set; }
       
    }
}
