using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.UPS
{
	public class OAuthResponse
	{
		[JsonProperty("token_type")]
		public string TokenType { get; set; }

		[JsonProperty("issued_at")]
		public string IssuedDT { get; set; }

		[JsonProperty("client_id")]
		public string ClientID { get; set; }

		[JsonProperty("access_token")]
		public string AccessToken { get; set; }

		[JsonProperty("scope")]
		public string Scope { get; set; }

		[JsonProperty("expires_in")]
		public int ExpiresInSeconds { get; set; }

		[JsonProperty("refresh_count")]
		public int RefreshCount { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }
	}
}