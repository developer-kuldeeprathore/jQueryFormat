USE [PropTerry]
GO
/****** Object:  StoredProcedure [dbo].[sp_GetInsertQuery]    Script Date: 04/18/2015 15:24:38 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetInsertQuery]
(
    @tableName NVARCHAR(255),
	@Condition NVARCHAR(1000) = '',
	@includeIdentity BIT = 0
)
AS
BEGIN

	DECLARE @Qry NVARCHAR(MAX),@Cols NVARCHAR(MAX)

	SELECT @Qry = ISNULL(@Qry + ''',','') + 'N' + '''''''' + ' + ' + 
					CASE WHEN SC.User_Type_ID IN (48,52,56,59,60,62,104,106,108) THEN 'CONVERT(NVARCHAR(MAX),Replace(ISNULL([' + SC.Name + '],0),'''''''','''''''''''')'
					ELSE 'CONVERT(NVARCHAR(MAX),Replace(ISNULL([' + SC.Name + '],''''),'''''''','''''''''''')'
					END   + ')' + ' + ' + '''''''''' + ' + '
	FROM Sys.Columns SC INNER JOIN Sys.Types ST 
	ON SC.System_Type_ID=ST.System_Type_ID 
	AND SC.User_Type_ID=ST.User_Type_ID 
	WHERE OBJECT_ID=OBJECT_ID(@tableName) AND ((SC.is_identity=0 AND @includeIdentity=0) OR @includeIdentity=1)
	ORDER BY SC.column_id

	SELECT @Cols = ISNULL(@Cols + ',','') + '[' + SC.Name + ']'
	FROM Sys.Columns SC INNER JOIN Sys.Types ST 
	ON SC.System_Type_ID=ST.System_Type_ID 
	AND SC.User_Type_ID=ST.User_Type_ID 
	WHERE OBJECT_ID=OBJECT_ID(@tableName) AND ((SC.is_identity=0 AND @includeIdentity=0) OR @includeIdentity=1)
	ORDER BY SC.column_id

	IF @Condition = ''
		SELECT @Qry = 'SELECT  ''INSERT INTO ' + @tableName + '(' + @Cols + ') SELECT ' + @Qry + '''''' + 
					  ' AS [Insert Queries For Table ' + @tableName + '] FROM ' + @tableName
	ELSE
		SELECT @Qry = 'SELECT  ''INSERT INTO ' + @tableName + '(' + @Cols + ') SELECT ' + @Qry + '''''' + 
					  ' AS [Insert Queries For Table ' + @tableName + '] FROM ' + @tableName + 
					  ' WHERE ' + @Condition

	IF @includeIdentity = 1
	BEGIN
	
		SET @Qry =	'SELECT ''SET IDENTITY_INSERT ' + @tableName + ' ON'' UNION ALL ' +
					@Qry +
					' UNION ALL SELECT ''SET IDENTITY_INSERT ' + @tableName + ' OFF'' ' +
					' UNION ALL SELECT '''' UNION ALL SELECT '''''
	
	END
	
	EXEC(@Qry)
		

END
GO
