![GitHub Repo stars](https://img.shields.io/github/stars/ABKAM2023/CS2-LR-FakeRank?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/ABKAM2023/CS2-LR-FakeRank/total?style=for-the-badge)

# EN
**This plugin may result in a token ban.**

[C#] [LR] Module FakeRank - is a module for the Levels Ranks plugin. It displays Core ranks in the kill table (TAB). This plugin allows the use of any types of ranks, including custom ones created by users.

# Installation
1. Install [C# Levels Ranks Core](https://github.com/ABKAM2023/CS2-LevelsRanks-Core/tree/v1.0) and [FakeRanks-RevealAll](https://github.com/Cruze03/FakeRanks-RevealAll)
2. Download [C#] [LR] Module - FakeRank
3. Unpack the archive and upload it to your game server
4. Configure settings_fakerank.json
5. Restart the server

# How to install custom ranks?​
1. In the config (addons/counterstrikesharp/configs/plugins/LevelsRanks/settings_fakerank.json), set the value "1" for the "Type" parameter;​
2. Specify the index of the "rank in TAB" in the module config - skillgroup(index).svg.​
3. Enter the command sm_lvl_reload in the server console.

# Configuration file (settings_fakerank.json)
Each ID in the config is the ID from settings_ranks.json.
```
{
  "LR_FakeRank": {
    // 1 - regular ranks from Match Making (1 - 18) or custom (19, 20, ...).
    // 2 - from Wingman (1 - 18)
    // 3 - from Premier mode (1-99999)
    "Type": "1",
    // Rank in LR Rank in TAB.
    "FakeRank": {
      "1": "1",
      "2": "2",
      "3": "3",
      "4": "4",
      "5": "5",
      "6": "6",
      "7": "7",
      "8": "8",
      "9": "9",
      "10": "10",
      "11": "11",
      "12": "12",
      "13": "13",
      "14": "14",
      "15": "15",
      "16": "16",
      "17": "17",
      "18": "18"
    }
  }
}
```

# RU
**За данный плагин, возможен бан токена.**

[C#] [LR] Модуль FakeRank - это модуль для плагина Levels Ranks. Он показывает ранги Ядра в таблице убийств (ТАБ). В этом плагине можно использовать любые типы званий, включая собственные, созданные пользователями.

# Установка
1. Установите [C# Levels Ranks Core](https://github.com/ABKAM2023/CS2-LevelsRanks-Core/tree/v1.0) и [FakeRanks-RevealAll](https://github.com/Cruze03/FakeRanks-RevealAll)
2. Скачайте [C#] [LR] Module - FakeRank
3. Распакуйте архив и загрузите его на игровой сервер
4. Настройте settings_fakerank.json
5. Перезапустите сервер

# Как установить кастомные звания?​
1. В конфиге (addons/counterstrikesharp/configs/plugins/LevelsRanks/settings_fakerank.json) установите значение "1" у параметра "Type"; ​
2. Укажите в конфиге модуля у "звания в ТАБ(е)" его индекс - skillgroup(индекс).svg. ​
3. Пропишите в консоль сервера sm_lvl_reload.

# Конфигурационный файл (settings_fakerank.json)
Каждый ID в конфиге — это ID из settings_ranks.json.
```
{
  "LR_FakeRank": {
    // 1 - обычные ранги из Match Making (1 - 18) или кастомные (19, 20, ...).
    // 2 - из Напарников (1 - 18)
    // 3 - из Премьер-режима (1-99999)
    "Type": "1",
    // Ранг в LR Ранг в TAB(е).
    "FakeRank": {
      "1": "1",
      "2": "2",
      "3": "3",
      "4": "4",
      "5": "5",
      "6": "6",
      "7": "7",
      "8": "8",
      "9": "9",
      "10": "10",
      "11": "11",
      "12": "12",
      "13": "13",
      "14": "14",
      "15": "15",
      "16": "16",
      "17": "17",
      "18": "18"
    }
  }
}
```
