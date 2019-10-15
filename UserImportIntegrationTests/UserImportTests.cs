using MbUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UserImportUtility;

namespace UserImportIntegrationTests
{
	[TestFixture]
	[Description("Some simple tests.")]
	public class UserImportTests
	{
		[SetUp]
		public void TestSetup()
		{
			var assembly = Assembly.GetExecutingAssembly();
			string resourceName = "UserImportIntegrationTests.Assets.users.csv";
			csvStream = assembly.GetManifestResourceStream(resourceName);
		}

		[TearDown]
		public void TestTeardown()
		{
			csvStream.Dispose();
			csvStream.Close();
		}

		[Test]
		[Author("Jonathan Read")]
		[Description("Test the user import function from a csv file")]
		public void ImportUsersTest()
		{
			//TODO: test with 10 users?
			UserImport ui = new UserImport();
			var imported = ui.ImportUsers(csvStream);

			Assert.IsTrue(imported);
		}

		Stream csvStream;
	}
}
