using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using System.Linq;

namespace replacefurnituremod
{
    public class playermod : ModPlayer
    {
        private static readonly ushort[] Tables = new ushort[] { TileID.Tables, TileID.Tables2, TileID.TinkerersWorkbench };
        private static readonly ushort[] Chests = new ushort[] { TileID.Containers, TileID.Containers2 };
        private static readonly ushort[] Anvils = new ushort[] { TileID.Anvils, TileID.MythrilAnvil };
        private static readonly ushort[] Furnaces = new ushort[] { TileID.Furnaces, TileID.Hellforge, TileID.AdamantiteForge };
        private static readonly ushort[] Signs = new ushort[] { TileID.Signs, TileID.AnnouncementBox };

        public static bool IsReplacement = false;

        private static int TileX = 0, TileY = 0;

        public override bool PreItemCheck()
        {
            Item item = player.inventory[player.selectedItem];
            if (item.type > 0 && item.createTile > -1 && player.controlUseItem && player.itemAnimation == 0)
            {
                TileX = (int)((Main.screenPosition.X + Main.mouseX) * (1f / 16));
                TileY = (int)((Main.screenPosition.Y + Main.mouseY) * (1f / 16));
                if(Main.tile[TileX, TileY].active())
                {
                    IsReplacement = true;
                }
            }
            return base.PreItemCheck();
        }

        public override void PostItemCheck()
        {
            if (!IsReplacement)
                return;
            Item item = player.inventory[player.selectedItem];
            if (item.type == 0 || item.createTile == -1 || player.itemAnimationMax < 1 || player.itemAnimation != player.itemAnimationMax - 1 || player.whoAmI != Main.myPlayer)
                return;
            //int TileX = (int)((Main.screenPosition.X + Main.mouseX) * (1f / 16));
            //int TileY = (int)((Main.screenPosition.Y + Main.mouseY) * (1f / 16));
            int CenterX = (int)(player.Center.X * (1f / 16)), CenterY = (int)(player.Center.Y * (1f / 16));
            if (Math.Abs(TileX - CenterX) < Player.tileRangeX + item.tileBoost + 1 &&
                Math.Abs(TileY - CenterY) < Player.tileRangeY + item.tileBoost + 1)
            {
                Tile tile = Main.tile[TileX, TileY];
                if (tile == null)
                    return;
                int TileAtPlacePosition = tile.active() ? tile.type : -1;
                int type = item.createTile;
                if (TileAtPlacePosition >= 0 && IsSameType(TileAtPlacePosition, type))
                {
                    ReplaceInWorld(TileX, TileY, item);
                }
            }
            IsReplacement = false;
        }

        private bool IsSameType(int TileType, int NewTileType)
        {
            switch (TileType)
            {
                case TileID.Chairs:
                case TileID.Thrones:
                case TileID.WorkBenches:
                case TileID.Benches:
                case TileID.Beds:
                case TileID.Chandeliers:
                case TileID.HangingLanterns:
                case TileID.Lamps:
                case TileID.Candles:
                case TileID.Candelabras:
                case TileID.Sinks:
                case TileID.Tombstones:
                case TileID.Pianos:
                    //case TileID.Dressers:
                    return TileType == NewTileType;
                case TileID.Tables:
                case TileID.Tables2:
                case TileID.TinkerersWorkbench:
                    return Tables.Contains((ushort)NewTileType);
                case TileID.Anvils:
                case TileID.MythrilAnvil:
                    return Anvils.Contains((ushort)NewTileType);
                case TileID.Furnaces:
                case TileID.Hellforge:
                case TileID.AdamantiteForge:
                    return Furnaces.Contains((ushort)NewTileType);
                /*case TileID.Containers:
                case TileID.Containers2:
                    return Chests.Contains((ushort)NewTileType);*/
                case TileID.Signs:
                case TileID.AnnouncementBox:
                    return Signs.Contains((ushort)NewTileType);
            }
            return false;
        }
        private void ReplaceInWorld(int x, int y, Item item)
        {
            Tile tile = Main.tile[x, y];
            if (tile == null)
                return;
            int TileAtPlacePosition = tile.active() ? tile.type : -1;
            int type = item.createTile, style = item.placeStyle;
            if (TileAtPlacePosition >= 0 && IsSameType(TileAtPlacePosition, type))
            {
                bool DepleteItem = false;
                //Do the replace script here
                switch (type)
                {
                    case TileID.Chandeliers: //Needs fixes.
                        {
                            short TileFrameX = (short)(tile.frameX % 54),
                                TileFrameY = (short)(tile.frameY % 54);
                            bool Lit = tile.frameX % 108 < 54;
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameX > 18)
                                PlaceX--;
                            if (TileFrameY >= 36)
                                PlaceY--;
                            if (TileFrameY >= 18)
                                PlaceY--;
                            WorldGen.KillTile(PlaceX, PlaceY);
                            WorldGen.PlaceTile(PlaceX, PlaceY, (ushort)type, style: style);
                            for(int x2 = -1; x2 < 2; x2++)
                            {
                                for(int y2 = 0; y2 < 3; y2++)
                                {
                                    tile = Main.tile[PlaceX + x2, PlaceY + y2];
                                    tile.frameX = (short)(tile.frameX - tile.frameX % 108 + 18 + 18 * x2 + (Lit ? 0 : 54));
                                }
                            }
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.HangingLanterns:
                        {
                            short TileFrameX = (short)(tile.frameX % 18),
                                TileFrameY = (short)(tile.frameY % 36);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameY >= 18)
                                PlaceY--;
                            bool Unlit = tile.frameX >= 18;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            for(int y2 = 0; y2 < 2; y2++)
                            {
                                Main.tile[TileFrameX, TileFrameY + y2].frameX = (short)(Unlit ? 18 : 0);
                            }
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Lamps:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 54);
                            bool Lit = tile.frameX < 18;
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameY < 18)
                                PlaceY++;
                            if (TileFrameY < 36)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            for(int y2 = -2; y2 < 1; y2++)
                            {
                                Main.tile[x, y + y2].frameX = (short)(Main.tile[x, y + y2].frameX - Main.tile[x, y + y2].frameX % 36 + (Lit ? 0 : 18));
                            }
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Candles:
                        {
                            short TileFrameX = (short)(tile.frameX % 36);
                            int PlaceX = x, PlaceY = y;
                            bool Lit = TileFrameX < 18;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            Main.tile[PlaceX, PlaceY].frameX = (short)(Main.tile[PlaceX, PlaceY].frameX - Main.tile[PlaceX, PlaceY].frameX % 18 + (Lit ? 0 : 18));
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Candelabras:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 36);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameY < 18)
                                PlaceY++;
                            bool Lit = tile.frameX < 36;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            for(int x2 = -1; x2 < 1; x2++)
                            {
                                for(int y2 = -1; y2 < 1; y2++)
                                {
                                    tile = Main.tile[PlaceX + x2, PlaceY + y2];
                                    tile.frameX = (short)(18 + x2 * 18 + (Lit ? 0 : 36));
                                }
                            }
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Chairs:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 40);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameY < 18)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            //Main.player[Main.myPlayer].direction = TileFrameX == 18 ? 1 : -1;
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            for (int y2 = -1; y2 < 1; y2++)
                            {
                                Main.tile[PlaceX, PlaceY + y2].frameX = TileFrameX;
                            }
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Thrones:
                        {
                            short TileFrameX = (short)(tile.frameX % 54),
                                TileFrameY = (short)(tile.frameY % 72);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameX > 18)
                                PlaceX--;
                            if (TileFrameY < 18)
                                PlaceY++;
                            if (TileFrameY < 36)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.WorkBenches:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 20);
                            int PlaceX = x, PlaceY = y;
                            //if (TileFrameX < 18)
                            //    PlaceX++;
                            if (TileFrameX >= 18)
                                PlaceX--;
                            //if (TileFrameY < 18)
                            //    PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Pianos:
                        {
                            short TileFrameX = (short)(tile.frameX % 54),
                                TileFrameY = (short)(tile.frameY % 36);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameX > 18)
                                PlaceX--;
                            if (TileFrameY < 18)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Tombstones:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 36);
                            int PlaceX = x, PlaceY = y;
                            //if (TileFrameX < 18)
                            //    PlaceX++;
                            if (TileFrameX >= 18)
                                PlaceX--;
                            if (TileFrameY < 18)
                                PlaceY++;
                            int SignPosition = Sign.ReadSign(PlaceX, PlaceY, false);
                            int SignXBackup = -1;
                            if (SignPosition != -1)
                            {
                                SignXBackup = Main.sign[SignPosition].x;
                                Main.sign[SignPosition].x = -1;
                            }
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            if (SignPosition != -1)
                                Main.sign[SignPosition].x = SignXBackup;
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Signs:
                    case TileID.AnnouncementBox:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 20);
                            int PlaceX = x, PlaceY = y;
                            byte PlacementOrientation = (byte)(tile.frameX / 36);
                            switch(PlacementOrientation)
                            {
                                default:
                                    if (TileFrameX >= 18)
                                        PlaceX--;
                                    if (TileFrameY < 18)
                                        PlaceY++;
                                    break;
                                case 1:
                                case 2:
                                case 4:
                                    if (TileFrameX >= 18)
                                        PlaceX--;
                                    if (TileFrameY >= 18)
                                        PlaceY--;
                                    break;
                                case 3:
                                    if (TileFrameX < 18)
                                        PlaceX++;
                                    if (TileFrameY >= 18)
                                        PlaceY--;
                                    break;
                            }
                            //if (TileFrameX < 18)
                            //    PlaceX++;
                            //if (TileFrameX >= 18)
                            //    PlaceX--;
                            //if (TileFrameY < 18)
                            //    PlaceY++;
                            int SignPosition = Sign.ReadSign(PlaceX, PlaceY, false);
                            int SignXBackup = -1;
                            if (SignPosition != -1)
                            {
                                SignXBackup = Main.sign[SignPosition].x;
                                Main.sign[SignPosition].x = -1;
                            }
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            if (SignPosition != -1)
                                Main.sign[SignPosition].x = SignXBackup;
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Sinks:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 38);
                            int PlaceX = x, PlaceY = y;
                            //if (TileFrameX < 18)
                            //    PlaceX++;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameY < 18)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Furnaces:
                    case TileID.Hellforge:
                    case TileID.AdamantiteForge:
                        {
                            short TileFrameX = (short)(tile.frameX % 54),
                                TileFrameY = (short)(tile.frameY % 38);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameX > 18)
                                PlaceX--;
                            if (TileFrameY < 18)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Anvils:
                    case TileID.MythrilAnvil:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 20);
                            int PlaceX = x, PlaceY = y;
                            //if (TileFrameX < 18)
                            //    PlaceX++;
                            if (TileFrameX >= 18)
                                PlaceX--;
                            //if (TileFrameY < 18)
                            //    PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Benches:
                        {
                            short TileFrameX = (short)(tile.frameX % 54),
                                TileFrameY = (short)(tile.frameY % 36);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameX > 18)
                                PlaceX--;
                            if (TileFrameY < 18)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Tables:
                    case TileID.Tables2:
                    case TileID.TinkerersWorkbench:
                        {
                            short TileFrameX = (short)(tile.frameX % 54),
                                TileFrameY = (short)(tile.frameY % 38);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameX > 18)
                                PlaceX--;
                            if (TileFrameY < 18)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Dressers: //bugged
                        {
                            short TileFrameX = (short)(tile.frameX % 54),
                                TileFrameY = (short)(tile.frameY % 36);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameX > 18)
                                PlaceX--;
                            if (TileFrameY < 18)
                                PlaceY++;
                            int ChestPos = Chest.FindChest(PlaceX - 1, PlaceY - 1), XBackup = -1;
                            if(ChestPos > -1)
                            {
                                XBackup = Main.chest[ChestPos].x;
                                Main.chest[ChestPos].x = -1;
                            }
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(PlaceX, PlaceY, true);
                            if (ChestPos > -1)
                            {
                                Main.chest[ChestPos].x = XBackup;
                            }
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Beds:
                        {
                            short TileFrameX = (short)(tile.frameX % 72),
                                TileFrameY = (short)(tile.frameY % 36);
                            bool FacingLeft = tile.frameX < 72;
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX < 18)
                                PlaceX++;
                            if (TileFrameX > 36)
                                PlaceX--;
                            if (TileFrameX > 18)
                                PlaceX--;
                            if (TileFrameY < 18)
                                PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            for (int x2 = -1; x2 < 3; x2++)
                            {
                                for (int y2 = -1; y2 < 1; y2++)
                                {
                                    Main.tile[PlaceX + x2, PlaceY + y2].frameX = (short)((FacingLeft ? 0 : 72) + 18 + 18 * x2);
                                }
                            }
                            WorldGen.SquareTileFrame(PlaceX, PlaceY, true);
                            DepleteItem = true;
                        }
                        break;
                    case TileID.Containers:
                    case TileID.Containers2:
                        {
                            short TileFrameX = (short)(tile.frameX % 36),
                                TileFrameY = (short)(tile.frameY % 38);
                            int PlaceX = x, PlaceY = y;
                            if (TileFrameX >= 18)
                                PlaceX--;
                            if (TileFrameY >= 18)
                                PlaceY--;
                            int ChestPosition = Chest.FindChest(PlaceX, PlaceY);
                            int ChestXBackup = -1;
                            if (ChestPosition > -1)
                            {
                                ChestXBackup = Main.chest[ChestPosition].x;
                                Main.chest[ChestPosition].x = -1;
                            }
                            PlaceY++;
                            WorldGen.KillTile(PlaceX, PlaceY, false, false, false);
                            if (ChestPosition > -1)
                            {
                                Main.chest[ChestPosition].x = ChestXBackup;
                            }
                            WorldGen.PlaceTile(PlaceX, PlaceY, type, style: style);
                            WorldGen.SquareTileFrame(x, y, true);
                            DepleteItem = true;
                        }
                        break;
                }
                if (DepleteItem)
                {
                    item.stack--;
                    if (item.stack <= 0)
                        item.SetDefaults(0);
                }
            }
        }
    }
}
