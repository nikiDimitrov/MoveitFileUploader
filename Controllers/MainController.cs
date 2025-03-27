using MoveitLocalFolderScanner.Helpers;
using MoveitLocalFolderScanner.Models;
using MoveitLocalFolderScanner.Views;

namespace MoveitLocalFolderScanner.Controller
{
    public class MainController
    {
        private User user;
        private MainView view;
        private string folderPath;

        public MainController()
        {
            view = new MainView();
        }

        public async Task Start()
        {
            view.ShowIntro();

            User user = null;
            while (user == null)
            {
                string[] userInput = view.GetUserParameters();
                string username = userInput[0];
                string password = userInput[1];

                try
                {
                    user = new User(username, password);
                }
                catch (ArgumentNullException e)
                {
                    view.DisplayError(e.Message);
                    user = null;
                }
            }

            string accessToken;
            try
            {
                accessToken = await MoveitCommunicator.Instance
                    .RetrieveAPIToken(user);
                user.AccessToken = accessToken;
            }
            catch (HttpRequestException e)
            {
                view.DisplayError(e.Message);
                return;
            }

            view.DisplaySuccess("User authorized!");

            this.user = user;
            await Options();
        }

        private async Task Options()
        {
            int option = view.DisplayOptions();
            while(option != 0)
            {
                switch(option)
                {
                    case 1:
                        SetFolder(); 
                        break;
                    case 2:
                        await ScanAndUploadNewFiles();
                        break;
                }
                option = view.DisplayOptions();
            }
        }

        private void SetFolder()
        {
            string folderPath = view.GetFolderPath();
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                view.DisplayError("Folder path is empty or does not exist!");
                return;
            }

            string[] files = Directory.GetFiles(folderPath);
            if (files == null || files.Length == 0)
            {
                view.DisplayError("Folder does not contain any files to upload!");
                return;
            }

            this.folderPath = folderPath;
            view.DisplaySuccess("Folder path set successfully!");
        }

        private async Task ScanAndUploadNewFiles()
        {
            if(folderPath == null)
            {
                view.DisplayError("Local folder not set!");
                return;
            }

            view.DisplayMessage("Scanning for new files...\n");

            string homeUserFolderId;
            try
            {
                homeUserFolderId = await MoveitCommunicator.Instance.GetHomeUserFolderId(user);
            }
            catch (FileNotFoundException e)
            {
                view.DisplayError("Home folder of user not found!");
                return;
            }

            List<string> missingFiles = new List<string>();
            try
            {
                missingFiles = await MoveitCommunicator.Instance.ScanForNewFiles(user, homeUserFolderId, folderPath);
            }
            catch(HttpRequestException e)
            {
                view.DisplayError(e.Message);
            }
       
            if(missingFiles.Count == 0)
            {
                view.DisplayMessage("No new files detected!");
            }
            else
            {
                foreach(string missingFilePath in missingFiles)
                {
                    await UploadFile(user, missingFilePath, homeUserFolderId);
                }
            }
        }

        private async Task UploadFile(User user, string filePath, string folderId)
        {
            try
            {
                view.DisplayFileUploadStart(filePath);
                await MoveitCommunicator.Instance.UploadFile(user, folderId, filePath);
                view.DisplaySuccess($"{Path.GetFileName(filePath)} uploaded successfully!");
            }
            catch(HttpRequestException e)
            {
                view.DisplayError(e.Message);
            }
        }

    }
}
