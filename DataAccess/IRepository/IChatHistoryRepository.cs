using IRepository;
using Models;

namespace DataAccess.IRepository
{
    public interface IChatHistoryRepository : IRepository<ChatHistory>
    {
        new long Add(ChatHistory chatHistory);
        new IEnumerable<ChatHistory> GetAll(Dictionary<string, dynamic>? condition, bool resolveRelation = false, bool lastOnly = true, int n = 10, bool whereConditionUseOR = false);
        new ChatHistory? Get(Dictionary<string, dynamic> condition, bool resolveRelation = false);
        new ChatHistory? Update(ChatHistory chatHistory);
        int Remove(int chatHistoryId);
        int RemoveRange(List<int> chatHistoryIds);
        long Count(Dictionary<string, dynamic>? condition = null);
    }
}
