using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using HttpWebAppTest;

namespace HttpWebAppTestTest
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{
			using (var form1 = new Form1())
			{
				//Assert.AreEqual(1, form1.Mul(1, 1));
			}
		}
	}
}
