ConfigEditor拡張機能インストール方法

0. VisualStudioで何か開いていたらすべて落とす。
1. ConfigEditor.vsixを右クリック→プログラムから開く→このPCで別アプリを探す
2. C:\Program Files (x86)\Common Files\Microsoft Shared\MSEnv\VSLauncher.exeを選択
   (一度選択しておくと、次回以降は"Microsoft Visual Studio Version Selector"という名前で選択可能)
3. インストーラが起動するので、指示に従ってインストール
4. VisualStudioを再起動してConfigファイルを開く
5. 拡張機能が働いていれば文字に色がついたりします



無効化・アンインストール方法
何かトラブルが起きたときに実行してください

1. ツール→拡張機能と更新プログラム
2. インストール済み拡張機能一覧にConfigEditorがあるはずなので、"無効化" or "アンインストール"を選択
3. Visual Studioを再起動して完了


更新方法

1. 配布されたConfigEditor.vsixを右クリック→プログラムから開く
2. Microsoft Visual Studio Version Selectorを選択
3. インストーラが起動するので、指示に従ってインストール
4. VisualStudioを再起動してConfigファイルを開く