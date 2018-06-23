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
    /// A <see cref="Material"/> asset proxy object.
    /// </summary>
    /// <seealso cref="FlaxEditor.Content.BinaryAssetProxy" />
    public class MaterialProxy : BinaryAssetProxy
    {
        private MaterialPreview _preview;

        /// <inheritdoc />
        public override string Name => "Material";

        /// <inheritdoc />
        public override EditorWindow Open(Editor editor, ContentItem item)
        {
            return new MaterialWindow(editor, item as AssetItem);
        }

        /// <inheritdoc />
        public override Color AccentColor => Color.FromRGB(0x16a085);

        /// <inheritdoc />
        public override ContentDomain Domain => Material.Domain;

        /// <inheritdoc />
        public override Type AssetType => typeof(Material);

        /// <inheritdoc />
        public override bool CanCreate(ContentFolder targetLocation)
        {
            return targetLocation.CanHaveAssets;
        }

        /// <inheritdoc />
        public override void Create(string outputPath)
        {
            if (Editor.CreateAsset(Editor.NewAssetType.Material, outputPath))
                throw new Exception("Failed to create new asset.");
        }

        /// <inheritdoc />
        public override void OnContentWindowContextMenu(ContextMenu menu, ContentItem item)
        {
            base.OnContentWindowContextMenu(menu, item);

            if (item is BinaryAssetItem binaryAssetItem)
            {
                var button = menu.AddButton("Create Material Instance", CreateMaterialInstanceClicked);
                button.Tag = binaryAssetItem;
            }
        }

        private void CreateMaterialInstanceClicked(ContextMenuButton obj)
        {
            var binaryAssetItem = (BinaryAssetItem)obj.Tag;
            CreateMaterialInstance(binaryAssetItem);
        }

        /// <summary>
        /// Creates the material instance from the given material.
        /// </summary>
        /// <param name="materialItem">The material item to use as a base material.</param>
        public static void CreateMaterialInstance(BinaryAssetItem materialItem)
        {
            if (materialItem == null)
                throw new ArgumentNullException();
            if (materialItem.ItemDomain != ContentDomain.Material)
                throw new ArgumentException();

            var materialIntanceProxy = Editor.Instance.ContentDatabase.GetProxy<MaterialInstance>();
            Editor.Instance.Windows.ContentWin.NewItem(materialIntanceProxy, (item) => OnMaterialInstanceCreated(item, materialItem));
        }

        private static void OnMaterialInstanceCreated(ContentItem item, BinaryAssetItem materialItem)
        {
            var assetItem = (AssetItem)item;
            var materialInstance = FlaxEngine.Content.Load<MaterialInstance>(assetItem.ID);
            if (materialInstance == null || materialInstance.WaitForLoaded())
            {
                Editor.LogError("Failed to load created material instance.");
                return;
            }

            materialInstance.BaseMaterial = FlaxEngine.Content.LoadAsync<Material>(materialItem.ID);
            materialInstance.Save();
        }

        /// <inheritdoc />
        public override void OnThumbnailDrawPrepare(ThumbnailRequest request)
        {
            if (_preview == null)
            {
                _preview = new MaterialPreview(false);
                _preview.RenderOnlyWithWindow = false;
                _preview.Task.Enabled = false;
                _preview.PostFxVolume.Settings.Eye_Technique = EyeAdaptationTechnique.None;
                _preview.PostFxVolume.Settings.Eye_Exposure = 0.1f;
                _preview.PostFxVolume.Settings.data.Flags4 |= 0b1001;
                _preview.Size = new Vector2(PreviewsCache.AssetIconSize, PreviewsCache.AssetIconSize);
                _preview.SyncBackbufferSize();
            }

            // TODO: disable streaming for dependant assets during thumbnail rendering (and restore it after)
        }

        /// <inheritdoc />
        public override bool CanDrawThumbnail(ThumbnailRequest request)
        {
            return _preview.HasLoadedAssets;
        }

        /// <inheritdoc />
        public override void OnThumbnailDrawBegin(ThumbnailRequest request, ContainerControl guiRoot, GPUContext context)
        {
            _preview.Material = (Material)request.Asset;
            _preview.Parent = guiRoot;

            _preview.Task.Internal_Render(context);
        }

        /// <inheritdoc />
        public override void OnThumbnailDrawEnd(ThumbnailRequest request, ContainerControl guiRoot)
        {
            _preview.Material = null;
            _preview.Parent = null;
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            if (_preview != null)
            {
                _preview.Dispose();
                _preview = null;
            }

            base.Dispose();
        }
    }
}
