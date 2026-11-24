namespace ElsakaHerbs.interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream stream, string originalFileName, string folder);
    Task DeleteFileAsync(string relativeUrl);
}
