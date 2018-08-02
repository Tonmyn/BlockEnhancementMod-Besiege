namespace BlockEnhancementMod.Tools
{
    public static class PrivateTools
    {
        ////得到私有字段的值
        //public static T GetPrivateField<T>(this object instance, string fieldname)
        //{
        //    BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
        //    Type type = instance.GetType();
        //    FieldInfo field = type.GetField(fieldname, flag);
        //    return (T)field.GetValue(instance);
        //}
        ////设置私有字段的值
        //public static void SetPrivateField(this object instance, string fieldname, object value)
        //{
        //    BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
        //    Type type = instance.GetType();
        //    FieldInfo field = type.GetField(fieldname, flag);
        //    field.SetValue(instance, value);
        //}
        ////得到私有属性的值
        //public static T GetPrivateProperty<T>(this object instance, string propertyname)
        //{
        //    BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
        //    Type type = instance.GetType();
        //    PropertyInfo field = type.GetProperty(propertyname, flag);
        //    return (T)field.GetValue(instance, null);
        //}

        ////设置私有属性的值： 
        //public static void SetPrivateProperty(this object instance, string propertyname, object value)
        //{
        //    BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
        //    Type type = instance.GetType();
        //    PropertyInfo field = type.GetProperty(propertyname, flag);
        //    field.SetValue(instance, value, null);
        //}
        ////调用私有方法：

        //public static T CallPrivateMethod<T>(this object instance, string name, params object[] param)
        //{
        //    BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic;
        //    Type type = instance.GetType();
        //    MethodInfo method = type.GetMethod(name, flag);
        //    return (T)method.Invoke(instance, param);
        //}
    }
}
