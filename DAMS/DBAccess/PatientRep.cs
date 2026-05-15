using DAMS.Models;
using System.Data.SqlClient;

namespace DAMS.DBAccess
{
    public class PatientRep
    {
        private readonly string _connectionString;
        public PatientRep(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Patient GetPatientById(int patientId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT PatientId, UserId, City,PatientName,Gender,DOB, Phone FROM Patients WHERE PatientId = @PatientId";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", patientId);
            try
            {
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new Patient
                    {
                        PatientId = (int)reader["PatientId"],
                        UserId = (int)reader["UserId"],
                        City = reader["City"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        PatientName = reader["PatientName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DateOfBirth = (DateTime)reader["DOB"]
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching patient by ID: " + ex.Message);
            }
            return null;
        }
        public Patient GetPatientByUserId(int userId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT PatientId, UserId, City,PatientName, Phone,Gender,DOB FROM Patients WHERE UserId = @UserId";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);
            try
            {
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new Patient
                    {
                        PatientId = (int)reader["PatientId"],
                        UserId = (int)reader["UserId"],
                        City = reader["City"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        PatientName = reader["PatientName"].ToString(),
                        Gender = reader["Gender"].ToString(),
                        DateOfBirth = (DateTime)reader["DOB"]
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching patient by User ID: " + ex.Message);
            }
            return null;
        }
        public bool ChkPatientByUserId(int userId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);

            string query = "SELECT 1 FROM Patients WHERE UserId = @UserId";

            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            try
            {
                connection.Open();

                using SqlDataReader reader = command.ExecuteReader();

                return reader.Read(); // true if exists, false otherwise
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking patient existence: " + ex.Message);
            }
        }
        public void AddPatient(Patient patient)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "INSERT INTO Patients (UserId, City, Phone, PatientName,Gender,DOB) VALUES (@UserId, @City, @Phone, @PatientName,@Gender,@DOB)";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", patient.UserId);
            command.Parameters.AddWithValue("@City", patient.City);
            command.Parameters.AddWithValue("@Phone", patient.Phone);
            command.Parameters.AddWithValue("@PatientName", patient.PatientName);
            command.Parameters.AddWithValue("@Gender", patient.Gender);
            command.Parameters.AddWithValue("@DOB", patient.DateOfBirth);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding patient: " + ex.Message);
            }

        }

        public void UpdatePatient(Patient patient)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "UPDATE Patients SET  City = @City, Phone = @Phone, PatientName = @PatientName,Gender=@Gender,DOB=@DOB WHERE PatientId = @PatientId";
            using SqlCommand command = new SqlCommand(query, connection);
            //command.Parameters.AddWithValue("@UserId", patient.UserId);
            command.Parameters.AddWithValue("@City", patient.City);
            command.Parameters.AddWithValue("@Phone", patient.Phone);
            command.Parameters.AddWithValue("@PatientName", patient.PatientName);
            command.Parameters.AddWithValue("@PatientId", patient.PatientId);
            command.Parameters.AddWithValue("@Gender", patient.Gender);
            command.Parameters.AddWithValue("@DOB", patient.DateOfBirth);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating patient: " + ex.Message);
            }
        }
        public int GetUserIdByPatientId(int patientId)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT UserId FROM Patients WHERE PatientId = @PatientId";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@PatientId", patientId);
            try
            {
                connection.Open();
                object result = command.ExecuteScalar();
                if (result != null)
                {
                    return (int)result;
                }
                else
                {
                    throw new Exception("Patient not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching User ID by Patient ID: " + ex.Message);
            }
        }
        public List<Patient> GetAllPatients()
        {
            List<Patient> patients = new List<Patient>();
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT PatientId, UserId, City, PatientName, Phone, Gender, DOB FROM Patients";
            using SqlCommand command = new SqlCommand(query, connection);
            try
            {
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    patients.Add(new Patient
                    {
                        PatientId = reader.GetInt32(0),
                        UserId = reader.GetInt32(1),
                        City = reader.GetString(2),
                        PatientName = reader.GetString(3),
                        Phone = reader.GetString(4),
                        Gender = reader.GetString(5),
                        DateOfBirth = reader.GetDateTime(6)
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching all patients: " + ex.Message);
            }
            return patients;
        }
    }
}
