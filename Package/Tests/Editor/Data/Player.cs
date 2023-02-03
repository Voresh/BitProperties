namespace Tests.Editor.Data {
    [BitProperty(typeof(bool), "Alive", 0, 0)]
    [BitProperty(typeof(int), "Team", 1, 2)]
    [BitProperty(typeof(bool), "Flag", 3, 3)]
    [BitProperty(typeof(int), "Health", 4, 7)]
    public partial class Player { }
}
