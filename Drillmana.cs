using Microsoft.Xna.Framework;
using System;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Drillmana
{
	public class Drillmana : Mod
	{
	}
	public class DrillPlayer : ModPlayer
	{
		Mod Alchemist => ModLoader.GetMod("AlchemistNPC");
		public int t = 0;//use timer
		public int pt = 0;//disable timer
		public bool p1 = false;//tracks movement disable
		public bool p2 = false;//tracks drill disable
		public float D = 0f;//depth in tiles
		public float C = 0f;//depth multiplier for mana drain, doubles every 400 tiles
		public override void PreUpdate()
		{
			if (p2 == true)
			{
				pt++;
				if (pt >= 540)
				{
					p2 = false;
					pt = 0;
				}
				else if (pt >= 200)
				{
					p1 = false;
				}
				
			}
			else if (p1 == true && p2 == false && (player.controlUseItem != true || player.HeldItem.type != ItemID.LaserDrill))
			{
				p2 = true;
				player.AddBuff (199, 540, false);
				Main.PlaySound(SoundID.Item9, player.Center);
			}
			else if (player.HeldItem.type == ItemID.LaserDrill && player.controlUseItem && p1 == false && p2 == false && player.altFunctionUse != 2 && player.HeldItem.GetGlobalItem<DrillItem>().H == false)
			{
				C = 1.39f * (1 + player.position.Y/6400);
				t++;
				if (t >= 5 && player.statMana < (int)C)
				{
					p1 = true;
					t = 0;
					Main.PlaySound(SoundID.Item122, player.Center);
				}
				else if (t >= 5)
				{
					player.statMana -= (int)C;
					t = 0;
				}
			}
		}
		public override void UpdateEquips(ref bool wallSpeedBuff, ref bool tileSpeedBuff, ref bool tileRangeBuff)
		{
			if (p1 == true)
			{
				player.dash = 0;
				if (Alchemist != null)
				{
					ModPlayer alchemistPlayer = player.GetModPlayer(Alchemist, "AlchemistNPCPlayer");
					Type alchemistPlayerType = alchemistPlayer.GetType();
					FieldInfo Blinker = alchemistPlayerType.GetField("Blinker", BindingFlags.Instance | BindingFlags.Public);
					Blinker.SetValue(alchemistPlayer, false);
				}
			}
		}
		public override void PostUpdateRunSpeeds()
		{
			if (p1 == true && p2 == false && player.velocity.X < 0.12f)
			{
				player.maxRunSpeed = 0f;
				player.runAcceleration = 0f;
			}
			else if (p1 == true && p2 == false)
			{
				if (player.runAcceleration >= 0.3f)
				{
					player.maxRunSpeed -= 0.6f;
					player.runAcceleration -= 0.26f;
				}
				else
				{
					player.maxRunSpeed = 0f;
					player.runAcceleration = 0f;
				}
			}
			else if (p1 == true && p2 == true)
			{
				player.maxRunSpeed = 0.4f;
				player.runAcceleration = 0.32f;
			}
		}
		public override void PreUpdateMovement()
		{
			if (p1 == true && p2 == false && player.velocity.X >= 0.12f)
			{
				player.velocity.X -= 0.12f;
				player.velocity.Y -= 0.16f;
			}
			else if (p1 == true && p2 == false)
			{
				player.velocity.X = 0f;
				player.velocity.Y = 0f;
			}
		}
		public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
		{
			D = player.position.Y/16;
			if (item.type == ItemID.LaserDrill && (int)D > 795)
			{				
				mult = (D-700)/112 + (D-790)/65 + 2.2f * (1 + (D-180)/200);
			}
			else if (item.type == ItemID.LaserDrill)
			{
				mult = 1.18f * (1 + (D-306)/340);
			}
			if (10 * mult >= player.statManaMax2)
			{
				p1 = true;
			}
		}
	}
	public class DrillItem : GlobalItem
	{
		public override bool InstancePerEntity => true;
		public override bool CloneNewInstances => true;
		public bool H = false;
		public override bool AltFunctionUse(Item item, Player player)
		{
			if (item.type == ItemID.LaserDrill)
			{
				return true;
			}
			return base.AltFunctionUse(item, player);
		}
		public override bool CanUseItem(Item item, Player player)
		{
			if (item.type == ItemID.LaserDrill)
			{
				if (player.GetModPlayer<DrillPlayer>().p2 == true)
				{
					return false;
				}
				else if (player.altFunctionUse == 2)
				{
					if (Main.mouseRightRelease)
					{
						if (H == false)
						{
							item.pick = 0;
							item.axe = 0;
							item.hammer = 100;
							item.mana = 0;
							Main.PlaySound(SoundID.Item4, player.Center);
							H = true;
							return false;
						}
						else if (H == true)
						{
							item.pick = 230;
							item.axe = 30;
							item.hammer = 0;
							item.mana = 7;
							Main.PlaySound(SoundID.Item4, player.Center);
							H = false;
							return false;
						}
					}
					return false;
				}
				return true;
			}
			return base.CanUseItem(item, player);
		}
		public override void SetDefaults(Item item)
		{
			if (item.type == ItemID.LaserDrill)
			{
				item.mana = 7;
			}
		}

	}
}