using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace wwwDesignersGuild.Models.Films
{
	public class VideoObjectJSONLD
	{
		/*
		  @context = "https://schema.org",
                                    @type = "VideoObject",
                                    name = s.Title,
                                    description = s.Description,
                                    contentUrl = s.WatchURL
		 */

		[JsonIgnore]
		public string VideoID { get; set; }

		[JsonProperty("@context")]
		public string Context { get; set; }

		[JsonProperty("@type")]
		public string Type { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("contentUrl")]
		public string ContentURL { get; set; }

		[JsonProperty("uploadDate")]
		public DateTime UploadDT { get; set; }

		public VideoObjectJSONLD(string videoId, string context, string type, string name, string description, string contentURL, DateTime uploadDT)
		{
			VideoID = videoId;
			Context = context;
			Type = type;
			Name = name;
			Description = description;
			ContentURL = contentURL;
			UploadDT = uploadDT;
		}

		[JsonProperty("thumbnailUrl")]
		public string ThumbnailURL
		{
			get
			{
				if (ContentURL.Contains("youtube"))
				{
					return $"https://img.youtube.com/vi/{ContentURL.Replace("https://www.youtube.com/watch?v=", "")}/maxresdefault.jpg";
				}
				else if (ContentURL.Contains("vimeo")) 
				{
					using (var client = new System.Net.Http.HttpClient())
					{
						var response = client.GetAsync($"https://vimeo.com/api/oembed.json?url=https://vimeo.com/{VideoID}").Result;

						if (response.IsSuccessStatusCode)
						{
							var stub = JsonConvert.DeserializeObject<VimeoVideoStub>(response.Content.ReadAsStringAsync().Result);

							UploadDT = stub.upload_date;

							return stub.thumbnail_url;
						}
					}
				}

				return "";
			}
		}
	}
}