package com.kimsunghoon.helloworldwearable;

import android.os.AsyncTask;
import android.util.Log;

import com.unity3d.player.UnityPlayer;

import java.io.DataOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

/**
 * Created by KimSunghoon on 2015. 7. 26..
 */
public class UploadFile extends AsyncTask<String, Void, String> {
    String fileName;
    HttpURLConnection conn = null;
    DataOutputStream dos = null;
    String lineEnd = "\r\n";
    String twoHyphens = "--";
    String boundary = "*****";
    int bytesRead, bytesAvailable, bufferSize;
    byte[] buffer;
    int maxBufferSize = 1 * 1024 * 1024;
    String uploadedFileName;

    private final static String serverUrl="http://52.69.154.248/";
    private final static String uploadServerUri = serverUrl+"uploadtoserver.php";//서버컴퓨터의 ip주소
    private final static String TAG="UploadFile";
    int serverResponseCode=0;
    String callUnityMethod;
    String openGallerySendToUnityObject;

    UploadFile(String galleryObject) {
        this.openGallerySendToUnityObject=galleryObject;
    }

    @Override
    protected String doInBackground(String... sourceFileUri) {
        fileName = sourceFileUri[0];
        callUnityMethod=sourceFileUri[1];
        File sourceFile = new File(sourceFileUri[0]);
        Log.d(TAG, "소스파일 이름: " + sourceFile.getName());
        uploadedFileName=sourceFile.getName();
        if (!sourceFile.isFile()) {
            Log.d(TAG, "source file is invalid.");
            return "";
        } else {
            try {
                // open a URL connection to the Servlet
                FileInputStream fileInputStream = new FileInputStream(sourceFile);
                URL url = new URL(uploadServerUri);

                // Open a HTTP  connection to  the URL
                conn = (HttpURLConnection) url.openConnection();
                conn.setDoInput(true); // Allow Inputs
                conn.setDoOutput(true); // Allow Outputs
                conn.setUseCaches(false); // Don't use a Cached Copy
                conn.setRequestMethod("POST");
                conn.setRequestProperty("Connection", "Keep-Alive");
                conn.setRequestProperty("ENCTYPE", "multipart/form-data");
                conn.setRequestProperty("Accept-charset", "UTF-8");
                conn.setRequestProperty("Content-Type", "multipart/form-data;boundary=" + boundary);
                conn.setRequestProperty("uploaded_file", fileName);

                dos = new DataOutputStream(conn.getOutputStream());

                dos.writeBytes(twoHyphens + boundary + lineEnd);
                dos.writeBytes("Content-Disposition: form-data; name=\"uploaded_file\";filename=\""
                        + new String(fileName.getBytes("UTF-8"),"ISO-8859-1") + "\"" + lineEnd);
                dos.writeBytes(lineEnd);

                // create a buffer of  maximum size
                bytesAvailable = fileInputStream.available();

                bufferSize = Math.min(bytesAvailable, maxBufferSize);
                buffer = new byte[bufferSize];

                // read file and write it into form...
                bytesRead = fileInputStream.read(buffer, 0, bufferSize);

                while (bytesRead > 0) {
                    dos.write(buffer, 0, bufferSize);
                    bytesAvailable = fileInputStream.available();
                    bufferSize = Math.min(bytesAvailable, maxBufferSize);
                    bytesRead = fileInputStream.read(buffer, 0, bufferSize);
                }

                // send multipart form data necessary after file data...
                dos.writeBytes(lineEnd);
                dos.writeBytes(twoHyphens + boundary + twoHyphens + lineEnd);

                //close the streams //
                dos.flush();
                dos.close();
                fileInputStream.close();

                // Responses from the server (code and message)
                serverResponseCode = conn.getResponseCode();
                String serverResponseMessage = conn.getResponseMessage();


                Log.i("uploadFile", "HTTP Response is : "
                        + serverResponseMessage + ": " + serverResponseCode);

                if (serverResponseCode == 200) {
                    Log.d(TAG, "file upload completed.");
                }

                // get response
                int ch;
                InputStream is = conn.getInputStream();
                StringBuffer b =new StringBuffer();
                while( ( ch = is.read() ) != -1 ){
                    b.append( (char)ch );
                }
                String s=b.toString();
                Log.e(TAG, "result = " + s);

            } catch (MalformedURLException ex) {
                Log.e("Upload file to server", "error: " + ex.getMessage(), ex);
            } catch (Exception e) {
                Log.e("Upload file to server Exception", "Exception : "
                        + e.getMessage(), e);
            }
        } // End else block
        return callUnityMethod;
    }

    @Override
    protected void onPostExecute(String callUnityMethod) {
        super.onPostExecute(callUnityMethod);
        String uploadedPath=serverUrl+"uploads/"+uploadedFileName;
        UnityPlayer.UnitySendMessage(openGallerySendToUnityObject, callUnityMethod, uploadedPath);
        uploadedFileName="";
    }
}
