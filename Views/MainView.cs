namespace MoveitLocalFolderScanner.Views
{
    public class MainView
    {
        public void ShowIntro()
        {
            Console.WriteLine("Welcome to MoveItFileUploader!\n");
        }

        public int DisplayOptions()
        {
            string[] options = new string[]
            {
                "1. Set folder path",
                "2. Scan files and upload new ones",
                "0. Exit"
            };

            foreach(string option in options)
            {
                Console.WriteLine(option);
            }

            return int.Parse(Console.ReadLine().Trim());
        }
        
        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void DisplayFileUploadStart(string filePath)
        {
            Console.WriteLine($"Uploading {Path.GetFileName(filePath)}...");
        }

        public string[] GetUserParameters()
        {
            Console.WriteLine("Username: ");
            string username = Console.ReadLine().Trim();
            Console.WriteLine("Password: ");
            string password = Console.ReadLine().Trim();
            Console.WriteLine();

            return new string[] { username, password };
        }

        public string GetFolderPath()
        {
            Console.WriteLine("Folder path: ");
            return Console.ReadLine();

        }

        public void DisplaySuccess(string message)
        {
            Console.WriteLine($"Success: {message}\n");
        }

        public void DisplayError(string message)
        {
            Console.WriteLine($"Error: {message}\n");
        }

    }
}
