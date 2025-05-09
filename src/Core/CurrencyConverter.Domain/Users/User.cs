using CurrencyConverter.Domain.Common;

namespace CurrencyConverter.Domain.Users
{
    public class User : AggregateRoot
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string Password;
        private readonly HashSet<string> _roles = new HashSet<string>();
        public IReadOnlyCollection<string> Roles => _roles;
        public bool IsActive { get; private set; }
        public DateTime LastLogin { get; private set; }
        public int DailyRequestQuota { get; private set; }
        public int MonthlyRequestQuota { get; private set; }

        private User() { } // For ORM

        public User(string username, string email, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

            Username = username;
            Email = email;
            Password = passwordHash;
            IsActive = true;
            DailyRequestQuota = 1000;  // Default values
            MonthlyRequestQuota = 30000;
            Id = Guid.NewGuid();
        }

        public void AddRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null or empty", nameof(role));

            _roles.Add(role);
        }

        public void RemoveRole(string role)
        {
            _roles.Remove(role);
        }

        public bool HasRole(string role)
        {
            return _roles.Contains(role);
        }

        public void UpdateQuotas(int dailyQuota, int monthlyQuota)
        {
            if (dailyQuota < 0)
                throw new ArgumentException("Daily quota cannot be negative", nameof(dailyQuota));

            if (monthlyQuota < 0)
                throw new ArgumentException("Monthly quota cannot be negative", nameof(monthlyQuota));

            DailyRequestQuota = dailyQuota;
            MonthlyRequestQuota = monthlyQuota;
        }

        public void RecordLogin()
        {
            LastLogin = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void Activate()
        {
            IsActive = true;
        }

        public bool VerifyPassword(string passwordHash)
        {
            return Password == passwordHash;
        }

        public void ChangePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("New password hash cannot be null or empty", nameof(newPasswordHash));

            Password = newPasswordHash;
        }
    }
}
