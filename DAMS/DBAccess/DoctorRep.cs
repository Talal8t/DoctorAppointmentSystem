using DAMS.Models;
using System.Data.SqlClient;

namespace DAMS.DBAccess
{
    public class DoctorRep
    {
        private readonly string _connectionString;

        public DoctorRep(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<Doctor> GetAllDoctors()
        {
            var doctors = new List<Doctor>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Doctors";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();

                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        doctors.Add(new Doctor
                        {
                            DoctorId = (int)reader["DoctorId"],
                            UserId = (int)reader["UserId"],
                            DepartmentId = (int)reader["DepartmentId"],
                            Specialization = reader["Specialization"].ToString(),
                            DoctorName = reader["DoctorName"].ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error fetching doctors: " + ex.Message);
                }
            }

            return doctors;
        }

        public Doctor GetDoctorById(int doctorId)
        {
            Doctor doctor = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Doctors WHERE DoctorId = @doctorId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@doctorId", doctorId);

                try
                {
                    connection.Open();

                    using SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        doctor = new Doctor
                        {
                            DoctorId = (int)reader["DoctorId"],
                            UserId = (int)reader["UserId"],
                            DepartmentId = (int)reader["DepartmentId"],
                            Specialization = reader["Specialization"].ToString(),
                            DoctorName = reader["DoctorName"].ToString()
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error fetching doctor by ID: " + ex.Message);
                }
            }

            return doctor;
        }

        public List<Doctor> GetDoctorsByDepartment(int departmentId)
        {
            var doctors = new List<Doctor>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Doctors WHERE DepartmentId = @departmentId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@departmentId", departmentId);

                try
                {
                    connection.Open();

                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        doctors.Add(new Doctor
                        {
                            DoctorId = (int)reader["DoctorId"],
                            UserId = (int)reader["UserId"],
                            DepartmentId = (int)reader["DepartmentId"],
                            Specialization = reader["Specialization"].ToString(),
                            DoctorName = reader["DoctorName"].ToString()
                        });
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error fetching doctors by department: " + ex.Message);
                }
            }

            return doctors;
        }

        public void AddDoctor(Doctor doctor)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"INSERT INTO Doctors 
                                (UserId, DepartmentId, Specialization, DoctorName) 
                                VALUES (@userId, @departmentId, @specialization, @doctorName)";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@userId", doctor.UserId);
                command.Parameters.AddWithValue("@departmentId", doctor.DepartmentId);
                command.Parameters.AddWithValue("@specialization", doctor.Specialization);
                command.Parameters.AddWithValue("@doctorName", doctor.DoctorName);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error adding doctor: " + ex.Message);
                }
            }
        }

        public void UpdateDoctor(Doctor doctor)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"UPDATE Doctors 
                                SET UserId=@userId, DepartmentId=@departmentId, 
                                    Specialization=@specialization, DoctorName=@doctorName 
                                WHERE DoctorId=@doctorId";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@userId", doctor.UserId);
                command.Parameters.AddWithValue("@departmentId", doctor.DepartmentId);
                command.Parameters.AddWithValue("@specialization", doctor.Specialization);
                command.Parameters.AddWithValue("@doctorName", doctor.DoctorName);
                command.Parameters.AddWithValue("@doctorId", doctor.DoctorId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error updating doctor: " + ex.Message);
                }
            }
        }

        public void DeleteDoctor(int doctorId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "DELETE FROM Doctors WHERE DoctorId = @doctorId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@doctorId", doctorId);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error deleting doctor: " + ex.Message);
                }
            }
        }

        public Doctor GetDoctorByUserId(int userId)
        {
            Doctor doctor = null;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Doctors WHERE UserId = @userId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@userId", userId);

                try
                {
                    connection.Open();

                    using SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        doctor = new Doctor
                        {
                            DoctorId = (int)reader["DoctorId"],
                            UserId = (int)reader["UserId"],
                            DepartmentId = (int)reader["DepartmentId"],
                            Specialization = reader["Specialization"].ToString(),
                            DoctorName = reader["DoctorName"].ToString()
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error fetching doctor by user ID: " + ex.Message);
                }
            }

            return doctor;
        }
        public int GetUserIdByDoctorId(int doctorId)
        {
            int userId = -1;
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT UserId FROM Doctors WHERE DoctorId = @doctorId";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@doctorId", doctorId);
                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                    {
                        userId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error fetching user ID by doctor ID: " + ex.Message);
                }
            }
            return userId;
        }
    }
}