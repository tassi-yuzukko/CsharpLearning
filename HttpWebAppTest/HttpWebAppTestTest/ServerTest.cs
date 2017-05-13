using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HttpWebAppTest;
using System.Net;
using System.IO;
using System.Threading;

namespace HttpWebAppTestTest
{
	[TestClass]
	public class ServerTest
	{
		private void RunServer(Action<WebClient> test)
		{
			// スレッド間で同期をとるためのオブジェクト
			var key = new object();

			using(var form1 = new Form1())
			{
				// ワーカスレッドが処理を開始しないようにロックする
				lock (key)
				{
					ThreadPool.QueueUserWorkItem((o) =>
					{
						lock (key)
						{
							// メインスレッドのWait状態を解除する
							Monitor.PulseAll(key);
						}

						using(var client = new WebClient())
						{
							// 関数引数で渡されたテストメソッドを実行する
							test(client);
						}

						// テストが完了したので、画面閉じる
						form1.Invoke(new Action(() =>
						{
						   form1.Close();
						}));
					});

					// 画面が表示状態になったら通知されるShownイベントのイベントハンドラでワーカスレッドへ制御を移す
					form1.Shown += (o, e) =>
					{
						// ワーカスレッドが処理を開始するまで待機状態（一時的にロックの解除）になる
						Monitor.Wait(key);
					};
					// メインスレッドが待ち状態となるようにダイアログとして実行する
					form1.ShowDialog();
				}
			}
		}

		[TestMethod]
		public void TestNotFound()
		{
			RunServer((client) =>
			{
				try
				{
					// このURIにアクセスすると404が返ることをテストする
					client.DownloadString("http://127.0.0.1:8192/lesson/");
					Assert.Fail("return OK");
				}
				// 404が返るとWebExceptionが通知される
				catch(WebException e)
				{
					Assert.AreEqual(WebExceptionStatus.ProtocolError, e.Status);

					using(var resp = e.Response as HttpWebResponse)
					{
						// 404か確認
						Assert.AreEqual(HttpStatusCode.NotFound, resp.StatusCode);
					}
				}
			});
		}

		[TestMethod]
		public void TestOK()
		{
			RunServer((client) =>
			{
				var test = client.DownloadString("http://127.0.0.1:8192/lesson/test/OK");
				Assert.AreEqual("OK", test);
			});
		}
	}
}
