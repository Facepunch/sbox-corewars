using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars
{
	public static partial class Util
	{
		private static Dictionary<string, bool> FileExistsCache { get; set; } = new();

		public static bool FileExistsCached( string fileName )
		{
			fileName = fileName.ToLower();

			if ( FileExistsCache.TryGetValue( fileName, out var value ) )
			{
				return value;
			}

			value = FileSystem.Mounted.FileExists( fileName );
			FileExistsCache[fileName] = value;
			return value;
		}
	}
}
