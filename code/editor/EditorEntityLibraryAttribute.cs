using System;
using Sandbox;

namespace Facepunch.CoreWars.Editor
{
	public class EditorEntityLibraryAttribute : LibraryAttribute
	{
		public string VolumeMaterial { get; set; }
		public string EditorModel { get; set; }
		public bool IsVolume { get; set; }
	}
}
