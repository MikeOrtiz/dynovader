<?php
// Check if Tar.php exist
if (!file_exists_in_include_path("Tar.php") || 
    !file_exists_in_include_path("Pear.php") ||
    !file_exists_in_include_path("Pear5.php"))
{
    echo "PEAR package Archive_Tar not found. Please install PEAR package Archive_Tar.";
    exit(1);
}

// Extract tgz, tar.gz, bz2, tbz files
define('_JEXEC', 1);

// include class
require("Tar.php");

if (!class_exists("Archive_Tar"))
{
    echo "PEAR package Archive_Tar not found. Please install PEAR package Archive_Tar.";
    exit(1);
}

if ($argc == 3)
{
    $fileName = $argv[1];
    $folderName = $argv[2];
    
    // extract contents of file
    $tar = new Archive_Tar($fileName, true);
    if ($tar->extract($folderName))
    {
        echo "Successfully extracted all files.";
    }
    else
    {
        echo "Could not extract files!";
        exit(1);
    }
}
else if ($argc == 4)
{
    $fileName = $argv[1];
    $folderName = $argv[2];
    $pathFilter = $argv[3];

    // extract contents of file
    $tar = new Archive_Tar($fileName);
    if ($tar->extractModify($folderName, $pathFilter))
    {
    $parts = explode("\\", $pathFilter);
    if (file_exists($parts[0]))
    {
        rrmdir($parts[0]);
    }
        echo "Successfully extracted all files.";
    }
    else
    {
        echo "Could not extract files!";
        exit(1);
    }
}
else
{
    echo "Invalid arguments. Usage: ExtractUtility.php <zipFileName> <folderName> [pathFilter]";
    exit(1);
}

// Check if a file exists in the include path
function file_exists_in_include_path($filename)
{
    // Check for absolute path
    if (realpath($filename) == $filename) {
        return true;
    }

    // Otherwise, treat as relative path
    $paths = explode(PATH_SEPARATOR, get_include_path());
    foreach ($paths as $path) {
        if (substr($path, -1) == DIRECTORY_SEPARATOR) {
            $fullpath = $path.$filename;
        } else {
            $fullpath = $path.DIRECTORY_SEPARATOR.$filename;
        }
        if (file_exists($fullpath)) {
            return true;
        }
    }

    return false;
}

// Recursively remove directory
function rrmdir($dir) 
{ 
    if (is_dir($dir)) 
    { 
        $objects = scandir($dir); 
        foreach ($objects as $object) 
        { 
            if ($object != "." && $object != "..") 
            { 
                if (filetype($dir."/".$object) == "dir") 
                    rrmdir($dir."/".$object); 
                else 
                    unlink($dir."/".$object); 
            } 
        }

        reset($objects); 
        rmdir($dir); 
   }
} 
?>
