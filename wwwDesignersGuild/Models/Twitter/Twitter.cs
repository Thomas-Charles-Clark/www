using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using Tweetinvi;
using System.Threading.Tasks;
using Tweetinvi.Auth;
using Tweetinvi.Parameters;
using Tweetinvi.Core.Web;
using Tweetinvi.Models;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace wwwDesignersGuild.Models.Twitter
{
    public class TwitterOAuthResponse
    {
        public string OAuthToken { get; set; }
        public string OAuthVerifier { get; set; }

        public TwitterOAuthResponse(string oauth_token, string oauth_verifier)
        {
            OAuthToken = oauth_token;
            OAuthVerifier = oauth_verifier;
        }
    }

    public class PassMoodboardTweet 
    { 
        public string shareText { get; set; }
        public string imageUrl { get; set; }
        public string authTokens { get; set; }
    }

    public class RequestTokenResponse
    { 
        public string AuthorizationURL { get; set; }
        public string AuthRequestID { get; set; }

        public RequestTokenResponse (string id, string url)
        {
            AuthRequestID = id;

            AuthorizationURL = url;
        }
    }

    public static class Twitter
    {
        private static readonly IAuthenticationRequestStore AuthRequestStore = new LocalAuthenticationRequestStore();

        public static async Task<RequestTokenResponse> RequestToken()
        {
            var client = new TwitterClient(
                ConfigurationManager.AppSettings["twitterConsumerKey"],
                ConfigurationManager.AppSettings["twitterConsumerSecret"]
            );

            var authRequestId = $"{Guid.NewGuid()}";

            var redirect = $"{HttpContext.Current.Request.Url.Scheme}://{HttpContext.Current.Request.Url.Authority}/moodboard/twitter-callback";

            // Add the user identifier as a query parameters that will be received by `ValidateTwitterAuth`
            var redirectURL = AuthRequestStore.AppendAuthenticationRequestIdToCallbackUrl(redirect, authRequestId);

            var authRequestToken = await client.Auth.RequestAuthenticationUrlAsync(redirectURL);

            await AuthRequestStore.AddAuthenticationTokenAsync(authRequestId, authRequestToken);

            return new RequestTokenResponse(authRequestId, authRequestToken.AuthorizationURL);
        }

        public static async Task<string> UploadMoodboardMediaAndTweet(PassMoodboardTweet objTweet)
        {
            var client = new TwitterClient(
                ConfigurationManager.AppSettings["twitterConsumerKey"],
                ConfigurationManager.AppSettings["twitterConsumerSecret"]
            );

            var requestParameters = await RequestCredentialsParameters.FromCallbackUrlAsync(objTweet.authTokens, AuthRequestStore);

            var userCreds = await client.Auth.RequestCredentialsAsync(requestParameters);

            var userClient = new TwitterClient(userCreds);

            var withoutQueryString = objTweet.imageUrl.Substring(0, objTweet.imageUrl.IndexOf('?'));

            var path = HttpContext.Current.Server.MapPath($"~/Content/Images/moodboards/{System.IO.Path.GetFileName(withoutQueryString)}");

            var uploadResult = await userClient.Upload.UploadTweetImageAsync(System.IO.File.ReadAllBytes(path));

            var hybridClient = new TwitterClient(
                ConfigurationManager.AppSettings["twitterConsumerKey"],
                ConfigurationManager.AppSettings["twitterConsumerSecret"],
                userCreds.AccessToken,
                userCreds.AccessTokenSecret);

            var poster = new TweetsV2Poster(hybridClient);

            var tweetParams = new PublishTweetParameters();

            tweetParams.Text = objTweet.shareText;

            tweetParams.MediaIds.Add(uploadResult.Id.Value);

            var postResult = await poster.PostTweet(new TweetParamsV2(tweetParams));

            var postResultObj = JsonConvert.DeserializeObject<TweetResultV2Wrapper>(postResult.Content);

            return postResultObj.data.URL;
        }
    }

    // With thanks: https://github.com/linvi/tweetinvi/issues/1147#issuecomment-1173174302
    public class TweetsV2Poster
    {
        // ----------------- Fields ----------------

        private readonly ITwitterClient client;

        // ----------------- Constructor ----------------

        public TweetsV2Poster(ITwitterClient client)
        {
            this.client = client;
        }

        public Task<ITwitterResult> PostTweet(TweetParamsV2 tweet)
        {
            return this.client.Execute.AdvanceRequestAsync(
                (ITwitterRequest request) =>
                {
                    var jsonBody = this.client.Json.Serialize(tweet);

                    // Technically this implements IDisposable,
                    // but if we wrap this in a using statement,
                    // we get ObjectDisposedExceptions,
                    // even if we create this in the scope of PostTweet.
                    //
                    // However, it *looks* like this is fine.  It looks
                    // like Microsoft's HTTP stuff will call
                    // dispose on requests for us (responses may be another story).
                    // See also: https://stackoverflow.com/questions/69029065/does-stringcontent-get-disposed-with-httpresponsemessage
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    request.Query.Url = "https://api.twitter.com/2/tweets";
                    request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                    request.Query.HttpContent = content;
                }
            );
        }
    }

    public class TweetParamsV2
    {
        public string text { get; set; }
        public TweetMediaV2 media { get; set; }

        public TweetParamsV2(PublishTweetParameters objParams)
        {
            text = objParams.Text ?? "";

            media = new TweetMediaV2(objParams.MediaIds);
        }
    }

    public class TweetMediaV2
    {
        public IEnumerable<string> media_ids { get; set; }

        public TweetMediaV2(IEnumerable<long> ids) 
        {
            media_ids = ids.Select(i => $"{i}");
        }
    }

    public class TweetResultV2Wrapper
    {
        public TweetResultV2 data { get; set; }
    }

    public class TweetResultV2
    {
        public long id { get; set; }
        public string text { get; set; }

        public string URL
        {
            get
            {
                return System.Text.RegularExpressions.Regex.Match(text, @"https://t.co/.*$")?.Value;
            }
            set
            {
                ;
            }
        }
    }

    /// <summary>
    /// There are a lot more fields according to:
    /// https://developer.twitter.com/en/docs/twitter-api/tweets/manage-tweets/api-reference/post-tweets
    /// but these are the ones we care about for our use case.
    /// </summary>
    public class TweetV2PostRequest
    {
        /// <summary>
        /// The text of the tweet to post.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;
    }
}