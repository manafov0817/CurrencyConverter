namespace CurrencyConverter.Domain.Users.Builders
{
    public class UserBuilder
    {
        private string _username;
        private string _email;
        private string _password;
        private readonly HashSet<string> _roles = new HashSet<string>();
        private bool _isActive = true;
        private int _dailyRequestQuota = 1000;
        private int _monthlyRequestQuota = 30000;

        public UserBuilder WithUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));

            _username = username;
            return this;
        }

        public UserBuilder WithEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be null or empty", nameof(email));

            _email = email;
            return this;
        }

        public UserBuilder WithPassword(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be null or empty", nameof(passwordHash));

            _password = passwordHash;
            return this;
        }

        public UserBuilder WithRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be null or empty", nameof(role));

            _roles.Add(role);
            return this;
        }

        public UserBuilder WithRoles(IEnumerable<string> roles)
        {
            if (roles == null)
                throw new ArgumentNullException(nameof(roles));

            foreach (var role in roles)
            {
                WithRole(role);
            }

            return this;
        }

        public UserBuilder SetActive(bool isActive)
        {
            _isActive = isActive;
            return this;
        }

        public UserBuilder WithDailyQuota(int dailyQuota)
        {
            if (dailyQuota < 0)
                throw new ArgumentException("Daily quota cannot be negative", nameof(dailyQuota));

            _dailyRequestQuota = dailyQuota;
            return this;
        }

        public UserBuilder WithMonthlyQuota(int monthlyQuota)
        {
            if (monthlyQuota < 0)
                throw new ArgumentException("Monthly quota cannot be negative", nameof(monthlyQuota));

            _monthlyRequestQuota = monthlyQuota;
            return this;
        }

        public User Build()
        {
            if (string.IsNullOrWhiteSpace(_username))
                throw new InvalidOperationException("Username must be set");

            if (string.IsNullOrWhiteSpace(_email))
                throw new InvalidOperationException("Email must be set");

            if (string.IsNullOrWhiteSpace(_password))
                throw new InvalidOperationException("Password hash must be set");

            var user = new User(_username, _email, _password);

            foreach (var role in _roles)
            {
                user.AddRole(role);
            }

            if (_isActive != user.IsActive)
            {
                if (_isActive)
                {
                    user.Activate();
                }
                else
                {
                    user.Deactivate();
                }
            }

            user.UpdateQuotas(_dailyRequestQuota, _monthlyRequestQuota);

            return user;
        }

        public static UserBuilder Create()
        {
            return new UserBuilder();
        }
    }
}
