namespace ShadowVolume
{
    public static class Docs
    {
        public static class Tooltip
        {
            // General settings
            public const string AllowCameraInShadow =
                "Enable if a camera will be in the shadow of this game object. If enabled, the shadow volume must be rendered twice degrading " +
                "pixel fillrate performance in particular. For better performance, disable this property whenever your project allows for it. " +
                "Under the hood, the implementation uses a 2-pass workaround for the 1-pass depth-fail algorithm.";

            public const string RenderLayer =
                "The render layer used to render the shadow volumes. Make sure that this layer is not rendered by the currently active forward renderer.";

            public const string BoundsPadFactor =
                "The factor to expand the shadow mesh bounds by in order to prevent the shadow from disappearing when the game object is outside the camera " +
                "view frustum. The bounds will be expanded by this value times the magnitude of the size vector of the original bounds.";

            // Sharp Shadow component
            public const string ShadowAsset =
                "The shadow asset to use when rendering this shadow. This property is automatically set when a model is post-processed but may be empty " +
                "if the sharp shadow component was added manually.";

            public const string CreateRuntimeShadowAsset =
                "Enable this property if there is no build-time mesh to create a shadow mesh asset from. This is usually the case for procedurally generated " +
                "meshes. A runtime shadow asset will take precedence over the shadow asset property, if specified.";

            public const string RuntimeCreationSettings =
                "The settings to use when creating the runtime shadow asset";

            // Show Sharp Shadows render feature
            public const string Enabled = "Quick toggle for switching sharp shadows on or off";

            public const string ShadowVolumeRenderLayer = "The shadow asset render layer to visualize";

            public const string NearExtrusionDistance =
                "The amount to extrude the part of the shadow volume facing towards the light";

            public const string FarExtrusionDistance =
                "The amount to extrude the part of the shadow volume facing away from the light";

            public const string ShadowIntensity = "The shadow intensity";

            public const string ShadeMode =
                "The shade mode to use when drawing the sharp shadows. 'Inject Into Screen Space Shadow Resolve Texture' renders a scene depth pass and the " +
                "shadow volumes into the screen space shadow resolve texture of the Lightweight/Universal Render Pipeline. In this mode, the sharp shadows are integrated " +
                "into the render pipeline the same way the built-in shadows are, which makes them play nice with other graphics features such as light-mapping " +
                "and non-main lights. The downside is that an extra render target the size of the screen is used and the scene must be rendered twice. 'Multiply " +
                "Scene After Opaque' draws the shadow volumes after all opaque objects have been rendered by simply multiplying the shadow intensity with the " +
                "current color. This does not play nice other graphics features but works well for high-contrast art styles. The upside of this mode is that it " +
                "performs really well, especially on low-end devices.";

            public const string MitigateSelfShadowArtifacts =
                "Reduce self-shadowing artifacts using a clever trick. Can look odd on complex concave meshes when using the 'Multiply Scene After Opaque' shade mode " +
                "but improves image quality for most kinds of meshes. Does not degrade image quality when using the 'Inject Into Screen Space Shadow Resolve Texture' " +
                "shade mode.";
        }
    }
}