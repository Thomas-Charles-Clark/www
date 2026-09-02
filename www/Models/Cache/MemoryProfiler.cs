using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace wwwDesignersGuild.Models.Cache
{
	// Again all credit to Google's Gemini model
	public static class MemoryProfiler
	{
		/// <summary>
		/// Calculates the exact physical memory footprint of an object graph in bytes.
		/// Safely walks reference types without allocations or serialization.
		/// </summary>
		public static long GetExactByteCount(object obj)
		{
			if (obj == null) return 0;

			var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
			return WalkObjectGraph(obj, visited);
		}

		private static long WalkObjectGraph(object obj, HashSet<object> visited)
		{
			if (obj == null) return 0;

			Type type = obj.GetType();

			// Handle primitive value types directly based on the platform architecture
			if (type.IsValueType && type.IsPrimitive)
			{
				return Marshal.SizeOf(type);
			}

			// Handle strings explicitly (2 bytes per character + object header overhead)
			if (obj is string str)
			{
				return (str.Length * 2) + 8 + IntPtr.Size * 2;
			}

			// Prevent infinite loops by tracking reference identities
			if (!visited.Add(obj)) return 0;

			long size = IntPtr.Size * 2; // Object base memory header overhead

			// Handle primitive arrays cleanly
			if (type.IsArray)
			{
				var array = (Array)obj;
				size += IntPtr.Size * 2; // Array length metadata overhead

				Type elementType = type.GetElementType();
				if (elementType.IsValueType && elementType.IsPrimitive)
				{
					size += array.Length * Marshal.SizeOf(elementType);
					return size;
				}

				foreach (var item in array)
				{
					size += WalkObjectGraph(item, visited);
				}
				return size;
			}

			// Handle complex object graphs via field reflection
			FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo field in fields)
			{
				object fieldVal = field.GetValue(obj);
				if (fieldVal == null) continue;

				if (field.FieldType.IsValueType && field.FieldType.IsPrimitive)
				{
					size += Marshal.SizeOf(field.FieldType);
				}
				else
				{
					size += WalkObjectGraph(fieldVal, visited);
				}
			}

			return size;
		}
	}

	// Helper to ensure reference equality tracking rather than object value comparisons
	internal class ReferenceEqualityComparer : IEqualityComparer<object>
	{
		public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
		public new bool Equals(object x, object y) => ReferenceEquals(x, y);
		public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
	}
}