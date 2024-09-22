using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.UserMessages;
using CounterStrikeSharp.API.Modules.Utils;
using LevelsRanksApi;
using Microsoft.Extensions.Logging;

namespace LevelsRanksModuleFakeRank
{
    [MinimumApiVersion(80)]
    public class LevelsRanksModuleFakeRank : BasePlugin
    {
        public override string ModuleName => "[LR] Module - FakeRank";
        public override string ModuleVersion => "1.0.3";
        public override string ModuleAuthor => "ABKAM designed by RoadSide Romeo & Wend4r";

        private Dictionary<int, (int competitiveRanking, int competitiveRankType)>? _ranksConfig;
        private readonly Dictionary<string, (int competitiveRanking, int competitiveRankType)> _playerRanks = new();
        private ILevelsRanksApi? _api;
        private readonly PluginCapability<ILevelsRanksApi> _apiCapability = new("levels_ranks");
        private IPlayerRankApi? _playerRankApi;
        private readonly PluginCapability<IPlayerRankApi> _playerRankApiCapability = new("PLAYER_RANK_API");
        private Dictionary<string, int> _lastKnownLevels = new();
        private ConcurrentDictionary<string, (int competitiveRanking, int competitiveRankType)> _rankCache = new();
        private ConcurrentDictionary<string, DateTime> _cacheTimestamps = new();

        private readonly ConcurrentDictionary<string, bool>
            _isCustomRankActive = new();

        private const float UpdateInterval = 1.0f;

        public override void Load(bool hotReload)
        {
            _playerRankApi = new PlayerRankApi(this);
            Capabilities.RegisterPluginCapability(_playerRankApiCapability, () => _playerRankApi);
        }

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
            AddTimer(UpdateInterval, async () => { await FetchPlayerRanks(); }, TimerFlags.REPEAT);
        }

        private async Task FetchPlayerRanks()
        {
            var players = Utilities.GetPlayers()
                .Where(player => !player.IsBot && player.TeamNum != (int)CsTeam.Spectator);
            var steamIds = players.Select(player => _api!.ConvertToSteamId(player.SteamID)).ToList();

            var playersToFetch = steamIds.Where(steamId =>
                    !_rankCache.TryGetValue(steamId, out var cachedRank) ||
                    !_cacheTimestamps.TryGetValue(steamId, out var cacheTime) ||
                    (DateTime.UtcNow - cacheTime).TotalSeconds >= UpdateInterval)
                .ToList();

            if (playersToFetch.Count == 0)
            {
                return;
            }

            foreach (var steamId in playersToFetch)
            {
                if (_api!.OnlineUsers.TryGetValue(steamId, out var onlineUser))
                {
                    var currentLevelId = onlineUser.Rank;

                    if (!_isCustomRankActive.TryGetValue(steamId, out var isCustomActive) || !isCustomActive)
                    {
                        if (!_lastKnownLevels.TryGetValue(steamId, out var lastLevel) || currentLevelId != lastLevel)
                        {
                            if (_ranksConfig != null && _ranksConfig.TryGetValue(currentLevelId, out var rankInfo))
                            {
                                _playerRanks[steamId] = rankInfo;
                                _lastKnownLevels[steamId] = currentLevelId;
                                _rankCache[steamId] = rankInfo;
                                _cacheTimestamps[steamId] = DateTime.UtcNow;
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogWarning($"Player {steamId} is not online. Skipping rank update.");
                }
            }
        }

        private void OnTick()
        {
            var players = Utilities.GetPlayers()
                .Where(player => !player.IsBot && player.TeamNum != (int)CsTeam.Spectator);

            var filter = new RecipientFilter();
            
            foreach (var player in players)
            {
                var steamId64 = player.SteamID;
                var steamId = _api!.ConvertToSteamId(steamId64);

                var customRank = PlayerRankApi.LoadPlayerRankFromFile(steamId64);

                if (customRank != null)
                {
                    if (player.CompetitiveRankType != (sbyte)customRank.RankType ||
                        player.CompetitiveRanking != customRank.Rank)
                    {
                        player.CompetitiveRankType = (sbyte)customRank.RankType;
                        player.CompetitiveRanking = customRank.Rank;
                        player.CompetitiveWins = 777;
                        filter.Add(player);
                    }
                }
                else
                {
                    if (_playerRanks.TryGetValue(steamId, out var rankInfo))
                    {
                        if (player.CompetitiveRankType != (sbyte)rankInfo.competitiveRankType ||
                            player.CompetitiveRanking != rankInfo.competitiveRanking)
                        {
                            player.CompetitiveRankType = (sbyte)rankInfo.competitiveRankType;
                            player.CompetitiveRanking = rankInfo.competitiveRanking;
                            player.CompetitiveWins = 777;
                            filter.Add(player);
                        }
                    }
                }
            }

            if (filter.Count > 0)
            {
                var msg = UserMessage.FromId(350);
                msg.Send(filter);
            }
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
            {
                if (fakeRanksObject is JsonElement fakeRanksElement)
                {
                    int rankType;
                    if (fakeRankSection.TryGetValue("Type", out var typeValue) &&
                        typeValue is JsonElement typeElement &&
                        typeElement.GetString() is string typeString && int.TryParse(typeString, out var type))
                    {
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
                    }
                    else
                    {
                        rankType = 12;
                    }

                    foreach (var rank in fakeRanksElement.EnumerateObject())
                    {
                        if (int.TryParse(rank.Name, out var level) &&
                            rank.Value.GetString() is string competitiveRankingString &&
                            int.TryParse(competitiveRankingString, out var competitiveRanking))
                        {
                            ranks[level] = (competitiveRanking, rankType);
                        }
                    }
                }
            }

            return ranks;
        }
    }
}
