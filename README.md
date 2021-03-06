# MyBookDatabase
このソフトウェアは日本の国立国会図書館サーチAPIを使用しています。
本のISBNを入力するとその本の情報を表示し、場合によっては保存します。
現在はプライマリで保存するだけで表示はISBNのリスト表示だけですが、のちのアップデートでソートや検索機能などをつける予定です。

# 使い方
## 初めて起動する場合
使い始めはデータ保存用のファイルが作成されていないため、それを最初に作成します。

1. ダブルクリックして起動します。
2. コンソールの表示が出たら何も入力せずに`Enter`を押します。
3. `y` キーを入力してEnter、または付属の1次元コードのOKを読んでください。
4. 好きな名前を入力します。`database` など
5. すると、ソフトウェアの同ディレクトリにその名前のファイルが作成されます。
6. これで準備は完了です、`exit` もしくは1次元コードで終了してください。

※作成されたファイルはテキストですが、編集しないでください。プログラムで読み込めなくなる可能性があります。

## 2回目以降の起動方法の作成

起動のたびにファイル名やパスを入力するのは面倒なので、ショートカットで指定して起動します。

1. exeファイルを右クリックして 「送る」からショートカットを作成してください。
2. 作成されたショートカットを右クリックしてプロパティを表示座せます。
3. ショートカットタブのリンク先の項目に半角スペースを空けて次の文字列を追記します。
4. 使い始めに入力したファイルが同ディレクトリ内にそのままあるのであればその名前、場所を移動したならばそのファイルへのパスを入力します。
5. これで準備完了です。ショートカットは移動できますが、ソフトウェアとデータファイルの場所は移動しないでください。

# バグが発生した場合。
このリポジトリのIssuesタブから問題を報告してください。
