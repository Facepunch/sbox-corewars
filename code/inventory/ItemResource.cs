﻿using Sandbox;
using System.Collections.Generic;

namespace Facepunch.CoreWars;

public class ItemResource : GameResource
{
	[Property]
	public string ItemName { get; set; }

	[Property]
	public string UniqueId { get; set; }

	[Property]
	public string Description { get; set; }

	[Property, ResourceType( "png" )]
	public string Icon { get; set; }

	[Property, ResourceType( "vmdl" )]
	public string WorldModel { get; set; } = "models/sbox_props/burger_box/burger_box.vmdl";

	[Property]
	public List<string> Tags { get; set; } = new();

	protected override void PostLoad()
	{
		if ( Host.IsServer || Host.IsClient )
		{
			InventorySystem.ReloadDefinitions();
		}

		base.PostLoad();
	}

	protected override void PostReload()
	{
		if ( Host.IsServer || Host.IsClient )
		{
			InventorySystem.ReloadDefinitions();
		}
		
		base.PostReload();
	}
}