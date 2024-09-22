using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;

public class PlayerRankApi : IPlayerRankApi
{
    private static LevelsRanksModuleFakeRank.LevelsRanksModuleFakeRank _core;
    private readonly Dictionary<ulong, (int originalRank, int originalRankType)> _originalRanks = new();

    public PlayerRankApi(LevelsRanksModuleFakeRank.LevelsRanksModuleFakeRank core)
    {
        _core = core;
    }

    public void DisableRank(CCSPlayerController player)
    {
        var steamId = player.SteamID;
        if (!_originalRanks.ContainsKey(steamId))
        {
            _originalRanks[steamId] = (player.CompetitiveRanking, player.CompetitiveRankType);
        }

        player.CompetitiveRanking = 0;
        player.CompetitiveRankType = 0;
    }

    public void SetCustomRank(CCSPlayerController player, int rank, int rankType)
    {
        var steamId = player.SteamID;
        if (!_originalRanks.ContainsKey(steamId))
        {
            _originalRanks[steamId] = (player.CompetitiveRanking, player.CompetitiveRankType);
        }

        player.CompetitiveRanking = rank;
        player.CompetitiveRankType = (sbyte)rankType;

        SavePlayerRankToFile(steamId, rank, rankType); 
    }

    public void ResetRank(CCSPlayerController player)
    {
        var steamId = player.SteamID;
        if (_originalRanks.TryGetValue(steamId, out var originalRank))
        {
            player.CompetitiveRanking = originalRank.originalRank;
            player.CompetitiveRankType = (sbyte)originalRank.originalRankType;

            _originalRanks.Remove(steamId); 
            DeletePlayerRankFile(steamId); 
        }
    }

    private void SavePlayerRankToFile(ulong steamId, int rank, int rankType)
    {
        var filePath = GetPlayerRankFilePath(steamId);
        var rankData = new PlayerRankData { Rank = rank, RankType = rankType };
        var json = JsonSerializer.Serialize(rankData);
        File.WriteAllText(filePath, json);
    }

    public static PlayerRankData? LoadPlayerRankFromFile(ulong steamId)
    {
        var filePath = GetPlayerRankFilePath(steamId);
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            var rankData = JsonSerializer.Deserialize<PlayerRankData>(json);
            return rankData;
        }

        return null;
    }

    private void DeletePlayerRankFile(ulong steamId)
    {
        var filePath = GetPlayerRankFilePath(steamId);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static string GetPlayerRankFilePath(ulong steamId)
    {
        var dataDirectory = Path.Combine(_core.ModuleDirectory, "PlayerData");
        if (!Directory.Exists(dataDirectory))
        {
            Directory.CreateDirectory(dataDirectory);
        }

        return Path.Combine(dataDirectory, $"{steamId}_rank.json");
    }
}

public class PlayerRankData
{
    public int Rank { get; set; }
    public int RankType { get; set; }
}
