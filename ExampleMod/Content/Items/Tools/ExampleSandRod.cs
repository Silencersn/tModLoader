using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ExampleMod.Content.Items.Tools
{
	// ExampleSandRod is a sand version of Dirt Rod
	// It can be used to move different sand blocks (including ExampleSand) around
	public class ExampleSandRod : ModItem
	{
		public override void SetStaticDefaults() {
			ItemID.Sets.DuplicationMenuToolsFilter[Type] = true;
		}

		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 28;

			// Copy from Dirt Rod
			Item.channel = true;
			Item.knockBack = 5f;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.UseSound = SoundID.Item8;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.rare = ItemRarityID.Blue;
			Item.noMelee = true;
			Item.value = Item.buyPrice(gold: 5);
		}

		public override bool CanUseItem(Player player) {
			if (player.whoAmI != Main.myPlayer)
				return false;

			// Calculate the tile position where the mouse is on
			Point tilePos = (Main.MouseScreen + Main.screenPosition).ToTileCoordinates();
			if (player.gravDir == -1f) {
				// If the screen is reversed, it is need to modify the Y of position
				tilePos.Y = (int)((Main.screenPosition.Y + Main.screenHeight - Main.mouseY) / 16);
			}
			Tile tile = Main.tile[tilePos];

			if (!tile.HasTile)
				return false;

			// If the tile is not sand, the item will not be used
			// This can be removed, if you want it not to move sand block only
			if (!Main.tileSand[tile.TileType])
				return false;

			// Get which projectile the tile will create when falling
			if (TileID.Sets.FallingBlockProjectile[tile.TileType] is not TileID.Sets.FallingBlockProjectileInfo data)
				return false;

			// Try to kill the tile whithout item dropping
			WorldGen.KillTile(tilePos.X, tilePos.Y, noItem: true);

			// Fail to kill tile
			if (Main.tile[tilePos].HasTile)
				return false;

			// If theh tile was kill successfully
			// set Item.shoot to data.FallingProjectileType to create the corresponding projectil of the killed tile
			Item.shoot = data.FallingProjectileType;

			// If it is on the multiplayer client, sync it to the server
			if (Main.netMode == NetmodeID.MultiplayerClient) {
				NetMessage.SendData(MessageID.TileManipulation, number: 4, number2: tilePos.X, number3: tilePos.Y);
			}
			
			return true;
		}

		public override bool CanShoot(Player player) {
			// Do not shoot if Item.shoot is ProjectileID.None
			return Item.shoot != ProjectileID.None;
		}

		public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) {
			// Make sure the spwan position of projectile is the same as mouse
			position = Main.MouseScreen + Main.screenPosition;
			if (player.gravDir == -1f) {
				position.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
			}
			player.LimitPointToPlayerReachableArea(ref position);
		}
	}
}
