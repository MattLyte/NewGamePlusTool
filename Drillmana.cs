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
		public int S = (int)Main.worldSurface;
		public int Cap1 = (int)((Main.rockLayer - Main.worldSurface) * 0.6);
		public int Cap2 = (int)(((Main.maxTilesY - Main.maxTilesY * 0.17 - Main.worldSurface) - (Main.rockLayer - Main.worldSurface)) * 0.1 + (Main.rockLayer - Main.worldSurface));
		public int Cap3 = (int)(((Main.maxTilesY - Main.maxTilesY * 0.17 - Main.worldSurface) - (Main.rockLayer - Main.worldSurface)) * 0.33 + (Main.rockLayer - Main.worldSurface));
		public int Cap4 = (int)(Main.maxTilesY - Main.maxTilesY * 0.17 - Main.worldSurface);
		public int t = 0;//use timer
		public int pt = 0;//disable timer
		public bool p1 = false;//tracks movement disable
		public bool p2 = false;//tracks drill disable
		public float D = 0f;//depth in tiles relative to surface
		public float C = 0f;//mana drain, ticks 3 times per second
		public override void PreUpdate()
		{
			if (p2 == true)//if player stopped using drill
			{
				pt++;
				if (pt >= 540)
				{
					p2 = false;
					Main.PlaySound(SoundID.Item9, player.Center);
					pt = 0;
				}
				else if (pt >= 200)
				{
					p1 = false;
				}
				
			}
			else if (p1 == true && p2 == false && (player.controlUseItem != true || player.HeldItem.type != ItemID.LaserDrill))//if mana 0 but still using drill
			{
				p2 = true;
				player.AddBuff (199, 540, false);
			}
			else if (NPC.downedPlantBoss == false && player.HeldItem.type == ItemID.LaserDrill && player.controlUseItem && p1 == false && p2 == false && player.altFunctionUse != 2 && player.HeldItem.GetGlobalItem<DrillItem>().H == false)//drain
			{
				t++;
				if (t >= 20)//cost ticks 3x per second
				{
					D = (player.position.Y/16 - S);
					if (D < 0)//scale cost from 5 at surface to 2 at pos.Y 0
					{
						C = 5 + (D / S) * 3;
					}
					else if (Main.hardMode)//scale to 23 at Cap4
					{
						C = 5 + (D / (Cap4 / 18));
					}
					else//scale to 32 at Cap4
					{
						C = 5 + (D / (Cap4 / 27));
					}
					if (player.statMana < (int)C)
					{
						p1 = true;
						Main.PlaySound(SoundID.Item122, player.Center);
					}
					else
					{
						player.statMana -= (int)C;
					}
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
			if (item.type == ItemID.LaserDrill)
			{
				if (NPC.downedPlantBoss)
				{
					mult = 0f;
				}
				else if (Main.hardMode)
				{
					D = (player.position.Y/16 - S);
					if (D < 0)
					{
						mult = 1.2f * (1 + D / (S + 1));
					}
					else if (D > Cap4 * 0.8f)
					{
						mult = 11.4f + 48.6f * ((D - Cap4 * 0.8f)/ (Cap4 * 0.2f));
					}
					else if (D > Cap4 * 0.55f)
					{
						mult = 3.1f + 8.3f * ((D - Cap4 * 0.55f) / (Cap4 * 0.25f));
					}
					else if (D > Cap4 * 0.3f)
					{
						mult = 1.8f + 1.3f * ((D - Cap4 * 0.3f) / (Cap4 * 0.25f));
					}
					else
					{
						mult = 1.2f + 0.6f * (D / (Cap4 * 0.3f));
					}
					if (10 * mult >= player.statManaMax2)
					{
						p1 = true;
					}
				}
				else if (NPC.downedBoss3)
				{
					D = (player.position.Y/16 - S);
					if (D < 0)
					{
						mult = 2.7f * (1 + D / (S + 1));
					}
					else if (D > Cap3 * 0.7f)
					{
						mult = 5.3f + 32.7f * ((D - Cap3 * 0.7f) / (Cap3 * 0.3f));
					}
					else if (D > Cap3 * 0.5f)
					{
						mult = 3.6f + 1.7f * ((D - Cap3 * 0.5f) / (Cap3 * 0.2f));
					}
					else
					{
						mult = 2.7f + 0.9f * (D / (Cap3 * 0.5f));
					}
					if (10 * mult >= player.statManaMax2)
					{
						p1 = true;
					}
				}
				else if (NPC.downedBoss1)
				{
					D = (player.position.Y/16 - S);
					if (D < 0)
					{
						mult = 4.4f * (1 + D / (S + 1));
					}
					else if (D > Cap2 * 0.55f)
					{
						mult = 5.8f + 19.2f * ((D - Cap2 * 0.55f)/ (Cap2 * 0.45f));
					}
					else
					{
						mult = 4.4f + 1.4f * (D / (Cap2 * 0.55f));
					}
					if (10 * mult >= player.statManaMax2)
					{
						p1 = true;
					}
				}
				else
				{
					D = (player.position.Y/16 - S);
					if (D < 0)
					{
						mult = 5.2f * (1 + D / (S + 1));
					}
					else
					{
						mult = 5.2f + 12.3f * (D / Cap1);
					}
					if (10 * mult >= player.statManaMax2)
					{
						p1 = true;
					}
				}
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