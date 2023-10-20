namespace SRParser.Helper;

public class PropertyExceptionChecker
{
    // 方法來檢查取得屬性的值是否會丟出異常
    public static bool TryGetPropertyValue(object instance, string propertyName, out object value)
    {
        // 初始化回傳值為 null
        value = null;

        // 確保實例和屬性名稱不是 null
        if (instance == null || string.IsNullOrEmpty(propertyName))
        {
            return false;
        }

        try
        {
            // 使用反射來嘗試取得屬性的值
            var propertyInfo = instance.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                return false; // 屬性不存在
            }

            value = propertyInfo.GetValue(instance);
            return true; // 成功取得屬性的值，沒有丟出異常
        }
        catch (Exception)
        {
            // 丟出異常，回傳 false
            return false;
        }
    }
}