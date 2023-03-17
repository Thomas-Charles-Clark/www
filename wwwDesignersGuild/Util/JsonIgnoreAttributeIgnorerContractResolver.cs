using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace wwwDesignersGuild.Util
{
    // Thank you, https://stackoverflow.com/questions/37954376/ignore-jsonignore-attribute-on-serialization-deserialization#answer-37954657
    public class JsonIgnoreAttributeIgnorerContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.Ignored = false; // Here is the magic
            return property;
        }
    }
}