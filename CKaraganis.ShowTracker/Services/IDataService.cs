using CKaraganis.ShowTracker.Models;

namespace CKaraganis.ShowTracker.Services;

public interface IDataService
{
    Task<IEnumerable<Show>> GetAllShowsAsync();
}