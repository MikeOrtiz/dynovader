<?php

$path = drupal_get_path('module', 'bing_maps');

require_once "$path/BingMapsPHPSDK/lib/Msft/Bing/Exception.php";
require_once "$path/BingMapsPHPSDK/lib/Msft/Bing/IPersistenceHandler.php";

/**
 * Drupal implementation of IPersistenceHandler interface.
 *
 * This class uses Drupal backend database for storing/retrieving data.
 * The Bing Maps Drupal module uses this class as the persistence handler with the Bing Maps SDK 
 *
 */
class DrupalPersistenceHandler implements Msft_Bing_IPersistenceHandler
{
	/**
     * @var DatabaseConnection
     */
	protected $db;

	/**
	 * Constructor
	 *
	 */
	function __construct()
	{	
		$this->db = null;
	}

	/**
	 * Destructor
	 *
	 */
	public function __destruct()
	{
	}
 
    /**
     * Initialize any connections to database, files etc
     *
     * @return boolean
     */ 	
	public function initialize()
	{
		$this->db = Database::getConnection();
		
		return true;
	}

    /**
     * Close all open connections
     *
     * @return boolean
     */ 
	public function uninitialize()
	{
		$this->db = null;
		
		return true;
	}

    /**
     * Create the given entity
     *
	 * @param Msft_Bing_Entity $ent
     * @return boolean
     */ 	
	public function createEntity($ent)
	{
		if (($ent == null) || ($ent == ''))
		{
			throw new Msft_Bing_MapException('Invalid entity parameter');		
		}
		
		if ($this->db == null) 
		{
			throw new Msft_Bing_MapException('Not connected to Drupal database. Invoke Initialize() method');
		}	

		$whichDriver = db_driver();
		if($whichDriver == 'mysql')
		{
			// build Create Table statement		
			$sqlfmt = 'CREATE TABLE {'.$ent->EntityName.'} (%s)';    
		    if (($ent->Columns != null) && ($ent->Columns != ''))
		    {
		      $colDefs = array();
		      foreach($ent->Columns as $col)
		      {
		        if (($col->DataType == null) || ($col->DataType == ''))
		        {
					    throw new Msft_Bing_MapException('Invalid column definition for column '.$col->ColumnName);			          
		        }
		        $colDef = $col->ColumnName.' '.$col->DataType;
		        if (($col->MaxLength != null) && ($col->MaxLength != '') && (is_numeric($col->MaxLength)))
		        {
		          $colDef = $colDef.'('.$col->MaxLength.') ';
		        }
		        if (isset($col->AllowNull))
		        {
		          $nullVal = ($col->AllowNull == true) ? " NULL " : " NOT NULL ";
		          $colDef = $colDef.$nullVal;          
		        }
		        if (isset($col->PrimaryKey))
		        {
		          $primary = ($col->PrimaryKey == true) ? " PRIMARY KEY " : "";
		          $colDef = $colDef.$primary;          
		        }
		        if (isset($col->UniqueKey))
		        {
		          $unique = ($col->UniqueKey == true) ? " UNIQUE KEY " : "";
		          $colDef = $colDef.$unique;          
		        }        
		        if (isset($col->AutoIncrement))
		        {
		          $increment = ($col->AutoIncrement == true) ? " AUTO_INCREMENT " : "";
		          $colDef = $colDef.$increment;
		        }
		        if (isset($col->DefaultValue))
		        {
		          $colDef = $colDef." DEFAULT ".$col->DefaultValue." ";
		        }
		        
		        $colDefs[] = $colDef;
		      }
		    }
	    	$sql = sprintf($sqlfmt, implode(",", $colDefs));
		}
    	elseif ($whichDriver == 'sqlsrv')
    	{
    		// build Create Table statement	for SQL server
			$sqlfmt = 'CREATE TABLE {'.$ent->EntityName.'} (%s)';    
		    if (($ent->Columns != null) && ($ent->Columns != ''))
		    {
		      $colDefs = array();
		      foreach($ent->Columns as $col)
		      {
		        if (($col->DataType == null) || ($col->DataType == ''))
		        {
					    throw new Msft_Bing_MapException('Invalid column definition for column '.$col->ColumnName);			          
		        }
		        $colDef = $col->ColumnName.' '.$col->DataType;
		        if (($col->MaxLength != null) && ($col->MaxLength != '') && (is_numeric($col->MaxLength)))
		        {
		          $colDef = $colDef.'('.$col->MaxLength.') ';
		        }
		        if (isset($col->AllowNull))
		        {
		          $nullVal = ($col->AllowNull == true) ? " NULL " : " NOT NULL ";
		          $colDef = $colDef.$nullVal;          
		        }
		        if (isset($col->PrimaryKey))
		        {
		          $primary = ($col->PrimaryKey == true) ? " PRIMARY KEY " : "";
		          $colDef = $colDef.$primary;          
		        }
		        if (isset($col->UniqueKey))
		        {
		          $unique = ($col->UniqueKey == true) ? " UNIQUE KEY " : "";
		          $colDef = $colDef.$unique;          
		        }        
		        if (isset($col->AutoIncrement))
		        {
		          $increment = ($col->AutoIncrement == true) ? " IDENTITY " : "";
		          $colDef = $colDef.$increment;
		        }
		        if (isset($col->DefaultValue))
		        {
		          $colDef = $colDef." DEFAULT ".$col->DefaultValue." ";
		        }
		        
		        $colDefs[] = $colDef;
		      }
		    }
	    	$sql = sprintf($sqlfmt, implode(",", $colDefs));
    	}

    	
    	try {
			// execute API to create given table			
			$this->db->query($sql);
    	}
		catch (Exception $e)
		{		
			throw new Msft_Bing_MapException('Unable to create table '.$ent->EntityName.'. '.$e);			
		}		
		
		return true;
	}

    /**
     * Drop the given entity
     *
	 * @param string $entityName
     * @return boolean
     */ 	
	public function dropEntity($entityName)
	{
		if (($entityName == null) || ($entityName == ''))
		{
			throw new Msft_Bing_MapException('Invalid entityName parameter');		
		}
	
		if ($this->db == null) 
		{
			throw new Msft_Bing_MapException('Not connected to Drupal database. Invoke Initialize() method');
		}	
		
		// execute SQL statement to drop given table
		$sql = 'DROP TABLE {'.$entityName.'}';
		try {
			$this->db->query($sql);
		}
		catch (Exception $e)
		{		
			throw new Msft_Bing_MapException('Unable to drop table '.$entityName.'. '.$e);			
		}		
		
		return true;
	}
	
    /**
     * Checks whether the given entity exists or not
     *
	 * @param string $entityName
     * @return boolean
     */ 	
	public function doesEntityExist($entityName)
	{
		if (($entityName == null) || ($entityName == ''))
		{
			throw new Msft_Bing_MapException('Invalid entityName parameter');		
		}
	
		if ($this->db == null) 
		{
			throw new Msft_Bing_MapException('Not connected to Drupal database. Invoke Initialize() method');
		}	
				
		return db_table_exists($entityName);
	}
	

    /**
     * Insert an entity row
     *
	 * @param string $entityName
	 * @param Msft_Bing_Row $row 
     * @return boolean
     */ 	
	public function insertRow($entityName, $row)
	{
		if (($entityName == null) || ($entityName == '') || ($row == null) || ($row == ''))
		{
			throw new Msft_Bing_MapException('Invalid parameters');		
		}
	
		if ($this->db == null) 
		{
			throw new Msft_Bing_MapException('Not connected to Drupal database. Invoke Initialize() method');
		}	
			
		
		// insert row with given values in the given table
		// use Drupal table generalization "{}"		
		$sqlfmt = 'INSERT INTO {'.$entityName.'} (%s) VALUES (%s)';
		if (($row->Data != null) && ($row->Data != ''))
		{
			$row->Data = (array)$row->Data;
			// get column names		
			$columnNames = array_keys($row->Data);
			// get column names		
			$values = array_values($row->Data);
		}
		
		$sql = sprintf($sqlfmt, implode(",", $columnNames), implode(",", $values));		
		try 
		{
			$this->db->query($sql);
		}
		catch (Exception $e)
		{		
			throw new Msft_Bing_MapException('Unable to insert into table '.$entityName.'. '.$e);			
		}		
		
		return true;
	}

	/**
     * Update entity rows which match the given filter.
	 * The column values to be updated are passed in $rowValue
     * If $whereFilter is null or empty, all rows in the entity will be updated 
     *
	 * @param string $entityName
	 * @param string $whereFilter Example: "firstname='Tom' OR (age>25 AND age<=50)"
	 * @param Msft_Bing_Row $rowValue New column values
     * @return boolean
     */ 	
	public function updateRows($entityName, $rowValue, $whereFilter)
	{
		if (($entityName == null) || ($entityName == '') || ($rowValue == null) || ($rowValue == ''))
		{
			throw new Msft_Bing_MapException('Invalid parameters');		
		}
	
		if ($this->db == null) 
		{
			throw new Msft_Bing_MapException('Not connected to Drupal database. Invoke Initialize() method');
		}	
				
		// update matching rows with given values in the given table		
		$sql = 'UPDATE {'.$entityName.'} SET ';
		// build column values
		if (($rowValue->Data != null) && ($rowValue->Data != ''))
		{
			$rowValue->Data = (array)$rowValue->Data;
			foreach(array_keys($rowValue->Data) as $columnName)
			{
				$sql = $sql.$columnName.'='.$rowValue->Data[$columnName];
				$sql = $sql.',';
			}
			$sql = rtrim($sql, ",");
		}
		if (($whereFilter != null) && ($whereFilter != ''))
		{
			$sql = $sql.' WHERE '.$whereFilter;
		}
		
		try {
			$this->db->query($sql);
		}
		catch (Exception $e)
		{		
			throw new Msft_Bing_MapException('Unable to update table '.$entityName.'. '.$e);			
		}		
		
		return true;
	}

    /**
     * Delete entity rows which match the given filter
     * If $whereFilter is null or empty, all rows in the entity will be deleted 
     *
	 * @param string $entityName
	 * @param string $whereFilter Example: "firstname='Tom' OR (age>25 AND age<=50)"
     * @return boolean
     */ 	
	public function deleteRows($entityName, $whereFilter)
	{
		if (($entityName == null) || ($entityName == ''))
		{
			throw new Msft_Bing_MapException('Invalid entityName parameter');		
		}
	
		if ($this->db == null) 
		{
			throw new Msft_Bing_MapException('Not connected to Drupal database. Invoke Initialize() method');
		}	
				
		// delete matching rows from the given table		
		$sql = 'DELETE FROM {'.$entityName.'}';
		if (($whereFilter != null) && ($whereFilter != ''))
		{
			$sql = $sql.' WHERE '.$whereFilter;
		}
		try {
			$this->db->query($sql);
		}
		catch (Exception $e)
		{		
			throw new Msft_Bing_MapException('Unable to delete from table '.$entityName.'. '.$e);			
		}		
		
		return true;
	}

    /**
     * Select rows in the given entity which match the given filter
     * If $whereFilter is null or empty, all rows in the entity will be returned 
     *
	 * @param string $entityName
	 * @param string $whereFilter Example: "firstname='Tom' OR (age>25 AND age<=50)"
     * @return Msft_Bing_Row[]
     */ 	
	public function selectRows($entityName, $whereFilter)
	{
		if (($entityName == null) || ($entityName == ''))
		{
			throw new Msft_Bing_MapException('Invalid entityName parameter');		
		}
	
		if ($this->db == null) 
		{
			throw new Msft_Bing_MapException('Not connected to Drupal database. Invoke Initialize() method');
		}	
				
		// select matching rows from the given table
		$sql = "SELECT * FROM {".$entityName."}";
		if (($whereFilter != null) && ($whereFilter != ''))
		{
			$sql = $sql.' WHERE '.$whereFilter;
		}
		
		$rows = array();
		try {
			$result = $this->db->query($sql);
			
			// While a row of data exists, put that row in $row as an associative array
			while (($data = $result->fetchAssoc()) != false) 
			{
				$row = new Msft_Bing_Row();
				$row->Data = $data;
				$rows[] = $row;			
			}
		}
		catch (Exception $e)
		{		
			throw new Msft_Bing_MapException('Unable to query table '.$entityName.'. '.$e);			
		}	
		
		return $rows;
	}
}

?>
