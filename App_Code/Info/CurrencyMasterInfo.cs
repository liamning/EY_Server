/// <summary>
///     Resultset to be JSON stringified and set back to client.
/// </summary>

using System.Collections.Generic;
public class DataTableResultSet<T>
{
    /// <summary>Array of records. Each element of the array is itself an array of columns</summary>
    public List<T> data = new List<T>();

    /// <summary>value of draw parameter sent by client</summary>
    public int draw;

    /// <summary>filtered record count</summary>
    public int recordsFiltered;

    /// <summary>total record count in resultset</summary>
    public int recordsTotal;
     
}

 