namespace Facepunch.CoreWars.Editor
{
	public abstract class EditorAction
	{
		public virtual string Name => "Action";

		public virtual void Undo()
		{

		}

		public virtual void Perform()
		{

		}
	}
}
