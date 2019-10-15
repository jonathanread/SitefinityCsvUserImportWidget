using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Sitefinity.Security;
using UserImportUtility.Models;
using System.Web.Security;
using Telerik.Sitefinity.Security.Model;
using Telerik.Sitefinity.Data;
using Telerik.Sitefinity.Abstractions;
using System.Web;

namespace UserImportUtility
{
	public class UserImport
	{
		public bool ImportUsers(Stream fileUpload)
		{
			bool imported = true;
			int index = 1;
			string transactionName = $"userUploadTransaction_{index}";

			UserManager userManager = UserManager.GetManager(null, transactionName);
			UserProfileManager profileManager = UserProfileManager.GetManager(null, transactionName);
			RoleManager roleManager = RoleManager.GetManager(null, transactionName);
			List<UserCsvRecord> failedImportList = new List<UserCsvRecord>();
			using (var reader = new StreamReader(fileUpload))
			{
				using (var csv = new CsvReader(reader))
				{
					var records = csv.GetRecords<UserCsvRecord>().ToList();
					if (records != null && records.Count() > 0)
					{
						int totalUsers = records.Count();
						int transactionCount = 100;
						var lastItem = records.Last();
						foreach (var item in records)
						{
							if (item.IsValid())
							{
								User user = userManager.CreateUser(item.UserName, item.Password, item.Email, "", "", false, null, out MembershipCreateStatus status);

								if (status == MembershipCreateStatus.Success)
								{
									if (profileManager.CreateProfile(user, Guid.NewGuid(), typeof(SitefinityProfile)) is SitefinityProfile sfProfile)
									{
										sfProfile.FirstName = item.FirstName;
										sfProfile.LastName = item.LastName;
										profileManager.RecompileItemUrls(sfProfile);
										AddUserToRoles(user, item.Role.Split(';').ToList(), roleManager);
									}
								}
								else
								{
									Log.Write($"User '{item.Email}' was not created successfully.  Message: {status}");

								}

								if (index % transactionCount == 0 || item == lastItem)
								{
									TransactionManager.CommitTransaction(transactionName);
									transactionName = $"userUploadTransaction_{++index}";
									userManager = UserManager.GetManager(null, transactionName);
									profileManager = UserProfileManager.GetManager(null, transactionName);
								}
							}
							else
							{
								Log.Write($"User from line {index} was not imported due to missing data, Email, First name or Last name.");

							}
						}
					}
					else
					{
						Log.Write($"Conversion from CSV file failed or file was empty, try again.");
						imported = false;
					}
				}
			}
			return imported;
		}

		public Role CreateRole(string roleName, RoleManager roleManager)
		{
			Role role = null;

			if (roleManager.GetRoles().Where(r => r.Name == roleName).FirstOrDefault() == null)
			{
				role = roleManager.CreateRole(roleName);
			}
			return role;
		}

		public void AddUserToRoles(User user, List<string> rolesToAdd, RoleManager roleManager)
		{
			roleManager.Provider.SuppressSecurityChecks = true;

			if (user != null)
			{
				foreach (var roleName in rolesToAdd)
				{
					if (roleManager.RoleExists(roleName))
					{
						Role role = roleManager.GetRole(roleName);
						roleManager.AddUserToRole(user, role);
					}
					else
					{
						Role role = CreateRole(roleName, roleManager);
						roleManager.AddUserToRole(user, role);
					}
				}
			}

			roleManager.Provider.SuppressSecurityChecks = false;
		}
	}
}

