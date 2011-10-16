
namespace System.Windows
{
    public static class FreezableEx
    {
        public static T AsFreeze<T>(this T freezee) where T : Freezable
        {
            if (!freezee.IsFrozen && freezee.CanFreeze)
                freezee.Freeze();
            return freezee;
        }

        public static T CloneFreeze<T>(this T freezee) where T : Freezable
        {
            return (T)freezee.GetAsFrozen();
        }

        public static T CloneFreezeNew<T>(this T freezee) where T : Freezable
        {
            return (T)freezee.Clone().AsFreeze();
        }
    }
}
