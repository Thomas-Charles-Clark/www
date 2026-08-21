using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Caching;

using wwwDesignersGuild.Models.Cache;

public class CacheMetadataExtractor
{
    public static List<CacheEntryMetadata> GetActiveEntriesMetadata(MemoryCache cache)
    {
        var metadataList = new List<CacheEntryMetadata>();
        DateTime utcNow = DateTime.UtcNow; // Standard reference point for validation

        // 1. Get the private internal property 'AllSRefTargets' which unpacks '_storeRefs'
        PropertyInfo targetsProp = typeof(MemoryCache).GetProperty("AllSRefTargets", BindingFlags.NonPublic | BindingFlags.Instance);
        if (targetsProp == null) return metadataList;

        // This returns a collection typed as an array of System.Runtime.Caching.MemoryCacheStore
        Array storesArray = targetsProp.GetValue(cache) as Array;
        if (storesArray == null) return metadataList;

        // 2. Iterate through each memory cache store partition
        foreach (object storePartition in storesArray)
        {
            if (storePartition == null) continue;

            // 3. Extract the '_entries' field (which is a Hashtable) from the MemoryCacheStore instance
            FieldInfo entriesField = storePartition.GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
            if (entriesField == null) continue;

            var entriesHashtable = entriesField.GetValue(storePartition) as Hashtable;
            if (entriesHashtable == null) continue;

            // Hashtable reads are thread-safe, but locking SyncRoot ensures consistency during reflection parsing
            lock (entriesHashtable.SyncRoot)
            {
                foreach (DictionaryEntry entry in entriesHashtable)
                {
                    object cacheEntryValue = entry.Value;
                    if (cacheEntryValue == null) continue;

                    Type entryType = cacheEntryValue.GetType();

                    // 4. Extract the absolute expiration timestamp
                    FieldInfo absExpField = entryType.GetField("_utcAbsExp", BindingFlags.NonPublic | BindingFlags.Instance);
                    DateTime utcAbsExp = absExpField != null ? (DateTime)absExpField.GetValue(cacheEntryValue) : DateTime.MaxValue;

                    // EXCLUSION FILTER: If the item has expired but hasn't been garbage collected yet, skip it completely
                    if (utcAbsExp <= utcNow)
                    {
                        continue;
                    }

                    // 5. Use reflection to parse individual metadata properties for active items
                    PropertyInfo keyProp = entryType.GetProperty("Key", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    PropertyInfo valProp = entryType.GetProperty("Value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                    FieldInfo slidExpField = entryType.GetField("_slidingExp", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo priorityField = entryType.GetField("_fields", BindingFlags.NonPublic | BindingFlags.Instance);
                    FieldInfo lastAccessField = entryType.GetField("_utcLastUpdateUsage", BindingFlags.NonPublic | BindingFlags.Instance);

                    string key = keyProp?.GetValue(cacheEntryValue) as string;
                    object value = valProp?.GetValue(cacheEntryValue);
                    TimeSpan slidingExp = slidExpField != null ? (TimeSpan)slidExpField.GetValue(cacheEntryValue) : TimeSpan.Zero;
                    DateTime utcLastAccess = lastAccessField != null ? (DateTime)lastAccessField.GetValue(cacheEntryValue) : DateTime.MinValue;

                    metadataList.Add(new CacheEntryMetadata
                    {
                        Key = key,
                        Value = value,
                        AbsoluteExpiration = utcAbsExp == DateTime.MaxValue ? DateTimeOffset.MaxValue : new DateTimeOffset(utcAbsExp, TimeSpan.Zero),
                        SlidingExpiration = slidingExp,
                        LastAccessed = utcLastAccess == DateTime.MinValue ? DateTimeOffset.MinValue : new DateTimeOffset(utcLastAccess, TimeSpan.Zero)
                    });
                }
            }
        }

        return metadataList;
    }
}
