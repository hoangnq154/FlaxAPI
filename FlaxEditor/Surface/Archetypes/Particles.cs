// Copyright (c) 2012-2019 Wojciech Figat. All rights reserved.

using System;
using System.Linq;
using FlaxEditor.GUI;
using FlaxEngine;
using FlaxEngine.GUI;

namespace FlaxEditor.Surface.Archetypes
{
    /// <summary>
    /// Contains archetypes for nodes from the Particles group.
    /// </summary>
    public static class Particles
    {
        /// <summary>
        /// The particle value types.
        /// </summary>
        public enum ValueTypes
        {
            /// <summary>
            ///  <see cref="float"/>
            /// </summary>
            Float,

            /// <summary>
            /// <see cref="FlaxEngine.Vector2"/>
            /// </summary>
            Vector2,

            /// <summary>
            /// <see cref="FlaxEngine.Vector3"/>
            /// </summary>
            Vector3,

            /// <summary>
            /// <see cref="FlaxEngine.Vector4"/>
            /// </summary>
            Vector4,

            /// <summary>
            /// <see cref="int"/>
            /// </summary>
            Int,

            /// <summary>
            /// <see cref="uint"/>
            /// </summary>
            Uint,
        }

        /// <summary>
        /// Customized <see cref="SurfaceNode"/> for main particle emitter node.
        /// </summary>
        /// <seealso cref="FlaxEditor.Surface.SurfaceNode" />
        public class ParticleEmitterNode : SurfaceNode
        {
            /// <summary>
            /// The particle emitter modules set header (per context).
            /// </summary>
            /// <seealso cref="FlaxEngine.GUI.ContainerControl" />
            public class ModulesHeader : ContainerControl
            {
                private static readonly string[] Names =
                {
                    "Spawn",
                    "Initialize",
                    "Update",
                    "Render",
                };

                /// <summary>
                /// The header height.
                /// </summary>
                public const float HeaderHeight = FlaxEditor.Surface.Constants.NodeHeaderSize;

                /// <summary>
                /// Gets the type of the module.
                /// </summary>
                public ParticleModules.ModuleType ModuleType { get; }

                /// <summary>
                /// The add module button.
                /// </summary>
                public readonly Button AddModuleButton;

                /// <summary>
                /// Initializes a new instance of the <see cref="ModulesHeader"/> class.
                /// </summary>
                /// <param name="parent">The parent emitter node.</param>
                /// <param name="type">The module type.</param>
                public ModulesHeader(ParticleEmitterNode parent, ParticleModules.ModuleType type)
                {
                    ModuleType = type;
                    Parent = parent;
                    CanFocus = false;

                    float addButtonWidth = 80.0f;
                    float addButtonHeight = 16.0f;
                    AddModuleButton = new Button(Width * 0.5f, Height - addButtonHeight - 2.0f, addButtonWidth, addButtonHeight)
                    {
                        Text = "Add Module",
                        TooltipText = "Add new particle modules to the emitter",
                        AnchorStyle = AnchorStyle.BottomCenter,
                        Parent = this
                    };
                    AddModuleButton.ButtonClicked += OnAddModuleButtonClicked;
                }

                private void OnAddModuleButtonClicked(Button button)
                {
                    var modules = ParticleModules.Nodes.Where(x => (ParticleModules.ModuleType)x.DefaultValues[1] == ModuleType);

                    // Show context menu with list of module types to add
                    var cm = new ItemsListContextMenu(180);
                    foreach (var module in modules)
                    {
                        cm.AddItem(new ItemsListContextMenu.Item(module.Title, module.TypeID)
                        {
                            TooltipText = module.Description,
                        });
                    }
                    cm.ItemClicked += item => AddModule((ushort)item.Tag);
                    cm.SortChildren();
                    cm.Show(this, button.BottomLeft);
                }

                private void AddModule(ushort typeId)
                {
                    var parent = (SurfaceNode)Parent;
                    parent.Surface.Context.SpawnNode(15, typeId, Vector2.Zero);
                }

                /// <inheritdoc />
                public override void Draw()
                {
                    var style = Style.Current;
                    var backgroundRect = new Rectangle(Vector2.Zero, Size);
                    var mousePosition = ((SurfaceNode)Parent).MousePosition;
                    mousePosition = PointFromParent(ref mousePosition);

                    // Background
                    Render2D.FillRectangle(backgroundRect, style.BackgroundNormal);

                    // Header
                    var idx = (int)ModuleType;
                    var headerRect = new Rectangle(0, 0, Width, HeaderHeight);
                    var headerColor = style.BackgroundHighlighted;
                    if (headerRect.Contains(mousePosition))
                        headerColor *= 1.07f;
                    Render2D.FillRectangle(headerRect, headerColor);
                    Render2D.DrawText(style.FontLarge, Names[idx], headerRect, style.Foreground, TextAlignment.Center, TextAlignment.Center);

                    DrawChildren();
                }
            }

            /// <summary>
            /// Gets the particle emitter surface.
            /// </summary>
            public ParticleEmitterSurface ParticleSurface => (ParticleEmitterSurface)Surface;

            /// <summary>
            /// The particle modules sets headers (per context).
            /// </summary>
            public readonly ModulesHeader[] Headers = new ModulesHeader[4];

            /// <inheritdoc />
            public ParticleEmitterNode(uint id, VisjectSurfaceContext context, NodeArchetype nodeArch, GroupArchetype groupArch)
            : base(id, context, nodeArch, groupArch)
            {
                Headers[0] = new ModulesHeader(this, ParticleModules.ModuleType.Spawn);
                Headers[1] = new ModulesHeader(this, ParticleModules.ModuleType.Initialize);
                Headers[2] = new ModulesHeader(this, ParticleModules.ModuleType.Update);
                Headers[3] = new ModulesHeader(this, ParticleModules.ModuleType.Render);
            }

            /// <inheritdoc />
            public override void Draw()
            {
                var style = Style.Current;
                var backgroundRect = new Rectangle(Vector2.Zero, Size);

                // Background
                Render2D.FillRectangle(backgroundRect, style.BackgroundNormal);

                // Header
                var headerColor = style.BackgroundHighlighted;
                if (_headerRect.Contains(ref _mousePosition))
                    headerColor *= 1.07f;
                Render2D.FillRectangle(_headerRect, headerColor);
                Render2D.DrawText(style.FontLarge, Title, _headerRect, style.Foreground, TextAlignment.Center, TextAlignment.Center);

                DrawChildren();

                // Options border
                var optionsAreaStart = FlaxEditor.Surface.Constants.NodeHeaderSize + 3.0f;
                var optionsAreaHeight = 6 * FlaxEditor.Surface.Constants.LayoutOffsetY + 6.0f;
                Render2D.DrawRectangle(new Rectangle(1, optionsAreaStart, Width - 2, optionsAreaHeight), style.BackgroundSelected, 1.5f);

                // Selection outline
                if (_isSelected)
                {
                    backgroundRect.Expand(1.5f);
                    var colorTop = Color.Orange;
                    var colorBottom = Color.OrangeRed;
                    Render2D.DrawRectangle(backgroundRect, colorTop, colorTop, colorBottom, colorBottom, 1.5f);
                }
            }

            /// <inheritdoc />
            public override void OnSurfaceLoaded()
            {
                base.OnSurfaceLoaded();

                // Always keep root node in the back (modules with lay on top of it)
                IndexInParent = 0;

                // Link and update modules
                ParticleSurface._rootNode = this;
                ParticleSurface.ArrangeModulesNodes();
            }

            /// <inheritdoc />
            protected override void SetLocationInternal(ref Vector2 location)
            {
                base.SetLocationInternal(ref location);

                if (Surface != null && ParticleSurface._rootNode == this)
                {
                    // Update modules to match root node location
                    ParticleSurface.ArrangeModulesNodes();
                }
            }

            /// <inheritdoc />
            public override void Dispose()
            {
                // Unlink
                ParticleSurface._rootNode = null;

                base.Dispose();
            }
        }

        /// <summary>
        /// Customized <see cref="SurfaceNode"/> for particle data access node.
        /// </summary>
        /// <seealso cref="FlaxEditor.Surface.SurfaceNode" />
        public class ParticleAttributeNode : SurfaceNode
        {
            /// <inheritdoc />
            public ParticleAttributeNode(uint id, VisjectSurfaceContext context, NodeArchetype nodeArch, GroupArchetype groupArch)
            : base(id, context, nodeArch, groupArch)
            {
            }

            /// <inheritdoc />
            public override void OnSurfaceLoaded()
            {
                base.OnSurfaceLoaded();

                UpdateOutputBoxType();
            }

            /// <inheritdoc />
            public override void SetValue(int index, object value)
            {
                base.SetValue(index, value);

                // Update on type change
                if (index == 1)
                    UpdateOutputBoxType();
            }

            private void UpdateOutputBoxType()
            {
                ConnectionType type;
                switch ((ValueTypes)Values[1])
                {
                case ValueTypes.Float:
                    type = ConnectionType.Float;
                    break;
                case ValueTypes.Vector2:
                    type = ConnectionType.Vector2;
                    break;
                case ValueTypes.Vector3:
                    type = ConnectionType.Vector3;
                    break;
                case ValueTypes.Vector4:
                    type = ConnectionType.Vector4;
                    break;
                case ValueTypes.Int:
                    type = ConnectionType.Integer;
                    break;
                case ValueTypes.Uint:
                    type = ConnectionType.UnsignedInteger;
                    break;
                default: throw new ArgumentOutOfRangeException();
                }
                GetBox(0).CurrentType = type;
            }
        }

        /// <summary>
        /// The nodes for that group.
        /// </summary>
        public static NodeArchetype[] Nodes =
        {
            new NodeArchetype
            {
                TypeID = 1,
                Create = (id, context, arch, groupArch) => new ParticleEmitterNode(id, context, arch, groupArch),
                Title = "Particle Emitter",
                Description = "Main particle emitter node. Contains a set of modules per emitter context. Modules are executed in order from top to bottom of the stack.",
                Flags = NodeFlags.ParticleEmitterGraph | NodeFlags.NoRemove | NodeFlags.NoSpawnViaGUI | NodeFlags.NoCloseButton,
                Size = new Vector2(300, 600),
                DefaultValues = new object[]
                {
                    1024, // Capacity
                    (int)ParticlesSimulationMode.Default, // Simulation Mode
                    (int)ParticlesSimulationSpace.World, // Simulation Space
                    true, // Enable Pooling
                    BoundingBox.Zero, // Custom Bounds
                },
                Elements = new[]
                {
                    // Capacity
                    NodeElementArchetype.Factory.Text(0, 0, "Capacity", 100.0f, 16.0f, "The particle system capacity (the maximum amount of particles to simulate at once)."),
                    NodeElementArchetype.Factory.Integer(110, 0, 0, -1, 1),

                    // Simulation Mode
                    NodeElementArchetype.Factory.Text(0, 1 * Surface.Constants.LayoutOffsetY, "Simulation Mode", 100.0f, 16.0f, "The particles simulation execution mode."),
                    NodeElementArchetype.Factory.ComboBox(110, 1 * Surface.Constants.LayoutOffsetY, 80, 1, typeof(ParticlesSimulationMode)),

                    // Simulation Space
                    NodeElementArchetype.Factory.Text(0, 2 * Surface.Constants.LayoutOffsetY, "Simulation Space", 100.0f, 16.0f, "The particles simulation space."),
                    NodeElementArchetype.Factory.ComboBox(110, 2 * Surface.Constants.LayoutOffsetY, 80, 2, typeof(ParticlesSimulationSpace)),

                    // Enable Pooling
                    NodeElementArchetype.Factory.Text(0, 3 * Surface.Constants.LayoutOffsetY, "Enable Pooling", 100.0f, 16.0f, "True if enable pooling emitter instance data, otherwise immediately dispose. Pooling can improve performance and reduce memory usage."),
                    NodeElementArchetype.Factory.Bool(110, 3 * Surface.Constants.LayoutOffsetY, 3),

                    // Custom Bounds
                    NodeElementArchetype.Factory.Text(0, 4 * Surface.Constants.LayoutOffsetY, "Custom Bounds", 100.0f, 16.0f, "The custom bounds to use for the particles. Set to zero to use automatic bounds (valid only for CPU particles)."),
                    NodeElementArchetype.Factory.Box(110, 4 * Surface.Constants.LayoutOffsetY, 4),
                }
            },

            // Particle data access nodes
            new NodeArchetype
            {
                TypeID = 100,
                Create = (id, context, arch, groupArch) => new ParticleAttributeNode(id, context, arch, groupArch),
                Title = "Particle Attribute",
                Description = "Particle attribute data access node. Use it to read the particle data.",
                Flags = NodeFlags.MaterialGraph | NodeFlags.ParticleEmitterGraph,
                Size = new Vector2(200, 50),
                DefaultValues = new object[]
                {
                    "Color", // Name
                    (int)ValueTypes.Vector4, // ValueType
                },
                Elements = new[]
                {
                    NodeElementArchetype.Factory.TextBox(0, 0, 120, TextBox.DefaultHeight, 0, false),
                    NodeElementArchetype.Factory.ComboBox(0, Surface.Constants.LayoutOffsetY, 120, 1, typeof(ValueTypes)),
                    NodeElementArchetype.Factory.Output(0, string.Empty, ConnectionType.All, 0),
                }
            },
        };
    }
}
