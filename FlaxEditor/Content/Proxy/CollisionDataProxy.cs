// Copyright (c) 2012-2018 Wojciech Figat. All rights reserved.

using System;
using FlaxEditor.Content.Thumbnails;
using FlaxEditor.Viewport.Previews;
using FlaxEditor.Windows;
using FlaxEditor.Windows.Assets;
using FlaxEngine;
using FlaxEngine.GUI;
using FlaxEngine.Rendering;

namespace FlaxEditor.Content
{
    /// <summary>
    /// A <see cref="CollisionData"/> asset proxy object.
    /// </summary>
    /// <seealso cref="FlaxEditor.Content.BinaryAssetProxy" />
    public class CollisionDataProxy : BinaryAssetProxy
    {
        /// <inheritdoc />
        public override string Name => "Collision Data";

        /// <inheritdoc />
        public override EditorWindow Open(Editor editor, ContentItem item)
        {
            return new CollisionDataWindow(editor, item as AssetItem);
        }

        /// <inheritdoc />
        public override Color AccentColor => Color.FromRGB(0x2c3e50);

        /// <inheritdoc />
        public override ContentDomain Domain => CollisionData.Domain;

        /// <inheritdoc />
        public override Type AssetType => typeof(CollisionData);

        /// <inheritdoc />
        public override bool CanCreate(ContentFolder targetLocation)
        {
            return targetLocation.CanHaveAssets;
        }

        /// <inheritdoc />
        public override void Create(string outputPath)
        {
            if (Editor.CreateAsset(Editor.NewAssetType.CollisionData, outputPath))
                throw new Exception("Failed to create new asset.");
        }
    }
}
