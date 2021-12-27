using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim
{
    public interface IPCLStorageDb
    {
        Task<string> GetFile(string fileName);
        ValueTask<T> GetJson<T>(string fileName);
        Task SaveFile(string fileName, string fileContent);
        Task<bool> SaveFile<T>(string fileName, T obj);
        Task RemoveFile(string fileName);
    }
}
