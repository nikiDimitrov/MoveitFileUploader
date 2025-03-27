using MoveitLocalFolderScanner.Controller;
using MoveitLocalFolderScanner.Helpers;
using MoveitLocalFolderScanner.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

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
