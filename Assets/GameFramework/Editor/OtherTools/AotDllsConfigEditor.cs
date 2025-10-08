namespace GameFramework.EditorTools
{
    [EditorToolMenu("AOT泛型补充配置", null, 3)]
    public class AotDllsConfigEditor : StripLinkConfigEditor
    {
        public override string ToolName => "AOT泛型补充配置";
        protected override void InitEditorMode()
        {
            this.SetEditorMode(ConfigEditorMode.AotDllConfig);
        }
    }
}
