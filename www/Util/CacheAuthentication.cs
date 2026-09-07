using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Filters;

namespace wwwDesignersGuild.Util
{
    public class CacheAuthentication : Attribute, System.Web.Http.Filters.IAuthenticationFilter
    {
        public bool AllowMultiple
        {
            get
            {
                return false;
            }
        }

        public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
        {
            AuthenticationHeaderValue authorization = context.Request.Headers.Authorization;

            var headers = context.Request.Headers.Select(p => new { p.Key, p.Value });

			var cacheBustingHeader = headers?.Where(p => p.Key.ToLowerInvariant() == "cachebustingkey").SingleOrDefault();

			if (String.IsNullOrEmpty(cacheBustingHeader?.Value.FirstOrDefault()))
            {
                var message = new
                {
                    Reason = "Missing Credentials A",
                    Headers = headers
                };

                context.ErrorResult = new AuthenticationFailureResult(message, context.Request);

                return Task.FromResult(0);
            }
            else if (cacheBustingHeader?.Value.Count() != 1)
			{
				var message = new
				{
					Reason = "Missing Credentials B",
					Headers = headers
				};

				context.ErrorResult = new AuthenticationFailureResult(message, context.Request);

				return Task.FromResult(0);
            }
            else if (cacheBustingHeader?.Value.FirstOrDefault() != "9079C890-8ADB-47C2-8D26-458C773EFEDA")
			{
				var message = new
				{
					Reason = "Missing Credentials C",
					Headers = headers
				};

				context.ErrorResult = new AuthenticationFailureResult(message, context.Request);

				return Task.FromResult(0);
            }

            // 2. If there are no credentials, do nothing.
            if (authorization == null)
			{
				var message = new
				{
					Reason = "Missing Credentials D",
					Headers = headers
				};

				context.ErrorResult = new AuthenticationFailureResult(message, context.Request);

				return Task.FromResult(0);
            }

            // 3. If there are credentials but the filter does not recognize the 
            //    authentication scheme, do nothing.
            if (authorization.Scheme != "Basic")
			{
				var message = new
				{
					Reason = "Missing Credentials E",
					Headers = headers
				};

				context.ErrorResult = new AuthenticationFailureResult(message, context.Request);

				return Task.FromResult(0);
            }

            // 4. If there are credentials that the filter understands, try to validate them:

            // 5. If the credentials are bad, set the error result.
            if (String.IsNullOrEmpty(authorization.Parameter))
			{
				var message = new
				{
					Reason = "Missing Credentials F",
					Headers = headers
				};

				context.ErrorResult = new AuthenticationFailureResult(message, context.Request);

				return Task.FromResult(0);
            }

            Tuple<string, string> userNameAndPassword = ExtractUserNameAndPassword(authorization.Parameter);

            if (userNameAndPassword == null)
			{
				var message = new
				{
					Reason = "Invalid Credentials G",
					Headers = headers
				};

				context.ErrorResult = new AuthenticationFailureResult(message, context.Request);

				return Task.FromResult(0);
			}

            string userName = userNameAndPassword.Item1;
            string password = userNameAndPassword.Item2;

            if (userName == "CAdmin" && password == "TapestryFlower22")
            {
                string identity = "";

                IPrincipal principal = new GenericPrincipal(
                    new GenericIdentity(identity), new string[] { "CacheBuster" });
                Thread.CurrentPrincipal = principal;
                HttpContext.Current.User = principal;

            }

            return Task.FromResult(0);
        }

        public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
        {
            var challenge = new AuthenticationHeaderValue("Basic");
            context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);
            return Task.FromResult(0);
        }

        private static Tuple<string, string> ExtractUserNameAndPassword(string authorizationParameter)
        {
            byte[] credentialBytes;

            try
            {
                credentialBytes = Convert.FromBase64String(authorizationParameter);
            }
            catch (FormatException)
            {
                return null;
            }

            // The currently approved HTTP 1.1 specification says characters here are ISO-8859-1.
            // However, the current draft updated specification for HTTP 1.1 indicates this encoding is infrequently
            // used in practice and defines behavior only for ASCII.
            Encoding encoding = Encoding.ASCII;
            // Make a writable copy of the encoding to enable setting a decoder fallback.
            encoding = (Encoding)encoding.Clone();
            // Fail on invalid bytes rather than silently replacing and continuing.
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
            string decodedCredentials;

            try
            {
                decodedCredentials = encoding.GetString(credentialBytes);
            }
            catch (DecoderFallbackException)
            {
                return null;
            }

            if (String.IsNullOrEmpty(decodedCredentials))
            {
                return null;
            }

            int colonIndex = decodedCredentials.IndexOf(':');

            if (colonIndex == -1)
            {
                return null;
            }

            string userName = decodedCredentials.Substring(0, colonIndex);
            string password = decodedCredentials.Substring(colonIndex + 1);
            return new Tuple<string, string>(userName, password);
        }
    }

    public class AddChallengeOnUnauthorizedResult : IHttpActionResult
    {
        public AddChallengeOnUnauthorizedResult(AuthenticationHeaderValue challenge, IHttpActionResult innerResult)
        {
            Challenge = challenge;
            InnerResult = innerResult;
        }

        public AuthenticationHeaderValue Challenge { get; private set; }

        public IHttpActionResult InnerResult { get; private set; }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            HttpResponseMessage response = await InnerResult.ExecuteAsync(cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Only add one challenge per authentication scheme.
                if (!response.Headers.WwwAuthenticate.Any((h) => h.Scheme == Challenge.Scheme))
                {
                    response.Headers.WwwAuthenticate.Add(Challenge);
                }
            }

            return response;
        }
    }

    public class AuthenticationFailureResult : IHttpActionResult
    {
        public AuthenticationFailureResult(object jsonContent, HttpRequestMessage request)
        {
            JsonContent = jsonContent;
            Request = request;
        }

        public HttpRequestMessage Request { get; private set; }

        public Object JsonContent { get; private set; }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                RequestMessage = Request,
                Content = new ObjectContent(JsonContent.GetType(), JsonContent, new JsonMediaTypeFormatter())
            };
            return response;
        }
    }

    public class PassCachePurge
    {
        public int? TradeAccountID { get; set; }
        public string ContentType { get; set; }
        public int ContentID { get; set; }
        public short CountryID { get; set; }
        public byte LanguageID { get; set; }
        public DateTime ContentDate { get; set; }
        public bool? TradeSwitch { get; set; }
        public byte? templateId { get; set; }
        public DateTimeOffset? ExplicitExpiry { get; set; }
        public IEnumerable<long> PageSectionIDs { get; set; }
        public string CountryURL { get; set; }
        public bool? PlatformSwitch { get; set; }
    }
}