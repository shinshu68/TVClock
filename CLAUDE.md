# TVClock

Windows のタスクバーの時計が小さく見づらいため、常に最前面に大きく時刻 (時:分) を表示する WPF 製の常駐時計アプリ。

開発方針・スコープ・設計の詳細は docs/plan.md を参照する。

## 技術スタック
- C# / WPF (.NET 10、`net10.0-windows`)
- 対象 OS：Windows 10 / 11
- ビルド・実行は Windows 側で行う (WPF は WSL では動かない)

## ディレクトリ構成
- `TVClock.slnx`：ソリューションファイル
- `src/TVClock/`：WPF プロジェクト (アプリ本体)
- `tests/`：単体テストのプロジェクト (xUnit)
- `tools/`：開発用のスクリプト (アイコンの画像を作り直す `New-AppIcon.ps1` など)
- `docs/`：開発方針などのドキュメント

## コマンド
Windows の PowerShell で、プロジェクトのルートから実行する。

ビルド：

```powershell
dotnet build
```

テスト：

```powershell
dotnet test
```

整形 (コミットの前、`git add` の前に必ずかける)：

```powershell
dotnet format
```

実行：

```powershell
dotnet run --project src/TVClock
```

配布用の exe を作成し、常用する場所 (`%LOCALAPPDATA%\Programs\TVClock\TVClock.exe`) に置く。時計が起動中だと exe を上書きできないので、先に終了しておく：

```powershell
dotnet publish src/TVClock -c Release -r win-x64 -o "$env:LOCALAPPDATA\Programs\TVClock"
```

配布用の exe は .NET ランタイムを同梱しない。動かす PC には .NET 10 デスクトップ ランタイム (x64) が必要。

## 動作確認
- このマシンの Windows で、ビルドした開発用の exe (`src\TVClock\bin\Debug\net10.0-windows\TVClock.exe`) を起動して確かめる
- 常用の時計 (`%LOCALAPPDATA%\Programs\TVClock\TVClock.exe`) が動いているときは、確認の前に Claude が終了し、確認が終わったら起動し直す。同時に動かすと、時計とトレイアイコンが 2 つずつ出て、設定ファイルを互いに上書きする (多重起動を防ぐ版では、開発用の exe が起動しない)
- 開発用の exe と常用の時計は、同じ設定ファイル (`%APPDATA%\TVClock\settings.json`) と自動起動の登録を使う。確認中に変えた位置や設定は、常用の時計にもそのまま残る
- Claude が確かめるのは、プロセスの起動・終了とスクリーンショットで見える範囲。ドラッグやメニューの操作など、それでは確かめられない部分だけをユーザーに頼む

## 値をそろえる名前
片方だけ変えても、ビルドエラーにならずに動かなくなる。
- `src/TVClock/TVClock.csproj` の `EmbeddedResource` の `LogicalName` ↔ `src/TVClock/TrayIcon.cs` の `IconResourceName`：ずれると、トレイアイコンを読み込めない
- `src/TVClock/AppSettings.cs` のプロパティ名 ↔ 以前の版が保存した `settings.json` のキー：変えると、保存済みの設定を読まなくなる
- `src/TVClock/AutoStart.cs` の `DefaultValueName` ↔ 以前の版がレジストリに登録した値の名前：変えると、古い登録が残り、メニューのチェックとずれる
- `src/TVClock/SingleInstance.cs` の `DefaultName` ↔ 以前の版が使っている Mutex の名前：変えると、以前の版と新しい版を同時に起動できてしまう

## プロジェクト固有のルール
- 表示は「時:分」のみ。秒・日付・曜日は追加しない (追加する場合は先に docs/plan.md のスコープを更新する)
- 設定 (位置・文字サイズ・透明度など) はユーザーごとの領域に保存し、リポジトリ内や exe と同じフォルダには書き込まない
- Win32 API (クリックスルー用の拡張ウィンドウスタイルなど) を呼ぶ処理は 1 か所のクラスにまとめ、画面のコードから直接 P/Invoke しない
- NuGet パッケージの追加は、グローバルのルールどおり理由を添えて確認してから行う
