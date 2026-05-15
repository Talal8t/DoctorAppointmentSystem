using DAMS.DBAccess;
using DAMS.Models;

namespace DAMS.Services
{
    public class DoctorService
    {
        private readonly DoctorRep _doctorRep;
        private readonly DepartmentRep _departmentRep;

        public DoctorService(DoctorRep doctorRep, DepartmentRep departmentRep)
        {
            _doctorRep = doctorRep;
            _departmentRep = departmentRep;
        }
        public List<Models.Doctor> GetAllDoctors()
        {
            return _doctorRep.GetAllDoctors();
        }
        public Doctor GetDoctorById(int doctorId)
        {
            if (doctorId <= 0)
                throw new Exception("Invalid Doctor ID");

            var doctor = _doctorRep.GetDoctorById(doctorId);

            if (doctor == null)
                throw new Exception("Doctor not found");

            return doctor;
        }
        public void AddDoctor(Doctor doctor)
        {

            if (doctor.UserId <= 0)
                throw new Exception("Invalid User ID");

            if (doctor.DepartmentId <= 0)
                throw new Exception("Invalid Department ID");

            if (string.IsNullOrWhiteSpace(doctor.DoctorName))
                throw new Exception("Doctor name cannot be empty");

            if (string.IsNullOrWhiteSpace(doctor.Specialization))
                throw new Exception("Specialization is required");


            var dept = _departmentRep.GetDepartmentById(doctor.DepartmentId);
            if (dept == null)
                throw new Exception("Department not found");


            _doctorRep.AddDoctor(doctor);
        }
        public void UpdateDoctor(Doctor doctor)
        {
            var existing = _doctorRep.GetDoctorById(doctor.DoctorId);

            if (existing == null)
                throw new Exception("Doctor not found");


            var dept = _departmentRep.GetDepartmentById(doctor.DepartmentId);
            if (dept == null)
                throw new Exception("Invalid department");

            _doctorRep.UpdateDoctor(doctor);
        }
        public void DeleteDoctor(int doctorId)
        {
            var existing = _doctorRep.GetDoctorById(doctorId);

            if (existing == null)
                throw new Exception("Doctor not found");

            _doctorRep.DeleteDoctor(doctorId);
        }
        public List<Doctor> GetDoctorsByDepartment(int departmentId)
        {
            var dept = _departmentRep.GetDepartmentById(departmentId);

            if (dept == null)
                throw new Exception("Department not found");

            return _doctorRep.GetDoctorsByDepartment(departmentId);

        }
        public async Task<bool> DoctorExists(int doctorId)
        {
            return await Task.FromResult(_doctorRep.GetDoctorById(doctorId) != null);
        }
        public async Task<bool> GetDoctorByUserId(int userId)
        {
            return await Task.FromResult(_doctorRep.GetDoctorByUserId(userId) != null);
        }
        public async Task<Doctor> GetDoctorByUserIdFlow(int userId)
        {
            var doctor = _doctorRep.GetDoctorByUserId(userId);
            if (doctor == null)
                throw new Exception("Doctor profile not found");
            return await Task.FromResult(doctor);
        }
    }
}
