using Facepunch.CoreWars.Inventory;

namespace Facepunch.CoreWars
{
	public class ArmorLegsShopItemTier1 : BaseArmorShopItem<ArmorLegsTier1>
	{
		public override string Name => "Light Legs Armor";
		public override string Description => "A low protection legs armor piece.";
		public override Dictionary<Type, int> Costs => new()
		{
			[typeof( IronItem )] = 16
		};

		public override string GetIcon( Player player )
		{
			return "textures/items/armor_legs_1.png";
		}
	}
}
