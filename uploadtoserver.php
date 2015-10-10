<?php
    header("Content-Type: multipart/form-data; charset=UTF-8");
    $file_path="uploads/";
    $file_name=$_FILES['uploaded_file']['name'];
    //if (($_FILES["uploaded_file"]["type"] == "image/png") && ($_FILES["uploaded_file"]["size"] < 20000000000)) 
    //{ 
      if ($_FILES['uploaded_file']['error'] > 0)
      {
         echo "Return Code: " . $_FILES['uploaded_file']['error'] . "";
      }
      else
      {

         echo "==>".var_dump(iconv_get_encoding('all'))."";
         echo "Upload: " . $_FILES['uploaded_file']['name'] . "";
         echo "Type: " . $_FILES['uploaded_file']['type'] . "";
         echo "Size: " . ($_FILES['uploaded_file']['size'] / 1024) . " Kb";
         echo "Temp file: " . $_FILES['uploaded_file']['tmp_name'] . "";

         if (file_exists($file_path . $_FILES['uploaded_file']['name']))
         {
             echo $_FILES['uploaded_file']['name'] . " already exists. ";
         }
         else
         {
              //move_uploaded_file($_FILES['uploaded_file']['tmp_name'], $file_path . $_FILES['uploaded_file']['name']);
              move_uploaded_file($_FILES['uploaded_file']['tmp_name'], $file_path . $file_name);
              echo "Stored in: " . $file_path . $file_name;
         }
      }
    //}
    //else 
    //{ 
    //  echo "Invalid file"; 
    //}
    //phpinfo(); 
?>
