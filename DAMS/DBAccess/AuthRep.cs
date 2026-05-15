using DAMS.Models;
using System.Data.SqlClient;

namespace DAMS.DBAccess
{
    public class AuthRep
    {
        private readonly string _connection;
        public AuthRep(string connection)
        {
            _connection = connection;
        }

        public User Login(string email, string role)
        {
            using SqlConnection sqlConnection = new SqlConnection(_connection);

            string query = @"Select * from users where Email=@e and  role=@r";

            SqlCommand cmd = new SqlCommand(query, sqlConnection);
            cmd.Parameters.AddWithValue("@e", email);
            //cmd.Parameters.AddWithValue("@p", password);
            cmd.Parameters.AddWithValue("@r", role);

            try
            {
                sqlConnection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    string dbPassword = reader["Password"].ToString();

                    return new User
                    {
                        UserId = (int)reader["UserId"],
                        Name = reader["Name"].ToString(),
                        Role = reader["Role"].ToString(),
                        Password = reader["Password"].ToString(),
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void Register(User user)
        {
            using SqlConnection sqlConnection = new SqlConnection(_connection);
            string query = @"Insert into users (Name, Email, Password, Role, CreatedAt) 
                             values (@n, @e, @p, @r, @c)";
            SqlCommand cmd = new SqlCommand(query, sqlConnection);
            cmd.Parameters.AddWithValue("@n", user.Name);
            cmd.Parameters.AddWithValue("@e", user.Email);
            cmd.Parameters.AddWithValue("@p", user.Password);
            cmd.Parameters.AddWithValue("@r", user.Role);
            cmd.Parameters.AddWithValue("@c", DateTime.Now);
            try
            {
                sqlConnection.Open();
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                sqlConnection.Close();
            }
        }
        public User GetUserByEmail(string email)
        {
            using SqlConnection sqlConnection = new SqlConnection(_connection);
            string query = @"Select * from users where Email=@e";
            SqlCommand cmd = new SqlCommand(query, sqlConnection);
            cmd.Parameters.AddWithValue("@e", email);
            try
            {
                sqlConnection.Open();
                using SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        UserId = Convert.ToInt32(reader["UserId"]),
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Password = reader["Password"].ToString(),
                        Role = reader["Role"].ToString()
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
    }
}
