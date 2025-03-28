using MoveitLocalFolderScanner.Controller;
using System.Text;

namespace MoveitLocalFolderScanner
{
    internal class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            MainController controller = new MainController();
            await controller.Start();
        }
    }
}
