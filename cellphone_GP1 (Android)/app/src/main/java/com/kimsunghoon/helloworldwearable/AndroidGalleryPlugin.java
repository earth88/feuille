//package com.kimsunghoon.helloworldwearable;
//
//import android.app.Activity;
//import android.content.Context;
//import android.content.Intent;
//import android.net.Uri;
//import android.os.Bundle;
//import android.util.Log;
//
//import com.unity3d.player.UnityPlayer;
//
///**
// * Created by KimSunghoon on 2015. 7. 22..
// */
//public class AndroidGalleryPlugin extends UnityPlayerActivity {
//
//    public Uri ImageURI;
//    static final int REQUEST_CODE = 1;
//    private static Activity _activity;
//
//    @Override
//    protected void onCreate(Bundle arg0) {
//        super.onCreate(arg0);
//        _activity = UnityPlayer.currentActivity;
//    }
//
//    public static void StartOpenGallery(Context mContext) {
//        Log.d("AndroidGalleryPlugin", "Start Open Gallery");
//        Context _context = mContext;
//        Intent intent = new Intent();
//        intent.setType("image/*");
//        intent.setAction(Intent.ACTION_GET_CONTENT);
//        _activity.startActivityForResult(Intent.createChooser(intent, "Select Picture"), REQUEST_CODE);
//    }
//
//    @Override
//    public void onActivityResult(int requestCode, int resultCode, Intent data) {
//        Log.d("AndroidGalleryPlugin", "onActivityResult");
//    }
//
//    // And to convert the image URI to the direct file system path of the image file
//    public String getRealPathFromURI(Uri contentUri) {
//
//        Log.d("AndroidGalleryPlugin", "Get URI");
//        return "";
//    }
//}