using CKaraganis.ShowTracker.Enums;

namespace CKaraganis.ShowTracker.Models;

public record Show(
    int Id, 
    string ShowName, 
    int EpisodeCount, 
    int IndexingEpisode, 
    DateOnly IndexingDate, 
    int EpisodesPerInterval, 
    IntervalUnit IntervalUnit);