namespace TeamFlash
{
    public interface IBuildIndicator
    {
        void Reset();
        void Show(BuildStatus status);
    }
}
