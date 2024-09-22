using CounterStrikeSharp.API.Core;

public interface IPlayerRankApi
{
    void DisableRank(CCSPlayerController player); 
    void SetCustomRank(CCSPlayerController player, int rank, int rankType);
    void ResetRank(CCSPlayerController player);
}