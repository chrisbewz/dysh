sealed partial class Build
{
    T From<T>()
        where T : INukeBuild
        => (T)(object)this;
}