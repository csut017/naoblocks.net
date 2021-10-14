using System.Threading.Tasks;

namespace NaoBlocks.Web.Communications
{
    public interface IRemoteStore
    {
        string CheckRemote(string robot);

        Task<string> StartAsync(string robot, string password);

        Task<string> StopAsync(string robot);
    }
}