using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MonQ.Data.SqlClient
{
	/// <summary>
	/// TSQL类用于实现Microsoft SQL Server所支持的大部分函数，以帮助开发人员用于Linker查询。
	/// 这个类所有的成员只能用于Linker查询中的Lambda表达式，直接调用将抛出异常，若用于Linq to SQL也将抛出异常。
	/// 关于Linker查询请参考EX.Data.SqlClient.SqlQuery&lt;EntityType>
	/// </summary>
	public static class TSQL
	{
		/// <summary>
		/// 确定特定字符串是否与指定模式相匹配。虽然定义了返回值，但是此方法只能用于WHERE子句中，关于匹配模式请参考备注。
		/// </summary>
		/// <param name="match_expression">任何有效的字符数据类型的表达式。</param>
		/// <param name="pattern">要在 match_expression 中搜索并且可以包括下列有效通配符的特定字符串。pattern 的最大长度可达 8,000 字节。关于通配符请参考备注</param>
		/// <returns>bool</returns>
		/// <remarks>
		/// 模式可以包含常规字符和通配符。模式匹配过程中，常规字符必须与字符串中指定的字符完全匹配。但是，通配符可以与字符串的任意部分相匹配。与使用 = 和 != 字符串比较运算符相比，使用通配符可使 LIKE 运算符更加灵活。如果任何一个参数都不属于字符串数据类型，则 SQL Server 数据库引擎会将其转换为字符串数据类型（如果可能）。
		/// 
		/// %：包含零个或多个字符的任意字符串。
		/// WHERE title LIKE '%computer%' 将查找在书名中任意位置包含单词 "computer" 的所有书名。
		/// _（下划线）：任何单个字符。
		/// WHERE au_fname LIKE '_ean' 将查找以 ean 结尾的所有 4 个字母的名字（Dean、Sean 等）。
		/// 
		/// [ ]：指定范围 ([a-f]) 或集合 ([abcdef]) 中的任何单个字符。
		/// WHERE au_lname LIKE '[C-P]arsen' 将查找以 arsen 结尾并且以介于 C 与 P 之间的任何单个字符开始的作者姓氏，例如 Carsen、Larsen、Karsen 等。在范围搜索中，范围包含的字符可能因排序规则的排序规则而异。
		/// 
		/// [^]：不属于指定范围 ([a-f]) 或集合 ([abcdef]) 的任何单个字符。
		/// WHERE au_lname LIKE 'de[^l]%' 将查找以 de 开始并且其后的字母不为 l 的所有作者的姓氏。
		/// </remarks>
		[TSQLFormattor("{0} LIKE '%' + {1} + '%'")]
		public static bool LIKE(object match_expression, object pattern)
		{
			throw new NotSupportedException();
		}

		#region 字符串函数
		/// <summary>
		/// 返回字符表达式最左端字符的 ASCII 代码值。
		/// </summary>
		/// <param name="character_expression">是类型为 char 或 varchar的表达式。</param>
		/// <returns>int</returns>
		public static int ASCII(object character_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 根据 Unicode 标准所进行的定义，用给定整数代码返回 Unicode 字符。
		/// </summary>
		/// <param name="integer_expression">介于 0 与 65535 之间的所有正整数。如果指定了超出此范围的值，将返回 NULL。</param>
		/// <returns>nchar(1)</returns>
		public static string NCHAR(object integer_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回由四个字符组成的代码 (SOUNDEX) 以评估两个字符串的相似性。
		/// </summary>
		/// <param name="character_expression">是字符数据的字母数字表达式。character_expression 可以是常数、变量或列。</param>
		/// <returns>char</returns>
		/// <remarks>
		/// SOUNDEX 将 alpha 字符串转换成由四个字符组成的代码，以查找相似的词或名称。代码的第一个字符是 character_expression 的第一个字符，代码的第二个字符到第四个字符是数字。将忽略 character_expression 中的元音，除非它们是字符串的第一个字母。可以嵌套字符串函数。
		/// </remarks>
		public static string SOUNDEX(object character_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将 int ASCII 代码转换为字符的字符串函数。
		/// </summary>
		/// <param name="integer_expression">介于 0 和 255 之间的整数。如果整数表达式不在此范围内，将返回 NULL 值。</param>
		/// <returns>char(1)</returns>
		public static string CHAR(object integer_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中某模式第一次出现的起始位置；如果在全部有效的文本和字符数据类型中没有找到该模式，则返回零。
		/// </summary>
		/// <param name="pattern">一个字符串。可以使用通配符，但 pattern 之前和之后必须有 % 字符（搜索第一个和最后一个字符时除外）。pattern 是短字符数据类型类别的表达式。</param>
		/// <param name="expression">一个表达式，通常为要在其中搜索指定模式的列，expression 为字符串数据类型类别。</param>
		/// <returns>char</returns>
		public static int PATINDEX(object pattern, object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 由数字数据转换来的字符数据。
		/// </summary>
		/// <param name="float_expression">是带小数点的近似数字 (float) 数据类型的表达式。不要在 STR 函数中将函数或子查询用作 float_expression。</param>
		/// <param name="length">是总长度，包括小数点、符号、数字或空格。默认值为 10。</param>
		/// <param name="decimal">是小数点右边的位数。</param>
		/// <returns>char</returns>
		/// <remarks>
		/// 如果为 STR 提供 length 和 decimal 参数值，则这些值应该是正数。在默认情况下或者小数参数为 0 时，数字四舍五入为整数。指定长度应该大于或等于小数点前面的数字加上数字符号（若有）的长度。短的 float_expression 在指定长度内右对齐，长的 float_expression 则截断为指定的小数位数。例如，STR(12,10) 输出的结果是 12，在结果集内右对齐。而 STR(1223, 2) 则将结果集截断为 **。可以嵌套字符串函数。
		/// 说明  若要转换为 Unicode 数据，请在 CONVERT 或 CAST 转换函数内使用 STR。
		/// </remarks>
		public static string STR(object float_expression, object length, object @decimal)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 由数字数据转换来的字符数据。
		/// </summary>
		/// <param name="float_expression">是带小数点的近似数字 (float) 数据类型的表达式。不要在 STR 函数中将函数或子查询用作 float_expression。</param>
		/// <param name="length">是总长度，包括小数点、符号、数字或空格。默认值为 10。</param>
		/// <returns>int</returns>
		/// <remarks>
		/// 如果为 STR 提供 length 和 decimal 参数值，则这些值应该是正数。在默认情况下或者小数参数为 0 时，数字四舍五入为整数。指定长度应该大于或等于小数点前面的数字加上数字符号（若有）的长度。短的 float_expression 在指定长度内右对齐，长的 float_expression 则截断为指定的小数位数。例如，STR(12,10) 输出的结果是 12，在结果集内右对齐。而 STR(1223, 2) 则将结果集截断为 **。可以嵌套字符串函数。
		/// 说明  若要转换为 Unicode 数据，请在 CONVERT 或 CAST 转换函数内使用 STR。
		/// </remarks>
		public static string STR(object float_expression, object length)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 由数字数据转换来的字符数据。
		/// </summary>
		/// <param name="float_expression">是带小数点的近似数字 (float) 数据类型的表达式。不要在 STR 函数中将函数或子查询用作 float_expression。</param>
		/// <returns>char</returns>
		/// <remarks>
		/// 如果为 STR 提供 length 和 decimal 参数值，则这些值应该是正数。在默认情况下或者小数参数为 0 时，数字四舍五入为整数。指定长度应该大于或等于小数点前面的数字加上数字符号（若有）的长度。短的 float_expression 在指定长度内右对齐，长的 float_expression 则截断为指定的小数位数。例如，STR(12,10) 输出的结果是 12，在结果集内右对齐。而 STR(1223, 2) 则将结果集截断为 **。可以嵌套字符串函数。
		/// 说明  若要转换为 Unicode 数据，请在 CONVERT 或 CAST 转换函数内使用 STR。
		/// </remarks>
		public static string STR(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 以整数返回两个字符表达式的 SOUNDEX 值之差。 
		/// </summary>
		/// <param name="character_expression1">是类型 char 或 varchar 的表达式。</param>
		/// <param name="character_expression2">是类型 char 或 varchar 的表达式。</param>
		/// <returns>int</returns>
		/// <remarks>
		/// 返回的整数是 SOUNDEX 值中相同字符的个数。返回的值从 0 到 4 不等，4 表示 SOUNDEX 值相同。
		/// </remarks>
		public static int DIFFERENCE(object character_expression1, object character_expression2)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回带有分隔符的 Unicode 字符串，分隔符的加入可使输入的字符串成为有效的 Microsoft® SQL Server™ 分隔标识符。
		/// </summary>
		/// <param name="character_string">Unicode 字符数据字符串。character_string 是 sysname 值。</param>
		/// <param name="quote_character">用作分隔符的单字符字符串。可以是单引号 (')、左括号或右括号 ([]) 或者双引号 (")。如果未指定 quote_character，则使用括号。</param>
		/// <returns>nvarchar(129)</returns>
		public static string QUOTENAME(object character_string, object quote_character)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回带有分隔符的 Unicode 字符串，分隔符的加入可使输入的字符串成为有效的 Microsoft® SQL Server™ 分隔标识符。
		/// </summary>
		/// <param name="character_expression">由字符数据组成的表达式。character_expression 可以是常量、变量，也可以是字符或二进制数据的列。</param>
		/// <param name="start">是一个整形值，指定删除和插入的开始位置。如果 start 或 length 是负数，则返回空字符串。如果 start 比第一个 character_expression 长，则返回空字符串。</param>
		/// <param name="length">是一个整数，指定要删除的字符数。如果 length 比第一个 character_expression 长，则最多删除到最后一个 character_expression 中的最后一个字符。</param>
		/// <param name="character_expression2">用作分隔符的单字符字符串。可以是单引号 (')、左括号或右括号 ([]) 或者双引号 (")。如果未指定 quote_character，则使用括号。</param>
		/// <returns>如果 character_expression 是一个支持的字符数据类型，则返回字符数据。如果 character_expression 是一个支持的 binary 数据类型，则返回二进制数据。</returns>
		/// <remarks>
		/// 可以嵌套字符串函数。
		/// </remarks>
		public static string STUFF(object character_expression, object start, object length, object character_expression2)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回由重复的空格组成的字符串。
		/// </summary>
		/// <param name="integer_expression">是表示空格个数的正整数。如果 integer_expression 为负，则返回空字符串。</param>
		/// <returns>
		/// char
		/// </returns>
		/// <remarks>
		/// 若要在 Unicode 数据中包括空格，请使用 REPLICATE 而非 SPACE。
		/// </remarks>
		public static string SPACE(object integer_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回带有分隔符的 Unicode 字符串，分隔符的加入可使输入的字符串成为有效的 Microsoft® SQL Server™ 分隔标识符。
		/// </summary>
		/// <param name="character_expression">字符或二进制数据表达式。character_expression 可以是常量、变量或列。character_expression 必须是可以隐式地转换为 varchar 的数据类型。否则，请使用 CAST 函数显式转换 character_expression。</param>
		/// <param name="integer_expression">是正整数。如果 integer_expression 为负，则返回空字符串。</param>
		/// <returns>varchar</returns>
		/// <remarks>
		/// 兼容级别可能影响返回值。有关兼容级别的更多信息，请参见 sp_dbcmptlevel。 
		/// </remarks>
		public static string LEFT(object character_expression, object integer_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 以指定的次数重复字符表达式。
		/// </summary>
		/// <param name="character_expression">由字符数据组成的字母数字表达式。character_expression 可以是常量或变量，也可以是字符列或二进制数据列。</param>
		/// <param name="integer_expression">是正整数。如果 integer_expression 为负，则返回空字符串。</param>
		/// <returns>varchar
		/// character_expression 必须为可隐性转换为 varchar 的数据类型。否则，使用 CAST 函数显式转换 character_expression。
		/// </returns>
		/// <remarks>
		/// 兼容级别可能影响返回值。有关兼容级别的更多信息，请参见 sp_dbcmptlevel。 
		/// </remarks>
		public static string REPLICATE(object character_expression, object integer_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回字符、binary、text 或 image 表达式的一部分。有关可与该函数一起使用的有效 Microsoft® SQL Server™ 数据类型的更多信息，请参见数据类型。
		/// </summary>
		/// <param name="expression">是字符串、二进制字符串、text、image、列或包含列的表达式。不要使用包含聚合函数的表达式。</param>
		/// <param name="start">是一个整数，指定子串的开始位置。</param>
		/// <param name="length">是一个整数，指定子串的长度（要返回的字符数或字节数）。</param>
		/// <returns>varchar
		/// 是一个整数，指定子串的长度（要返回的字符数或字节数）。
		/// </returns>
		/// <remarks>
		/// 由于在 text 数据上使用 SUBSTRING 时 start 和 length 指定字节数，因此 DBCS 数据（如日本汉字）可能导致在结果的开始或结束位置拆分字符。此行为与 READTEXT 处理 DBCS 的方式一致。然而，由于偶而会出现奇怪的结果，建议对 DBCS 字符使用 ntext 而非 text。
		/// 在字符数中必须指定使用 ntext、char 或 varchar 数据类型的偏移量（start 和 length）。在字节数中必须指定使用 text、image、binary 或 varbinary 数据类型的偏移量。 
		/// 说明  兼容级别可能影响返回值。有关兼容级别的更多信息，请参见 sp_dbcmptlevel。 
		/// </remarks>
		public static string SUBSTRING(object expression, object start, object length)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回给定字符串表达式的字符（而不是字节）个数，其中不包含尾随空格。
		/// </summary>
		/// <param name="string_expression">返回给定字符串表达式的字符（而不是字节）个数，其中不包含尾随空格。</param>
		/// <returns>
		/// int
		/// </returns>
		public static int LEN(object string_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回字符表达式的反转。
		/// </summary>
		/// <param name="string_expression">由字符数据组成的表达式。character_expression 可以是常量、变量，也可以是字符或二进制数据的列。</param>
		/// <returns>varchar</returns>
		/// <remarks>
		/// character_expression 必须为可隐性转换为 varchar 的数据类型。否则，使用 CAST 显式转换 character_expression。
		/// </remarks>
		public static string REVERSE(object string_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 按照 Unicode 标准的定义，返回输入表达式的第一个字符的整数值。
		/// </summary>
		/// <param name="ncharacter_expression">是 nchar 或 nvarchar 表达式。</param>
		/// <returns>int</returns>
		public static int UNICODE(object ncharacter_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将大写字符数据转换为小写字符数据后返回字符表达式。
		/// </summary>
		/// <param name="character_expression">是字符或二进制数据表达式。character_expression 可以是常量、变量或列。character_expression 必须是可以隐性转换为 varchar 的数据类型。否则，使用 CAST 显式转换 character_expression。</param>
		/// <returns>varchar</returns>
		public static string LOWER(object character_expression)
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// 返回字符串中从右边开始指定个数的 integer_expression 字符。
		/// </summary>
		/// <param name="character_expression">由字符数据组成的表达式。character_expression 可以是常量、变量，也可以是字符或二进制数据的列。</param>
		/// <param name="integer_expression">是起始位置，用正整数表示。如果 integer_expression 是负数，则返回一个错误。</param>
		/// <returns>varchar</returns>
		/// <remarks>
		/// character_expression 必须为可隐性转换为 varchar 的数据类型。否则，使用 CAST 显式转换 character_expression。
		/// </remarks>
		public static string RIGHT(object character_expression, object integer_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回将小写字符数据转换为大写的字符表达式。
		/// </summary>
		/// <param name="character_expression">是字符或二进制数据表达式。character_expression 可以是常量、变量或列。character_expression 必须是可以隐性转换为 varchar 的数据类型。否则，使用 CAST 显式转换 character_expression。</param>
		/// <returns>varchar</returns>
		public static string UPPER(object character_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 删除起始空格后返回字符表达式。
		/// </summary>
		/// <param name="character_expression">是字符或二进制数据表达式。character_expression 可以是常量、变量或列。character_expression 必须是可以隐性转换为 varchar 的数据类型。否则，使用 CAST 显式转换 character_expression。</param>
		/// <returns>varchar</returns>
		public static string LTRIM(object character_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 截断所有尾随空格后返回一个字符串。
		/// </summary>
		/// <param name="character_expression">是字符或二进制数据表达式。character_expression 可以是常量、变量或列。character_expression 必须是可以隐性转换为 varchar 的数据类型。否则，使用 CAST 显式转换 character_expression。</param>
		/// <returns>varchar</returns>
		public static string RTRIM(object character_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 用第三个表达式替换第一个字符串表达式中出现的所有第二个给定字符串表达式。
		/// </summary>
		/// <param name="string_expression1">待搜索的字符串表达式。可以是字符数据或二进制数据。</param>
		/// <param name="string_expression2">待查找的字符串表达式。可以是字符数据或二进制数据。</param>
		/// <param name="string_expression3">替换用的字符串表达式。可以是字符数据或二进制数据。</param>
		/// <returns>
		/// 如果 string_expression（1、2 或 3）是支持的字符数据类型之一，则返回字符数据。如果 string_expression（1、2 或 3）是支持的 binary 数据类型之一，则返回二进制数据。
		/// </returns>
		public static string REPLACE(object string_expression1, object string_expression2, object string_expression3)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region 数学函数
		/// <summary>
		/// 返回指定数值表达式的绝对值（正值）的数学函数。
		/// </summary>
		/// <param name="numeric_expression">精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。</param>
		/// <returns>返回与 numeric_expression 相同的类型。</returns>
		public static int ABS(object numeric_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回以弧度指定的角的相应角度。
		/// </summary>
		/// <param name="numeric_expression">精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。</param>
		/// <returns>float</returns>
		public static float DEGREES(object numeric_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回从 0 到 1 之间的随机 float 值。
		/// </summary>
		/// <param name="seed">提供种子值的整数表达式（tinyint、smallint 或 int）。如果未指定 seed，则 Microsoft SQL Server 数据库引擎 随机分配种子值。对于指定的种子值，返回的结果始终相同。</param>
		/// <returns>float</returns>
		public static float RAND(object seed)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 数学函数，返回其余弦是所指定的 float 表达式的角（弧度）；也称为反余弦。
		/// </summary>
		/// <param name="float_expression">类型为 float 或类型可以隐式转换为 float 的表达式，取值范围从 -1 到 1。对超过此范围的值，函数将返回 NULL 并报告域错误。</param>
		/// <returns>float</returns>
		public static float ACOS(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定的 float 表达式的指数值。
		/// </summary>
		/// <param name="float_expression">float 类型或能隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		public static float EXP(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回一个数值，舍入到指定的长度或精度。
		/// </summary>
		/// <param name="numeric_expression">精确数值或近似数值数据类别（bit 数据类型除外）的表达式。</param>
		/// <param name="length">numeric_expression 的舍入精度。length 必须是 tinyint、smallint 或 int 类型的表达式。如果 length 为正数，则将 numeric_expression 舍入到 length 指定的小数位数。如果 length 为负数，则将 numeric_expression 小数点左边部分舍入到 length 指定的长度。</param>
		/// <param name="function">要执行的操作的类型。function 必须为 tinyint、smallint 或 int。如果省略 function 或其值为 0（默认值），则将舍入 numeric_expression。如果指定了 0 以外的值，则将截断 numeric_expression。</param>
		/// <returns>返回与 numeric_expression 相同的类型。</returns>
		/// <remarks>
		/// ROUND 始终返回一个值。如果 length 为负数，并且大于小数点前的数字个数，则 ROUND 将返回 0。
		/// </remarks>
		public static float ROUND(object numeric_expression, object length, object function)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回以弧度表示的角，其正弦为指定 float 表达式。也称为反正弦。
		/// </summary>
		/// <param name="float_expression">类型 float 或可隐式转换为 float 的类型的表达式，其取值范围从 -1 到 1。对超过此范围的值，函数将返回 NULL 并且报告域错误。</param>
		/// <returns>float</returns>
		public static float ASIN(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回小于或等于指定数值表达式的最大整数。
		/// </summary>
		/// <param name="numeric_expression">精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。</param>
		/// <returns>返回与 numeric_expression 相同的类型。</returns>
		public static int FLOOR(object numeric_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式的正号 (+1)、零 (0) 或负号 (-1)。
		/// </summary>
		/// <param name="numeric_expression">精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。</param>
		/// <returns>正号 (+1)、零 (0) 或负号 (-1)。</returns>
		public static int SIGN(object numeric_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回以弧度表示的角，其正切为指定的 float 表达式。它也称为反正切函数。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		public static float ATAN(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定 float 表达式的自然对数。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		public static float LOG(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 以近似数字 (float) 表达式返回指定角度（以弧度为单位）的三角正弦值。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		public static float SIN(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回以弧度表示的角，该角位于正 X 轴和原点至点 (y, x) 的射线之间，其中 x 和 y 是两个指定的浮点表达式的值。
		/// </summary>
		/// <param name="float_expression1">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <param name="float_expression2">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		public static float ATN2(object float_expression1, object float_expression2)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定 float 表达式的常用对数（即：以 10 为底的对数）。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		/// <remarks>LOG10 和 POWER 函数彼此反向相关。例如，10 ^ LOG10(n) = n。</remarks>
		public static float LOG10(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定浮点值的平方根。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		public static float SQRT(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回大于或等于指定数值表达式的最小整数。
		/// </summary>
		/// <param name="numeric_expression">精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。</param>
		/// <returns>返回与 numeric_expression 相同的类型。</returns>
		public static int CEILING(object numeric_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回 PI 的常量值。
		/// </summary>
		/// <returns>3.14159265358979</returns>
		public static float PI()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定浮点值的平方。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		public static float SQUARE(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 一个数学函数，返回指定表达式中以弧度表示的指定角的三角余弦。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>float</returns>
		public static float COS(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式的指定幂的值。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <param name="y">对 float_expression 进行幂运算的幂值。y 可以是精确数值或近似数值数据类别的表达式（bit 数据类型除外）。</param>
		/// <returns>与 float_expression 相同。</returns>
		public static float COS(object float_expression, object y)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式的指定幂的值。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <param name="y">对 float_expression 进行幂运算的幂值。y 可以是精确数值或近似数值数据类别的表达式（bit 数据类型除外）。</param>
		/// <returns>与 float_expression 相同。</returns>
		public static float POWER(object float_expression, object y)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回输入表达式的正切值。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <param name="y">对 float_expression 进行幂运算的幂值。y 可以是精确数值或近似数值数据类别的表达式（bit 数据类型除外）。</param>
		/// <returns>与 float_expression 相同。</returns>
		public static float TAN(object float_expression, object y)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回输入表达式的正切值。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>与 float_expression 相同。</returns>
		public static float TAN(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 一个数学函数，返回指定的 float 表达式中所指定角度（以弧度为单位）的三角余切值。
		/// </summary>
		/// <param name="float_expression">float 类型或可以隐式转换为 float 类型的表达式。</param>
		/// <returns>与 float_expression 相同。</returns>
		public static float COT(object float_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 对于在数值表达式中输入的度数值返回弧度值。
		/// </summary>
		/// <param name="numeric_expression">是精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。</param>
		/// <returns>与 numeric_expression 相同。</returns>
		public static float RADIANS(object numeric_expression)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region 日期和时间函数
		/// <summary>
		/// 返回包含计算机的日期和时间的 datetime2(7) 值，SQL Server 的实例正在该计算机上运行。
		/// </summary>
		/// <returns>返回包含计算机的日期和时间的 datetime2(7) 值，SQL Server 的实例正在该计算机上运行。时区偏移量未包含在内。</returns>
		public static DateTime SYSDATETIME()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回包含计算机的日期和时间的 datetimeoffset(7) 值，SQL Server 的实例正在该计算机上运行。时区偏移量包含在内。
		/// </summary>
		/// <returns>返回包含计算机的日期和时间的 datetimeoffset(7) 值，SQL Server 的实例正在该计算机上运行。时区偏移量包含在内。</returns>
		public static DateTime SYSDATETIMEOFFSET()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回包含计算机的日期和时间的 datetime2 值，SQL Server 的实例正在该计算机上运行。日期和时间作为 UTC 时间（通用协调时间）返回。秒部分精度规范的范围为 1 至 7 位。默认精度为 7 位数。
		/// </summary>
		/// <returns>返回包含计算机的日期和时间的 datetime2(7) 值，SQL Server 的实例正在该计算机上运行。日期和时间作为 UTC 时间（通用协调时间）返回。</returns>
		public static DateTime SYSUTCDATETIME()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回当前数据库系统时间戳，返回值的类型为 datetime，并且不含数据库时区偏移量。 此值得自运行 SQL Server 实例的计算机的操作系统。
		/// </summary>
		/// <returns>返回包含计算机的日期和时间的 datetime2(7) 值，SQL Server 的实例正在该计算机上运行。时区偏移量未包含在内。</returns>
		/// <remarks>
		/// 此函数是 ANSI SQL，等价于 GETDATE。
		/// </remarks>
		public static DateTime CURRENT_TIMESTAMP
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回包含计算机的日期和时间的 datetime2 值，SQL Server 的实例正在该计算机上运行。日期和时间作为 UTC 时间（通用协调时间）返回。秒部分精度规范的范围为 1 至 7 位。默认精度为 7 位数。
		/// </summary>
		/// <returns>返回包含计算机的日期和时间的 datetime2(7) 值，SQL Server 的实例正在该计算机上运行。时区偏移量未包含在内。</returns>
		public static DateTime GETDATE()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 以 datetime 值的形式返回当前数据库系统的时间戳。 数据库时区偏移量未包含在内。 此值表示当前的 UTC 时间（协调世界时）。 此值得自运行 SQL Server 实例的计算机的操作系统。
		/// </summary>
		/// <returns>返回包含计算机的日期和时间的 datetime2(7) 值，SQL Server 的实例正在该计算机上运行。日期和时间作为 UTC 时间（通用协调时间）返回。</returns>
		public static DateTime GETUTCDATE()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表示指定 date 的指定 datepart 的字符串
		/// </summary>
		/// <param name="datepart">返回的 date 的哪部分。TSQL已经将可选参数封装为熟悉，例如TSQL.year，请勿与YEAR()方法混淆</param>
		/// <param name="date">是一个表达式，可以解析为 time、date、smalldatetime、datetime、datetime2 或 datetimeoffset 值。date 可以是表达式、列表达式、用户定义的变量或字符串文字。</param>
		/// <returns>返回表示指定日期的指定 datepart 的字符串。</returns>
		public static string DATENAME(object datepart, object date)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表示指定 date 的指定 datepart 的整数。
		/// </summary>
		/// <param name="datepart">返回的 date 的哪部分。TSQL已经将可选参数封装为熟悉，例如TSQL.year，请勿与YEAR()方法混淆</param>
		/// <param name="date">是一个表达式，可以解析为 time、date、smalldatetime、datetime、datetime2 或 datetimeoffset 值。date 可以是表达式、列表达式、用户定义的变量或字符串文字。</param>
		/// <returns>返回表示指定 date 的指定 datepart 的整数。</returns>
		public static int DATEPART(object datepart, object date)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表示指定 date 的“日”部分的整数。
		/// </summary>
		/// <param name="date">是一个表达式，可以解析为 time、date、smalldatetime、datetime、datetime2 或 datetimeoffset 值。date 可以是表达式、列表达式、用户定义的变量或字符串文字。</param>
		/// <returns>返回表示指定 date 的“日”部分的整数。</returns>
		public static int DAY(object date)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表示指定 date 的“月”部分的整数。
		/// </summary>
		/// <param name="date">是一个表达式，可以解析为 time、date、smalldatetime、datetime、datetime2 或 datetimeoffset 值。date 可以是表达式、列表达式、用户定义的变量或字符串文字。</param>
		/// <returns>返回表示指定 date 的“月”部分的整数。</returns>
		public static int MONTH(object date)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表示指定 date 的“年”部分的整数。
		/// </summary>
		/// <param name="date">是一个表达式，可以解析为 time、date、smalldatetime、datetime、datetime2 或 datetimeoffset 值。date 可以是表达式、列表达式、用户定义的变量或字符串文字。</param>
		/// <returns>返回表示指定 date 的“年”部分的整数。</returns>
		public static int YEAR(object date)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定的 startdate 和 enddate 之间所跨的指定 datepart 边界的计数（带符号的整数）。
		/// </summary>
		/// <param name="datepart">是指定所跨边界类型的 startdate 和 enddate 的一部分。TSQL已经将可选参数封装为熟悉，例如TSQL.year，请勿与YEAR()方法混淆</param>
		/// <param name="startdate">是一个表达式，可以解析为 time、date、smalldatetime、datetime、datetime2 或 datetimeoffset 值。date 可以是表达式、列表达式、用户定义的变量或字符串文字。从 enddate 减去 startdate。</param>
		/// <param name="enddate">请参阅 startdate。</param>
		/// <returns>返回指定的 startdate 和 enddate 之间所跨的指定 datepart 边界的计数（带符号的整数）。</returns>
		/// <remarks>
		/// 如果返回值超出 int 的范围（-2,147,483,648 到 +2,147,483,647），则会返回一个错误。 对于 millisecond，startdate 与 enddate 之间的最大差值为 24 天 20 小时 31 分钟 23.647 秒。 对于 second，最大差值为 68 年。
		/// 
		/// 如果为 startdate 和 enddate 都只指定了时间值，并且 datepart 不是时间 datepart，则会返回 0。
		/// 
		/// 在计算返回值时不使用 startdate 或 endate 的时区偏移量部分。
		/// 
		/// 由于 smalldatetime 仅精确到分钟，因此将 smalldatetime 值用作 startdate 或 enddate 时，返回值中的秒和毫秒将始终设置为 0。
		/// 
		/// 如果只为某个日期数据类型的变量指定时间值，则所缺日期部分的值将设置为默认值：1900-01-01。 如果只为某个时间或日期数据类型的变量指定日期值，则所缺时间部分的值将设置为默认值：00:00:00。 如果 startdate 和 enddate 中有一个只含时间部分，另一个只含日期部分，则所缺时间和日期部分将设置为各自的默认值。
		/// 
		/// 如果 startdate 和 enddate 属于不同的日期数据类型，并且其中一个的时间部分或秒的小数部分精度比另一个高，则另一个的所缺部分将设置为 0。
		/// </remarks>
		public static int DATEDIFF(object datepart, object startdate, object enddate)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将指定 number 时间间隔（有符号整数）与指定 date 的指定 datepart 相加后，返回该 date。
		/// </summary>
		/// <param name="datepart">是与 integer number 相加的 date 部分。TSQL已经将可选参数封装为熟悉，例如TSQL.year，请勿与YEAR()方法混淆</param>
		/// <param name="number">是一个表达式，可以解析为与 date 的 datepart 相加的 int。用户定义的变量是有效的。如果您指定一个带小数的值，则将小数截去且不进行舍入。</param>
		/// <param name="date">是一个表达式，可以解析为 time、date、smalldatetime、datetime、datetime2 或 datetimeoffset 值。date 可以是表达式、列表达式、用户定义的变量或字符串文字。如果表达式是字符串文字，则它必须解析为一个 datetime 值。为避免不确定性，请使用四位数年份。有关两位数年份的信息，请参阅 two digit year cutoff 选项。</param>
		/// <returns>返回指定的 startdate 和 enddate 之间所跨的指定 datepart 边界的计数（带符号的整数）。</returns>
		/// <remarks>		
		/// 返回数据类型为 date 参数的数据类型，字符串文字除外。
		/// 
		/// 字符串文字的返回数据类型为 datetime。如果字符串文字的秒数小数位数超过三位 (.nnn) 或包含时区偏移量部分，将引发错误。
		/// </remarks>
		public static DateTime DATEADD(object datepart, object number, object date)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回从存储的时区偏移量变为指定的新时区偏移量时得到的 datetimeoffset 值。
		/// </summary>
		/// <param name="DATETIMEOFFSET">是一个可以解析为 datetimeoffset(n) 值的表达式。</param>
		/// <param name="time_zone">是一个格式为 [+|-]TZH:TZM 的字符串，或是一个表示时区偏移量的带符号的整数（分钟数），假定它能够感知夏时制并作出相应的调整。</param>
		/// <returns>具有 DATETIMEOFFSET 参数的小数精度的 datetimeoffset。</returns>
		/// <remarks>		
		/// 使用 SWITCHOFFSET 可选择与最初存储的时区偏移量不同的时区偏移量的 datetimeoffset 值。SWITCHOFFSET 不会更新存储的 time_zone 值。
		/// 
		/// SWITCHOFFSET 可用于更新 datetimeoffset 列。
		/// </remarks>
		/// <example>
		/// CREATE TABLE dbo.test 
		///     (
		///     ColDatetimeoffset datetimeoffset
		///     );
		/// GO
		/// INSERT INTO dbo.test 
		/// VALUES ('1998-09-20 7:45:50.71345 -5:00');
		/// GO
		/// SELECT SWITCHOFFSET (ColDatetimeoffset, '-08:00') 
		/// FROM dbo.test;
		/// GO
		/// --Returns: 1998-09-20 04:45:50.7134500 -08:00
		/// SELECT ColDatetimeoffset
		/// FROM dbo.test;
		/// --Returns: 1998-09-20 07:45:50.7134500 -05:00
		/// </example>
		public static DateTime SWITCHOFFSET(object DATETIMEOFFSET, object time_zone)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 如果输入 expression 是 datetime 或 smalldatetime 数据类型的有效日期或时间值，则返回 1；否则，返回 0。
		/// </summary>
		/// <param name="expression">是除 text、ntext 或 image 表达式以外的任意表达式，可隐式转换为 nvarchar 以评估是否为 datetime 或 smalldatetime 数据类型的有效日期或时间值。如果 expression 的类型为 varchar，则值会转换为 nvarchar(4000)。如果 varchar 值因为过大而需要截断，将引发错误。</param>
		/// <returns>int</returns>
		/// <remarks>
		/// 只有与 CONVERT 函数一起使用，同时指定了 CONVERT 样式参数且样式不等于 0、100、9 或 109 时，ISDATE 才是确定的。
		/// 
		/// ISDATE 的返回值取决于 SET DATEFORMAT、SET LANGUAGE 和 default language 选项设定的设置。有关示例，请参阅示例 C。
		/// 
		/// 对于类型为 time、date、datetime2 或 datetimeoffset 的用户定义变量或数据库列，ISDATE 返回 0。对于秒的小数部分精度超过三位小数或包含时区偏移量日期部分的字符串，ISDATE 返回 0。若要验证这些数据类型的值，请使用带有对应数据类型参数的 CONVERT 函数并处理错误 241。错误 241 返回如下消息：“从字符串向日期和/或时间转换时失败。”
		/// </remarks>
		public static int ISDATE(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回从 datetime2 表达式转换而来的一个 datetimeoffset 值。
		/// </summary>
		/// <param name="expression">一个解析为 datetime2 值的表达式。</param>
		/// <param name="time_zone">一个表示时区偏移量（以分钟为单位）的表达式。范围从 +14 到 -13。该表达式被解释为指定 time_zone 的本地时间。</param>
		/// <returns>datetimeoffset. 小数精度与 datetime 参数相同。</returns>
		public static DateTime TODATETIMEOFFSET(object expression, object time_zone)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region 时间函数所需常量
		/// <summary>
		/// 年
		/// </summary>
		public static object year
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// 年
		/// </summary>
		public static object yy
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// 年
		/// </summary>
		public static object yyyy
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// 季度
		/// </summary>
		public static object quarter
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// 季度
		/// </summary>
		public static object qq
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// 季度
		/// </summary>
		public static object q
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		/// <summary>
		/// 月
		/// </summary>
		public static object month
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 月
		/// </summary>
		public static object mm
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 月
		/// </summary>
		public static object m
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 一年中的第几天
		/// </summary>
		public static object dayofyear
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 一年中的第几天
		/// </summary>
		public static object dy
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 一年中的第几天
		/// </summary>
		public static object y
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 日
		/// </summary>
		public static object day
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 日
		/// </summary>
		public static object dd
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 日
		/// </summary>
		public static object d
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 周
		/// </summary>
		public static object week
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 周
		/// </summary>
		public static object wk
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 周
		/// </summary>
		public static object ww
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 一周的第几天
		/// </summary>
		public static object weekday
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 一周的第几天
		/// </summary>
		public static object dw
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 小时
		/// </summary>
		public static object hour
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 小时
		/// </summary>
		public static object hh
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 分钟
		/// </summary>
		public static object minute
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 分钟
		/// </summary>
		public static object mi
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 分钟
		/// </summary>
		public static object n
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 秒
		/// </summary>
		public static object second
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 秒
		/// </summary>
		public static object ss
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 秒
		/// </summary>
		public static object s
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 毫秒
		/// </summary>
		public static object millisecond
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 毫秒
		/// </summary>
		public static object ms
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 微秒 需要SQL Server 2008版本的支持
		/// 1毫秒=1000微秒
		/// </summary>
		public static object microsecond
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 微秒 需要SQL Server 2008版本的支持
		/// 1毫秒=1000微秒
		/// </summary>
		public static object mcs
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 纳秒(10-9秒)
		/// 1纳秒=0.001微秒
		/// 1纳秒=1000皮秒
		/// 1纳秒=0.000001毫秒
		/// 1纳秒=0.00000 0001秒
		/// </summary>
		public static object nanosecond
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 纳秒(10-9秒)
		/// 1纳秒=0.001微秒
		/// 1纳秒=1000皮秒
		/// 1纳秒=0.000001毫秒
		/// 1纳秒=0.00000 0001秒
		/// </summary>
		public static object ns
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 时区偏移
		/// </summary>
		public static object TZoffset
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 时区偏移
		/// </summary>
		public static object tz
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region 配置函数
		/// <summary>
		/// 返回对会话进行 SET DATEFIRST 操作所得结果的当前值。
		/// </summary>
		public static int DATEFIRST
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回有关当前 SET 选项的信息。
		/// </summary>
		public static int OPTIONS
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回当前数据库的当前 timestamp 数据类型的值。这一时间戳值在数据库中必须是唯一的。
		/// </summary>
		public static object DBTS
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回远程 SQL Server 数据库服务器在登录记录中显示的名称。
		/// </summary>
		public static string REMSERVER
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回当前使用的语言的本地语言标识符 (ID)。
		/// </summary>
		public static int LANGID
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回运行 SQL Server 的本地服务器的名称。
		/// </summary>
		public static string SERVERNAME
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回当前所用语言的名称。
		/// </summary>
		public static string LANGUAGE
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回 SQL Server 正在其下运行的注册表项的名称。若当前实例为默认实例，则 @@SERVICENAME 返回 MSSQLSERVER；若当前实例是命名实例，则该函数返回该实例名。
		/// </summary>
		public static string SERVICENAME
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回当前会话的当前锁定超时设置（毫秒）。
		/// </summary>
		public static int LOCK_TIMEOUT
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回当前用户进程的会话 ID。
		/// </summary>
		public static int SPID
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回 SQL Server 实例允许同时进行的最大用户连接数。返回的数值不一定是当前配置的数值。
		/// </summary>
		public static int MAX_CONNECTIONS
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回 TEXTSIZE 选项的当前值。
		/// </summary>
		public static int TEXTSIZE
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 按照服务器中的当前设置，返回 decimal 和 numeric 数据类型所用的精度级别。
		/// </summary>
		public static int MAX_PRECISION
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回当前的 SQL Server 安装的版本、处理器体系结构、生成日期和操作系统。
		/// </summary>
		/// <remarks>
		/// @@VERSION 结果显示为一个 nvarchar 字符串。可以使用 SERVERPROPERTY (Transact-SQL) 函数检索各个属性值。
		/// </remarks>
		public static string VERSION
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回对本地服务器上执行的当前存储过程的嵌套级别（初始值为 0）。有关嵌套级别的信息，请参阅嵌套存储过程。
		/// </summary>
		public static int NESTLEVEL
		{
			get
			{
				throw new NotSupportedException();
			}
		}
		#endregion

		#region 系统统计函数
		/// <summary>
		/// 返回 SQL Server 自上次启动以来尝试的连接数，无论连接是成功还是失败。
		/// </summary>
		public static int CONNECTIONS
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回 SQL Server 自上次启动后从网络读取的输入数据包数。
		/// </summary>
		public static int PACK_RECEIVED
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回 SQL Server 自上次启动后的工作时间。其结果以 CPU 时间增量或“滴答数”表示，此值为所有 CPU 时间的累积，因此，可能会超出实际占用的时间。乘以 @@TIMETICKS 即可转换为微秒。
		/// </summary>
		public static int CPU_BUSY
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回 SQL Server 自上次启动后写入网络的输出数据包个数。
		/// </summary>
		public static int PACK_SENT
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回每个时钟周期的微秒数。
		/// </summary>
		public static int TIMETICKS
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回 SQL Server 自上次启动后的空闲时间。结果以 CPU 时间增量或“时钟周期”表示，并且是所有 CPU 的累积，因此该值可能超过实际经过的时间。乘以 @@TIMETICKS 即可转换为微秒。
		/// </summary>
		public static int IDLE
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回自上次启动 SQL Server 之后 SQL Server 所遇到的磁盘写入错误数。
		/// </summary>
		public static int TOTAL_ERRORS
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回自从 SQL Server 最近一次启动以来，SQL Server 已经用于执行输入和输出操作的时间。其结果是 CPU 时间增量（时钟周期），并且是所有 CPU 的累积值，所以，它可能超过实际消逝的时间。乘以 @@TIMETICKS 即可转换为微秒。
		/// </summary>
		public static int IO_BUSY
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回 SQL Server 自上次启动后由 SQL Server 读取（非缓存读取）的磁盘的数目。
		/// </summary>
		public static object TOTAL_READ
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回自上次启动 SQL Server 后，在 SQL Server 连接上发生的网络数据包错误数。
		/// </summary>
		public static object PACKET_ERRORS
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回自上次启动 SQL Server 以来 SQL Server 所执行的磁盘写入数。
		/// </summary>
		public static object TOTAL_WRITE
		{
			get
			{
				throw new NotSupportedException();
			}
		}


		#endregion

		#region 系统函数
		/// <summary>
		/// 返回当前会话的应用程序名称（如果应用程序进行了设置）。
		/// </summary>
		/// <returns>nvarchar(128)</returns>
		public static string APP_NAME()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将一种数据类型的表达式转换为另一种数据类型的表达式。
		/// </summary>
		/// <param name="expression">任何有效的表达式。</param>
		/// <param name="data_type">目标数据类型。这包括 xml、bigint 和 sql_variant。不能使用别名数据类型。有关可用数据类型的详细信息，请参阅数据类型 (Transact-SQL)。</param>
		/// <returns>返回转换为 data_type 的 expression。</returns>
		[TSQLFormattor("CAST({0} AS {1})")]
		public static object CAST(object expression, SqlDbType data_type)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将一种数据类型的表达式转换为另一种数据类型的表达式。
		/// </summary>
		/// <param name="data_type">目标数据类型。这包括 xml、bigint 和 sql_variant。不能使用别名数据类型。有关可用数据类型的详细信息，请参阅数据类型 (Transact-SQL)。</param>
		/// <param name="length">指定目标数据类型长度的可选整数。默认值为 30。</param>
		/// <param name="expression">任何有效的表达式。</param>
		/// <param name="style">指定 CONVERT 函数如何转换 expression 的整数表达式。如果样式为 NULL，则返回 NULL。该范围是由 data_type 确定的。</param>
		/// <returns>返回转换为 data_type 的 expression。</returns>
		[TSQLFormattor("CONVERT({0}({1}), {2}, {3})")]
		public static object CONVERT(SqlDbType data_type, int length, object expression, int style)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将一种数据类型的表达式转换为另一种数据类型的表达式。
		/// </summary>
		/// <param name="data_type">目标数据类型。这包括 xml、bigint 和 sql_variant。不能使用别名数据类型。有关可用数据类型的详细信息，请参阅数据类型 (Transact-SQL)。</param>
		/// <param name="expression">任何有效的表达式。</param>
		/// <param name="length">指定目标数据类型长度的可选整数。默认值为 30。</param>
		/// <returns>返回转换为 data_type 的 expression。</returns>
		[TSQLFormattor("CONVERT({0}({1}), {2})")]
		public static object CONVERT(SqlDbType data_type, int length, object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将一种数据类型的表达式转换为另一种数据类型的表达式。
		/// </summary>
		/// <param name="data_type">目标数据类型。这包括 xml、bigint 和 sql_variant。不能使用别名数据类型。有关可用数据类型的详细信息，请参阅数据类型 (Transact-SQL)。</param>
		/// <param name="expression">任何有效的表达式。</param>
		/// <returns>返回转换为 data_type 的 expression。</returns>
		[TSQLFormattor("CONVERT({0}, {1})")]
		public static object CONVERT(SqlDbType data_type, object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将一种数据类型的表达式转换为另一种数据类型的表达式。
		/// </summary>
		/// <param name="data_type">目标数据类型。这包括 xml、bigint 和 sql_variant。不能使用别名数据类型。有关可用数据类型的详细信息，请参阅数据类型 (Transact-SQL)。</param>
		/// <param name="expression">任何有效的表达式。</param>
		/// <param name="style">指定 CONVERT 函数如何转换 expression 的整数表达式。如果样式为 NULL，则返回 NULL。该范围是由 data_type 确定的。</param>
		/// <returns>返回转换为 data_type 的 expression。</returns>
		[TSQLFormattor("CONVERT({0}, {1}, {2})")]
		public static object CONVERT(SqlDbType data_type, object expression, int style)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回其参数中第一个非空表达式。 
		/// </summary>
		/// <param name="expression">任何类型的表达式。</param>
		/// <returns>返回数据类型优先级最高的 expression 的数据类型。</returns>
		[TSQLFormattor("COALESCE", TSQLFormattor.FLAG_DYNAMIC_PARAMETERS)]
		public static object COALESCE(params object[] expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定排序规则的属性。
		/// </summary>
		/// <param name="collation_name">排序规则的名称。collation_name 的数据类型为 nvarchar(128)，无默认值。</param>
		/// <param name="property">排序规则的属性。property 的数据类型为 varchar(128)</param>
		/// <returns>sql_variant</returns>
		public static object COLLATIONPROPERTY(object collation_name, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回 varbinary 位模式，它指示表或视图中插入或更新了哪些列。COLUMNS_UPDATED 可用于 Transact-SQL INSERT 或 UPDATE 触发器主体内部的任意位置，以测试该触发器是否应执行某些操作。
		/// </summary>
		/// <returns>varbinary </returns>
		public static object COLUMNS_UPDATED()
		{
			throw new NotSupportedException();
		}


		/// <summary>
		/// 返回当前用户的名称。此函数等价于 USER_NAME()。
		/// </summary>
		[TSQLFormattor("CURRENT_USER")]
		public static string CURRENT_USER
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回用于表示任何表达式的字节数。
		/// </summary>
		/// <param name="expression">任何数据类型的表达式。</param>
		/// <returns>如果 expression 的数据类型为 varchar(max)、nvarchar(max) 或 varbinary(max) 数据类型，则返回 bigint；否则返回 int。</returns>
		/// <remarks>对于 varchar、varbinary、text、image、nvarchar 和 ntext 数据类型，DATALENGTH 尤其有用，因为这些数据类型可以存储长度可变的数据。
		/// NULL 的 DATALENGTH 的结果是 NULL。
		/// </remarks>
		public static int DATALENGTH(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回执行的上一个 Transact-SQL 语句的错误号。
		/// </summary>
		public static int ERROR
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回发生错误的行号，该错误导致运行 TRY…CATCH 构造的 CATCH 块。
		/// </summary>
		/// <returns>int</returns>
		public static int ERROR_LINE()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回导致 TRY…CATCH 构造的 CATCH 块运行的错误的消息文本。
		/// </summary>
		/// <returns>nvarchar(2048) </returns>
		public static string ERROR_MESSAGE()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回错误的错误号，该错误会导致运行 TRY…CATCH 结构的 CATCH 块。
		/// </summary>
		/// <returns>int</returns>
		public static int ERROR_NUMBER()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回在其中出现了导致 TRY…CATCH 构造的 CATCH 块运行的错误的存储过程或触发器的名称。
		/// </summary>
		/// <returns>nvarchar(126)</returns>
		public static string ERROR_PROCEDURE()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回导致 TRY…CATCH 构造的 CATCH 块运行的错误的严重级别。
		/// </summary>
		/// <returns>int</returns>
		public static int ERROR_SEVERITY()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回导致 TRY…CATCH 构造的 CATCH 块运行的错误状态号。
		/// </summary>
		/// <returns>int</returns>
		public static int ERROR_STATE()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 根据 sys.messages 中现有的消息构造一条消息。FORMATMESSAGE 的功能与 RAISERROR 语句的功能类似。但是，RAISERROR 会立即打印消息，而 FORMATMESSAGE 则返回供进一步处理的格式化消息。
		/// </summary>
		/// <param name="msg_number">存储在 sys.messages 中的消息的 ID。如果 msg_number &lt;= 13000，或者此消息不在 sys.messages 中，则返回 NULL。</param>
		/// <param name="param_value">供在消息中使用的参数值。可以是多个参数值。值的顺序必须与占位符变量在消息中出现的次序相同。值的最大数目为 20。</param>
		/// <returns>nvarchar </returns>
		[TSQLFormattor("FORMATMESSAGE", TSQLFormattor.FLAG_DYNAMIC_PARAMETERS)]
		public static string FORMATMESSAGE(object msg_number, params object[] param_value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回此会话的数据库的默认为空性。
		/// </summary>
		/// <param name="database">为其返回为空性信息的数据库的名称。database 的数据类型为 char 或 nchar。如果为 char，则 database 隐式转换为 nchar</param>
		/// <returns>int</returns>
		public static int GETANSINULL(object database)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回此会话的数据库的默认为空性。
		/// </summary>
		/// <returns>int</returns>
		public static int GETANSINULL()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回工作站标识号。
		/// </summary>
		/// <returns>char(10) </returns>
		public static string HOST_ID()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回工作站名。
		/// </summary>
		/// <returns>nvarchar(128) </returns>
		public static string HOST_NAME()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回为某个会话和作用域中指定的表或视图生成的最新的标识值。
		/// </summary>
		/// <param name="table_name">其标识值被返回的表的名称。table_name 的数据类型为 varchar，无默认值。</param>
		/// <returns>numeric(38,0)</returns>
		public static long IDENT_CURRENT(object table_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回增量值（返回形式为 numeric (@@MAXPRECISION,0)），该值是在带有标识列的表或视图中创建标识列时指定的。
		/// </summary>
		/// <param name="table_or_view">指定表或视图以检查有效的标识增量值的表达式。table_or_view 可以是带有引号的字符串常量，也可以是变量、函数或列名。table_or_view 的数据类型为 char、nchar、varchar 或 nvarchar。</param>
		/// <returns>numeric</returns>
		public static int IDENT_INCR(object table_or_view)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回原始种子值（返回形式为 numeric(@@MAXPRECISION,0)），该值是在表或视图中创建标识列时指定的。使用 DBCC CHECKIDENT 更改标识列的当前值不会更改此函数返回的值。
		/// </summary>
		/// <param name="table_or_view">指定表或视图以检查标识种子值的表达式。table_or_view 可以是带有引号的字符串常量，也可以是变量、函数或列名。table_or_view 的数据类型为 char、nchar、varchar 或 nvarchar。</param>
		/// <returns>numeric</returns>
		public static long IDENT_SEED(object table_or_view)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 只用于在带有 INTO table 子句的 SELECT 语句中将标识列插入到新表中。尽管类似，但是 IDENTITY 函数不是与 CREATE TABLE 和 ALTER TABLE 一起使用的 IDENTITY 属性。
		/// </summary>
		/// <param name="data_type">标识列的数据类型。标识列的有效数据类型可以是任何整数数据类型类别的数据类型（bit 数据类型除外），也可以是 decimal 数据类型。</param>
		/// <param name="seed">要分配给表中第一行的整数值。为每一个后续行分配下一个标识值，该值等于上一个 IDENTITY 值加上 increment 值。如果既没有指定 seed，也没有指定 increment ，那么它们都默认为 1。</param>
		/// <param name="increment">要加到表中后续行的 seed 值上的整数值。</param>
		/// <param name="column_name">将插入到新表中的列的名称。</param>
		/// <returns>返回与 data_type 相同的数据类型。</returns>
		[TSQLFormattor("IDENTITY({0}, {1}, {2}) AS {3}")]
		public static long IDENTITY(SqlDbType data_type, object seed, object increment, object column_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 只用于在带有 INTO table 子句的 SELECT 语句中将标识列插入到新表中。尽管类似，但是 IDENTITY 函数不是与 CREATE TABLE 和 ALTER TABLE 一起使用的 IDENTITY 属性。
		/// </summary>
		/// <param name="data_type">标识列的数据类型。标识列的有效数据类型可以是任何整数数据类型类别的数据类型（bit 数据类型除外），也可以是 decimal 数据类型。</param>
		/// <param name="seed">要分配给表中第一行的整数值。为每一个后续行分配下一个标识值，该值等于上一个 IDENTITY 值加上 increment 值。如果既没有指定 seed，也没有指定 increment ，那么它们都默认为 1。</param>
		/// <param name="column_name">将插入到新表中的列的名称。</param>
		/// <returns>返回与 data_type 相同的数据类型。</returns>
		[TSQLFormattor("IDENTITY({0}, {1}) AS {2}")]
		public static long IDENTITY(SqlDbType data_type, object seed, object column_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 只用于在带有 INTO table 子句的 SELECT 语句中将标识列插入到新表中。尽管类似，但是 IDENTITY 函数不是与 CREATE TABLE 和 ALTER TABLE 一起使用的 IDENTITY 属性。
		/// </summary>
		/// <param name="data_type">标识列的数据类型。标识列的有效数据类型可以是任何整数数据类型类别的数据类型（bit 数据类型除外），也可以是 decimal 数据类型。</param>
		/// <param name="column_name">将插入到新表中的列的名称。</param>
		/// <returns>返回与 data_type 相同的数据类型。</returns>
		[TSQLFormattor("IDENTITY({0}) AS {1}")]
		public static long IDENTITY(SqlDbType data_type, object column_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回最后插入的标识值的系统函数。
		/// </summary>
		/// <returns>numeric(38,0) </returns>
		[TSQLFormattor("@@IDENTITY")]
		public static long IDENTITY()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 使用指定的替换值替换 NULL。
		/// </summary>
		/// <param name="check_expression">将被检查是否为 NULL 的表达式。check_expression 可以为任何类型。</param>
		/// <param name="replacement_value">当 check_expression 为 NULL 时要返回的表达式。replacement_value 必须是可以隐式转换为 check_expresssion 类型的类型。</param>
		/// <returns>返回与 check_expression 相同的类型。</returns>
		/// <remarks>
		/// 如果 check_expression 不为 NULL，则返回它的值；否则，在将 replacement_value 隐式转换为 check_expression 的类型（如果这两个类型不同）后，则返回前者。
		/// </remarks>
		public static object ISNULL(object check_expression, object replacement_value)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 确定表达式是否为有效的数值类型。
		/// </summary>
		/// <param name="expression">要计算的表达式。</param>
		/// <returns>当输入表达式的计算结果为有效的 numeric 数据类型时，ISNUMERIC 返回 1；否则返回 0。</returns>
		public static int ISNUMERIC(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 创建 uniqueidentifier 类型的唯一值。
		/// </summary>
		/// <returns>uniqueidentifier</returns>
		public static object NEWID()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 如果两个指定的表达式相等，则返回空值。
		/// </summary>
		/// <param name="expression1">任何有效的标量表达式。</param>
		/// <param name="expression2">任何有效的标量表达式。</param>
		/// <returns>返回类型与第一个 expression 相同。如果两个表达式不相等，则 NULLIF 返回第一个 expression 的值。如果表达式相等，则 NULLIF 返回第一个 expression 类型的空值。</returns>
		public static object NULLIF(object expression1, object expression2)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回对象名称的指定部分。可以检索的对象部分有对象名、所有者名称、数据库名称和服务器名称。
		/// </summary>
		/// <param name="object_name">要检索其指定部分的对象的名称。object_name 的数据类型为 sysname。此参数是可选的限定对象名称。如果对象名称的所有部分都是限定的，则此名称可包含四部分：服务器名称、数据库名称、所有者名称以及对象名称。</param>
		/// <param name="object_piece">要返回的对象部分。object_piece 的数据类型为 int 值，可以为下列值。
		/// 1 = 对象名称，
		/// 2 = 架构名称，
		/// 3 = 数据库名称，
		/// 4 = 服务器名称。</param>
		/// <returns>nchar</returns>
		/// <remarks>
		/// 如果存在下列条件之一，则 PARSENAME 返回 NULL：
		/// object_name 或 object_piece 为 NULL。
		/// 发生语法错误。
		/// 请求的对象部分长度为 0，且不是有效的 Microsoft SQL Server 标识符。长度为零的对象的名称将使整个限定名称无效。
		/// </remarks>
		public static string PARSENAME(object object_name, object object_piece)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回连接到 SQL Server 实例的登录名。您可以在具有众多显式或隐式上下文切换的会话中使用该函数返回原始登录的标识。
		/// </summary>
		/// <returns>sysname</returns>
		public static string ORIGINAL_LOGIN()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回受上一语句影响的行数。如果行数大于 20 亿，请使用 ROWCOUNT_BIG。
		/// </summary>
		/// <remarks>
		/// Transact-SQL 语句可以通过下列方式设置 @@ROWCOUNT 的值：
		/// 将 @@ROWCOUNT 设置为受影响或被读取的行的数目。可以将行发送到客户端，也可以不发送。
		/// 
		/// 保留前一个语句执行中的 @@ROWCOUNT。
		/// 将 @@ROWCOUNT 重置为 0 但不将该值返回到客户端。
		/// 执行简单分配的语句始终将 @@ROWCOUNT 值设置为 1。不将任何行发送到客户端。这些语句的示例如下：SET @local_variable、RETURN、READTEXT 以及不带查询 Select 语句，如 SELECT GETDATE() 或 SELECT 'Generic Text'。
		/// 在查询中执行分配或使用 RETURN 的语句将 @@ROWCOUNT 值设置为受查询影响或由查询读取的行数，例如：SELECT @local_variable = c1 FROM t1。
		/// 数据操作语言 (DML) 语句将 @@ROWCOUNT 值设置为受查询影响的行数，并将该值返回到客户端。DML 语句不会将任何行发送到客户端。
		/// DECLARE CURSOR 和 FETCH 将 @@ROWCOUNT 值设置为 1。
		/// EXECUTE 语句保留前一个 @@ROWCOUNT。
		/// USE、SET &lt;option>、DEALLOCATE CURSOR、CLOSE CURSOR、BEGIN TRANSACTION 或 COMMIT TRANSACTION 等语句将 ROWCOUNT 值重置为 0。
		/// </remarks>
		public static int ROWCOUNT
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回已执行的上一语句影响的行数。该函数的功能与 @@ROWCOUNT 类似，区别在于 ROWCOUNT_BIG 的返回类型为 bigint。
		/// </summary>
		/// <returns>bigint</returns>
		/// <remarks>
		/// 位于 SELECT 语句之后时，该函数返回由 SELECT 语句返回的行数。
		/// 位于 INSERT、UPDATE 或 DELETE 语句之后时，该函数返回受数据修改语句影响的行数。
		/// 位于 IF 这类不返回行的语句之后时，该函数返回 0。
		/// </remarks>
		public static long ROWCOUNT_BIG()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回插入到同一作用域中的标识列内的最后一个标识值。一个范围是一个模块：存储过程、触发器、函数或批处理。因此，如果两个语句处于同一个存储过程、函数或批处理中，则它们位于相同的作用域中。
		/// </summary>
		/// <returns>numeric</returns>
		public static long SCOPE_IDENTITY()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回有关服务器实例的属性信息。
		/// </summary>
		/// <param name="propertyname">一个表达式，包含要返回的服务器属性信息。</param>
		/// <returns>sql_variant</returns>
		public static object SERVERPROPERTY(object propertyname)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回会话的 SET 选项设置。
		/// </summary>
		/// <param name="option">该会话的当前选项设置。</param>
		/// <returns>sql_variant</returns>
		public static object SESSIONPROPERTY(object option)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// SESSION_USER 返回当前数据库中当前上下文的用户名。
		/// </summary>
		[TSQLFormattor("SESSION_USER")]
		public static string SESSION_USER
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回上次更新指定表的统计信息的日期。
		/// </summary>
		/// <param name="table_id">所用表的 ID。</param>
		/// <param name="stats_id">所用统计信息的 ID。</param>
		/// <returns>datetime</returns>
		public static DateTime STATS_DATE(object table_id, object stats_id)
		{
			throw new NotSupportedException();
		}


		/// <summary>
		/// 当未指定默认值时，允许将系统为当前登录提供的值插入表中。
		/// </summary>
		[TSQLFormattor("SYSTEM_USER")]
		public static object SYSTEM_USER
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回当前连接的活动事务数。
		/// </summary>
		public static object TRANCOUNT
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回一个布尔值，指示是否对表或视图的指定列进行了 INSERT 或 UPDATE 尝试。可以在 Transact-SQL INSERT 或 UPDATE 触发器主体中的任意位置使用 UPDATE()，以测试触发器是否应执行某些操作。
		/// </summary>
		/// <param name="column">要为 INSERT 或 UPDATE 操作测试的列的名称。由于表名是在触发器的 ON 子句中指定的，因此不要在列名前包含表名。列可以是 SQL Server 支持的任何数据类型。但是，计算列不能用于此上下文。</param>
		/// <returns>Boolean</returns>
		public static bool UPDATE(object column)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 基于指定的标识号返回数据库用户名。
		/// </summary>
		/// <param name="id">与数据库用户关联的标识号。id 的数据类型为 int。需要使用括号。</param>
		/// <returns>nvarchar(256)</returns>
		public static string USER_NAME(object id)
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// 返回数据库当前上下文中的当前用户名。
		/// </summary>
		/// <returns>nvarchar(256)</returns>
		public static string USER_NAME()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 用于报告当前正在运行的请求的用户事务状态的标量函数。XACT_STATE 指示请求是否有活动的用户事务，以及是否能够提交该事务。
		/// </summary>
		/// <returns>
		/// smallint 
		/// </returns>
		/// <remarks>
		/// XACT_STATE 返回下列值。
		/// 1
		/// 当前请求有活动的用户事务。请求可以执行任何操作，包括写入数据和提交事务。
		/// 
		/// 0
		/// 当前请求没有活动的用户事务。
		/// 
		/// -1
		/// 当前请求具有活动的用户事务，但出现了致使事务被归类为无法提交的事务的错误。请求无法提交事务或回滚到保存点；它只能请求完全回滚事务。请求在回滚事务之前无法执行任何写操作。请求在回滚事务之前只能执行读操作。事务回滚之后，请求便可执行读写操作并可开始新的事务。
		/// 
		/// 当批处理结束运行时，数据库引擎将自动回滚所有不可提交的活动事务。如果事务进入不可提交状态时未发送错误消息，则当批处理结束时，将向客户端应用程序发送一个错误消息。该消息指示检测到并回滚了一个不可提交的事务。有关无法提交的事务的详细信息，请参阅在 Transact-SQL 中使用 TRY...CATCH。
		/// </remarks>
		public static int XACT_STATE()
		{
			throw new NotSupportedException();
		}


		#endregion

		#region 安全函数
		/// <summary>
		/// 返回用户的登录标识号。
		/// </summary>
		/// <param name="login">用户的登录名。login 的数据类型为 nchar。如果将 login 指定为 char，则 login 将隐式转换为 nchar。login 可以是有权限连接到 SQL Server 实例的任何 SQL Server 登录名或 Windows 用户或组。如果未指定 login，则返回当前用户的登录标识号。</param>
		/// <returns>int</returns>
		public static int SUSER_ID(object login)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 评估当前用户对安全对象的有效权限。 
		/// </summary>
		/// <param name="securable">安全对象的名称。如果安全对象是服务器本身，则此值应设置为 NULL。securable 是类型为 sysname 的标量表达式。没有默认值。</param>
		/// <param name="securable_class">测试权限的安全对象的类名。securable_class 是类型为 nvarchar(60) 的标量表达式。</param>
		/// <param name="permission">类型为 sysname 的非空标量表达式，表示要检查的权限名称。没有默认值。权限名称 ANY 是通配符。</param>
		/// <param name="sub_securable">类型为 sysname 的可选标量表达式，表示测试权限的安全对象子实体的名称。默认值为 NULL。</param>
		/// <param name="sub_securable_class">类型为 nvarchar(60) 的可选标量表达式，表示测试权限的安全对象子实体的类。默认值为 NULL。</param>
		/// <returns>
		/// int
		/// 如果查询失败，则返回 NULL。
		/// </returns>
		public static object Has_Perms_By_Name(object securable, object securable_class, object permission, object sub_securable, object sub_securable_class)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 评估当前用户对安全对象的有效权限。 
		/// </summary>
		/// <param name="securable">安全对象的名称。如果安全对象是服务器本身，则此值应设置为 NULL。securable 是类型为 sysname 的标量表达式。没有默认值。</param>
		/// <param name="securable_class">测试权限的安全对象的类名。securable_class 是类型为 nvarchar(60) 的标量表达式。</param>
		/// <param name="permission">类型为 sysname 的非空标量表达式，表示要检查的权限名称。没有默认值。权限名称 ANY 是通配符。</param>
		/// <param name="sub_securable">类型为 sysname 的可选标量表达式，表示测试权限的安全对象子实体的名称。默认值为 NULL。</param>
		/// <returns>
		/// int
		/// 如果查询失败，则返回 NULL。
		/// </returns>
		public static object Has_Perms_By_Name(object securable, object securable_class, object permission, object sub_securable)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 评估当前用户对安全对象的有效权限。 
		/// </summary>
		/// <param name="securable">安全对象的名称。如果安全对象是服务器本身，则此值应设置为 NULL。securable 是类型为 sysname 的标量表达式。没有默认值。</param>
		/// <param name="securable_class">测试权限的安全对象的类名。securable_class 是类型为 nvarchar(60) 的标量表达式。</param>
		/// <param name="permission">类型为 sysname 的非空标量表达式，表示要检查的权限名称。没有默认值。权限名称 ANY 是通配符。</param>
		/// <returns>
		/// int
		/// 如果查询失败，则返回 NULL。
		/// </returns>
		public static int Has_Perms_By_Name(object securable, object securable_class, object permission)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定登录名的安全标识号 (SID)。
		/// </summary>
		/// <param name="login">用户的登录名。login 的数据类型为 sysname。login作为可选项，可以为 Microsoft SQL Server 登录名或 Microsoft Windows 用户或组。如果未指定 login，则返回有关当前安全上下文的信息。</param>
		/// <returns>varbinary(85) </returns>
		public static object SUSER_SID(object login)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 指示当前用户是否为指定 Microsoft Windows 组或 Microsoft SQL Server 数据库角色的成员。
		/// </summary>
		/// <param name="groupOrRole">
		/// ' group ' 
		/// 被检查的 Windows 组的名称；必须采用格式 Domain\Group。group 的数据类型为 sysname。
		/// 
		/// ' role ' 
		/// 被检查的 SQL Server 角色的名称。role 的数据类型为 sysname，它可以包括数据库固定角色或用户定义的角色，但不包括服务器角色。
		/// </param>
		/// <returns>
		/// 0
		/// 当前用户不是 group 或 role 的成员。
		/// 
		/// 1
		/// 当前用户是 group 或 role 的成员。
		/// 
		/// NULL
		/// group 或 role 无效。
		/// </returns>
		public static int IS_MEMBER(object groupOrRole)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回与安全标识号 (SID) 关联的登录名。
		/// </summary>
		/// <param name="server_user_sid">登录名的安全标识号。server_user_sid 为可选参数，其数据类型为 varbinary(85)。server_user_sid 可以是任何 SQL Server 登录名或 Microsoft Windows 用户或组的安全标识号。如果未指定 server_user_sid，则返回有关当前用户的信息。</param>
		/// <returns>nvarchar(128)</returns>
		public static string SUSER_SNAME(object server_user_sid)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 指示 SQL Server 登录名是否为指定固定服务器角色的成员。
		/// </summary>
		/// <param name="role">要检查的服务器角色的名称。role 的数据类型为 sysname。
		/// role 的有效值包括：
		/// sysadmin 
		/// dbcreator 
		/// bulkadmin 
		/// diskadmin 
		/// processadmin
		/// serveradmin
		/// setupadmin 
		/// securityadmin 
		///</param>
		/// <param name="login">要检查的 SQL Server 登录名。login 的数据类型为 sysname，默认值为 NULL。如果未指定值，则结果将根据当前执行上下文而定。</param>
		/// <returns>int 
		/// IS_SRVROLEMEMBER 返回以下值。
		/// 0
		/// login 不是 role 的成员。
		/// 1
		/// login 不是 role 的成员。
		/// NULL
		/// role 或 login 无效。
		///</returns>
		public static object IS_SRVROLEMEMBER(object role, object login)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 指示 SQL Server 登录名是否为指定固定服务器角色的成员。
		/// </summary>
		/// <param name="role">要检查的服务器角色的名称。role 的数据类型为 sysname。
		/// role 的有效值包括：
		/// sysadmin 
		/// dbcreator 
		/// bulkadmin 
		/// diskadmin 
		/// processadmin
		/// serveradmin
		/// setupadmin 
		/// securityadmin 
		///</param>
		/// <returns>int 
		/// IS_SRVROLEMEMBER 返回以下值。
		/// 0
		/// login 不是 role 的成员。
		/// 1
		/// login 不是 role 的成员。
		/// NULL
		/// role 或 login 无效。
		///</returns>
		public static object IS_SRVROLEMEMBER(object role)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回一个包含位图的值，该值指示当前用户的语句、对象或列权限。
		/// </summary>
		/// <param name="objectid">安全对象的 ID。如果未指定 objectid，则位图值包含当前用户的语句权限；否则，位图包含当前用户对该安全对象的权限。指定的安全对象必须在当前数据库中。使用 OBJECT_ID 函数确定 objectid 值。</param>
		/// <param name="column">返回其权限信息的列的可选名。该列必须是 objectid 指定的表中的有效列名。</param>
		/// <returns>int</returns>
		public static int PERMISSIONS(object objectid, object column)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回一个包含位图的值，该值指示当前用户的语句、对象或列权限。
		/// </summary>
		/// <param name="objectid">安全对象的 ID。如果未指定 objectid，则位图值包含当前用户的语句权限；否则，位图包含当前用户对该安全对象的权限。指定的安全对象必须在当前数据库中。使用 OBJECT_ID 函数确定 objectid 值。</param>
		/// <returns>int</returns>
		public static int PERMISSIONS(object objectid)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回用户的登录标识名。
		/// </summary>
		/// <param name="server_user_id">用户的登录标识号。可选参数 server_user_id 的数据类型为 int。server_user_id 可以是允许连接到 SQL Server 实例的任何 SQL Server 登录名或 Microsoft Windows 用户或用户组的登录标识号。</param>
		/// <returns>nvarchar(128)</returns>
		public static string SUSER_NAME(object server_user_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 则返回当前用户的登录标识名。
		/// </summary>
		/// <returns>nvarchar(128)</returns>
		public static string SUSER_NAME()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回与架构名称关联的架构 ID。
		/// </summary>
		/// <param name="schema_name">架构的名称。schema_name 的数据类型为 sysname。</param>
		/// <returns>int</returns>
		public static int SCHEMA_ID(object schema_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回调用方的默认架构的 ID。
		/// </summary>
		/// <returns>int</returns>
		public static int SCHEMA_ID()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回数据库用户的标识号
		/// </summary>
		/// <param name="user">要使用的用户名。user 的数据类型为 nchar。如果指定的是 char 类型的值，则将其隐式转换为 nchar。需要使用括号。</param>
		/// <returns>int</returns>
		public static int USER_ID(object user)
		{
			throw new NotSupportedException();
		}
		/// <summary>
		/// 返回当前用户的标识号
		/// </summary>
		/// <returns>int</returns>
		public static int USER_ID()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回与架构 ID 关联的架构名称。
		/// </summary>
		/// <param name="schema_id">架构的 ID。schema_id 的数据类型为 int。如果没有定义 schema_id，则 SCHEMA_NAME 将返回调用方的默认架构的名称。</param>
		/// <returns>如果 schema_id 不是有效 ID，则返回 NULL。</returns>
		public static string SCHEMA_NAME(object schema_id)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region 对称加密函数
		/// <summary>
		/// 使用对称密钥加密数据。
		/// </summary>
		/// <param name="key_GUID">用于加密 cleartext 的密钥的 GUID。uniqueidentifier.</param>
		/// <param name="cleartext">要使用密钥加密的数据。</param>
		/// <param name="add_authenticator">指示是否将验证器与 cleartext 一起加密。在使用验证器时必须为 1。int.</param>
		/// <param name="authenticator">派生验证器的数据</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		/// <remarks>
		/// 验证器有助于禁止对加密字段进行整个值替换。
		/// 如果加密数据时指定了验证器参数，则使用 DecryptByKey 对数据进行解密时，需要相同的验证器。在加密时，验证器的哈希将与纯文本一起加密。解密时，必须将同一验证器传递给 DecryptByKey。如果这两个验证器不匹配，则解密会失败。这指示该值在加密值之后已发生移动。建议使用存储结果的表的主键作为此参数的值
		/// 对称加密和解密速度相对较快，适于处理大量数据。
		/// </remarks>
		public static object EncryptByKey(object key_GUID, object cleartext, object add_authenticator, object authenticator)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 使用对称密钥加密数据。
		/// </summary>
		/// <param name="key_GUID">用于加密 cleartext 的密钥的 GUID。uniqueidentifier.</param>
		/// <param name="cleartext">要使用密钥加密的数据。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		public static object EncryptByKey(object key_GUID, object cleartext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 使用对称密钥对数据进行解密。
		/// </summary>
		/// <param name="ciphertext">已使用密钥进行加密的数据。ciphertext 的数据类型为 varbinary。</param>
		/// <param name="add_authenticator">指示是否与明文一起加密验证器。对数据进行加密时，该值必须与传递给 EncryptByKey 的值相同。add_authenticator 的数据类型为 int。</param>
		/// <param name="authenticator">用于生成验证器的数据。必须与提供给 EncryptByKey 的值相匹配。authenticator 的数据类型为 sysname。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		/// <remarks>DecryptByKey 使用对称密钥。该对称密钥必须已经在数据库中打开。可以同时打开多个密钥。不必只在解密密码之前才打开密钥。
		/// 对称加密和解密速度相对较快，适于处理大量数据。
		/// </remarks>
		public static object DecryptByKey(object ciphertext, object add_authenticator, object authenticator)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 用通行短语加密数据。
		/// </summary>
		/// <param name="passphrase">用于生成对称密钥的通行短语。</param>
		/// <param name="cleartext">要加密的明文。</param>
		/// <param name="add_authenticator">指示是否将验证器与明文一起加密。如果将添加验证器，则为 1。int. </param>
		/// <param name="authenticator">用于派生验证器的数据。sysname. </param>
		/// <returns>varbinary（最大大小为 8000 个字节）。</returns>
		/// <remarks>
		/// 通行短语是包含空格的密码。
		/// 使用通行短语的优点在于，与相对较长的字符串相比，有含义的短语或句子更容易记忆。此函数不检查密码复杂性。
		/// </remarks>
		public static object EncryptByPassPhrase(object passphrase, object cleartext, object add_authenticator, object authenticator)
		{
			throw new NotSupportedException();
		}


		/// <summary>
		/// 用通行短语加密数据。
		/// </summary>
		/// <param name="passphrase">用于生成对称密钥的通行短语。</param>
		/// <param name="cleartext">要加密的明文。</param>
		/// <returns>varbinary（最大大小为 8000 个字节）。</returns>
		/// <remarks>
		/// 通行短语是包含空格的密码。
		/// 使用通行短语的优点在于，与相对较长的字符串相比，有含义的短语或句子更容易记忆。此函数不检查密码复杂性。
		/// </remarks>
		public static object EncryptByPassPhrase(object passphrase, object cleartext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 对使用通行短语加密的数据进行解密。
		/// </summary>
		/// <param name="passphrase">将用于生成解密密钥的通行短语。</param>
		/// <param name="ciphertext">要解密的加密文本。</param>
		/// <param name="add_authenticator">指示是否将验证器与明文一起加密。如果将添加验证器，则为 1。int. </param>
		/// <param name="authenticator">用于派生验证器的数据。sysname. </param>
		/// <returns>varbinary（最大大小为 8000 个字节）。</returns>
		/// <remarks>
		/// 执行该函数无需任何权限。
		/// 如果使用了错误的通行短语或验证器信息，则返回 NULL。
		/// 该通行短语用于生成一个解密密钥，该密钥不会持久化。
		/// 如果对加密文本进行解密时包括验证器，则必须在解密时提供该验证器。如果解密时提供的验证器值与使用数据加密的验证器值不匹配，则解密将失败。
		/// </remarks>
		public static object DecryptByPassPhrase(object passphrase, object ciphertext, object add_authenticator, object authenticator)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 对使用通行短语加密的数据进行解密。
		/// </summary>
		/// <param name="passphrase">将用于生成解密密钥的通行短语。</param>
		/// <param name="ciphertext">要解密的加密文本。</param>
		/// <returns>varbinary（最大大小为 8000 个字节）。</returns>
		/// <remarks>
		/// 执行该函数无需任何权限。
		/// 如果使用了错误的通行短语或验证器信息，则返回 NULL。
		/// 该通行短语用于生成一个解密密钥，该密钥不会持久化。
		/// 如果对加密文本进行解密时包括验证器，则必须在解密时提供该验证器。如果解密时提供的验证器值与使用数据加密的验证器值不匹配，则解密将失败。
		/// </remarks>
		public static object DecryptByPassPhrase(object passphrase, object ciphertext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回当前数据库中对称密钥的 ID。
		/// </summary>
		/// <param name="Key_Name">数据库中对称密钥的名称。</param>
		/// <returns>int</returns>
		/// <remarks>
		/// 临时密钥的名称必须以数字符号 (#) 开头。
		/// </remarks>
		public static int Key_ID(object Key_Name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回数据库中对称密钥的 GUID。
		/// </summary>
		/// <param name="Key_Name">数据库中对称密钥的名称。</param>
		/// <returns>uniqueidentifier</returns>
		/// <remarks>
		/// 如果创建密钥时指定了标识值，则其 GUID 为该标识值的 MD5 哈希。如果未指定标识值，则服务器生成 GUID。
		/// 如果密钥为临时密钥，则密钥名称必须以数字符号 (#) 开头。
		/// </remarks>
		public static object Key_GUID(object Key_Name)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region 非对称加密函数
		/// <summary>
		/// 使用非对称密钥加密数据。
		/// </summary>
		/// <param name="Asym_Key_ID">数据库中非对称密钥的 ID。int.</param>
		/// <param name="plaintext">这是类型为 nvarchar、char、varchar、binary、varbinary 或 nchar 的，包含要用非对称密钥加密的数据。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		/// <remarks>
		/// 与使用对称密钥进行加密和解密相比，使用非对称密钥进行加密和解密时的系统开销要高得多。建议您不要使用非对称密钥加密大型数据集，例如表中的用户数据。而应该使用强对称密钥加密数据并使用非对称密钥加密对称密钥。
		/// </remarks>
		public static object EncryptByAsmKey(object Asym_Key_ID, object plaintext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 使用非对称密钥解密数据。
		/// </summary>
		/// <param name="Asym_Key_ID">数据库中非对称密钥的 ID。int.</param>
		/// <param name="ciphertext">这是类型为 varbinary 的，其中包含已用非对称密钥加密的数据。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		/// <remarks>
		/// 与使用对称密钥进行加密和解密相比，使用非对称密钥进行加密和解密时的系统开销要高得多。当处理大型数据集（例如表中的用户数据）时，不推荐使用非对称密钥。
		/// </remarks>
		public static object DecryptByAsmKey(object Asym_Key_ID, object ciphertext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 使用证书的公钥加密数据。
		/// </summary>
		/// <param name="certificate_ID">数据库中证书的 ID。int.</param>
		/// <param name="cleartext">类型为 nvarchar、char、varchar、binary、varbinary 或 nchar 的，其中包含将以证书的公钥进行加密的数据</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		/// <remarks>
		/// 该函数使用证书的公钥对数据进行加密。只能使用相应的私钥对加密文本进行解密。此类非对称转换较比使用对称密钥进行加密和解密的方法，其开销更大。因此，建议在处理大型数据集（如多个表中的用户数据）时不使用非对称加密。
		/// </remarks>
		public static object EncryptByCert(object certificate_ID, object cleartext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 用证书的私钥解密数据。
		/// </summary>
		/// <param name="certificate_ID">数据库中证书的 ID。int.</param>
		/// <param name="ciphertext">已用证书的公钥加密的数据的字符串。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		/// <remarks>
		/// 该函数使用证书的公钥对数据进行加密。只能使用相应的私钥对加密文本进行解密。此类非对称转换较比使用对称密钥进行加密和解密的方法，其开销更大。因此，建议在处理大型数据集（如多个表中的用户数据）时不使用非对称加密。
		/// </remarks>
		public static object DecryptByCert(object certificate_ID, object ciphertext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回证书的 ID。
		/// </summary>
		/// <param name="cert_name">数据库中证书的名称。</param>
		/// <returns>int</returns>
		public static object Cert_ID(object cert_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回非对称密钥的 ID。
		/// </summary>
		/// <param name="Asym_Key_Name">数据库中非对称密钥的名称。</param>
		/// <returns>int</returns>
		public static object AsymKey_ID(object Asym_Key_Name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定证书属性的值。
		/// </summary>
		/// <param name="Cert_ID">证书 ID。Cert_ID 的数据类型为 int。</param>
		/// <param name="PropertyName">
		/// 属性名称，必须以单引号 (') 括起。
		/// </param>
		/// <returns>
		/// 返回类型取决于在函数调用中指定的属性。所有的返回值都包装在 sql_variant 返回类型中。
		/// Expiry_Date 和 Start_Date 返回 datetime。
		/// Cert_Serial_Number、Issuer_Name、Subject 和 String_SID 返回 nvarchar。
		/// SID 返回 varbinary。
		/// </returns>
		/// <remarks>
		/// PropertyName的值
		/// 'Expiry_Date':证书的失效日期。
		/// 'Start_Date':证书开始生效的日期。
		/// 'Issuer_Name':证书颁发者的名称。
		/// 'Cert_Serial_Number':证书序列号。
		/// 'Subject':证书的主题。
		/// 'SID':证书的 SID。这也是映射到该证书的所有登录或用户的 SID。
		/// 'String_SID':字符串形式的证书的 SID。这也是映射到该证书的所有登录或用户的 SID。
		/// </remarks>
		public static object CertProperty(object Cert_ID, object PropertyName)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region 签名和签名验证
		/// <summary>
		/// 当前数据库中非对称密钥的 ID。Asym_Key_ID 的数据类型为 int。
		/// </summary>
		/// <param name="Asym_Key_ID">当前数据库中非对称密钥的 ID。Asym_Key_ID 的数据类型为 int。</param>
		/// <param name="plaintext">类型为 nvarchar、char、varchar 或 nchar 的变量，其中包含将使用非对称密钥进行签名的数据。</param>
		/// <param name="password">用于保护私钥的密码。password 的数据类型为 nvarchar(128)。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		public static object SignByAsymKey(object Asym_Key_ID, object plaintext, object password)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 当前数据库中非对称密钥的 ID。Asym_Key_ID 的数据类型为 int。
		/// </summary>
		/// <param name="Asym_Key_ID">当前数据库中非对称密钥的 ID。Asym_Key_ID 的数据类型为 int。</param>
		/// <param name="plaintext">类型为 nvarchar、char、varchar 或 nchar 的变量，其中包含将使用非对称密钥进行签名的数据。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		public static object SignByAsymKey(object Asym_Key_ID, object plaintext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 测试经过数字签名的数据在签名之后是否发生了更改。
		/// </summary>
		/// <param name="Asym_Key_ID">数据库中非对称密钥证书的 ID。</param>
		/// <param name="clear_text">正在验证的明文数据。</param>
		/// <param name="signature">附加到已签名数据中的签名。varbinary。</param>
		/// <returns>如果签名匹配，则返回 1，否则返回 0。</returns>
		public static int VerifySignedByAsmKey(object Asym_Key_ID, object clear_text, object signature)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 使用证书对文本进行签名并返回签名。
		/// </summary>
		/// <param name="certificate_ID">当前数据库中证书的 ID。certificate_ID 的数据类型为 int。</param>
		/// <param name="cleartext">类型为 nvarchar、char、varchar 或 nchar 的变量，其中包含要签名的数据。</param>
		/// <param name="password">用来对证书私钥进行加密的密码。password 的数据类型为 nvarchar(128)。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		public static object SignByCert(object certificate_ID, object cleartext, object password)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 使用证书对文本进行签名并返回签名。
		/// </summary>
		/// <param name="certificate_ID">当前数据库中证书的 ID。certificate_ID 的数据类型为 int。</param>
		/// <param name="cleartext">类型为 nvarchar、char、varchar 或 nchar 的变量，其中包含要签名的数据。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		public static object SignByCert(object certificate_ID, object cleartext)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 测试经过数字签名的数据在签名之后是否发生了更改。
		/// </summary>
		/// <param name="Cert_ID">数据库中证书的 ID。int</param>
		/// <param name="signed_data">类型为 nvarchar、char、varchar 或 nchar 的变量，上述类型包含已使用证书进行签名的数据。</param>
		/// <param name="signature">附加到已签名数据中的签名。varbinary。</param>
		/// <returns></returns>
		public static int VerifySignedByCert(object Cert_ID, object signed_data, object signature)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region 含自动密钥处理的对称解密
		/// <summary>
		/// 使用通过证书自动解密的对称密钥进行解密。
		/// </summary>
		/// <param name="cert_ID">用于保护对称密钥的证书的 ID。cert_ID 的数据类型为 int。</param>
		/// <param name="cert_password">用于保护证书私钥的密码。如果私钥受数据库主密钥保护，则该值可以是 NULL。cert_password 的数据类型为 varchar。</param>
		/// <param name="ciphertext">它是使用密钥进行加密的数据。ciphertext 的数据类型为 varbinary。</param>
		/// <param name="add_authenticator">指示是否与明文一起加密验证器。对数据进行加密时，该值必须与传递给 EncryptByKey 的值相同。如果使用了验证器，则为 1。add_authenticator 的数据类型为 int。</param>
		/// <param name="authenticator">从中生成验证器的数据。必须与提供给 EncryptByKey 的值相匹配。authenticator 的数据类型为 sysname。</param>
		/// <returns>最大大小为 8,000 个字节的 varbinary。</returns>
		/// <remarks>
		/// DecryptByKeyAutoCert 组合了 OPEN SYMMETRIC KEY 和 DecryptByKey 的功能。在单个操作中，它可以解密对称密钥，并使用该密钥解密密码文本。
		/// </remarks>
		public static object DecryptByKeyAutoCert(object cert_ID, object cert_password, object ciphertext, object add_authenticator, object authenticator)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region 元数据函数

		/// <summary>
		/// 返回 Transact-SQL 当前模块的对象标识符 (ID)。Transact-SQL 模块可以是存储过程、用户定义函数或触发器。不能在 CLR 模块或进程内数据访问接口中指定 @@PROCID 。
		/// </summary>
		public static object PROCID
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// 返回有关程序集的属性的信息。
		/// </summary>
		/// <param name="assembly_name">程序集的名称。</param>
		/// <param name="property_name">
		/// 要检索其有关信息的属性的名称。值定义可参考备注。
		/// </param>
		/// <returns>sql_variant</returns>
		/// <remarks>property_name 可以为下列值之一。
		/// CultureInfo:程序集的区域设置。
		/// PublicKey:程序集的公钥或公钥令牌。
		/// MvID:由编译器生成的完整的程序集版本标识号。
		/// VersionMajor:由四部分组成的程序集版本标识号的主要组成部分（第一部分）。
		/// VersionMinor:由四部分组成的程序集版本标识号的次要组成部分（第二部分）。
		/// VersionBuild:由四部分组成的程序集版本标识号的内部版本组成部分（第三部分）。
		/// VersionRevision:由四部分组成的程序集版本标识号的修订版组成部分（第四部分）。
		/// SimpleName:程序集的简称。
		/// Architecture:程序集的处理器体系结构。
		/// CLRName:对程序集的简单名称、版本号、区域性、公钥以及体系结构进行编码的规范字符串。该值唯一地标识公共语言运行时 (CLR) 端的程序集。
		/// </remarks>
		public static object ASSEMBLYPROPERTY(object assembly_name, object property_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回有关全文目录属性的信息。
		/// </summary>
		/// <param name="catalog_name">包含全文目录名称的表达式。</param>
		/// <param name="property">
		/// 包含全文目录属性名称的表达式。属性及返回的信息的说明参考备注。
		/// </param>
		/// <returns>int</returns>
		/// <remarks>
		/// AccentSensitivity:区分重音设置。0 = 不区分重音，1 = 区分重音 
		/// IndexSize:全文目录的逻辑大小 (MB)。有关详细信息，请参阅本主题后面的“备注”。 
		/// ItemCount:全文目录中当前全文索引项的数目。 
		/// LogSize:仅为保持向后兼容性。总是返回 0。与 Microsoft Search 服务全文目录关联的错误日志组合集的大小，以字节为单位。
		/// MergeStatus:是否正在进行主合并。0 = 未进行主合并，1 = 正在进行主合并 
		/// PopulateCompletionAge:上一次全文索引填充的完成时间与 01/01/1990 00:00:00 之间的时间差（秒）。仅针对完全和增量爬网填充进行了更新。如果未发生填充，则返回 0。
		/// PopulateStatus:0 = 空闲，1 = 正在进行完全填充，2 = 已暂停，3 = 已中止，4 = 正在恢复，5 = 关闭，6 = 正在进行增量填充，7 = 正在生成索引，8 = 磁盘已满。已暂停。9 = 更改跟踪 
		/// UniqueKeyCount:全文目录中的唯一键数。 
		/// ImportStatus:是否将导入全文目录。0 = 不将导入全文目录。1 = 将导入全文目录。
		/// </remarks>
		public static int FULLTEXTCATALOGPROPERTY(object catalog_name, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回列的定义长度（以字节为单位）。
		/// </summary>
		/// <param name="table">要确定其列长度信息的表的名称。table 是 nvarchar 类型的表达式。</param>
		/// <param name="column">要确定其长度的列的名称。column 是 nvarchar 类型的表达式。</param>
		/// <returns>smallint</returns>
		public static int COL_LENGTH(object table, object column)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回与全文引擎属性有关的信息。可以使用 sp_fulltext_service 设置和检索这些属性。
		/// </summary>
		/// <param name="property">
		/// 包含全文服务级别属性名称的表达式。属性及返回信息的说明参考备注。
		/// </param>
		/// <returns>int</returns>
		/// <remarks>
		/// ResourceUsage:返回 0。支持此属性只是为了保持向后兼容性。
		/// ConnectTimeout:返回 0。支持此属性只是为了保持向后兼容性。
		/// IsFulltextInstalled:在 SQL Server 的当前实例中安装全文组件。0 = 未安装全文组件。1 = 已安装全文组件。NULL = 输入无效或发生错误。
		/// DataTimeout:返回 0。支持此属性只是为了保持向后兼容性。 
		/// LoadOSResources:指示此 SQL Server 实例中是否注册并使用了操作系统断字符和筛选器。默认情况下，禁用此属性，以防止更新程序因疏忽而对操作系统 (OS) 的行为进行更改。如果允许使用 OS 资源，则可以访问在 Microsoft 索引服务中注册的语言和文档类型的资源，但不能安装特定于实例的资源。如果允许加载 OS 资源，请确保 OS 资源是受信任的已签名二进制文件；否则，当 VerifySignature 设置为 1 时，将无法加载它们。0 = 仅使用特定于此 SQL Server 实例的筛选器和断字符。1 = 加载 OS 筛选器和断字符。
		/// VerifySignature:指定 Microsoft Search 服务是否仅加载已签名的二进制文件。默认情况下，仅加载已签名的可信二进制文件。0 = 不检查二进制文件是否已签名。1 = 验证是否仅加载了已签名的可信二进制文件。
		/// </remarks>
		public static int FULLTEXTSERVICEPROPERTY(object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 根据指定的对应表标识号和列标识号返回列的名称。
		/// </summary>
		/// <param name="table_id">包含列的表的标识号。table_id 的类型为 int。</param>
		/// <param name="column_id">列的标识号。column_id 参数的类型为 int。</param>
		/// <returns>sysname</returns>
		public static string COL_NAME(object table_id, object column_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回索引列名称。对于 XML 索引，返回 NULL。
		/// </summary>
		/// <param name="table_or_view_name">表或索引视图的名称。table_or_view_name 必须使用单引号分隔，并且可由数据库名称和架构名称完全限定。</param>
		/// <param name="index_id">索引的 ID。index_ID 的数据类型为 int。</param>
		/// <param name="key_id">索引键列的位置。key_ID 的数据类型为 int。</param>
		/// <returns>nvarchar (128 ) </returns>
		public static string INDEX_COL(object table_or_view_name, object index_id, object key_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回有关列或过程参数的信息。
		/// </summary>
		/// <param name="id">一个表达式，其中包含表或过程的标识符 (ID)。</param>
		/// <param name="column">一个表达式，其中包含列或参数的名称。</param>
		/// <param name="property">
		/// 一个表达式，其中包含要为 id 返回的信息，值定义请参考备注。
		/// </param>
		/// <returns>int</returns>
		/// <remarks>
		/// AllowsNull:允许空值。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// ColumnId:对应于 sys.columns.column_id 的列 ID 值。 列 ID，注意： 查询多列时，列 ID 值的序列中可能出现间隔。 
		/// FullTextTypeColumn:表中的 TYPE COLUMN，其中包含 column 的文档类型信息。 列的全文 TYPE COLUMN 的 ID，作为此属性的第二个参数传递。 
		/// IsComputed:列是计算列。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// IsCursorType:过程参数类型为 CURSOR。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// IsDeterministic:列是确定性列。此属性只适用于计算列和视图列。 1 = TRUE，0 = FALSE，NULL = 输入无效。非计算列或视图列。 
		/// IsFulltextIndexed:列已经注册为全文索引。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// IsIdentity:列使用 IDENTITY 属性。1 = TRUE，0 = FALSE。 NULL = 输入无效。 
		/// IsIdNotForRepl:列检查 IDENTITY_INSERT 设置。如果指定了 IDENTITY NOT FOR REPLICATION，则不检查 IDENTITY_INSERT 设置。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// IsIndexable:可以对列进行索引。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// IsOutParam:过程参数是输出参数。1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// IsPrecise:列是精确列。此属性只适用于确定性列。 1 = TRUE，0 = FALSE。 NULL = 输入无效。不是确定性列 
		/// IsRowGuidCol:列具有 uniqueidentifier 数据类型，并且定义了 ROWGUIDCOL 属性。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// IsSystemVerified:列的确定性和精度属性可以使用 数据库引擎 验证。此属性只应用于计算列和视图中的列。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// IsXmlIndexable:可以在 XML 索引中使用 XML 列。 1 = TRUE，0 = FALSE，NULL = 输入无效。 
		/// Precision:列或参数的数据类型的长度。 指定的列数据类型的长度，-1 = xml 或大值类型，NULL = 输入无效。 
		/// Scale:列或参数的数据类型的小数位数。 小数位数，NULL = 输入无效。 
		/// SystemDataAccess:列是由访问 SQL Server 的系统目录或虚拟系统表中数据的函数派生的。此属性只应用于计算列和视图中的列。 1 = TRUE（指示只读访问。），0 = FALSE，NULL = 输入无效。 
		/// UserDataAccess:列是由访问储存于 SQL Server 本地实例的用户表中数据的函数派生的。此属性只应用于计算列和视图中的列。 1 = TRUE（指示只读访问。），0 = FALSE，NULL = 输入无效。 
		/// UsesAnsiTrim:第一次创建表时，ANSI_PADDING 设置为 ON。此属性仅应用于列或者 char 或 varchar 类型的参数。 1= TRUE，0= FALSE，NULL = 输入无效。 
		/// IsSparse:列为稀疏列。有关详细信息，请参阅使用稀疏列。 1= TRUE，0= FALSE，NULL = 输入无效。 
		/// IsColumnSet:列为列集。有关详细信息，请参阅使用列集。 1= TRUE，0= FALSE，NULL = 输入无效。
		/// </remarks>
		public static int COLUMNPROPERTY(object id, object column, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回有关索引键的信息。对于 XML 索引，返回 NULL。
		/// </summary>
		/// <param name="object_ID">表或索引视图的对象标识号。object_ID 的数据类型为 int。</param>
		/// <param name="index_ID">索引标识号。index_ID 的数据类型为 int。</param>
		/// <param name="key_ID">索引键列的位置。key_ID 的数据类型为 int。</param>
		/// <param name="property">要返回其信息的属性的名称。property 是字符串，可以是下列值之一：
		/// ColumnId  索引的 key_ID 位置上的列 ID。
		/// IsDescending  存储索引列的排序顺序。1 = 降序 0 = 升序
		/// </param>
		/// <returns>int</returns>
		public static int INDEXKEY_PROPERTY(object object_ID, object index_ID, object key_ID, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定数据库和属性名的命名数据库属性值。
		/// </summary>
		/// <param name="database">一个表达式，包含要返回其命名属性信息的数据库名。 database 是 nvarchar(128)。</param>
		/// <param name="property">一个表达式，包含要返回的数据库属性的名称。值定义可参考备注
		/// </param>
		/// <returns>int</returns>
		/// <remarks>property 的数据类型为 varchar(128)，可以是下列值之一：
		///IsAnsiNullDefault:数据库遵循ISO规则，允许Null值。1=TRUE，0=FALSE，NULL=输入无效
		///IsAnsiNullsEnabled:所有与Null的比较将取值为未知。1=TRUE，0=FALSE，NULL=输入无效
		///IsAnsiWarningsEnabled:如果发生了标准错误条件，则将发出错误消息或警告消息。1=TRUE，0=FALSE，NULL=输入无效
		///IsAutoClose:数据库在最后一位用户退出后完全关闭并释放资源。1=TRUE，0=FALSE，NULL=输入无效
		///IsAutoCreateStatistics:如果表中数据更改造成统计信息过期，则自动更新现有统计信息。1=TRUE，0=FALSE，NULL=输入无效
		///IsAutoShrink:数据库文件可以自动定期收缩。1=TRUE，0=FALSE，NULL=输入无效
		///IsAutoUpdateStatistics:启用自动更新统计信息数据库选项。1=TRUE，0=FALSE，NULL=输入无效
		///IsBulkCopy:数据库允许无日志记录的操作。1=TRUE，0=FALSE，NULL=输入无效
		///IsCloseCursorsOnCommitEnabled:提交事务时打开的游标已关闭。1=TRUE，0=FALSE，NULL=输入无效
		///IsDboOnly:数据库处于仅DBO-only访问模式。1=TRUE，0=FALSE，NULL=输入无效
		///IsDetached:分离操作分离了数据库。1=TRUE，0=FALSE，NULL=输入无效
		///IsEmergencyMode:启用紧急模式，允许使用可疑数据库。1=TRUE，0=FALSE，NULL=输入无效
		///IsFulltextEnabled:数据库已启用全文功能。1=TRUE，0=FALSE，NULL=输入无效
		///IsInLoad:正在装载数据库。1=TRUE，0=FALSE，NULL=输入无效
		///IsInRecovery:正在恢复数据库。1=TRUE，0=FALSE，NULL=输入无效
		///IsInStandBy:数据库以只读方式联机，并允许还原日志。1=TRUE，0=FALSE，NULL=输入无效
		///IsLocalCursorsDefault:游标声明默认为LOCAL。1=TRUE，0=FALSE，NULL=输入无效
		///IsNotRecovered:数据库不能恢复。1=TRUE，0=FALSE，NULL=输入无效
		///IsNullConcat:Null串联操作数产生NULL。1=TRUE，0=FALSE，NULL=输入无效
		///IsOffline:数据库脱机。1=TRUE，0=FALSE，NULL=输入无效
		///IsParameterizationForced:PARAMETERIZATION数据库SET选项为FORCED。1=TRUE，0=FALSE，NULL=输入无效
		///IsQuotedIdentifiersEnabled:可对标识符使用英文双引号。1=TRUE，0=FALSE，NULL=输入无效
		///IsReadOnly:数据库处于只读访问模式。1=TRUE，0=FALSE，NULL=输入无效
		///IsRecursiveTriggersEnabled:已启用触发器递归触发。1=TRUE，0=FALSE，NULL=输入无效
		///IsShutDown:数据库启动时遇到问题。1=TRUE，0=FALSE，NULL1=输入无效
		///IsSingleUser:数据库处于单用户访问模式。1=TRUE，0=FALSE，NULL=输入无效
		///IsSuspect:数据库可疑。1=TRUE，0=FALSE，NULL=输入无效
		///IsTruncLog:数据库截断其登录检查点。1=TRUE，0=FALSE，NULL=输入无效
		///Version:创建数据库时使用的MicrosoftSQLServer代码的内部版本号。标识为仅供参考。不提供支持。不保证以后的兼容性。版本号=数据库处于打开状态。NULL=数据库关闭。
		///</remarks>
		public static int DATABASEPROPERTY(object database, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 根据指定的表标识号、索引或统计信息名称以及属性名称，返回已命名的索引或统计信息属性值。对于 XML 索引，返回 NULL。
		/// </summary>
		/// <param name="object_ID">一个表达式，包含要为其提供索引属性信息的表或索引视图的对象标识号。object_ID 的数据类型为 int。</param>
		/// <param name="index_or_statistics_name">一个表达式，包含要为其返回属性信息的索引或统计信息的名称。index_or_statistics_name 的数据类型为 nvarchar(128)。</param>
		/// <param name="property">一个表达式，包含要返回的数据库属性的名称。property 的数据类型为 varchar(128)，值定义请参考备注。
		/// </param>
		/// <returns>int</returns>
		/// <remarks>
		/// IndexDepth:索引的深度。索引级别数。NULL=XML索引或输入无效。
		/// IndexFillFactor:创建索引或最后重新生成索引时使用的填充因子值。填充因子
		/// IndexID:指定表或索引视图上索引的索引ID。索引ID
		/// IsAutoStatistics:统计信息是由ALTERDATABASE的AUTO_CREATE_STATISTICS选项生成的。1=True，0=False或XML索引。
		/// IsClustered:索引是聚集的。1=True，0=False或XML索引。
		/// IsDisabled:索引被禁用。1=True，0=False，NULL=输入无效。
		/// IsFulltextKey:索引是表的全文键。1=True，0=False或XML索引。NULL=输入无效。
		/// IsHypothetical:索引是假设的，不能直接用作数据访问路径。假设索引包含列级统计信息，由数据库引擎优化顾问维护和使用。1=True，0=False或XML索引，NULL=输入无效。
		/// IsPadIndex:索引指定每个内部节点上将要保持空闲的空间。1=True，0=False或XML索引。
		/// IsPageLockDisallowed:通过ALTERINDEX的ALLOW_PAGE_LOCKS选项设置的页锁定值。:1=不允许页锁定。0=允许页锁定。NULL=输入无效。
		/// IsRowLockDisallowed:通过ALTERINDEX的ALLOW_ROW_LOCKS选项设置的行锁定值。:1=不允许行锁定。0=允许行锁定。NULL=输入无效。
		/// IsStatistics:index_or_statistics_name是通过CREATESTATISTICS语句或ALTERDATABASE的AUTO_CREATE_STATISTICS选项创建的统计信息。1=True，0=False或XML索引。
		/// IsUnique:索引是唯一的。1=True，0=False或XML索引。
		/// </remarks>
		public static int INDEXPROPERTY(object object_ID, object index_or_statistics_name, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定数据库的指定数据库选项或属性的当前设置。
		/// </summary>
		/// <param name="database">表示要为其返回命名属性信息的数据库的名称。 database 的数据类型为 nvarchar(128)。</param>
		/// <param name="property">表示要返回的数据库属性的名称的表达式。property 的数据类型为 varchar(128)，定义可参考备注。
		/// </param>
		/// <returns>sql_variant</returns>
		/// <remarks>
		/// Collation：数据库的默认排序规则名称。排序规则名称，NULL=数据库没有启动。基本数据类型：nvarchar(128)
		/// ComparisonStyle：排序规则的Windows比较样式。ComparisonStyle是通过使用以下值计算得到的位图：忽略大小写=1，忽略重音=2，忽略Kana=65536，忽略宽度=131072；例如，196609的默认值是将忽略大小写、忽略Kana和忽略宽度选项合并在一起的结果。返回比较样式。对所有二进制排序规则均返回0。基本数据类型：int
		/// IsAnsiNullDefault：数据库遵循ISO规则，允许Null值。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsAnsiNullsEnabled：所有与Null的比较将取值为未知。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsAnsiPaddingEnabled：在比较或插入前，字符串将被填充到相同长度。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsAnsiWarningsEnabled：如果发生了标准错误条件，则将发出错误消息或警告消息。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsArithmeticAbortEnabled：如果执行查询时发生溢出或被零除错误，则将结束查询。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsAutoClose：数据库在最后一位用户退出后完全关闭并释放资源。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsAutoCreateStatistics：在查询优化期间自动生成优化查询所需的缺失统计信息。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsAutoShrink：数据库文件可以自动定期收缩。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsAutoUpdateStatistics：如果表中数据更改造成统计信息过期，则自动更新现有统计信息。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsCloseCursorsOnCommitEnabled：提交事务时打开的游标已关闭。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsFulltextEnabled：数据库已启用全文功能。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int；注意：此属性的值无效。用户数据库始终启用全文搜索。SQLServer的未来版本中将删除此列。请不要在新的开发工作中使用此列，并尽快修改当前还在使用任何这些列的应用程序。
		/// IsInStandBy：数据库以只读方式联机，并允许还原日志。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsLocalCursorsDefault：游标声明默认为LOCAL。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsMergePublished：如果安装了复制，则可以发布数据库表供合并复制。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsNullConcat：Null串联操作数产生NULL。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsNumericRoundAbortEnabled：表达式中缺少精度时将产生错误。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsParameterizationForced：PARAMETERIZATION数据库SET选项为FORCED。1=TRUE，0=FALSE，NULL=输入无效
		/// IsQuotedIdentifiersEnabled：可对标识符使用英文双引号。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsPublished：如果安装了复制，可以发布数据库表供快照复制或事务复制。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsRecursiveTriggersEnabled：已启用触发器递归触发。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsSubscribed：数据库已订阅发布。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsSyncWithBackup：数据库为发布数据库或分发数据库，并且在还原时不用中断事务复制。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// IsTornPageDetectionEnabled：SQLServer数据库引擎检测到因电力故障或其他系统故障造成的不完全I/O操作。1=TRUE，0=FALSE，NULL=输入无效，基本数据类型：int
		/// LCID：排序规则的Windows区域设置标识符(LCID)。LCID值（十进制格式）。基本数据类型：int,若要查看LCID值的列表（十六进制格式），请参阅安装程序中的排序规则设置。
		/// Recovery：数据库的恢复模式。FULL=完整恢复模式，BULK_LOGGED=大容量日志记录模型，SIMPLE=简单恢复模式，基本数据类型：nvarchar(128)
		/// SQLSortOrder：SQLServer早期版本中支持的SQLServer排序顺序ID。0=数据库使用的是Windows排序规则，>0=SQLServer排序顺序ID，NULL=输入无效或数据库未启动，基本数据类型：tinyint
		/// Status：数据库状态。ONLINE=数据库可用于查询。OFFLINE=数据库已被显式置于脱机状态。RESTORING=正在还原数据库。RECOVERING=正在恢复数据库，尚不能用于查询。SUSPECT=数据库未恢复。EMERGENCY=数据库处于紧急只读状态。只有sysadmin成员可进行访问。基本数据类型：nvarchar(128)
		/// Updateability：指示是否可修改数据。READ_ONLY=可读取但不能修改数据。READ_WRITE=可读取和修改数据。基本数据类型：nvarchar(128)。
		/// UserAccess：指示哪些用户可以访问数据库。SINGLE_USER=一次仅一个db_owner、dbcreator或sysadmin用户，RESTRICTED_USER=仅限db_owner、dbcreator和sysadmin角色的成员，MULTI_USER=所有用户，基本数据类型：nvarchar(128)
		/// Version：用于创建数据库的SQLServer代码的内部版本号。标识为仅供参考。不提供支持。不保证以后的兼容性。版本号=数据库处于打开状态。NULL=数据库没有启动。基本数据类型：int
		/// </remarks>
		public static object DATABASEPROPERTYEX(object database, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回架构范围内对象的数据库对象标识号。
		/// </summary>
		/// <param name="object_name">要使用的对象。object_name 的数据类型为 varchar 或 nvarchar。如果 object_name 的数据类型为 varchar，则它将隐式转换为 nvarchar。可以选择是否指定数据库和架构名称。</param>
		/// <param name="object_type">架构范围的对象类型。object_type 的数据类型为 varchar 或 nvarchar。如果 object_type 的数据类型为 varchar，则它将隐式转换为 nvarchar。有关对象类型的列表，请参阅 sys.objects (Transact-SQL) 中的 type 列。</param>
		/// <returns>int</returns>
		public static int OBJECT_ID(object object_name, object object_type)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回架构范围内对象的数据库对象标识号。
		/// </summary>
		/// <param name="object_name">要使用的对象。object_name 的数据类型为 varchar 或 nvarchar。如果 object_name 的数据类型为 varchar，则它将隐式转换为 nvarchar。可以选择是否指定数据库和架构名称。</param>
		/// <returns>int</returns>
		public static int OBJECT_ID(object object_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回数据库标识 (ID) 号。
		/// </summary>
		/// <param name="database_name">用于返回对应的数据库 ID 的数据库名称。database_name 的数据类型为 sysname。</param>
		/// <returns>int</returns>
		public static int DB_ID(object database_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回当前数据库标识 (ID) 号。
		/// </summary>
		/// <returns>int</returns>
		public static int DB_ID()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回架构范围内对象的数据库对象名称。有关架构范围内对象的列表，请参阅 sys.objects (Transact-SQL)。
		/// </summary>
		/// <param name="object_id">要使用的对象的 ID。object_id 的数据类型为 int，并假定为指定数据库或当前数据库上下文中的架构范围内的对象。</param>
		/// <param name="database_id">要在其中查找对象的数据库的 ID。database_id 的数据类型为 int。</param>
		/// <returns>sysname </returns>
		public static string OBJECT_NAME(object object_id, object database_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回架构范围内对象的数据库对象名称。有关架构范围内对象的列表，请参阅 sys.objects (Transact-SQL)。
		/// </summary>
		/// <param name="object_id">要使用的对象的 ID。object_id 的数据类型为 int，并假定为指定数据库或当前数据库上下文中的架构范围内的对象。</param>
		/// <returns>sysname </returns>
		public static string OBJECT_NAME(object object_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回数据库名称。
		/// </summary>
		/// <param name="database_id">要返回的数据库的标识号 (ID)。database_id 的数据类型为 int，无默认值。</param>
		/// <returns>nvarchar(128)</returns>
		public static string DB_NAME(object database_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回当前数据库名称。
		/// </summary>
		/// <returns>nvarchar(128)</returns>
		public static string DB_NAME()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回当前数据库中架构范围内的对象的有关信息。有关架构范围内对象的列表，请参阅 sys.objects (Transact-SQL)。不能将此函数用于不属于架构范围内的对象，如数据定义语言 (DDL) 触发器和事件通知。
		/// </summary>
		/// <param name="id">是表示当前数据库中对象 ID 的表达式。id 的数据类型为 int，并假定为当前数据库上下文中的架构范围内的对象。</param>
		/// <param name="property">
		/// 属性名称
		/// </param>
		/// <returns>int</returns>
		/// <remarks>
		/// CnstIsClustKey（约束）：具有聚集索引的PRIMARYKEY约束。1=TRUE，0=False
		/// CnstIsColumn（约束）：单个列上的CHECK、DEFAULT或FOREIGNKEY约束。1=TRUE，0=False
		/// CnstIsDeleteCascade（约束）：具有ONDELETECASCADE选项的FOREIGNKEY约束。1=TRUE，0=False
		/// CnstIsDisabled（约束）：禁用的约束。1=TRUE，0=False
		/// CnstIsNonclustKey（约束）：非聚集索引的PRIMARYKEY或UNIQUE约束。1=TRUE，0=False
		/// CnstIsNotRepl（约束）：使用NOTFORREPLICATION关键字定义的约束。1=TRUE，0=False
		/// CnstIsNotTrusted（约束）：启用约束时未检查现有行，因此可能不是所有行都适用该约束。1=TRUE，0=False
		/// CnstIsUpdateCascade（约束）：具有ONUPDATECASCADE选项的FOREIGNKEY约束。1=TRUE，0=False
		/// ExecIsAfterTrigger（触发器）：AFTER触发器。1=TRUE，0=False
		/// ExecIsAnsiNullsOn：Transact-SQL函数、Transact-SQL过程、Transact-SQL触发器、视图创建时的ANSI_NULLS设置。1=TRUE，0=False
		/// ExecIsDeleteTrigger（触发器）：DELETE触发器。1=TRUE，0=False
		/// ExecIsFirstDeleteTrigger（触发器）：对表执行DELETE时触发的第一个触发器。1=TRUE，0=False
		/// ExecIsFirstInsertTrigger（触发器）：对表执行INSERT时触发的第一个触发器。1=TRUE，0=False
		/// ExecIsFirstUpdateTrigger（触发器）：对表执行UPDATE时触发的第一个触发器。1=TRUE，0=False
		/// ExecIsInsertTrigger（触发器）：INSERT触发器。1=TRUE，0=False
		/// ExecIsInsteadOfTrigger（触发器）：INSTEADOF触发器。1=TRUE，0=False
		/// ExecIsLastDeleteTrigger（触发器）：对表执行DELETE时激发的最后一个触发器。1=TRUE，0=False
		/// ExecIsLastInsertTrigger（触发器）：对表执行INSERT时激发的最后一个触发器。1=TRUE，0=False
		/// ExecIsLastUpdateTrigger（触发器）：对表执行UPDATE时激发的最后一个触发器。1=TRUE，0=False
		/// ExecIsQuotedIdentOn：Transact-SQL函数、Transact-SQL过程、Transact-SQL触发器、视图创建时的QUOTED_IDENTIFIER设置。1=TRUE，0=False
		/// ExecIsStartup（过程）：启动过程。1=TRUE，0=False
		/// ExecIsTriggerDisabled（触发器）：禁用的触发器。1=TRUE，0=False
		/// ExecIsTriggerNotForRepl（触发器）：定义为NOTFORREPLICATION的触发器。1=TRUE，0=False
		/// ExecIsUpdateTrigger（触发器）：UPDATE触发器。1=TRUE，0=False
		/// HasAfterTrigger（表、视图）：表或视图具有AFTER触发器。1=TRUE，0=False
		/// HasDeleteTrigger（表、视图）：表或视图具有DELETE触发器。1=TRUE，0=False
		/// HasInsertTrigger（表、视图）：表或视图具有INSERT触发器。1=TRUE，0=False
		/// HasInsteadOfTrigger（表、视图）：表或视图具有INSTEADOF触发器。1=TRUE，0=False
		/// HasUpdateTrigger（表、视图）：表或视图具有UPDATE触发器。1=TRUE，0=False
		/// IsAnsiNullsOn：Transact-SQL函数、Transact-SQL过程、表、Transact-SQL触发器、视图指定表的ANSINULLS选项设置为ON。这表示所有对空值的比较都取值为UNKNOWN。只要表存在，此设置将应用于表定义中的所有表达式，包括计算列和约束。1=TRUE，0=False
		/// IsCheckCnst（架构范围内的任何对象）：CHECK约束。1=TRUE，0=False
		/// IsConstraint（架构范围内的任何对象）：列或表的单列CHECK、DEFAULT或FOREIGNKEY约束。1=TRUE，0=False
		/// IsDefault（架构范围内的任何对象）：绑定的默认值。1=TRUE，0=False
		/// IsDefaultCnst（架构范围内的任何对象）：DEFAULT约束。1=TRUE，0=False
		/// IsDeterministic（函数、视图）：函数或视图的确定性属性。1=确定，0=不确定
		/// IsEncrypted：Transact-SQL函数、Transact-SQL过程、表、Transact-SQL触发器和视图，指示模块语句的原始文本已转换为模糊格式。模糊代码的输出在SQLServer2005的任何目录视图中都不能直接显示。对系统表或数据库文件没有访问权限的用户不能检索模糊文本。但是，能够通过DAC端口访问系统表的用户或能够直接访问数据库文件的用户可以检索此文本。此外，能够向服务器进程附加调试器的用户可在运行时从内存中检索原始过程。1=已加密，0=未加密，基本数据类型：int
		/// IsExecuted（架构范围内的任何对象）：可执行对象（视图、过程、函数或触发器）。1=TRUE，0=False
		/// IsExtendedProc（架构范围内的任何对象）：扩展过程。1=TRUE，0=False
		/// IsForeignKey（架构范围内的任何对象）：FOREIGNKEY约束。1=TRUE，0=False
		/// IsIndexed（表、视图）：包含索引的表或视图。1=TRUE，0=False
		/// IsIndexable（表、视图）：可以创建索引的表或视图。1=TRUE，0=False
		/// IsInlineFunction（函数）：内联函数。1=内联函数，0=非内联函数
		/// IsMSShipped（架构范围内的任何对象）：安装SQLServer过程中创建的对象。1=TRUE，0=False
		/// IsPrimaryKey（架构范围内的任何对象）：PRIMARYKEY约束。1=TRUE，0=False
		/// NULL=非函数，或对象ID无效。
		/// IsProcedure（架构范围内的任何对象）：过程。1=TRUE，0=False
		/// IsQuotedIdentOn：Transact-SQL函数、Transact-SQL过程、表、Transact-SQL触发器、视图、CHECK约束、DEFAULT定义，指定对象的引号标识符设置为ON。这表示用英文双引号分隔对象定义中涉及的所有表达式中的标识符。1=ON，0=OFF
		/// IsQueue（架构范围内的任何对象）：ServiceBroker队列，1=TRUE，0=False
		/// IsReplProc（架构范围内的任何对象）：复制过程。1=TRUE，0=False
		/// IsRule（架构范围内的任何对象）：绑定规则。1=TRUE，0=False
		/// IsScalarFunction（函数）：标量值函数。1=标量值函数，0=非标量值函数
		/// IsSchemaBound（函数、视图）：使用SCHEMABINDING创建的绑定到架构的函数或视图。1=绑定到架构，0=不绑定架构。
		/// IsSystemTable（表）：系统表。1=TRUE，0=False
		/// IsTable（表）：表。1=TRUE，0=False
		/// IsTableFunction（函数）：表值函数。1=表值函数，0=非表值函数
		/// IsTrigger（架构范围内的任何对象）：触发器。1=TRUE，0=False
		/// IsUniqueCnst（架构范围内的任何对象）：UNIQUE约束。1=TRUE，0=False
		/// IsUserTable（表）：用户定义的表。1=TRUE，0=False
		/// IsView（视图）：视图。1=TRUE，0=False
		/// OwnerId（架构范围内的任何对象）：对象的所有者。注意：架构所有者不一定是对象所有者。例如，子对象（其parent_object_id为非空值）将始终返回与父对象相同的所有者ID。
		/// Nonnull=对象所有者的数据库用户ID。
		/// TableDeleteTrigger（表）：表具有DELETE触发器。>1=指定类型的第一个触发器的ID。
		/// TableDeleteTriggerCount（表）：表具有指定数目的DELETE触发器。>0=DELETE触发器数目。
		/// TableFullTextMergeStatus（表）：表所具有的全文索引当前是否正在合并。0=表没有全文索引，或者全文索引未在合并。1=全文索引正在合并。
		/// TableFullTextBackgroundUpdateIndexOn（表）：表已启用全文后台更新索引（自动更改跟踪）。1=TRUE，0=False
		/// TableFulltextCatalogId（表）：表的全文索引数据所在的全文目录的ID。非零=全文目录ID，它与全文索引表中标识行的唯一索引相关。0=表没有全文索引。
		/// TableFulltextChangeTrackingOn（表）：表已启用全文更改跟踪。1=TRUE，0=False
		/// TableFulltextDocsProcessed（表）：自开始全文索引以来所处理的行数。在为进行全文搜索而正在编制索引的表中，将一个行的所有列视为要编制索引的文档的一部分。0=没有完成的活动爬网或全文索引。>0=以下选项之一：自从开始完整、增量或手动更改跟踪填充以来，由插入或更新操作处理的文档数。自从执行以下操作以来由插入或更新操作处理的行数：启用具有后台更新索引填充功能的更改跟踪、更改全文索引架构、重建全文目录或重新启动SQLServer的实例等。NULL=表没有全文索引。注意：此属性不监视已删除行，也不对已删除行进行计数。
		/// TableFulltextFailCount（表）：全文搜索未编制索引的行数。0=填充已完成。>0=以下选项之一：自从开始完整、增量和手动更新更改跟踪填充以来未编制索引的文档数。对于具有后台更新索引功能的更改跟踪，则为自从开始填充或重新启动填充以来未编制索引的行数。这可能由架构更改、目录重建、服务器重新启动等引起。
		/// NULL=表没有全文索引。
		/// TableFulltextItemCount（表）：成功编制了全文索引的行数。
		/// TableFulltextKeyColumn（表）：与参与全文索引定义的单列唯一索引关联的列的ID。0=表没有全文索引。
		/// TableFulltextPendingChanges（表）：要处理的挂起更改跟踪项的数目。0=未启用更改跟踪。NULL=表没有全文索引。
		/// TableFulltextPopulateStatus（表）：0=空闲。1=正在进行完全填充。2=正在进行增量填充。3=正在传播所跟踪的更改。4=正在进行后台更新索引（例如，自动跟踪更改）。5=全文索引已中止或暂停。
		/// TableHasActiveFulltextIndex（表）：表具有活动的全文索引。1=TRUE，0=False
		/// TableHasCheckCnst（表）：表具有CHECK约束。1=TRUE，0=False
		/// TableHasClustIndex（表）：表具有聚集索引。1=TRUE，0=False
		/// TableHasDefaultCnst（表）：表具有DEFAULT约束。1=TRUE，0=False
		/// TableHasDeleteTrigger（表）：表具有DELETE触发器。1=TRUE，0=False
		/// TableHasForeignKey（表）：表具有FOREIGNKEY约束。1=TRUE，0=False
		/// TableHasForeignRef（表）：表由FOREIGNKEY约束引用。1=TRUE，0=False
		/// TableHasIdentity（表）：表具有标识列。1=TRUE，0=False
		/// TableHasIndex（表）：表具有任意类型的索引。1=TRUE，0=False
		/// TableHasInsertTrigger（表）：对象具有INSERT触发器。1=TRUE，0=False
		/// TableHasNonclustIndex（表）：表有非聚集索引。1=TRUE，0=False
		/// TableHasPrimaryKey（表）：表具有主键。1=TRUE，0=False
		/// TableHasRowGuidCol（表）：表的uniqueidentifier列具有ROWGUIDCOL。1=TRUE，0=False
		/// TableHasTextImage（表）：表具有text、ntext或image列。1=TRUE，0=False
		/// TableHasTimestamp（表）：表具有timestamp列。1=TRUE，0=False
		/// TableHasUniqueCnst（表）：表具有UNIQUE约束。1=TRUE，0=False
		/// TableHasUpdateTrigger（表）：对象有UPDATE触发器。1=TRUE，0=False
		/// TableHasVarDecimalStorageFormat（表）：为vardecimal存储格式启用表。1=TRUE，0=False
		/// TableInsertTrigger（表）：表具有INSERT触发器。>1=指定类型的第一个触发器的ID。
		/// TableInsertTriggerCount（表）：表有指定数目的INSERT触发器。>0=INSERT触发器的个数。
		/// TableIsFake（表）：表不是真实的表。它将由SQLServer数据库引擎根据需要在内部进行具体化。1=TRUE，0=False
		/// TableIsLockedOnBulkLoad（表）：bcp或BULKINSERT作业导致表被锁。1=TRUE，0=False，
		/// TableIsPinned（表）：驻留表以将其保留在数据缓存中。0=False，SQLServer2005及更高版本不支持此功能。
		/// TableTextInRowLimit（表）：textinrow允许的最大节数。如果未设置textinrow选项，则返回0。
		/// TableUpdateTrigger（表）：表具有UPDATE触发器。>1=指定类型的第一个触发器的ID。
		/// TableUpdateTriggerCount（表）：表有指定数目的UPDATE触发器。>0=UPDATE触发器的个数。
		/// TableHasColumnSet（表）：表具有列集。0=False，1=TRUE，有关详细信息，请参阅使用列集。
		/// </remarks>
		public static int OBJECTPROPERTY(object id, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回当前数据库中给定逻辑文件名的文件标识 (ID) 号。
		/// </summary>
		/// <param name="file_name">一个 sysname 类型的表达式，表示要返回文件 ID 的文件的名称。</param>
		/// <returns>smallint</returns>
		public static int FILE_ID(object file_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回当前数据库中架构范围内的对象的有关信息。有关这些对象的列表，请参阅 sys.objects (Transact-SQL)。OBJECTPROPERTYEX 不能用于非架构范围内的对象，如数据定义语言 (DDL) 触发器和事件通知。
		/// </summary>
		/// <param name="id">是表示当前数据库中对象 ID 的表达式。id 的数据类型为 int，并假定为当前数据库上下文中的架构范围内的对象。</param>
		/// <param name="property">
		/// 一个表达式，包含要为 ID 所指定的对象返回的信息。返回类型为 sql_variant。各属性值的基本数据类型参考备注。
		/// </param>
		/// <returns>int</returns>
		/// <remarks>
		/// BaseType（架构范围内的任何对象）：标识对象的基类型。当指定的对象为SYNONYM时，将返回基础对象的基类型。
		/// Nonnull=对象类型，基本数据类型：char(2)
		/// CnstIsClustKey（约束）：具有聚集索引的PRIMARYKEY约束。1=TRUE，0=False，基本数据类型：int
		/// CnstIsColumn（约束）：单个列上的CHECK、DEFAULT或FOREIGNKEY约束。1=TRUE，0=False，基本数据类型：int
		/// CnstIsDeleteCascade（约束）：具有ONDELETECASCADE选项的FOREIGNKEY约束。1=TRUE，0=False，基本数据类型：int
		/// CnstIsDisabled（约束）：禁用的约束。1=TRUE，0=False，基本数据类型：int
		/// CnstIsNonclustKey（约束）：具有非聚集索引的PRIMARYKEY约束。1=TRUE，0=False，基本数据类型：int
		/// CnstIsNotRepl（约束）：使用NOTFORREPLICATION关键字定义的约束。1=TRUE，0=False，基本数据类型：int
		/// CnstIsNotTrusted（约束）：启用约束时未检查现有行。所以，可能不是所有行都受约束的控制。1=TRUE，0=False，基本数据类型：int
		/// CnstIsUpdateCascade（约束）：具有ONUPDATECASCADE选项的FOREIGNKEY约束。1=TRUE，0=False，基本数据类型：int
		/// ExecIsAfterTrigger（触发器）：AFTER触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsAnsiNullsOn（Transact-SQL函数、Transact-SQL过程、Transact-SQL触发器、视图）：创建时的ANSI_NULLS设置。1=TRUE，0=False，基本数据类型：int
		/// ExecIsDeleteTrigger（触发器）：DELETE触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsFirstDeleteTrigger（触发器）：对表执行DELETE时触发的第一个触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsFirstInsertTrigger（触发器）：对表执行INSERT时触发的第一个触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsFirstUpdateTrigger（触发器）：对表执行UPDATE时触发的第一个触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsInsertTrigger（触发器）：INSERT触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsInsteadOfTrigger（触发器）：INSTEADOF触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsLastDeleteTrigger（触发器）：对表执行DELETE时激发的最后一个触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsLastInsertTrigger（触发器）：对表执行INSERT时激发的最后一个触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsLastUpdateTrigger（触发器）：对表执行UPDATE时激发的最后一个触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsQuotedIdentOn（Transact-SQL函数、Transact-SQL过程、Transact-SQL触发器、视图）：创建时的QUOTED_IDENTIFIER设置。1=TRUE，0=False，基本数据类型：int
		/// ExecIsStartup（过程）：启动过程。1=TRUE，0=False，基本数据类型：int
		/// ExecIsTriggerDisabled（触发器）：禁用的触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsTriggerNotForRepl（触发器）：定义为NOTFORREPLICATION的触发器。1=TRUE，0=False，基本数据类型：int
		/// ExecIsUpdateTrigger（触发器）：UPDATE触发器。1=TRUE，0=False，基本数据类型：int
		/// HasAfterTrigger（表、视图）：表或视图具有AFTER触发器。1=TRUE，0=False，基本数据类型：int
		/// HasDeleteTrigger（表、视图）：表或视图具有DELETE触发器。1=TRUE，0=False，基本数据类型：int
		/// HasInsertTrigger（表、视图）：表或视图具有INSERT触发器。1=TRUE，0=False，基本数据类型：int
		/// HasInsteadOfTrigger（表、视图）：表或视图具有INSTEADOF触发器。1=TRUE，0=False，基本数据类型：int
		/// HasUpdateTrigger（表、视图）：表或视图具有UPDATE触发器。1=TRUE，0=False，基本数据类型：int
		/// IsAnsiNullsOn（Transact-SQL函数、Transact-SQL过程、表、Transact-SQL触发器、视图）：指定表的ANSINULLS选项设置为ON，表示所有与Null的比较都取值为UNKNOWN。只要表存在，此设置将应用于表定义中的所有表达式，包括计算列和约束。1=TRUE，0=False，基本数据类型：int
		/// IsCheckCnst（架构范围内的任何对象）：CHECK约束。1=TRUE，0=False，基本数据类型：int
		/// IsConstraint（架构范围内的任何对象）：约束。1=TRUE，0=False，基本数据类型：int
		/// IsDefault（架构范围内的任何对象）：绑定的默认值。1=TRUE，0=False，基本数据类型：int
		/// IsDefaultCnst（架构范围内的任何对象）：DEFAULT约束。1=TRUE，0=False，基本数据类型：int
		/// IsDeterministic（标量值函数和表值函数、视图）：函数或视图的确定性属性。1=确定，0=不确定，基本数据类型：int
		/// IsEncrypted（Transact-SQL函数、Transact-SQL过程、表、Transact-SQL触发器、视图）：指示模块语句的原始文本已转换为模糊格式。模糊代码的输出在SQLServer2005的任何目录视图中都不能直接显示。对系统表或数据库文件没有访问权限的用户不能检索模糊文本。但是，能沟通过DAC端口访问系统表的用户或能够直接访问数据库文件的用户可以使用此文本。此外，能够向服务器进程附加调试器的用户可在运行时从内存中检索原始过程。1=已加密，0=未加密，基本数据类型：int
		/// IsExecuted（架构范围内的任何对象）：指定对象可以执行（视图、过程、函数或触发器）。1=TRUE，0=False，基本数据类型：int
		/// IsExtendedProc（架构范围内的任何对象）：扩展过程。1=TRUE，0=False，基本数据类型：int
		/// IsForeignKey（架构范围内的任何对象）：FOREIGNKEY约束。1=TRUE，0=False，基本数据类型：int
		/// IsIndexed（表、视图）：带有索引的表或视图。1=TRUE，0=False，基本数据类型：int
		/// IsIndexable（表、视图）：可以创建索引的表或视图。1=TRUE，0=False，基本数据类型：int
		/// IsInlineFunction（功能）：内联函数。1=内联函数，0=非内联函数，基本数据类型：int
		/// IsMSShipped（架构范围内的任何对象）：在安装SQLServer期间创建的对象。1=TRUE，0=False，基本数据类型：int
		/// IsPrecise（计算列、函数、用户定义类型、视图）：指示对象是否包含不精确计算，如浮点运算。1=精确，0=不精确，基本数据类型：int
		/// IsPrimaryKey（架构范围内的任何对象）：PRIMARYKEY约束。1=TRUE，0=False，基本数据类型：int
		/// IsProcedure（架构范围内的任何对象）：过程。1=TRUE，0=False，基本数据类型：int
		/// IsQuotedIdentOn（CHECK约束、DEFAULT定义、Transact-SQL函数、Transact-SQL过程、表、Transact-SQL触发器、视图）：指定对象的带引号的标识符设置为ON，表示在对象定义所涉及的所有表达式中，都由双引号分隔标识符。1=TRUE，0=False，基本数据类型：int
		/// IsQueue（架构范围内的任何对象）：ServiceBroker队列，1=TRUE，0=False，基本数据类型：int
		/// IsReplProc（架构范围内的任何对象）：复制过程。1=TRUE，0=False，基本数据类型：int
		/// IsRule（架构范围内的任何对象）：绑定规则。1=TRUE，0=False，基本数据类型：int
		/// IsScalarFunction（功能）：标量值函数。1=标量值函数，0=非标量值函数，基本数据类型：int
		/// IsSchemaBound（函数、视图）：使用SCHEMABINDING创建的绑定到架构的函数或视图。1=绑定到架构，0=不绑定到架构，基本数据类型：int
		/// IsSystemTable（表）：系统表。1=TRUE，0=False，基本数据类型：int
		/// IsSystemVerified（计算列、函数、用户定义类型、视图）：对象的精度和确定性属性可以由SQLServer进行验证。1=TRUE，0=False，基本数据类型：int
		/// IsTable（表）：表。1=TRUE，0=False，基本数据类型：int
		/// IsTableFunction（功能）：表值函数。1=表值函数，0=非表值函数，基本数据类型：int
		/// IsTrigger（架构范围内的任何对象）：触发器。1=TRUE，0=False，基本数据类型：int
		/// IsUniqueCnst（架构范围内的任何对象）：UNIQUE约束。1=TRUE，0=False，基本数据类型：int
		/// IsUserTable（表）：用户定义的表。1=TRUE，0=False，基本数据类型：int
		/// IsView（视图）：视图。1=TRUE，0=False，基本数据类型：int
		/// OwnerId（架构范围内的任何对象）：对象的所有者。注意：架构所有者不一定是对象所有者。例如，子对象（其parent_object_id为非Null）将始终返回与父对象相同的所有者ID。Nonnull=对象所有者的数据库用户ID。NULL=不支持的对象类型，或对象ID无效。基本数据类型：int
		/// SchemaId（架构范围内的任何对象）：与对象关联的架构的ID。
		/// Nonnull=对象的架构ID。基本数据类型：int
		/// SystemDataAccess（函数、视图）：对象访问SQLServer的本地实例中的系统数据、系统目录或虚拟系统表。0=无，1=读取，基本数据类型：int
		/// TableDeleteTrigger（表）：表具有DELETE触发器。>1=指定类型的第一个触发器的ID。基本数据类型：int
		/// TableDeleteTriggerCount（表）：表具有指定数目的DELETE触发器。
		/// Nonnull=DELETE触发器数，基本数据类型：int
		/// TableFullTextMergeStatus（表）：表所具有的全文索引当前是否正在合并。0=表没有全文索引，或者全文索引未在合并。1=全文索引正在合并。
		/// TableFullTextBackgroundUpdateIndexOn（表）：表已启用全文后台更新索引（自动更改跟踪）。1=TRUE，0=False，基本数据类型：int
		/// TableFulltextCatalogId（表）：表的全文索引数据所在的全文目录的ID。非零=全文目录ID，它与全文索引表中标识行的唯一索引相关。0=表没有全文索引。基本数据类型：int
		/// TableFullTextChangeTrackingOn（表）：表已启用全文更改跟踪。1=TRUE，0=False，基本数据类型：int
		/// TableFulltextDocsProcessed（表）：自开始全文索引以来所处理的行数。在为进行全文搜索而正在编制索引的表中，将一个行的所有列视为要编制索引的文档的一部分。0=没有完成的活动爬网或全文索引。>0=以下选项之一：。自从开始完整、增量或手动更改跟踪填充以来，由插入或更新操作处理的文档数。自从执行以下操作以来由插入或更新操作处理的行数：启用具有后台更新索引填充功能的更改跟踪、更改全文索引架构、重建全文目录或重新启动SQLServer的实例等。NULL=表没有全文索引。基本数据类型：int。注意此属性不监视已删除行，也不对已删除行进行计数。
		/// TableFulltextFailCount（表）：全文搜索不索引的行数。0=填充已完成。>0=以下选项之一：。自从开始完整、增量和手动更新更改跟踪填充以来未编制索引的文档数。对于具有后台更新索引功能的更改跟踪，则为自从开始填充或重新启动填充以来未编制索引的行数。这可能由架构更改、重新生成目录、服务器重新启动等原因引起。NULL=表没有全文索引。基本数据类型：int
		/// TableFulltextItemCount（表）：Nonnull=成功进行全文索引的行数。NULL=表没有全文索引。基本数据类型：int
		/// TableFulltextKeyColumn（表）：与参与全文索引定义的单列唯一索引关联的列的ID。0=表没有全文索引。基本数据类型：int
		/// TableFulltextPendingChanges（表）：要处理的挂起更改跟踪项的数目。0=未启用更改跟踪。NULL=表没有全文索引。基本数据类型：int
		/// TableFulltextPopulateStatus（表）：0=空闲。1=正在进行完全填充。2=正在进行增量填充。3=正在传播所跟踪的更改。4=正在进行后台更新索引（例如，自动跟踪更改）。5=全文索引已中止或暂停。基本数据类型：int
		/// TableHasActiveFulltextIndex（表）：表具有活动的全文索引。1=TRUE，0=False，基本数据类型：int
		/// TableHasCheckCnst（表）：表具有CHECK约束。1=TRUE，0=False，基本数据类型：int
		/// TableHasClustIndex（表）：表具有聚集索引。1=TRUE，0=False，基本数据类型：int
		/// TableHasDefaultCnst（表）：表具有DEFAULT约束。1=TRUE，0=False，基本数据类型：int
		/// TableHasDeleteTrigger（表）：表具有DELETE触发器。1=TRUE，0=False，基本数据类型：int
		/// TableHasForeignKey（表）：表具有FOREIGNKEY约束。1=TRUE，0=False，基本数据类型：int
		/// TableHasForeignRef（表）：表由FOREIGNKEY约束引用。1=TRUE，0=False，基本数据类型：int
		/// TableHasIdentity（表）：表具有标识列。1=TRUE，0=False，基本数据类型：int
		/// TableHasIndex（表）：表具有任意类型的索引。1=TRUE，0=False，基本数据类型：int
		/// TableHasInsertTrigger（表）：对象具有INSERT触发器。1=TRUE，0=False，基本数据类型：int
		/// TableHasNonclustIndex（表）：表具有非聚集索引。1=TRUE，0=False，基本数据类型：int
		/// TableHasPrimaryKey（表）：表具有主键。1=TRUE，0=False，基本数据类型：int
		/// TableHasRowGuidCol（表）：表的uniqueidentifier列具有ROWGUIDCOL。1=TRUE，0=False，基本数据类型：int
		/// TableHasTextImage（表）：表具有text、ntext或image列。1=TRUE，0=False，基本数据类型：int
		/// TableHasTimestamp（表）：表具有timestamp列。1=TRUE，0=False，基本数据类型：int
		/// TableHasUniqueCnst（表）：表具有UNIQUE约束。1=TRUE，0=False，基本数据类型：int
		/// TableHasUpdateTrigger（表）：对象具有UPDATE触发器。1=TRUE，0=False，基本数据类型：int
		/// TableHasVarDecimalStorageFormat（表）：为vardecimal存储格式启用表。1=TRUE，0=False
		/// TableInsertTrigger（表）：表具有INSERT触发器。>1=指定类型的第一个触发器的ID。基本数据类型：int
		/// TableInsertTriggerCount（表）：表具有指定数目的INSERT触发器。>0=INSERT触发器的个数。基本数据类型：int
		/// TableIsFake（表）：表不是真实的表。它将由数据库引擎根据需要在内部进行具体化。1=TRUE，0=False，基本数据类型：int
		/// TableIsLockedOnBulkLoad（表）：由于bcp或BULKINSERT作业，表被锁定。1=TRUE，0=False，基本数据类型：int
		/// TableIsPinned（表）：驻留表以将其保留在数据缓存中。0=False
		/// SQLServer2005及更高版本不支持此功能。
		/// TableTextInRowLimit（表）：表设置了textinrow选项。>0=textinrow所允许的最大字节数。0=未设置textinrow选项。基本数据类型：int
		/// TableUpdateTrigger（表）：表具有UPDATE触发器。>1=指定类型的第一个触发器的ID。基本数据类型：int
		/// TableUpdateTriggerCount（表）：表具有指定数目的UPDATE触发器。>0=UPDATE触发器的个数。基本数据类型：int
		/// UserDataAccess（函数、视图）：指示对象访问SQLServer的本地实例中的用户数据和用户表。1=读取，0=无，基本数据类型：int
		/// TableHasColumnSet（表）：表具有列集。0=False，1=TRUE。有关详细信息，请参阅使用列集。
		/// </remarks>
		public static int OBJECTPROPERTYEX(object id, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回当前数据库中的数据、日志或全文文件的指定逻辑文件名的文件标识 (ID) 号。
		/// </summary>
		/// <param name="file_name">一个 sysname 类型的表达式，表示要返回文件 ID 的文件的名称。</param>
		/// <returns>int</returns>
		public static int FILE_IDEX(object file_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回给定文件标识 (ID) 号的逻辑文件名。
		/// </summary>
		/// <param name="file_id">是要返回其文件名的文件标识号。file_id 的数据类型为 int。</param>
		/// <returns>nvarchar(128) </returns>
		public static string FILE_NAME(object file_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定文件组名称的文件组标识 (ID) 号。
		/// </summary>
		/// <param name="filegroup_name">sysname 类型的表达式，表示要为其返回文件组 ID 的文件组名。</param>
		/// <returns>int</returns>
		/// <remarks>filegroup_name 与 sys.filegroups 目录视图中的 name 列相对应。</remarks>
		public static int FILEGROUP_ID(object filegroup_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回有关 sql_variant 值的基本数据类型和其他信息。
		/// </summary>
		/// <param name="expression">类型为 sql_variant 的表达式。</param>
		/// <param name="property">包含将为其提供信息的 sql_variant 属性的名称。property 的数据类型为 varchar(128)，值定义请参考备注。</param>
		/// <returns>sql_variant </returns>
		/// <remarks>
		/// BaseType：SQLServer数据类型，例如：bigint，binary，char，date，datetime，datetime2，datetimeoffset，decimal，float，int，money，nchar，numeric，nvarchar，real，smalldatetime，smallint，smallmoney，time，tinyint，uniqueidentifier，varbinary，varchar，sysname，NULL=输入无效。
		/// Precision：数值基本数据类型的位数：datetime=23，smalldatetime=16，float=53，real=24，decimal(p,s)和numeric(p,s)=p，money=19，smallmoney=10，bigint=19，int=10，smallint=5，tinyint=3，bit=1，所有其他类型=0，int，NULL=输入无效。
		/// Scale：数值基本数据类型的小数点后的位数：decimal(p,s)和numeric(p,s)=s，money和smallmoney=4，datetime=3，所有其他类型=0，int，NULL=输入无效。
		/// TotalBytes：同时容纳值的元数据和数据所需的字节数。在检查sql_variant列中数据的最大一侧时，该信息很有用。如果该值大于900，则索引创建将失败。int，NULL=输入无效。
		/// Collation：代表特定sql_variant值的排序规则。sysname，NULL=输入无效。
		/// MaxLength：最大数据类型长度（字节）。例如，nvarchar(50)的MaxLength是100，int的MaxLength是4。int，NULL=输入无效。
		/// </remarks>
		public static object SQL_VARIANT_PROPERTY(object expression, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定文件组标识 (ID) 号的文件组名。
		/// </summary>
		/// <param name="filegroup_id">要返回文件组名的文件组 ID 号。filegroup_id 的数据类型为 smallint。</param>
		/// <returns>nvarchar(128) </returns>
		/// <remarks>
		/// filegroup_id 与 sys.filegroups 目录视图中的 data_space_id 列相对应。
		/// </remarks>
		public static string FILEGROUP_NAME(object filegroup_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定数据类型名称的 ID。
		/// </summary>
		/// <param name="type_name">数据类型的名称。type_name 的数据类型为 nvarchar。type_name 可以是系统数据类型或用户定义的数据类型。</param>
		/// <returns>int</returns>
		public static int TYPE_ID(object type_name)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 提供文件组和属性名时，返回指定的文件组属性值。
		/// </summary>
		/// <param name="filegroup_name">类型为 sysname 的表达式，它表示要为之返回指定属性信息的文件组名称。</param>
		/// <param name="property">类型为 varchar(128) 的表达式，它包含要返回的文件组属性的名称，值定义请参考备注。</param>
		/// <returns>int</returns>
		/// <remarks>
		/// IsReadOnly：文件组是只读的。1=True，0=False，NULL=输入无效。
		/// IsUserDefinedFG：文件组是用户定义文件组。1=True，0=False，NULL=输入无效。
		/// IsDefault：文件组是默认的文件组。1=True，0=False，NULL=输入无效。
		/// </remarks>
		public static int FILEGROUPPROPERTY(object filegroup_name, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定类型 ID 的未限定的类型名称。
		/// </summary>
		/// <param name="type_id">要使用的类型的 ID。type_id 的数据类型为 int，它可以引用调用方有权访问的任意架构中的类型。</param>
		/// <returns>sysname</returns>
		public static string TYPE_NAME(object type_id)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 指定文件名和属性名时，返回指定的文件名属性值。
		/// </summary>
		/// <param name="file_name">包含与将为之返回属性信息的当前数据库相关联的文件名的表达式。file_name 的数据类型为 nchar(128)。</param>
		/// <param name="property">包含将返回的文件属性名的表达式。property 的数据类型为 varchar(128)，值定义请参考备注。</param>
		/// <returns>int</returns>
		/// <remarks>
		/// IsReadOnly：文件组是只读的。1=True，0=False，NULL=输入无效。
		/// IsPrimaryFile：文件为主文件。1=True，0=False，NULL=输入无效。
		/// IsLogFile：文件为日志文件。1=True，0=False，NULL=输入无效。
		/// SpaceUsed：指定的文件使用的空间量。在文件中分配的页数
		/// </remarks>
		public static int FILEPROPERTY(object file_name, object property)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回有关数据类型的信息。
		/// </summary>
		/// <param name="type">数据类型的名称。</param>
		/// <param name="property">要为数据类型返回的信息类型。值定义请参考备注。</param>
		/// <returns>int</returns>
		/// <remarks>
		/// AllowsNull：数据类型允许空值。1=True，0=False，NULL=找不到数据类型。
		/// OwnerId：类型的所有者。注意：架构所有者不一定是类型所有者。Nonnull=类型所有者的数据库用户ID。NULL=不支持的类型，或类型ID无效。
		/// Precision：数据类型的精度。数字位数或字符个数。-1=xml或较大值数据类型NULL=找不到数据类型。
		/// Scale：数据类型的小数位数。数据类型的小数位的个数。NULL=数据类型不是numeric或找不到。
		/// UsesAnsiTrim：创建数据类型时ANSI填充设置为ON。1=True，0=False，NULL=数据类型找不到，或不是二进制数据类型或字符串数据类型。
		/// </remarks>
		public static int TYPEPROPERTY(object type, object property)
		{
			throw new NotSupportedException();
		}


		#endregion

		#region 聚合函数

		/// <summary>
		/// 返回组中各值的平均值。将忽略空值。
		/// </summary>
		/// <param name="expression">是精确数值或近似数值数据类别（bit 数据类型除外）的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>返回类型由 expression 的计算结果类型确定。</returns>
		public static int AVG(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中各值的平均值。将忽略空值。
		/// </summary>
		/// <param name="expression">是精确数值或近似数值数据类别（bit 数据类型除外）的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>返回类型由 expression 的计算结果类型确定。</returns>
		[TSQLFormattor("AVG(DISTINCT {0})")]
		public static int AVG_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表达式中的最小值
		/// </summary>
		/// <param name="expression">常量、列名、函数以及算术运算符、位运算符和字符串运算符的任意组合。MIN 可用于 numeric、char、varchar 或 datetime 列，但不能用于 bit 列。不允许使用聚合函数和子查询。</param>
		/// <returns>返回与 expression 相同的值。</returns>
		public static T MIN<T>(T expression) where T : struct
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表达式中的最小值
		/// </summary>
		/// <param name="expression">常量、列名、函数以及算术运算符、位运算符和字符串运算符的任意组合。MIN 可用于 numeric、char、varchar 或 datetime 列，但不能用于 bit 列。不允许使用聚合函数和子查询。</param>
		/// <returns>返回与 expression 相同的值。</returns>
		[TSQLFormattor("MIN(DISTINCT {0})")]
		public static T MIN_DISTINCT<T>(T expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中各值的校验和。空值将被忽略。
		/// </summary>
		/// <param name="expression">一个整数表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>将所有 expression 值的校验值作为 int 返回。</returns>
		public static int CHECKSUM_AGG(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中各值的校验和。空值将被忽略。
		/// </summary>
		/// <param name="expression">一个整数表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>将所有 expression 值的校验值作为 int 返回。</returns>
		[TSQLFormattor("CHECKSUM_AGG(DISTINCT {0})")]
		public static int CHECKSUM_AGG_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表达式中所有值的和或仅非重复值的和。SUM 只能用于数字列。空值将被忽略。
		/// </summary>
		/// <param name="expression">常量、列或函数与算术、位和字符串运算符的任意组合。expression 是精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>以最精确的 expression 数据类型返回所有 expression 值的和。</returns>
		public static long SUM(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表达式中所有值的和或仅非重复值的和。SUM 只能用于数字列。空值将被忽略。
		/// </summary>
		/// <param name="expression">常量、列或函数与算术、位和字符串运算符的任意组合。expression 是精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>以最精确的 expression 数据类型返回所有 expression 值的和。</returns>
		[TSQLFormattor("SUM(DISTINCT {0})")]
		public static long SUM_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中的项数。此方法等价于COUNT(*)
		/// </summary>
		/// <returns>int</returns>
		[TSQLFormattor("COUNT(*)")]
		public static int COUNT()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中的项数。此方法等价于COUNT(1)
		/// </summary>
		/// <returns>int</returns>
		[TSQLFormattor("COUNT(1)")]
		public static int COUNT1()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中的项数。COUNT 与 COUNT_BIG 函数类似。两个函数唯一的差别是它们的返回值。COUNT 始终返回 int 数据类型值。COUNT_BIG 始终返回 bigint 数据类型值。
		/// </summary>
		/// <param name="expression">除 text、image 或 ntext 以外任何类型的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>int</returns>
		public static int COUNT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中的项数。COUNT 与 COUNT_BIG 函数类似。两个函数唯一的差别是它们的返回值。COUNT 始终返回 int 数据类型值。COUNT_BIG 始终返回 bigint 数据类型值。
		/// </summary>
		/// <param name="expression">除 text、image 或 ntext 以外任何类型的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>int</returns>
		[TSQLFormattor("COUNT(DISTINCT {0})")]
		public static int COUNT_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中所有值的标准偏差。
		/// </summary>
		/// <param name="expression">是一个数值表达式。不允许聚合函数和子查询。expression 是精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。</param>
		/// <returns>float</returns>
		public static float STDEV(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中所有值的标准偏差。
		/// </summary>
		/// <param name="expression">是一个数值表达式。不允许聚合函数和子查询。expression 是精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。</param>
		/// <returns>float</returns>
		[TSQLFormattor("STDEV(DISTINCT {0})")]
		public static float STDEV_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中的项数。此方法等价于COUNT_BIG(*)
		/// </summary>
		/// <returns>bigint</returns>
		[TSQLFormattor("COUNT_BIG(*)")]
		public static object COUNT_BIG()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中的项数。此方法等价于COUNT_BIG(1)
		/// </summary>
		/// <returns>bigint</returns>
		[TSQLFormattor("COUNT_BIG(1)")]
		public static object COUNT_BIG1()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中的项数。COUNT_BIG 的用法与 COUNT 函数类似。两个函数唯一的差别是它们的返回值。COUNT_BIG 始终返回 bigint 数据类型值。COUNT 始终返回 int 数据类型值。
		/// </summary>
		/// <param name="expression">是任何类型的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>bigint</returns>
		public static long COUNT_BIG(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回组中的项数。COUNT_BIG 的用法与 COUNT 函数类似。两个函数唯一的差别是它们的返回值。COUNT_BIG 始终返回 bigint 数据类型值。COUNT 始终返回 int 数据类型值。
		/// </summary>
		/// <param name="expression">是任何类型的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>bigint</returns>
		[TSQLFormattor("COUNT_BIG(DISTINCT {0})")]
		public static long COUNT_BIG_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中所有值的总体标准偏差。
		/// </summary>
		/// <param name="expression">一个数值表达式。不允许聚合函数和子查询。</param>
		/// <returns>float</returns>
		public static float STDEVP(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中所有值的总体标准偏差。
		/// </summary>
		/// <param name="expression">一个数值表达式。不允许聚合函数和子查询。</param>
		/// <returns>float</returns>
		[TSQLFormattor("STDEVP(DISTINCT {0})")]
		public static float STDEVP_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 指示是否聚合 GROUP BY 列表中的指定列表达式。在结果集中，如果 GROUPING 返回 1 则指示聚合；返回 0 则指示不聚合。如果指定了 GROUP BY，则 GROUPING 只能用在 SELECT &lt;select> 列表、HAVING 和 ORDER BY 子句中。
		/// </summary>
		/// <param name="column_expression">列或者 GROUP BY 子句中包含列的表达式。</param>
		/// <returns>tinyint</returns>
		public static int GROUPING(object column_expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中所有值的方差。
		/// </summary>
		/// <param name="expression">精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>float</returns>
		public static object VAR(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中所有值的方差。
		/// </summary>
		/// <param name="expression">精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>float</returns>
		[TSQLFormattor("VAR(DISTINCT {0})")]
		public static object VAR_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表达式的最大值。
		/// </summary>
		/// <param name="expression">常量、列名、函数以及算术运算符、位运算符和字符串运算符的任意组合。MAX 可用于 numeric, character 列和 datetime 列，但不能用于 bit 列。不允许使用聚合函数和子查询。</param>
		/// <returns>返回与 expression 相同的值。</returns>
		/// <remarks>
		/// MAX 忽略任何空值。
		/// 对于字符列，MAX 查找按排序序列排列的最大值。
		/// </remarks>
		public static T MAX<T>(T expression) where T : struct
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回表达式的最大值。
		/// </summary>
		/// <param name="expression">常量、列名、函数以及算术运算符、位运算符和字符串运算符的任意组合。MAX 可用于 numeric, character 列和 datetime 列，但不能用于 bit 列。不允许使用聚合函数和子查询。</param>
		/// <returns>返回与 expression 相同的值。</returns>
		/// <remarks>
		/// MAX 忽略任何空值。
		/// 对于字符列，MAX 查找按排序序列排列的最大值。
		/// </remarks>
		[TSQLFormattor("MAX(DISTINCT {0})")]
		public static T MAX_DISTINCT<T>(T expression) where T : struct
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中所有值的总体方差
		/// </summary>
		/// <param name="expression">是精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>float</returns>
		public static float VARP(object expression)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回指定表达式中所有值的总体方差
		/// </summary>
		/// <param name="expression">是精确数字或近似数字数据类型类别（bit 数据类型除外）的表达式。不允许使用聚合函数和子查询。</param>
		/// <returns>float</returns>
		[TSQLFormattor("VARP(DISTINCT {0})")]
		public static float VARP_DISTINCT(object expression)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region 排名函数
		/// <summary>
		/// 返回结果集的分区内每行的排名。行的排名是相关行之前的排名数加一。
		/// </summary>
		/// <param name="over_clause">
		/// OVER子句，使用TSQL.OVER()来生成。
		/// </param>
		/// <returns>int</returns>
		[TSQLFormattor("RANK() OVER ({0})")]
		public static int RANK(OVER_CLAUSE over_clause)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 将有序分区中的行分发到指定数目的组中。各个组有编号，编号从一开始。对于每一个行，NTILE 将返回此行所属的组的编号。
		/// </summary>
		/// <param name="integer_expression">一个正整数常量表达式，用于指定每个分区必须被划分成的组数。integer_expression 的类型可以为 int 或 bigint。</param>
		/// <param name="over_clause">
		/// OVER子句，使用TSQL.OVER()来生成。
		/// </param>
		/// <returns>int</returns>
		[TSQLFormattor("NTILE({0}) OVER ({1})")]
		public static int NTILE(object integer_expression, OVER_CLAUSE over_clause)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回结果集分区中行的排名，在排名中没有任何间断。行的排名等于所讨论行之前的所有排名数加一。
		/// </summary>
		/// <param name="over_clause">
		/// OVER子句，使用TSQL.OVER()来生成。
		/// </param>
		/// <returns>int</returns>
		[TSQLFormattor("DENSE_RANK() OVER ({0})")]
		public static int DENSE_RANK(OVER_CLAUSE over_clause)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 返回结果集分区内行的序列号，每个分区的第一行从 1 开始。
		/// </summary>
		/// <param name="over_clause">
		/// OVER子句，使用TSQL.OVER()来生成。
		/// </param>
		/// <returns>int</returns>
		[TSQLFormattor("ROW_NUMBER() OVER ({0})")]
		public static int ROW_NUMBER(OVER_CLAUSE over_clause)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// 确定在应用关联的开窗函数之前，行集的分区和排序。适用于排名开窗函数。此方法返回一个开窗函数的分区接口，你可以使用这个接口继续完成一个或者多个ORDER BY子句的构建
		/// </summary>
		/// <param name="partition_by_clause">
		/// 将结果集分为多个分区。开窗函数分别应用于每个分区，并为每个分区重新启动计算。
		/// </param>
		/// <returns>
		/// 返回一个开窗函数的分区接口，你可以使用这个接口继续完成一个或者多个ORDER BY子句的构建
		/// </returns>
		[TSQLFormattor(TSQLFormattor.FLAG_OVER_CLAUSE)]
		public static OVER_CLAUSE OVER(params object[] partition_by_clause)
		{
			throw new NotSupportedException();
		}
		#endregion

		/// <summary>
		/// 声明TSQL的成员格式化方法以便SqlQuery构建对应的SQL语句
		/// </summary>
		[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
		internal class TSQLFormattor : Attribute
		{
			/// <summary>
			/// 旗标，通知TSQLFormattor直接以参数替换的方法进行格式化
			/// </summary>
			internal const int FLAG_NORMAL = 0x00;

			/// <summary>
			/// 旗标，通知TSQLFormattor将方法格式化为动态参数的方法
			/// </summary>
			internal const int FLAG_DYNAMIC_PARAMETERS = 0x01;

			/// <summary>
			/// 旗标，通知TSQLFormattor将方法格式化为动态参数的方法
			/// </summary>
			internal const int FLAG_OVER_CLAUSE = 0x02;

			private const string CONST_PARTITION = "PARTITION BY ";

			/// <summary>
			/// 格式化句柄
			/// </summary>
			public Func<string[], string> FormatHandler
			{
				get
				{
					return this.formatHandler;
				}
			}

			private Func<string[], string> formatHandler;

			public TSQLFormattor(string format)
			{
				this.formatHandler = new Func<string[], string>((parameters) => { return string.Format(format, parameters); });
			}

			public TSQLFormattor(int flag)
			{
				switch (flag)
				{
					case FLAG_OVER_CLAUSE:
						this.formatHandler = OVER_CLAUSE_HANDLER;
						break;
					default:
						throw new ArgumentException("flag");
				}
			}

			public TSQLFormattor(string format, object flag)
			{
				this.formatHandler = CreateDynamicParameterMethodHandler(format);
			}

			private string OVER_CLAUSE_HANDLER(params string[] partition_by_clause)
			{
				if (partition_by_clause.Length == 0) return string.Empty;
				StringBuilder sb = new StringBuilder();
				sb.Append(CONST_PARTITION);
				sb.Append(partition_by_clause[0]);
				for (int i = 1, count = partition_by_clause.Length; i < count; i++)
				{
					sb.Append(',');
					sb.Append(partition_by_clause[i]);
				}
				return sb.ToString();
			}

			internal static Func<string[], string> CreateDynamicParameterMethodHandler(string name)
			{
				return (parameters) =>
				{
					StringBuilder sb = new StringBuilder();
					sb.Append(name);
					sb.Append('(');
					if (parameters.Length > 0)
					{
						sb.Append(parameters[0]);
						for (int i = 1, count = parameters.Length; i < count; i++)
						{
							sb.Append(',');
							sb.Append(parameters[i]);
						}
					}
					sb.Append(')');
					return sb.ToString();
				};
			}

		}

		/// <summary>
		/// 开窗函数分区接口，你可以使用这个接口继续完成一个或者多个ORDER BY子句的构建。
		/// </summary>
		/// <remarks>
		/// 本接口无实际业务功能，不要实现本接口
		/// </remarks>
		public interface OVER_CLAUSE
		{
			/// <summary>
			/// 指定要排序的列。升序。此方法返回分区接口以便进行次级排序设定。
			/// </summary>
			/// <param name="order_by_expression">列名</param>
			/// <returns>
			/// 返回分区接口以便进行次级排序设定
			/// </returns>
			OVER_CLAUSE ORDER_BY(object order_by_expression);

			/// <summary>
			/// 指定要排序的列。降序。此方法返回分区接口以便进行次级排序设定。
			/// </summary>
			/// <param name="order_by_expression">列名</param>
			/// <returns>
			/// 返回分区接口以便进行次级排序设定
			/// </returns>
			OVER_CLAUSE ORDER_BY_DESC(object order_by_expression);
		}
	}
}
