using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.LookupAnything.Framework.Constants;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.LookupAnything.Framework.Targets
{
    /// <summary>Positional metadata about a wild tree.</summary>
    public class TreeTarget : GenericTarget
    {
        /*********
        ** Public methods
        *********/
        /// <summary>Construct an instance.</summary>
        /// <param name="obj">The underlying in-game object.</param>
        /// <param name="tilePosition">The object's tile position in the current location (if applicable).</param>
        public TreeTarget(Tree obj, Vector2? tilePosition = null)
            : base(TargetType.WildTree, obj, tilePosition) { }

        /// <summary>Get a rectangle which roughly bounds the visible sprite.</summary>
        /// <remarks>Reverse-engineered from <see cref="Tree.draw"/>.</remarks>
        public override Rectangle GetSpriteArea()
        {
            Rectangle tile = base.GetSpriteArea();
            Tree tree = (Tree)this.Value;
            Rectangle sprite = this.GetSourceRectangle(tree);

            int width = sprite.Width * Game1.pixelZoom;
            int height = sprite.Height * Game1.pixelZoom;
            int x = tile.X + (tile.Width / 2) - width / 2;
            int y = tile.Y + tile.Height - height;

            return new Rectangle(x, y, width, height);
        }

        /// <summary>Get whether the visible sprite intersects the specified coordinate. This can be an expensive test.</summary>
        /// <param name="tile">The tile to search.</param>
        /// <param name="position">The viewport-relative coordinates to search.</param>
        /// <param name="spriteArea">The approximate sprite area calculated by <see cref="GenericTarget.GetSpriteArea"/>.</param>
        /// <remarks>Reverse engineered from <see cref="Tree.draw"/>.</remarks>
        public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
        {
            // get tree
            Tree tree = (Tree)this.Value;
            WildTreeGrowthStage growth = (WildTreeGrowthStage)tree.growthStage;

            // get sprite data
            Texture2D spriteSheet = GameHelper.GetPrivateField<Texture2D>(tree, "texture");
            SpriteEffects spriteEffects = tree.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // check tree sprite
            if (this.SpriteIntersectsPixel(tile, position, spriteArea, spriteSheet, this.GetSourceRectangle(tree), spriteEffects))
                return true;

            // check stump attached to bottom of grown tree
            if (growth == WildTreeGrowthStage.Tree)
            {
                Rectangle stumpSpriteArea = new Rectangle(spriteArea.Center.X - (Tree.stumpSourceRect.Width / 2 * Game1.pixelZoom), spriteArea.Y + spriteArea.Height - Tree.stumpSourceRect.Height * Game1.pixelZoom, Tree.stumpSourceRect.Width * Game1.pixelZoom, Tree.stumpSourceRect.Height * Game1.pixelZoom);
                if (stumpSpriteArea.Contains((int)position.X, (int)position.Y) && this.SpriteIntersectsPixel(tile, position, stumpSpriteArea, spriteSheet, Tree.stumpSourceRect, spriteEffects))
                    return true;
            }

            return false;
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Get the sprite sheet's source rectangle for the displayed sprite.</summary>
        /// <remarks>Reverse-engineered from <see cref="Tree.draw"/>.</remarks>
        private Rectangle GetSourceRectangle(Tree tree)
        {
            // stump
            if (tree.stump)
                return Tree.stumpSourceRect;

            // growing tree
            if (tree.growthStage < 5)
            {
                switch ((WildTreeGrowthStage)tree.growthStage)
                {
                    case WildTreeGrowthStage.Seed:
                        return new Rectangle(32, 128, 16, 16);
                    case WildTreeGrowthStage.Sprout:
                        return new Rectangle(0, 128, 16, 16);
                    case WildTreeGrowthStage.Sapling:
                        return new Rectangle(16, 128, 16, 16);
                    default:
                        return new Rectangle(0, 96, 16, 32);
                }
            }

            // grown tree
            return Tree.treeTopSourceRect;
        }
    }
}