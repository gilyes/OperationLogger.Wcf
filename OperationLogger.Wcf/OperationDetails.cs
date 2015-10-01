using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace OperationLogger.Wcf
{
	public class OperationDetails
	{
		public OperationDetails()
		{
			Parameters = new Dictionary<string, object>();
		}

		public IIdentity Identity { get; set; }

		public string OperationName { get; set; }

		public string Action { get; set; }

		public string UserName { get; set; }

		public bool IsAnonymous { get; set; }

		public Uri ServiceUri { get; set; }

		public string ClientAddress { get; set; }

		public Dictionary<string, object> Parameters { get; private set; }
	}
}