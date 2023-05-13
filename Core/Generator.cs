using GenCL.Utilities;
using GenCL.Models;

namespace GenCL.Core
{
    public class Generator
    {
        public static void Generate(string path, string projectName)
        {
            try
            {
                string sql = @"If(OBJECT_ID('tempdb..#TempTable') Is Not Null)
                    Drop Table #TempTable

                SELECT TABLE_NAME
                INTO	#TempTable
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_TYPE='BASE TABLE'

                DECLARE @Result AS TABLE
                (
	                TableName VARCHAR(200),
	                Code NVARCHAR(MAX)
                )

                DECLARE @TableName VARCHAR(200);
                SET @TableName = (SELECT TOP 1 TABLE_NAME FROM #TempTable ORDER BY TABLE_NAME)
                SELECT * FROM #TempTable";

                var dt = DataService.FindList<Table>(sql);
                CheckDestination(path);
                foreach (var x in dt)
                {
                    var script = GetScript(x.TABLE_NAME, projectName);
                    if (script != null)
                    {
                        string fileName = $"{path}\\{script.TableName}.cs";
                        if (File.Exists(fileName))
                            File.Delete(fileName);

                        File.WriteAllText(fileName, script.Code);
                    }


                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private static void CheckDestination(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        private static Script GetScript(string tableName, string projectName)
        {
            var result = new Script();
            try
            {
                string sql = @"If(OBJECT_ID('tempdb..#ResultScript') Is Not Null)
                    Drop Table #ResultScript

declare @TableName sysname = @TableName_
declare @NameSpace varchar(100) = @Project + '.Models';
declare @Result varchar(max) = 'namespace ' + @namespace + CHAR(10) + '{ ' + CHAR(10) + CHAR(9) + 'public class ' + @TableName + '
' + CHAR(9) + '{'

select @Result =  @Result + CHAR(9) + '
    public ' + ColumnType + NullableSign + ' ' + ColumnName + ' { get; set; }' +CHAR(10) + '
'
from
(
    select 
        replace(col.name, ' ', '_') ColumnName,
        column_id ColumnId,
        case typ.name 
            when 'bigint' then 'long'
            when 'binary' then 'byte[]'
            when 'bit' then 'bool'
            when 'char' then 'string'
            when 'date' then 'DateTime'
            when 'datetime' then 'DateTime'
            when 'datetime2' then 'DateTime'
            when 'datetimeoffset' then 'DateTimeOffset'
            when 'decimal' then 'decimal'
            when 'float' then 'double'
            when 'image' then 'byte[]'
            when 'int' then 'int'
            when 'money' then 'decimal'
            when 'nchar' then 'string'
            when 'ntext' then 'string'
            when 'numeric' then 'decimal'
            when 'nvarchar' then 'string'
            when 'real' then 'float'
            when 'smalldatetime' then 'DateTime'
            when 'smallint' then 'short'
            when 'smallmoney' then 'decimal'
            when 'text' then 'string'
            when 'time' then 'TimeSpan'
            when 'timestamp' then 'long'
            when 'tinyint' then 'byte'
            when 'uniqueidentifier' then 'Guid'
            when 'varbinary' then 'byte[]'
            when 'varchar' then 'string'
            else 'UNKNOWN_' + typ.name
        end ColumnType,
        case 
            when col.is_nullable = 1 and typ.name in ('bigint', 'bit', 'date', 'datetime', 'datetime2', 'datetimeoffset', 'decimal', 'float', 'int', 'money', 'numeric', 'real', 'smalldatetime', 'smallint', 'smallmoney', 'time', 'tinyint', 'uniqueidentifier') 
            then '?' 
            else '' 
        end NullableSign
    from sys.columns col
        join sys.types typ on
            col.system_type_id = typ.system_type_id AND col.user_type_id = typ.user_type_id
    where object_id = object_id(@TableName)
) t
order by ColumnId

set @Result = @Result  + '
} '+ CHAR(10) +'}'


    CREATE TABLE #ResultScript 
                (
	                Code NVARCHAR(MAX)
                )

				INSERT INTO #ResultScript (Code) values (@Result)

				SELECT @TableName TableName,* from #resultscript";

                var param = new { TableName_ = tableName, Project = projectName };

                result = DataService.Find<Script>(sql, param);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
    }
}