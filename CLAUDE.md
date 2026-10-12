# TVClock

Windows のタスクバーの時計が小さく見づらいため、常に最前面に大きく時刻（時:分）を表示する WPF 製の常駐時計アプリ。

開発方針・スコープ・設計の詳細は docs/plan.md を参照する。

## 技術スタック
- C# / WPF（.NET 10、`net10.0-windows`）
- 対象 OS：Windows 10 / 11
- ビルド・実行は Windows 側で行う（WPF は WSL では動かない）

## ディレクトリ構成
- `TVClock.slnx`：ソリューションファイル
- `src/TVClock/`：WPF プロジェクト（アプリ本体）
- `tests/`：単体テストのプロジェクト（xUnit）
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

実行：

```powershell
dotnet run --project src/TVClock
```

配布用の exe を作成し、常用する場所（`%LOCALAPPDATA%\Programs\TVClock\TVClock.exe`）に置く。時計が起動中だと exe を上書きできないので、先に終了しておく：

```powershell
dotnet publish src/TVClock -c Release -r win-x64 -o "$env:LOCALAPPDATA\Programs\TVClock"
```

配布用の exe は .NET ランタイムを同梱しない。動かす PC には .NET 10 デスクトップ ランタイム（x64）が必要。

## プロジェクト固有のルール
- 表示は「時:分」のみ。秒・日付・曜日は追加しない（追加する場合は先に docs/plan.md のスコープを更新する）
- 設定（位置・文字サイズ・透明度など）はユーザーごとの領域に保存し、リポジトリ内や exe と同じフォルダには書き込まない
- Win32 API（クリックスルー用の拡張ウィンドウスタイルなど）を呼ぶ処理は 1 か所のクラスにまとめ、画面のコードから直接 P/Invoke しない
- NuGet パッケージの追加は、グローバルのルールどおり理由を添えて確認してから行う
