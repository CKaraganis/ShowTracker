using CKaraganis.ShowTracker.Enums;
using CKaraganis.ShowTracker.Models;
using CKaraganis.ShowTracker.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CKaraganis.ShowTracker.Pages;

public record ShowViewModel(Show Show, int CycleEndEpisode, DateOnly CycleEndDate, DateOnly CompletionDate, double ProgressPercent, bool IsCompleted, bool NotStarted);

public class IndexModel : PageModel
{
    private readonly IDataService _dataService;

    public IndexModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public IEnumerable<ShowViewModel> Shows { get; set; } = [];

    public async Task OnGetAsync()
    {
        var shows = await _dataService.GetAllShowsAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);
        Shows = shows.Select(s =>
        {
            var notStarted = today < s.IndexingDate;
            var completed = CompletedIntervals(s, today);
            var cycleNumber = completed + 1;
            var endEpisode = Math.Min(s.IndexingEpisode + cycleNumber * s.EpisodesPerInterval, s.EpisodeCount);
            var endDate = AddIntervals(s.IndexingDate, cycleNumber, s.IntervalUnit);
            var pct = Math.Min((double)endEpisode / s.EpisodeCount * 100, 100);
            var intervalsToFinish = (int)Math.Ceiling((double)(s.EpisodeCount - s.IndexingEpisode) / s.EpisodesPerInterval);
            var completionDate = AddIntervals(s.IndexingDate, intervalsToFinish, s.IntervalUnit);
            return new ShowViewModel(s, endEpisode, endDate, completionDate, pct, endEpisode >= s.EpisodeCount, notStarted);
        })
        .OrderBy(vm => vm.IsCompleted ? 2 : vm.NotStarted ? 1 : 0)
        .ThenBy(vm => vm.Show.ShowName)
        .ToList();
    }

    private static int CompletedIntervals(Show show, DateOnly today)
    {
        if (today <= show.IndexingDate) return 0;

        return show.IntervalUnit switch
        {
            IntervalUnit.Day => today.DayNumber - show.IndexingDate.DayNumber,
            IntervalUnit.Week => (today.DayNumber - show.IndexingDate.DayNumber) / 7,
            IntervalUnit.Month => CompletedMonths(show.IndexingDate, today),
            IntervalUnit.Year => CompletedYears(show.IndexingDate, today),
            _ => 0
        };
    }

    private static int CompletedMonths(DateOnly from, DateOnly to)
    {
        var months = (to.Year - from.Year) * 12 + (to.Month - from.Month);
        if (to.Day < from.Day) months--;
        return Math.Max(0, months);
    }

    private static int CompletedYears(DateOnly from, DateOnly to)
    {
        var years = to.Year - from.Year;
        var anniversary = new DateOnly(to.Year, from.Month, Math.Min(from.Day, DateTime.DaysInMonth(to.Year, from.Month)));
        if (to < anniversary) years--;
        return Math.Max(0, years);
    }

    private static DateOnly AddIntervals(DateOnly date, int n, IntervalUnit unit) => unit switch
    {
        IntervalUnit.Day => date.AddDays(n),
        IntervalUnit.Week => date.AddDays(n * 7),
        IntervalUnit.Month => DateOnly.FromDateTime(date.ToDateTime(TimeOnly.MinValue).AddMonths(n)),
        IntervalUnit.Year => DateOnly.FromDateTime(date.ToDateTime(TimeOnly.MinValue).AddYears(n)),
        _ => date
    };
}
