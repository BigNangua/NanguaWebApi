using System.Data;
using System.Reflection;

public static class DataRowExtensions
{
    /// <summary>
    /// 初始化数据库查询的对象，将未定义的值进行转换成对应类型的默认值
    /// </summary>
    /// <param name="row"></param>
    public static void ConvertNullsToDefaultValues(this DataRow row)
    {
        foreach (DataColumn column in row.Table.Columns)
        {
            if (row.IsNull(column))
            {
                if (column.DataType == typeof(string))
                {
                    row[column] = string.Empty;
                }
                else if (column.DataType == typeof(decimal))
                {
                    row[column] = default(decimal); // 或者你可以设置为其他默认值
                }
                // 你可以根据需要添加更多类型的处理
                else
                {
                    row[column] = Activator.CreateInstance(column.DataType);
                }
            }
        }
    }

    /// <summary>  
    /// 将Datatable转换为List集合  
    /// </summary>  
    /// <typeparam name="T">类型参数</typeparam>  
    /// <param name="dt">datatable表</param>  
    /// <returns></returns>  
    public static List<T> DataTableToList<T>(DataTable dt)
    {
        var list = new List<T>();
        Type t = typeof(T);
        var plist = new List<PropertyInfo>(typeof(T).GetProperties());

        foreach (DataRow item in dt.Rows)
        {
            T s = System.Activator.CreateInstance<T>();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                PropertyInfo info = plist.Find(p => p.Name == dt.Columns[i].ColumnName);
                if (info != null)
                {
                    if (!Convert.IsDBNull(item[i]))
                    {
                        info.SetValue(s, item[i], null);
                    }
                }
            }
            list.Add(s);
        }
        return list;
    }

}
