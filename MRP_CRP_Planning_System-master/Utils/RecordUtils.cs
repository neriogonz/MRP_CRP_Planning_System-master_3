namespace CRP_MRP_Planning_Web.Utils
{
    public static class RecordUtils
    {
        public static void CopyProperties<T>(T source, T target)
        {
            if (source == null || target == null)
            {
                return;
            }

            foreach (var property in source.GetType().GetProperties())
            {
                property.SetValue(target, property.GetValue(source));
            }
        }
    }
}
