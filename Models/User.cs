namespace MoveitLocalFolderScanner.Models
{
    public class User
    {
        private string username;
        private string password;
        private string accessToken;

        public string Username
		{
			get { return username; }
            private set
            {
                if(string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("Username is null!");
                }
            }
		}

		public string Password
		{
			get { return password; }
            private set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("Password" +
                        " is null!");
                }
            }
        }

		public string AccessToken
		{
			get { return accessToken; }
            set { accessToken = value; }
		}

        public User(string username, string password)
        {
            this.username = username;
            this.password = password;
            this.accessToken = "";
        }

    }
}
