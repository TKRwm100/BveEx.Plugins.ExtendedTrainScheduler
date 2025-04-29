# BveEx.Plugins.ExtendedTrainScheduler
【BVE/BveEX】公式の他列車走行スケジュール拡張プラグインを改造し停車時刻から減速度を自動計算できるようにしたプラグインです

追加した構文についての解説です.
他列車走行スケジュール拡張プラグイン-各構文の解説の項を参考に執筆しました.

以下、「～.XXX」は全て「BveEx.User.Toukaitetudou.ExtendedTrainScheduler.XXX」を表すものとします。

`～.Train[trainKey].Accelerate.ToHereAt(t, a);`

目標速度 v [km/h] の加減速を、時刻 t にその距離程 “まで” で完了するように行います。

`～.Train[trainKey].StopAtUntil( t1, t2, a, v);`

減速し、時刻 t1 にその距離程に停車します。時刻 t2 になると発車し、加速度 a [km/h/s] で速度 v [km/h] まで加速します。
時刻 t1, t2 の指定方法はBVE標準のTrain.Enable構文と同様です。時刻を表す文字列 'hh:mm:ss' または 00:00:00 からの経過時間 [s] で指定してください。

`～.Train[trainKey].StopAt( t, st, a, v0);`

減速し、時刻 t1 にその距離程に停車します。st [s] 経過すると発車し、加速度 a [km/h/s] で速度 v [km/h] まで加速します。
時刻 t の指定方法はBVE標準のTrain.Enable構文と同様です。時刻を表す文字列 'hh:mm:ss' または 00:00:00 からの経過時間 [s] で指定してください。

従来の他列車走行スケジュール拡張プラグインについては [BveEX 公式ホームページ](https://bveex.okaoka-depot.com/plugins/extended-train-scheduler) 等をご覧ください。

## ライセンス

[PolyForm Noncommercial License 1.0.0](LICENSE.md)

## 改造元プラグイン

[他列車走行スケジュール拡張プラグイン](https://github.com/automatic9045/BveEx.Plugins.ExtendedTrainScheduler)

Copyright © 2024 automatic9045

## 使用ライブラリ等
### [BveEx.CoreExtensions](https://github.com/automatic9045/BveEX) (PolyForm NonCommercial 1.0.0)
Copyright (c) 2022 automatic9045

### [BveEx.PluginHost](https://github.com/automatic9045/BveEX) (PolyForm NonCommercial 1.0.0)
Copyright (c) 2022 automatic9045

### [Harmony](https://github.com/pardeike/Harmony) (MIT)
Copyright (c) 2017 Andreas Pardeike

### [ObjectiveHarmonyPatch](https://github.com/automatic9045/ObjectiveHarmonyPatch) (MIT)
Copyright (c) 2022 automatic9045

### [SlimDX](https://www.nuget.org/packages/SlimDX/) (MIT)
Copyright (c) 2013  exDreamDuck
