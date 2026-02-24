namespace ProjectZ.Editor
{
    public static class EditorBootstrap
    {
        public static EditorManager Create(ProjectZ.Game1 game)
            => new EditorManager(game);
    }
}