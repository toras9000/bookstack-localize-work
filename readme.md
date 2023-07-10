# BookStack ローカライズ作業環境

このリポジトリは BookStack のローカライズ作業用の環境です。  
Docker コンテナによる BookStack サーバと、サポート用のC#スクリプトファイルを含んでいます。  

## ファイル一覧

各ファイルの簡単な説明を以下に記載します。  
基本的にはスクリプトを実行すればローカライズの作業準備が整う想定です。  

- docker/*
    - Dockerコンテナ環境用ファイル群。
    - 構成については後述します。
- 10.restart.csx
    - BookStack を実行するコンテナの起動または再起動を行うスクリプト。
    - コンテナが起動してアクセス可能になったら、ブラウザが開くはずです。
- 15.reset-restart.csx
    - マウントボリュームデータを削除してコンテナの起動または再起動を行うスクリプト。
    - 同ディレクトリ内の `volumes` ディレクトリが削除されます。
- 20.make-test-entities.csx
    - BookStack 上にいくつかの確認用エンティティを作成するスクリプト。
    - 手間を省くためだけのものであるため、必ずしも必要なスクリプトではありません。
- 90.recv-webhook.csx
    - Webhookの受信サーバを実行する。
    - エンドポイントにPOSTされたJSONデータを表示します。
    - BookStack の Webhook 送信に関係する動作の確認用です。
- 91.recv-mail.csx
    - メールの受信サーバを実行する。
    - 受信したメールをファイルのダンプします。
    - BookStack からのメールに関係する動作の確認用です。

## C#スクリプト

### 準備

C#スクリプトを実行するために以下をインストールする必要があります。  
- .NET 7.0 SDK
- dotnet-script v1.4.0 以降

.NET SDKは以下でダウンロード可能です。  
C#スクリプトは実行時にコンパイルが行われるため、RuntimeではなくSDKが必要であることに注意してください。  
- https://dotnet.microsoft.com/download

dotnet-script は .NET SDK のセットアップ後に以下を実行することでインストール可能です。  
```
dotnet tool install -g dotnet-script
```

### 実行
実行環境の準備が整ったら、以下のようにスクリプトを実行できます。  
初回実行ではスクリプトがコンパイルされるため、実行開始まである程度時間がかかります。  
```
dotnet script ./10.restart.csx
```

もし環境が Windows であれば、以下のコマンドで関連付けハンドラを登録できます。  
登録された `.NET Host` をスクリプトファイルに関連付けすることでダブルクリックなどでスクリプト実行できます。  
```
dotnet script register
```

## コンテナ環境

同梱の docker フォルダ配下のdocker用ファイルは以下のような利用方法を想定したものです。  
スクリプト `10.restart.csx` では必要に応じてこれを順に実行して環境を準備しています。  

1. `docker-compose.init.yml` でコンテナからローカライズ用のファイルを取り出す。
    - `volumes` フォルダ配下に取り出したファイルがコピーされる。
1. `docker-compose.yml` で取り出したファイルをマウントして BookStack を実行する。
    - これにより、`volumes` 配下のファイルを編集するだけローカライズテキストの表示を確認することができる。

起動するコンテナには起動時に実行されるスクリプトと論理テーマによるカスタマイズを加えています。  
- docker/assets/init/custom-setup.sh
    - `docker-compose.yml` で起動時に実行されるようにマウントしています。
    - 実行すると以下を行います。
        - 論理テーマを BookStack で利用可能な場所へコピー
        - カスタム環境変数の値を BookStack の .env に反映
            - `docker-compose.yml` に記述している `CUSTOM_*` の環境変数のことです。
        - 論理テーマを利用してテスト用のAPIトークンを生成
            - このトークンは `20.make-test-entities.csx` で利用しています。
- docker/assets/my-theme/functions.php
    - テスト用APIトークン生成用のカスタムコマンドを定義するもの。

docker-compose.yml の extra_hosts の指定により、コンテナ内では `localize-host-gateway` の名前解決先がホストマシンのアドレスとなるようにしています。  
環境変数の指定とカスタムスクリプトでメールサーバ設定が行われるため、`91.recv-mail.csx` を起動しておけばそのままメールを受信・ダンプできるはずです。  
また、`90.recv-webhook.csx` を起動して最初に表示されるエンドポイントアドレスをBookStackのWebhook設定画面で指定することにより、Wehbookで送信されるJSONを確認できるはずです。  

