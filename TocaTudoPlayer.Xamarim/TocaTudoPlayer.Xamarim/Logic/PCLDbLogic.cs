using PCLStorage;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace TocaTudoPlayer.Xamarim.Logic
{
    public class PCLDbLogic : IPCLStorageDb
    {
        private IFolder _folder;
        public PCLDbLogic()
        {
            IFolder rootFolder = FileSystem.Current.LocalStorage;
            _folder = rootFolder.CreateFolderAsync("TocaTudoPCL", CreationCollisionOption.OpenIfExists).Result;
        }
        public async Task<string> GetFile(string fileName)
        {
            try
            {
                ExistenceCheckResult checkResult = await _folder.CheckExistsAsync(fileName);

                if (checkResult == ExistenceCheckResult.FileExists)
                {
                    IFile file = await _folder.GetFileAsync(fileName);
                    return await file.ReadAllTextAsync();
                }
            }
            catch { }

            return string.Empty;
        }
        public async ValueTask<T> GetJson<T>(string fileName)
        {
            try
            {
                ExistenceCheckResult checkResult = await _folder.CheckExistsAsync(fileName);

                if (checkResult == ExistenceCheckResult.FileExists)
                {
                    IFile file = await _folder.GetFileAsync(fileName);
                    Stream stream = await file.OpenAsync(PCLStorage.FileAccess.Read);

                    return await JsonSerializer.DeserializeAsync<T>(stream);
                }
            }
            catch  { }

            return default(T);
        }
        public async Task SaveFile(string fileName, string fileContent)
        {
            try
            {
                IFile file = await _folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await file.WriteAllTextAsync(fileContent);
            }
            catch { }
        }
        public async Task<bool> SaveFile<T>(string fileName, T obj)
        {
            try
            {
                string json = JsonSerializer.Serialize(obj);

                IFile file = await _folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                await file.WriteAllTextAsync(json);

                return true;
            }
            catch 
            {
                return false;
            }
        }
        public async Task RemoveFile(string fileName)
        {
            try
            {
                IFile file = await _folder.GetFileAsync(fileName);
                await file.DeleteAsync();
            }
            catch { }
        }
    }
}
