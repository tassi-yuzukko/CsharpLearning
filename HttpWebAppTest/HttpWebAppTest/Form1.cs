using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HttpWebAppTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private volatile HttpListener server;
		private Thread serverThread;

		/// <summary>
		/// サーバー用スレッドが実行するメソッド
		/// </summary>
		private void ServerLoop()
		{
			server.Prefixes.Add("http://+:8192/lesson/");
			server.Start();

			for (;;)
			{
				try
				{
					// GetContextメソッドはクライアントからのリクエストを受けtけるまで待ち状態となる
					var context = server.GetContext();

					// クライアントのリクエストをスレッドプールのスレッドに処理させる
					ThreadPool.QueueUserWorkItem((o) =>
					{
						var listner = o as HttpListenerContext;
						if (listner.Request.Url.AbsolutePath == "/lesson/test/OK")
						{
							listner.Response.StatusCode = 200;  // 200はHTTPのOKステータス
							listner.Response.ContentType = "text/plain";    // クライアントへテキストを返すことを指定する

							using (var writer = new StreamWriter(listner.Response.OutputStream))
							{
								writer.Write("OK");
								writer.Flush();
							}
						}
						else
						{
							listner.Response.StatusCode = 404;	// 404はHTTPのNotFound
						}
						// Closeでクライアントにレスポンスを返す
						listner.Response.Close();
					}, context);
				}
				catch(Exception e)
				{
					// プログラムの終了のためにHttpListnerが破棄された場合は、ObjectDisposedExceptionが通知されるので、処理を抜ける
					if (e is ObjectDisposedException)
					{
						break;
					}
					else
					{
						// それ以外の例外は無視して次のクライアントリクエスト受信処理を行う
					}
				}
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			// HttpListenerオブジェクトを生成する
			// serverはvolatile修飾がついているので、この時点で確実に設定される
			server = new HttpListener();

			// サーバ用スレッド作成、実行
			serverThread = new Thread(ServerLoop);
			serverThread.Start();
		}

		private void Form1_FormClosed(object sender, FormClosedEventArgs e)
		{
			// リクエストの受付を中止する
			server.Stop();

			// オブジェクトの破棄
			// →GetContext呼び出しがObjectDisposedExceptionを通知し、サーバ用スレッドの無限ループが終了する
			server.Close();
		}
	}
}
