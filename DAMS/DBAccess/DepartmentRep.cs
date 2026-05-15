using DAMS.Models;
using System.Data.SqlClient;

namespace DAMS.DBAccess
{
    public class DepartmentRep
    {
        private readonly string _connectionString;
        public DepartmentRep(string connectionString)
        {
            _connectionString = connectionString;
        }
        public bool ExistByName(string name)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT COUNT(*) FROM Departments WHERE Name = @name";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", name);
            try
            {
                connection.Open();
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error checking department existence: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        public List<Department> GetAllDepartments()
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT * FROM Departments";
            using SqlCommand command = new SqlCommand(query, connection);
            try
            {
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                List<Department> departments = new List<Department>();
                while (reader.Read())
                {
                    departments.Add(new Department
                    {
                        DepartmentId = (int)reader["DepartmentId"],
                        DepartmentName = reader["DepartmentName"].ToString(),
                    });
                }
                return departments;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching departments: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        public Department GetDepartmentById(int id)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "SELECT * FROM Departments WHERE DepartmentId = @id";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);
            try
            {
                connection.Open();
                using SqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new Department
                    {
                        DepartmentId = (int)reader["DepartmentId"],
                        DepartmentName = reader["DepartmentName"].ToString(),
                    };
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching department: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        public void AddDepartment(Department department)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "INSERT INTO Departments (Name) VALUES (@name)";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", department.DepartmentName);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding department: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        public void UpdateDepartment(Department department)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "UPDATE Departments SET Name = @name WHERE DepartmentId = @id";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", department.DepartmentName);
            command.Parameters.AddWithValue("@id", department.DepartmentId);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating department: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
        public void DeleteDepartment(Department department)
        {
            using SqlConnection connection = new SqlConnection(_connectionString);
            string query = "DELETE FROM Departments WHERE DepartmentId = @id";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", department.DepartmentId);
            try
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting department: " + ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }
     }
}
