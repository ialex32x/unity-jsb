using System.Reflection;

namespace QuickJS.Extra
{
    using QuickJS.Unity;
    using QuickJS.Binding;
    using UnityEngine;

    public class FairyGUICustomBinding : AbstractBindingProcess
    {
        public override string GetBindingProcessName()
        {
            return "FairyGUI";
        }
        
        public override void OnInitialize(BindingManager bindingManager)
        {
            ReflectBindValueOp.Register<FairyGUI.Margin>(Binding.Values.js_push_structvalue, Binding.Values.js_get_structvalue);
        }

        public override void OnPreCollectAssemblies(BindingManager bindingManager)
        {
            bindingManager.AddNamespaceBlacklist("WebSocketSharp");
        }

        public override void OnPreExporting(BindingManager bindingManager)
        {
            bindingManager.AddExportedType(typeof(FairyGUI.ProgressTitleType));
            bindingManager.AddExportedType(typeof(FairyGUI.PackageItemType));
            bindingManager.AddExportedType(typeof(FairyGUI.DestroyMethod));
            bindingManager.AddExportedType(typeof(FairyGUI.ObjectType));
            bindingManager.AddExportedType(typeof(FairyGUI.RelationType));
            bindingManager.AddExportedType(typeof(FairyGUI.GroupLayoutType));
            bindingManager.AddExportedType(typeof(FairyGUI.ChildrenRenderOrder));
            bindingManager.AddExportedType(typeof(FairyGUI.FlipType));
            bindingManager.AddExportedType(typeof(FairyGUI.FillMethod));
            bindingManager.AddExportedType(typeof(FairyGUI.ListLayoutType));
            bindingManager.AddExportedType(typeof(FairyGUI.CustomEase));
            bindingManager.AddExportedType(typeof(FairyGUI.GPathPoint));
            bindingManager.AddExportedType(typeof(FairyGUI.GPathPoint.CurveType));
            bindingManager.AddExportedType(typeof(FairyGUI.NTexture));
            bindingManager.AddExportedType(typeof(FairyGUI.Utils.ByteBuffer));
            bindingManager.AddExportedType(typeof(FairyGUI.OverflowType));
            bindingManager.AddExportedType(typeof(FairyGUI.FillType));
            bindingManager.AddExportedType(typeof(FairyGUI.PixelHitTestData));
            bindingManager.AddExportedType(typeof(FairyGUI.BitmapFont));
            bindingManager.AddExportedType(typeof(FairyGUI.BitmapFont.BMGlyph));
            bindingManager.AddExportedType(typeof(FairyGUI.TransitionActionType));
            bindingManager.AddExportedType(typeof(FairyGUI.AutoSizeType));
            bindingManager.AddExportedType(typeof(FairyGUI.TweenPropType));
            bindingManager.AddExportedType(typeof(FairyGUI.ScrollType));
            bindingManager.AddExportedType(typeof(FairyGUI.ScrollBarDisplayType));
            bindingManager.AddExportedType(typeof(FairyGUI.ButtonMode));
            bindingManager.AddExportedType(typeof(FairyGUI.AlignType));
            bindingManager.AddExportedType(typeof(FairyGUI.VertAlignType));
            bindingManager.AddExportedType(typeof(FairyGUI.OriginHorizontal));
            bindingManager.AddExportedType(typeof(FairyGUI.OriginVertical));
            bindingManager.AddExportedType(typeof(FairyGUI.ListSelectionMode));
            bindingManager.AddExportedType(typeof(FairyGUI.PopupDirection));
            bindingManager.AddExportedType(typeof(FairyGUI.NAudioClip));
            bindingManager.AddExportedType(typeof(FairyGUI.UIContentScaler));
            bindingManager.AddExportedType(typeof(FairyGUI.UIContentScaler.ScaleMode));
            bindingManager.AddExportedType(typeof(FairyGUI.UIContentScaler.ScreenMatchMode));
            bindingManager.AddExportedType(typeof(FairyGUI.EventContext));
            bindingManager.AddExportedType(typeof(FairyGUI.EventDispatcher));
            bindingManager.AddExportedType(typeof(FairyGUI.EventListener));
            bindingManager.AddExportedType(typeof(FairyGUI.GScrollBar));
            bindingManager.AddExportedType(typeof(FairyGUI.GPath));
            bindingManager.AddExportedType(typeof(FairyGUI.InputEvent));
            bindingManager.AddExportedType(typeof(FairyGUI.DisplayObject));
            bindingManager.AddExportedType(typeof(FairyGUI.Container))
                .AddTSMethodDeclaration("InvalidateBatchingState()");
            bindingManager.AddExportedType(typeof(FairyGUI.Stage));
            bindingManager.AddExportedType(typeof(FairyGUI.Controller));
            bindingManager.AddExportedType(typeof(FairyGUI.GObject));
            bindingManager.AddExportedType(typeof(FairyGUI.NGraphics));
            bindingManager.AddExportedType(typeof(FairyGUI.IFilter));
            bindingManager.AddExportedType(typeof(FairyGUI.IKeyboard));
            // bindingManager.AddExportedType(typeof(FairyGUI.UpdateContext));
            bindingManager.AddExportedType(typeof(FairyGUI.GGraph));
            bindingManager.AddExportedType(typeof(FairyGUI.Shape));
            bindingManager.AddExportedType(typeof(FairyGUI.GGroup));
            bindingManager.AddExportedType(typeof(FairyGUI.GImage));
            bindingManager.AddExportedType(typeof(FairyGUI.GLoader));
            bindingManager.AddExportedType(typeof(FairyGUI.GMovieClip));
            bindingManager.AddExportedType(typeof(FairyGUI.Utils.XMLList));
            bindingManager.AddExportedType(typeof(FairyGUI.Image));
            bindingManager.AddExportedType(typeof(FairyGUI.BlendMode));
            bindingManager.AddExportedType(typeof(FairyGUI.GearXY));
            bindingManager.AddExportedType(typeof(FairyGUI.GearLook));
            bindingManager.AddExportedType(typeof(FairyGUI.GearSize));
            bindingManager.AddExportedType(typeof(FairyGUI.GLoader3D));
            bindingManager.AddExportedType(typeof(FairyGUI.GTree));
            bindingManager.AddExportedType(typeof(FairyGUI.GTreeNode));
            bindingManager.AddExportedType(typeof(FairyGUI.MovieClip));
            bindingManager.AddExportedType(typeof(FairyGUI.MovieClip.Frame));
            bindingManager.AddExportedType(typeof(FairyGUI.TextFormat));
            bindingManager.AddExportedType(typeof(FairyGUI.GTextField));
            bindingManager.AddExportedType(typeof(FairyGUI.GRichTextField));
            bindingManager.AddExportedType(typeof(FairyGUI.GTextInput));
            bindingManager.AddExportedType(typeof(FairyGUI.GComponent))
                .AddTSMethodDeclaration("InvalidateBatchingState()");
            bindingManager.AddExportedType(typeof(FairyGUI.GList))
                .AddTSMethodDeclaration("RemoveChildAt(index: number): GObject");
            bindingManager.AddExportedType(typeof(FairyGUI.GRoot));
            bindingManager.AddExportedType(typeof(FairyGUI.GLabel));
            bindingManager.AddExportedType(typeof(FairyGUI.GButton));
            bindingManager.AddExportedType(typeof(FairyGUI.GComboBox));
            bindingManager.AddExportedType(typeof(FairyGUI.GProgressBar));
            bindingManager.AddExportedType(typeof(FairyGUI.GSlider));
            bindingManager.AddExportedType(typeof(FairyGUI.PopupMenu));
            bindingManager.AddExportedType(typeof(FairyGUI.ScrollPane));
            bindingManager.AddExportedType(typeof(FairyGUI.Transition));
            bindingManager.AddExportedType(typeof(FairyGUI.UIPackage));
            bindingManager.AddExportedType(typeof(FairyGUI.Window));
            bindingManager.AddExportedType(typeof(FairyGUI.GObjectPool));
            bindingManager.AddExportedType(typeof(FairyGUI.Relations));
            bindingManager.AddExportedType(typeof(FairyGUI.Timers));
            bindingManager.AddExportedType(typeof(FairyGUI.GTween));
            bindingManager.AddExportedType(typeof(FairyGUI.GTweener));
            bindingManager.AddExportedType(typeof(FairyGUI.EaseType));
            bindingManager.AddExportedType(typeof(FairyGUI.TweenValue));
            bindingManager.AddExportedType(typeof(FairyGUI.PackageItem));
            bindingManager.AddExportedType(typeof(FairyGUI.UIObjectFactory));
            bindingManager.AddExportedType(typeof(FairyGUI.MaterialManager));
            bindingManager.AddExportedType(typeof(FairyGUI.Margin));
            bindingManager.AddExportedType(typeof(FairyGUI.Utils.XML));
            bindingManager.AddExportedType(typeof(FairyGUI.TextFormat));
            bindingManager.AddExportedType(typeof(FairyGUI.TextFormat.SpecialStyle));

            bindingManager.AddExportedType(typeof(UnityEngine.AudioClip));
            bindingManager.AddExportedType(typeof(UnityEngine.AudioClipLoadType));
            bindingManager.AddExportedType(typeof(UnityEngine.AudioDataLoadState));
            bindingManager.AddExportedType(typeof(UnityEngine.AssetBundle));
            bindingManager.AddExportedType(typeof(UnityEngine.Sprite));
            bindingManager.AddExportedType(typeof(UnityEngine.Texture));
            bindingManager.AddExportedType(typeof(UnityEngine.Texture2D));
            bindingManager.AddExportedType(typeof(UnityEngine.Material));
            bindingManager.AddExportedType(typeof(UnityEngine.RenderMode));
            bindingManager.AddExportedType(typeof(UnityEngine.EventModifiers));
            bindingManager.AddExportedType(typeof(UnityEngine.KeyCode));

            bindingManager.AddExportedType(typeof(System.Collections.Generic.IList<FairyGUI.GObject>));
            bindingManager.AddExportedType(typeof(System.Collections.Generic.List<FairyGUI.PackageItem>));
            bindingManager.AddExportedType(typeof(System.Collections.Generic.List<FairyGUI.UIPackage>));
            bindingManager.AddExportedType(typeof(System.Collections.Generic.List<FairyGUI.GTreeNode>));
            bindingManager.AddExportedType(typeof(System.Collections.Generic.List<int>));
            bindingManager.AddExportedType(typeof(System.Collections.Generic.List<string>));
            bindingManager.AddExportedType(typeof(System.Collections.Generic.IEnumerable<FairyGUI.GPathPoint>));
            // bindingManager.AddExportedType(typeof(System.Collections.Generic.Dictionary<string, string>));
        }
    }
}