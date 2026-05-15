using DAMS.DBAccess;
using DAMS.Models;

namespace DAMS.Services
{
    public class PatientService
    {
        private readonly PatientRep _patientRepository;

        public PatientService(PatientRep patientRepository)
        {
            _patientRepository = patientRepository;
        }
        public Patient GetPatientById(int patientId)
        {
            if (patientId <= 0)
                throw new Exception("Invalid patient ID");

            var patient = _patientRepository.GetPatientById(patientId);

            if (patient == null)
                throw new Exception("Patient not found");

            return patient;
        }
        public Patient GetPatientByUserId(int userId)
        {
            if (userId <= 0)
                throw new Exception("Invalid user ID");

            var patient = _patientRepository.GetPatientByUserId(userId);

            if (patient == null)
                return null; // return null if not found, caller can check

            return patient;
        }
        public bool ChkPatientByUserId(int userId)
        {
            if (userId <= 0)
                throw new Exception("Invalid user ID");

            return _patientRepository.ChkPatientByUserId(userId);
        }

        public void AddPatient(Patient patient)
        {
            if (patient == null)
                throw new Exception("Patient cannot be null");
            if (string.IsNullOrWhiteSpace(patient.PatientName))
                throw new Exception("Patient name cannot be empty");
            if (string.IsNullOrWhiteSpace(patient.City))
                throw new Exception("City cannot be empty");
            if (string.IsNullOrWhiteSpace(patient.Phone))
                throw new Exception("Phone cannot be empty");
            if (_patientRepository.GetPatientByUserId(patient.UserId) != null)
                throw new Exception("A patient with this user ID already exists");
            _patientRepository.AddPatient(patient);
        }
        public void UpdatePatient(Patient patient)
        {
            if (patient == null)
                throw new Exception("Patient cannot be null");

            if (string.IsNullOrWhiteSpace(patient.PatientName))
                throw new Exception("Patient name cannot be empty");

            if (string.IsNullOrWhiteSpace(patient.City))
                throw new Exception("City cannot be empty");

            if (string.IsNullOrWhiteSpace(patient.Phone))
                throw new Exception("Phone cannot be empty");

            // 1. patient exists
            var existing = GetExistingPatient(patient.PatientId);

            // 2. Check duplicate userId (but allow same record)
            var byUserId = _patientRepository.GetPatientByUserId(patient.UserId);
            if (byUserId != null && byUserId.PatientId != patient.PatientId)
                throw new Exception("Another patient with this user ID already exists");

            // 3. Update
            _patientRepository.UpdatePatient(patient);
        }
        private Patient GetExistingPatient(int id)
        {
            var patient = _patientRepository.GetPatientById(id);
            if (patient == null)
                throw new Exception("Patient not found");
            return patient;
        }
        public bool PatientExists(int patientId)
        {
            if (patientId <= 0)
                throw new Exception("Invalid patient ID");
            return _patientRepository.GetPatientById(patientId) != null;
        }
        public List<Patient> GetAllPatients()
        {
            return _patientRepository.GetAllPatients();
        }
    }
}
