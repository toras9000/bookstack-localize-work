# BookStack ローカライズ作業環境

このリポジトリは BookStack のローカライズ作業用の環境です。  
Docker コンテナによる BookStack サーバと、サポート用のC# file-based appsファイル(ここではスクリプトと呼ぶ)を含んでいます。  

## ファイル一覧

各ファイルの簡単な説明を以下に記載します。  

- docker/*
    - Dockerコンテナ環境用ファイル群。
- @update-packages.cs1
    - 以下のスクリプトファイルをメンテするためのもの。
- @windows-associate.cs1
    - Windowsで .cs1 ファイルを .NET10 に関連付けるためのスクリプト
- @windows-associate.cmd
    - 上記関連付けスクリプトを最初に実行する時向けの補助バッチファイル
- 00.delete-data.cs1
    - コンテナ停止してボリュームと抽出データを削除するスクリプト。
    - 状態をリセットして最初からやり直すために使用します。
- 10.restart.cs1
    - コンテナを(再)起動してテスト用トークンを作成するスクリプト。
- 11.show-url.cs1
    - BookStackサービスコンテナのURLを表示するスクリプト。
- 20.make-test-entities.cs1
    - BookStack 上にいくつかの確認用エンティティを作成するスクリプト。
    - 手間を省くためだけのものであるため、必ずしも必要なスクリプトではありません。
- 90.recv-webhook.cs1
    - Webhookの受信サーバを実行する。
    - エンドポイントにPOSTされたJSONデータを表示します。
    - BookStack の Webhook 送信に関係する動作の確認用です。

基本的には `00.delete-data.cs1` と `10.restart.cs1` を実行すれば、
それ以前のデータがリセットされて新たにローカライズの作業準備が整う、という想定です。  

## C#スクリプト

### 実行準備

C# file-based appsを実行するために以下をインストールする必要があります。  
- .NET 10.0 SDK

.NET SDKは以下でダウンロード可能です。  
C#スクリプトは実行時にコンパイルが行われるため、RuntimeではなくSDKが必要であることに注意してください。  
- https://dotnet.microsoft.com/download

### スクリプト実行

実行環境の準備が整ったら、確実に実行するためには以下のようなコマンドラインで実行します。  
```
dotnet run --file ./10.restart.cs1
```

ファイルが file-based apps であると認識される条件を満たすようにしているはずなので、以下のいずれかでも実行できるかと思います。  
さらに、Linux環境下では shebang により dotnet にパスさえ通っていれば直接実行も可能なはずです。  
```
dotnet run ./10.restart.cs1
dotnet ./10.restart.cs1
```

もし環境が Windows であれば、同梱の関連付けスクリプト `@windows-associate.cs1` を利用することもできます。  
システムに .NET10 をインストールし、補助バッチファイル `@windows-associate.cmd` を実行すると、@windows-associate.cs1 を呼び出して関連付け登録を行います。
