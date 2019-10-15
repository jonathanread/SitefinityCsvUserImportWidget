using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserImportUtility.Models
{
	public class UserCsvRecord
	{
		public string Email { get; set; }
		public string UserName { get { return !userName.IsNullOrWhitespace() ? userName : this.Email; } set { userName = value; } }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Role { get; set; }
		public string Password { get; set; }
		private string userName;
		public bool IsValid()
		{
			return !this.Email.IsNullOrWhitespace() && !this.FirstName.IsNullOrWhitespace() && !this.LastName.IsNullOrWhitespace();
		}
	}
}
