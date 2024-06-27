using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Utils;
using LevelsRanksApi;
using System.Text.Json;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using Microsoft.Extensions.Logging;

namespace LevelsRanksModuleFakeRank;

[MinimumApiVersion(80)]
public class LevelsRanksModuleFakeRank : BasePlugin
{
    public override string ModuleName => "[LR] Module - FakeRank";
    public override string ModuleVersion => "1.0";
    public override string ModuleAuthor => "ABKAM designed by RoadSide Romeo & Wend4r";

    private Dictionary<int, (int competitiveRanking, int competitiveRankType)>? _ranksConfig;
    private readonly Dictionary<string, (int competitiveRanking, int competitiveRankType)> _playerRanks = new();
    private ILevelsRanksApi? _api;
    private readonly PluginCapability<ILevelsRanksApi> _apiCapability = new("levels_ranks");

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        base.OnAllPluginsLoaded(hotReload);

        _api = _apiCapability.Get();
        if (_api == null)
        {
            Server.PrintToConsole("Levels Ranks API is currently unavailable.");
            return;
        }

        CreateRanksConfig();
        _ranksConfig = LoadRanksConfig();

        RegisterListener<Listeners.OnTick>(OnTick);
        AddTimer(2.0f, () =>
        {
            var _ = FetchPlayerRanks();
        }, TimerFlags.REPEAT);
    }

    private void CreateRanksConfig()
    {
        var configDirectory = Path.Combine(Application.RootDirectory, "configs/plugins/LevelsRanks");
        var filePath = Path.Combine(configDirectory, "settings_fakerank.json");

        if (!File.Exists(filePath))
        {
            var defaultConfig = new
            {
                LR_FakeRank = new
                {
                    Type = "1",
                    FakeRank = new Dictionary<string, string>
                    {
                        { "1", "1" },
                        { "2", "2" },
                        { "3", "3" },
                        { "4", "4" },
                        { "5", "5" },
                        { "6", "6" },
                        { "7", "7" },
                        { "8", "8" },
                        { "9", "9" },
                        { "10", "10" },
                        { "11", "11" },
                        { "12", "12" },
                        { "13", "13" },
                        { "14", "14" },
                        { "15", "15" },
                        { "16", "16" },
                        { "17", "17" },
                        { "18", "18" }
                    }
                }
            };

            Directory.CreateDirectory(configDirectory);
            var json = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }
    }

    private Dictionary<int, (int competitiveRanking, int competitiveRankType)> LoadRanksConfig()
    {
        var configDirectory = Path.Combine(Application.RootDirectory, "configs/plugins/LevelsRanks");
        var filePath = Path.Combine(configDirectory, "settings_fakerank.json");

        var json = File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json);

        var ranks = new Dictionary<int, (int competitiveRanking, int competitiveRankType)>();

        if (config != null && config.TryGetValue("LR_FakeRank", out var fakeRankSection) &&
            fakeRankSection.TryGetValue("FakeRank", out var fakeRanksObject))
            if (fakeRanksObject is JsonElement fakeRanksElement)
            {
                int rankType;
                if (fakeRankSection.TryGetValue("Type", out var typeValue) && typeValue is JsonElement typeElement &&
                    typeElement.GetString() is string typeString && int.TryParse(typeString, out var type))
                    switch (type)
                    {
                        case 1:
                            rankType = 12;
                            break;
                        case 2:
                            rankType = 7;
                            break;
                        case 3:
                            rankType = 11;
                            break;
                        default:
                            rankType = 12;
                            break;
                    }
                else
                    rankType = 12;

                foreach (var rank in fakeRanksElement.EnumerateObject())
                    if (int.TryParse(rank.Name, out var level) &&
                        rank.Value.GetString() is string competitiveRankingString &&
                        int.TryParse(competitiveRankingString, out var competitiveRanking))
                        ranks[level] = (competitiveRanking, rankType);
            }

        return ranks;
    }

    private async Task FetchPlayerRanks()
    {
        var players = Utilities.GetPlayers().Where(player => !player.IsBot && player.TeamNum != (int)CsTeam.Spectator);

        var maxConcurrentTasks = 10;
        var semaphore = new SemaphoreSlim(maxConcurrentTasks);

        var tasks = players.Select(async player =>
        {
            await semaphore.WaitAsync();
            try
            {
                await UpdatePlayerRank(player);
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
    }

    private Dictionary<string, int> _lastKnownLevels = new();

    private async Task UpdatePlayerRank(CCSPlayerController player)
    {
        var steamId64 = player.SteamID;
        var steamId = _api!.ConvertToSteamId(steamId64);
        var currentRanks = await _api.GetCurrentRanksAsync();

        if (currentRanks.TryGetValue(steamId, out var currentLevelId))
        {
            if (!_lastKnownLevels.TryGetValue(steamId, out var lastLevel) || currentLevelId != lastLevel)
                if (_ranksConfig != null && _ranksConfig.TryGetValue(currentLevelId, out var rankInfo))
                {
                    _playerRanks[steamId] = rankInfo;
                    _lastKnownLevels[steamId] = currentLevelId;
                }
        }
        else
        {
            Logger.LogWarning($"No rank found for player {steamId} (SteamID64: {steamId64})");
        }
    }

    private void OnTick()
    {
        var players = Utilities.GetPlayers().Where(player => !player.IsBot && player.TeamNum != (int)CsTeam.Spectator);

        foreach (var player in players)
        {
            var steamId64 = player.SteamID;
            var steamId = _api!.ConvertToSteamId(steamId64);

            if (_playerRanks.TryGetValue(steamId, out var rankInfo))
                if (player.CompetitiveRankType != (sbyte)rankInfo.competitiveRankType ||
                    player.CompetitiveRanking != rankInfo.competitiveRanking)
                {
                    player.CompetitiveRankType = (sbyte)rankInfo.competitiveRankType;
                    player.CompetitiveRanking = rankInfo.competitiveRanking;
                    player.CompetitiveWins = 777;
                }
        }
    }
    [ConsoleCommand("css_lvl_reload", "Reloads the configuration files")]
    public void ReloadConfigsCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            try
            {
                LoadRanksConfig();
            }
            catch (Exception ex)
            {
                Logger.LogInformation($"Error reloading configuration: {ex.Message}");
            }
        }
    }
}