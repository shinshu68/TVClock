# アプリのアイコン (src/TVClock/Assets/TVClock.ico) を描いて作り直す。
# 色や形を変えたいときは、このスクリプトを書き換えてから、プロジェクトのルートで実行する。
#
#   powershell -ExecutionPolicy Bypass -File tools/New-AppIcon.ps1
#
# -PreviewPath を付けると、暗い背景と明るい背景に各サイズを並べた確認用の画像も書き出す。
param(
    [string]$OutputPath = (Join-Path $PSScriptRoot '..\src\TVClock\Assets\TVClock.ico'),
    [string]$PreviewPath
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Drawing

# アイコンに入れる大きさ。タスクトレイは画面の拡大率に応じて 16〜32、エクスプローラーなどは 48 以上を使う
$sizes = 16, 20, 24, 32, 40, 48, 64, 256

# 暗いタスクバーでも明るいタスクバーでも形が分かるよう、明るい本体を暗い色で縁取る
$bodyColor = [System.Drawing.ColorTranslator]::FromHtml('#F4F4F4')
$darkColor = [System.Drawing.ColorTranslator]::FromHtml('#20242C')

function New-RoundedRectangle([single]$x, [single]$y, [single]$width, [single]$height, [single]$radius) {
    $diameter = $radius * 2
    $path = New-Object System.Drawing.Drawing2D.GraphicsPath
    $path.AddArc($x, $y, $diameter, $diameter, 180, 90)
    $path.AddArc($x + $width - $diameter, $y, $diameter, $diameter, 270, 90)
    $path.AddArc($x + $width - $diameter, $y + $height - $diameter, $diameter, $diameter, 0, 90)
    $path.AddArc($x, $y + $height - $diameter, $diameter, $diameter, 90, 90)
    $path.CloseFigure()
    return $path
}

# 64x64 の方眼で描いたテレビを、指定した大きさに拡大・縮小して描く
function New-TvBitmap([int]$size) {
    $bitmap = New-Object System.Drawing.Bitmap($size, $size, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $graphics.Clear([System.Drawing.Color]::Transparent)
    $graphics.ScaleTransform($size / 64.0, $size / 64.0)

    # 小さいアイコンでは細部がつぶれるので、縁取りとアンテナを太くする
    $small = $size -le 24
    $outline = if ($small) { 4.0 } else { 3.0 }
    $antennaWidth = if ($small) { 6.0 } else { 5.0 }

    $bodyBrush = New-Object System.Drawing.SolidBrush($bodyColor)
    $darkBrush = New-Object System.Drawing.SolidBrush($darkColor)

    # アンテナ (縁取り → 本体の色の順に重ねる)。小さくてもテレビと分かるよう、長く太くする
    foreach ($pen in @(
            (New-Object System.Drawing.Pen($darkColor, ($antennaWidth + $outline * 2))),
            (New-Object System.Drawing.Pen($bodyColor, $antennaWidth)))) {
        $pen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
        $pen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
        $graphics.DrawLine($pen, 32.0, 24.0, 15.0, 7.0)
        $graphics.DrawLine($pen, 32.0, 24.0, 49.0, 7.0)
        $pen.Dispose()
    }

    # 脚。本体の下にはっきり出るよう、幅と高さを大きく取る
    foreach ($x in 11.0, 39.0) {
        $graphics.FillRectangle($darkBrush, ($x - $outline), 44.0, (14.0 + $outline * 2), (15.0 + $outline))
        $graphics.FillRectangle($bodyBrush, $x, 44.0, 14.0, 15.0)
    }

    # 本体
    $graphics.FillPath($darkBrush, (New-RoundedRectangle (4 - $outline) (22 - $outline) (56 + $outline * 2) (27 + $outline * 2) (8 + $outline)))
    $graphics.FillPath($bodyBrush, (New-RoundedRectangle 4 22 56 27 8))

    # 画面
    $graphics.FillPath($darkBrush, (New-RoundedRectangle 9 27 46 17 4))

    $bodyBrush.Dispose()
    $darkBrush.Dispose()
    $graphics.Dispose()
    return $bitmap
}

function ConvertTo-PngBytes([System.Drawing.Bitmap]$bitmap) {
    $stream = New-Object System.IO.MemoryStream
    $bitmap.Save($stream, [System.Drawing.Imaging.ImageFormat]::Png)
    return , $stream.ToArray()
}

# ICO に格納するビットマップ形式 (ヘッダー + 下の行から並べた BGRA の画素 + 透過マスク) に変換する
function ConvertTo-DibBytes([System.Drawing.Bitmap]$bitmap) {
    $size = $bitmap.Width
    $stream = New-Object System.IO.MemoryStream
    $writer = New-Object System.IO.BinaryWriter($stream)

    $writer.Write([uint32]40)           # ヘッダーの大きさ
    $writer.Write([int32]$size)         # 幅
    $writer.Write([int32]($size * 2))   # 高さ (画像と透過マスクの 2 つ分)
    $writer.Write([uint16]1)            # カラープレーン数
    $writer.Write([uint16]32)           # 1 ピクセルのビット数
    foreach ($unused in 1..6) {
        $writer.Write([uint32]0)        # 圧縮なし。残りの項目は使わない
    }

    for ($y = $size - 1; $y -ge 0; $y--) {
        for ($x = 0; $x -lt $size; $x++) {
            $color = $bitmap.GetPixel($x, $y)
            $writer.Write([byte]$color.B)
            $writer.Write([byte]$color.G)
            $writer.Write([byte]$color.R)
            $writer.Write([byte]$color.A)
        }
    }

    # 透過はアルファ値で表すので、透過マスクはすべて 0 にする (1 行は 4 バイト単位に切り上げる)
    $maskRowBytes = [int][Math]::Ceiling($size / 32.0) * 4
    $writer.Write((New-Object byte[] ($maskRowBytes * $size)))

    $writer.Flush()
    return , $stream.ToArray()
}

# 256 だけ PNG で、それより小さい画像はビットマップで格納する (小さい画像の PNG を読めないソフトがあるため)
$images = foreach ($size in $sizes) {
    $bitmap = New-TvBitmap $size
    $data = if ($size -ge 256) { ConvertTo-PngBytes $bitmap } else { ConvertTo-DibBytes $bitmap }
    [pscustomobject]@{ Size = $size; Bitmap = $bitmap; Data = $data }
}

$directory = Split-Path -Parent $OutputPath
New-Item -ItemType Directory -Force $directory | Out-Null

$file = New-Object System.IO.MemoryStream
$writer = New-Object System.IO.BinaryWriter($file)
$writer.Write([uint16]0)              # 予約
$writer.Write([uint16]1)              # 種類：アイコン
$writer.Write([uint16]$images.Count)  # 画像の数

$offset = 6 + 16 * $images.Count
foreach ($image in $images) {
    # 幅と高さは 1 バイトで、256 は 0 と書く
    $dimension = if ($image.Size -ge 256) { 0 } else { $image.Size }
    $writer.Write([byte]$dimension)
    $writer.Write([byte]$dimension)
    $writer.Write([byte]0)                    # パレットの色数 (なし)
    $writer.Write([byte]0)                    # 予約
    $writer.Write([uint16]1)                  # カラープレーン数
    $writer.Write([uint16]32)                 # 1 ピクセルのビット数
    $writer.Write([uint32]$image.Data.Length) # 画像データの大きさ
    $writer.Write([uint32]$offset)            # 画像データの位置
    $offset += $image.Data.Length
}
foreach ($image in $images) {
    $writer.Write($image.Data)
}
$writer.Flush()
[System.IO.File]::WriteAllBytes($OutputPath, $file.ToArray())
Write-Output "アイコンを書き出しました: $OutputPath"

if ($PreviewPath) {
    $gap = 12
    [int]$width = ($sizes | Measure-Object -Sum).Sum + $gap * ($sizes.Count + 1)
    [int]$rowHeight = 256 + $gap * 2
    $preview = New-Object System.Drawing.Bitmap($width, ($rowHeight * 2))
    $graphics = [System.Drawing.Graphics]::FromImage($preview)

    $row = 0
    foreach ($background in '#1F1F1F', '#F3F3F3') {
        $brush = New-Object System.Drawing.SolidBrush([System.Drawing.ColorTranslator]::FromHtml($background))
        $graphics.FillRectangle($brush, 0, ($rowHeight * $row), $width, $rowHeight)
        $brush.Dispose()

        $x = $gap
        foreach ($image in $images) {
            $graphics.DrawImageUnscaled($image.Bitmap, $x, ($rowHeight * $row + $gap))
            $x += $image.Size + $gap
        }
        $row++
    }

    $graphics.Dispose()
    $preview.Save($PreviewPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $preview.Dispose()
    Write-Output "確認用の画像を書き出しました: $PreviewPath"
}

foreach ($image in $images) {
    $image.Bitmap.Dispose()
}
