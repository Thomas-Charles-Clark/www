using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;

using wwwDesignersGuild.Models.Cache;

public class CacheMetadataExtractor
{
	// 1. CHANGED: Now returns IEnumerable to stream entries out one-by-one
	public static IEnumerable<CacheEntryMetadata> GetActiveEntriesMetadata(MemoryCache cache)
	{
		DateTime utcNow = DateTime.UtcNow;

		PropertyInfo targetsProp = typeof(MemoryCache).GetProperty("AllSRefTargets", BindingFlags.NonPublic | BindingFlags.Instance);
		if (targetsProp == null) yield break;

		Array storesArray = targetsProp.GetValue(cache) as Array;
		if (storesArray == null) yield break;

		// Cache reflection FieldInfos outside the loops to prevent overhead
		FieldInfo entriesField = null;
		FieldInfo absExpField = null;
		PropertyInfo keyProp = null;
		PropertyInfo valProp = null;
		FieldInfo slidExpField = null;
		FieldInfo lastAccessField = null;

		foreach (object storePartition in storesArray)
		{
			if (storePartition == null) continue;

			if (entriesField == null)
				entriesField = storePartition.GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);

			var entriesHashtable = entriesField.GetValue(storePartition) as Hashtable;
			if (entriesHashtable == null) continue;

			// We must copy the entries quickly under lock to prevent holding a long blocking lock on the live cache
			List<object> entriesCopy = new List<object>();
			lock (entriesHashtable.SyncRoot)
			{
				foreach (DictionaryEntry entry in entriesHashtable)
				{
					if (entry.Value != null) entriesCopy.Add(entry.Value);
				}
			}

			// Process the items outside of the Hashtable lock to keep the live app fast
			foreach (object cacheEntryValue in entriesCopy)
			{
				Type entryType = cacheEntryValue.GetType();

				if (absExpField == null) absExpField = entryType.GetField("_utcAbsExp", BindingFlags.NonPublic | BindingFlags.Instance);
				DateTime utcAbsExp = absExpField != null ? (DateTime)absExpField.GetValue(cacheEntryValue) : DateTime.MaxValue;

				if (utcAbsExp <= utcNow) continue;

				if (keyProp == null)
				{
					keyProp = entryType.GetProperty("Key", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
					valProp = entryType.GetProperty("Value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
					slidExpField = entryType.GetField("_slidingExp", BindingFlags.NonPublic | BindingFlags.Instance);
					lastAccessField = entryType.GetField("_utcLastUpdateUsage", BindingFlags.NonPublic | BindingFlags.Instance);
				}

				string key = keyProp?.GetValue(cacheEntryValue) as string;
				object value = valProp?.GetValue(cacheEntryValue);
				TimeSpan slidingExp = slidExpField != null ? (TimeSpan)slidExpField.GetValue(cacheEntryValue) : TimeSpan.Zero;
				DateTime utcLastAccess = lastAccessField != null ? (DateTime)lastAccessField.GetValue(cacheEntryValue) : DateTime.MinValue;

				// 2. Get byte counts using custom Gemini magic MemoryProfiler
				long calculatedBytes = MemoryProfiler.GetExactByteCount(value);

				// 3. Yield back control to the HTTP stream pipeline instantly
				yield return new CacheEntryMetadata
				{
					Key = key,
					AbsoluteExpiration = utcAbsExp,
					SlidingExpiration = slidingExp,
					LastAccessed = utcLastAccess,
					ByteCount = calculatedBytes
				};
			}
		}
	}

	public static CacheStateSummary GetCacheStateSummary(string cacheName, string[] keyBreakdown, MemoryCache cache)
	{
		var rawEntriesToProcess = new List<object>();
		DateTime utcNow = DateTime.UtcNow;

		// 1. Unpack MemoryCache store partitions via reflection
		PropertyInfo targetsProp = typeof(MemoryCache).GetProperty("AllSRefTargets", BindingFlags.NonPublic | BindingFlags.Instance);
		if (targetsProp == null) return null;

		Array storesArray = targetsProp.GetValue(cache) as Array;
		if (storesArray == null) return null;

		// 2. Quickly harvest active entries while minimizing lock durations
		foreach (object storePartition in storesArray)
		{
			if (storePartition == null) continue;

			FieldInfo entriesField = storePartition.GetType().GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
			if (entriesField == null) continue;

			var entriesHashtable = entriesField.GetValue(storePartition) as Hashtable;
			if (entriesHashtable == null) continue;

			// CRITICAL: Keep this lock window as short as humanly possible!
			lock (entriesHashtable.SyncRoot)
			{
				foreach (DictionaryEntry entry in entriesHashtable)
				{
					if (entry.Value != null)
					{
						// Just take a reference pointer to process safely outside the lock
						rawEntriesToProcess.Add(entry.Value);
					}
				}
			}
		}

		if (!rawEntriesToProcess.Any()) return null;

		var metadataList = new List<CacheEntryMetadata>();

		// 3. Process metadata and heavy serialization OUTSIDE of the internal cache lock
		foreach (object cacheEntryValue in rawEntriesToProcess)
		{
			Type entryType = cacheEntryValue.GetType();

			// Check expiration
			FieldInfo absExpField = entryType.GetField("_utcAbsExp", BindingFlags.NonPublic | BindingFlags.Instance);
			DateTime utcAbsExp = absExpField != null ? ((DateTime)absExpField.GetValue(cacheEntryValue)).ToUniversalTime() : DateTime.MaxValue;

			if (utcAbsExp <= utcNow) continue; // Skip expired records

			PropertyInfo keyProp = entryType.GetProperty("Key", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo valProp = entryType.GetProperty("Value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
			FieldInfo slidExpField = entryType.GetField("_slidingExp", BindingFlags.NonPublic | BindingFlags.Instance);
			FieldInfo lastAccessField = entryType.GetField("_utcLastUpdateUsage", BindingFlags.NonPublic | BindingFlags.Instance);

			string key = keyProp?.GetValue(cacheEntryValue) as string;
			object value = valProp?.GetValue(cacheEntryValue);
			TimeSpan slidingExp = slidExpField != null ? (TimeSpan)slidExpField.GetValue(cacheEntryValue) : TimeSpan.Zero;
			DateTime utcLastAccess = lastAccessField != null ? (DateTime)lastAccessField.GetValue(cacheEntryValue) : DateTime.MinValue;

			// Note: If memory usage spikes too high here, consider a custom binary size estimator instead of a full JSON serialization string dump.
			var byteCount = MemoryProfiler.GetExactByteCount(value);

			metadataList.Add(new CacheEntryMetadata
			{
				Key = key,
				KeyBreakdown = keyBreakdown,
				AbsoluteExpiration = utcAbsExp,
				SlidingExpiration = slidingExp,
				LastAccessed = utcLastAccess,
				ByteCount = byteCount
			});
		}

		if (metadataList.Any())
		{
			// FIX: Cast the Sum directly to decimal FIRST to force precise fractional calculation
			decimal totalBytes = (decimal)metadataList.Sum(p => p.ByteCount);

			return new CacheStateSummary
			{
				CacheName = cacheName,
				ObjectCount = metadataList.Count,
				KeyBreakdown = keyBreakdown,
				TotalMebibytes = totalBytes / (1024m * 1024m) // Ensure precise decimal division
			};
		}

		return null;
	}
}
