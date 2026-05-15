using DAMS.Models;
using System.Data;
using System.Data.SqlClient;

namespace DAMS.DBAccess
{
    public class UserRep
    {
        private readonly string _connection;
        public UserRep(string connection)
        {
            _connection = connection;
        }

        public List<User> GetAllUsers()
        {
            List<User> users = new List<User>();
            using SqlConnection sqlConnection = new SqlConnection(_connection);
            string query = @"Select UserId,Name,Role,Email,CreatedAt from users";
            using SqlCommand cmd = new SqlCommand(query, sqlConnection);
            try
            {
                sqlConnection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserId = (int)reader["UserId"],
                        Name = reader["Name"].ToString(),
                        Role = reader["Role"].ToString(),
                        Email = reader["Email"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching users", ex);
            }
        }
        public User GetUserById(int userId)
        {
            using SqlConnection sqlConnection = new SqlConnection(_connection);

            string query = @"Select UserId,Name,Role,Email,CreatedAt from users where userId=@id ";

            using SqlCommand cmd = new SqlCommand(query, sqlConnection);
            cmd.Parameters.AddWithValue("@id", userId);

            try
            {
                sqlConnection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    return new User
                    {
                        UserId = (int)reader["UserId"],
                        Name = reader["Name"].ToString(),
                        Role = reader["Role"].ToString(),
                        Email = reader["Email"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching user", ex);
            }

        }
        public void UpdateUser(User user)
        {
            using SqlConnection sqlConnection = new SqlConnection(_connection);
            
            
            string query = @"Update users set Name=@n,Email=@e,Password=@p,Role=@r where UserId=@id";
            using SqlCommand cmd = new SqlCommand(query, sqlConnection);
            cmd.Parameters.AddWithValue("@n", user.Name);
            cmd.Parameters.AddWithValue("@e", user.Email);
            cmd.Parameters.AddWithValue("@p", user.Password);
            cmd.Parameters.AddWithValue("@r", user.Role);
            cmd.Parameters.AddWithValue("@id", user.UserId);
            try
            {
                sqlConnection.Open();
                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating user", ex);
            }
        }
        public void DeleteUser(int userId)
        {
            using SqlConnection sqlConnection = new SqlConnection(_connection);
            
            
            string query = @"Delete from users where UserId=@id";
            using SqlCommand cmd = new SqlCommand(query, sqlConnection);
            cmd.Parameters.AddWithValue("@id", userId);
            try
            {
                sqlConnection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting user", ex);
            }

        }
        public List<User> GetUserByRole(string role)
        {
            List<User> users = new List<User>();
            using SqlConnection sqlConnection = new SqlConnection(_connection);
            string query = @"Select UserId,Name,Role,Email,CreatedAt from users where role=@r";
            using SqlCommand cmd = new SqlCommand(query, sqlConnection);
            cmd.Parameters.AddWithValue("@r", role);
            try
            {
                sqlConnection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    users.Add(new User
                    {
                        UserId = (int)reader["UserId"],
                        Name = reader["Name"].ToString(),
                        Role = reader["Role"].ToString(),
                        Email = reader["Email"].ToString(),
                        CreatedAt = (DateTime)reader["CreatedAt"]
                    });
                }
                return users;
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching users by role", ex);
            }

        }

    }
}
