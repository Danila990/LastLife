namespace Ui.Sandbox.WorldSpaceUI
{
    public interface IWorldSpaceUIService
    {
        public T GetUI<T>(string key) where T : WorldSpaceUI;
    }
}