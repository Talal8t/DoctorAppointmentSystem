using DAMS.DBAccess;
using DAMS.Models;

namespace DAMS.Services
{
    public class UserService
    {
        private readonly UserRep _userRep;
        private readonly AuthRep _authRep;

        public UserService(UserRep userRep, AuthRep authRep)
        {
            _userRep = userRep;
            _authRep = authRep;
        }
        public List<User> GetAllUsers()
        {
            return _userRep.GetAllUsers();
        }
        public User GetUserByID(int userId)
        {
            if (userId <= 0)
                throw new Exception("Invalid user ID");
            var user = _userRep.GetUserById(userId);
            if (user == null)
                throw new Exception("User not found");
            return user;
        }
        // LOGIN LOGIC
        public User ValidateUser(string email, string password, string role)
        {
            var user = _authRep.Login(email, role);

            if (user == null)
                return null;

            // TEMP password check (hash later)
            if (user.Password != password)
                return null;

            return user;
        }

        // UPDATE USER 
        public void UpdateUser(User updatedUser, User currentUser)
        {
            var existingUser = _userRep.GetUserById(updatedUser.UserId);

            if (existingUser == null)
                throw new Exception("User not found");

            // ROLE CHANGE RULE
            if (updatedUser.Role != existingUser.Role && currentUser.Role != "Admin")
                throw new Exception("Only admin can change roles");

            _userRep.UpdateUser(updatedUser);
        }

        // DELETE USER 
        public void DeleteUser(int userId, User currentUser)
        {
            var user = _userRep.GetUserById(userId);

            if (user == null)
                throw new Exception("User not found");

            if (currentUser.Role != "Admin")
                throw new Exception("Only admin can delete users");

            _userRep.DeleteUser(userId);
        }
        public void RegisterUser(User newUser)
        {
            // Check if email already exists
            var existingUser = _authRep.GetUserByEmail(newUser.Email);
            if (existingUser != null)
                throw new Exception("Email already in use");
            // TEMP password handling (hash later)
            _authRep.Register(newUser);
        }
        public bool GetUserById(int userId)
        {
            return _userRep.GetUserById(userId) != null;
        }
    }
}