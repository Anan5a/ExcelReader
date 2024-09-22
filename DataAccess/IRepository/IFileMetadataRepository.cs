using Models;

namespace IRepository
{
    public interface IFileMetadataRepository : IRepository<FileMetadata>
    {
        new long Add(FileMetadata fileMetadata);
        new IEnumerable<FileMetadata> GetAll(Dictionary<string, dynamic>? condition, bool resolveRelation = false);
        new FileMetadata? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        new FileMetadata? Update(FileMetadata fileMetadata);
        int Remove(int id);
        int RemoveRange(List<int> Ids);
        long Count(Dictionary<string, dynamic>? condition = null);

    }
}
