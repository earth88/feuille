package com.kimsunghoon.helloworldwearable;

import com.unity3d.player.*;
import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.res.Configuration;
import android.database.Cursor;
import android.graphics.PixelFormat;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.net.Uri;
import android.os.Bundle;
import android.os.Vibrator;
import android.provider.MediaStore;
import android.util.Log;
import android.view.KeyEvent;
import android.view.MotionEvent;
import android.view.Window;
import android.view.WindowManager;

import java.util.Timer;
import java.util.TimerTask;

public class UnityPlayerActivity extends Activity
{
	protected UnityPlayer mUnityPlayer; // don't change the name of this variable; referenced from native code

    //센서 매니저를 위한 변수들
    SensorManager mSensorManager;
    Sensor mSensorGyro;
    Sensor mSensorAccelerometer;
    Sensor mSensorOrientation;
    Sensor mSensorHeartrate;

    //로그를 위한 스트링 매크로
    private static final String TAG="CELLPHONE_TEST";

    //0.1초마다 전송해주는 타이머
    private TimerTask mTask;
    private Timer mTimer;

    //현재 움직여야 하는 object의 이름. 내 것인지 아닌지 판별하는 용도
    String currentObjectName ="";

	// Setup activity layout	@Override
	 protected void onCreate (Bundle savedInstanceState) {
        requestWindowFeature(Window.FEATURE_NO_TITLE);
        super.onCreate(savedInstanceState);
         getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);
        getWindow().setFormat(PixelFormat.RGBX_8888); // <--- This makes xperia play happy

        mUnityPlayer = new UnityPlayer(this);
        if (mUnityPlayer.getSettings().getBoolean("hide_status_bar", true)) {
            setTheme(android.R.style.Theme_NoTitleBar_Fullscreen);
            getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                    WindowManager.LayoutParams.FLAG_FULLSCREEN);
        }

        setContentView(mUnityPlayer);
        mUnityPlayer.requestFocus();

        // 센서 매니저와 센서들을 등록 (자이로스코프, 방향, 심장 박동)
        mSensorManager = (SensorManager) getSystemService(Context.SENSOR_SERVICE);
        mSensorAccelerometer = mSensorManager.getDefaultSensor(Sensor.TYPE_LINEAR_ACCELERATION);
        mSensorOrientation = mSensorManager.getDefaultSensor(Sensor.TYPE_ORIENTATION);
        mSensorHeartrate = mSensorManager.getDefaultSensor(Sensor.TYPE_HEART_RATE);
    }

	// Quit Unity
	@Override protected void onDestroy ()
	{
		mUnityPlayer.quit();
		super.onDestroy();
	}

	// Pause Unity
	@Override protected void onPause()
	{
		super.onPause();
        mTimer.cancel();
        mSensorManager.unregisterListener(mSensorListener);
		mUnityPlayer.pause();
	}

    private double x,y,z;
    float previousJumpCounter=0;
    float jumpCounter=0;
    float heartrate, previousHeartrate;
    long currentTime, lastTime;

	// Resume Unity
	@Override protected void onResume()
	{
		super.onResume();
        //센서매니저에 방향과 가속도계 센서 리스너를 등록
        mSensorManager.registerListener(mSensorListener, mSensorOrientation, SensorManager.SENSOR_DELAY_NORMAL);
        mSensorManager.registerListener(mSensorListener, mSensorAccelerometer, SensorManager.SENSOR_DELAY_GAME);
        mTask = new TimerTask() {
            @Override
            public void run() {
                if(currentObjectName.equals("")==false) {
                    //가속도계 데이터를 불러와서 object의 position을 바꿔주는 역할. 0.1초마다 수행됨
                    x=accel_data[0];
                    y=accel_data[2];
                    z=30;

                    final String leafPositionMessage = Double.toString(x) + "," + Double.toString(y) + "," + Double.toString(z);
                    UnityPlayer.UnitySendMessage(currentObjectName, "ChangeLeafPosition", leafPositionMessage);

                    final String cameraRotationMessage = Float.toString(sendingResult[0]);
                    UnityPlayer.UnitySendMessage(currentObjectName, "CameraRotation", cameraRotationMessage);

                    //@deprecated. 가속도계를 이용한 object를 흔들게 하는 효과. 서버 측으로 기능이 이전됨
                    /*
                    currentTime = System.currentTimeMillis();
                    jumpCounter = sendingResult[3];
                    if (previousJumpCounter != jumpCounter && currentTime - lastTime > 1000
                            && jumpCounter > 6 && jumpCounter < 10) {
                        UnityPlayer.UnitySendMessage(currentObjectName, "AccelCamera", "");
                        lastTime = System.currentTimeMillis();
                        previousJumpCounter = jumpCounter;
                    }
                    */

                    //@deprecated. heartrate를 이용한 배경 깜빡이기
                    /*
                    heartrate = sendingResult[4];
                    if (heartrate == 0) {
                        UnityPlayer.UnitySendMessage(currentObjectName, "CancelHeartrate", "");
                    } else if (heartrate != 0 && previousHeartrate != heartrate) {
                        previousHeartrate = heartrate;
                        UnityPlayer.UnitySendMessage(currentObjectName, "SetHeartrate", Float.toString(60 / heartrate));
                    }
                    */
                }
            }
        };
        mTimer=new Timer();
        //mTask는 0.1초마다 수행됨
        mTimer.schedule(mTask, 0, 100);
		mUnityPlayer.resume();
	}

	// This ensures the layout will be correct.
	@Override public void onConfigurationChanged(Configuration newConfig)
	{
		super.onConfigurationChanged(newConfig);
		mUnityPlayer.configurationChanged(newConfig);
	}

	// Notify Unity of the focus change.
	@Override public void onWindowFocusChanged(boolean hasFocus)
	{
		super.onWindowFocusChanged(hasFocus);
		mUnityPlayer.windowFocusChanged(hasFocus);
	}

	// For some reason the multiple keyevent type is not supported by the ndk.
	// Force event injection by overriding dispatchKeyEvent().
	@Override public boolean dispatchKeyEvent(KeyEvent event)
	{
		if (event.getAction() == KeyEvent.ACTION_MULTIPLE)
			return mUnityPlayer.injectEvent(event);
		return super.dispatchKeyEvent(event);
	}

    //현재 내 object의 이름을 등록. 내 object만 움직여야 하기 때문에 사용
    public void setCurrentObjectName(String name)
    {
        this.currentObjectName=name;
        Log.d(TAG, "setCurrentObjectName : " + name);
    }

	// Pass any events not handled by (unfocused) views straight to UnityPlayer
	@Override public boolean onKeyUp(int keyCode, KeyEvent event)     { return mUnityPlayer.injectEvent(event); }
	@Override public boolean onKeyDown(int keyCode, KeyEvent event)   { return mUnityPlayer.injectEvent(event); }
	@Override public boolean onTouchEvent(MotionEvent event)          { return mUnityPlayer.injectEvent(event); }
	/*API12*/ public boolean onGenericMotionEvent(MotionEvent event)  { return mUnityPlayer.injectEvent(event); }


    float[] accel_data=new float[3];
    float[] temp_orientation=new float[3];
    float inputHeartrate;
    float[] sendingResult=new float[5];

    SensorEventListener mSensorListener=new SensorEventListener() {
        @Override
        public void onSensorChanged(SensorEvent event) {
            //@Deprecated
            if(event.sensor.getType()==Sensor.TYPE_ORIENTATION) {
                temp_orientation=event.values;
            }
            //가속도계가 들어오면 accel_data에 저장한 후, mTask에서 참조하도록 함
            else if(event.sensor.getType()==Sensor.TYPE_LINEAR_ACCELERATION)
            {
                accel_data=event.values.clone();
            }
            //@Deprecated
            else if(event.sensor.getType()==Sensor.TYPE_HEART_RATE) {
                inputHeartrate=event.values[0];
            }

            sendingResult[0]=temp_orientation[0];
            sendingResult[1]=temp_orientation[1];
            sendingResult[2]=temp_orientation[2];
            sendingResult[3]=accel_data[2];
            sendingResult[4]=inputHeartrate;
        }
        @Override
        public void onAccuracyChanged(Sensor sensor, int accuracy) {
        }
    };

    private static final int CUSTOM_TEXTURE = 1;
    private static final int CUSTOM_BACKGROUND = 2;

    private Uri mImageCaptureUri;
    private String openGallerySendToUnityObject;

    //휴대폰 진동 울리게 하는 함수
    public void vibratePhone(String argv) {
        Vibrator vibe = (Vibrator) getSystemService(Context.VIBRATOR_SERVICE);
        vibe.vibrate(500);
    }

    //object의 텍스쳐를 갤러리에서 불러와 지정하는 함수.
    public void setCustomTexture(String toObjectName) {
        this.openGallerySendToUnityObject=toObjectName;
        Intent intent=new Intent(Intent.ACTION_PICK);
        intent.setType(MediaStore.Images.Media.CONTENT_TYPE);
        startActivityForResult(intent, CUSTOM_TEXTURE);
    }

    //@Deprecated. object의 배경을 갤러리에서 불러와 지정하는 함수.
    public void setCustomBackground(String toObjectName) {
        this.openGallerySendToUnityObject=toObjectName;
        Intent intent=new Intent(Intent.ACTION_PICK);
        intent.setType(MediaStore.Images.Media.CONTENT_TYPE);
        startActivityForResult(intent, CUSTOM_BACKGROUND);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        if(resultCode != RESULT_OK)
        {
            Log.d(TAG, "resultCode is invalid. Errcode: " + resultCode);
            return;
        }

        //갤러리로부터 선택한 이미지의 절대 경로를 받아옴
        mImageCaptureUri = data.getData();
        Log.d(TAG, mImageCaptureUri.toString());
        Log.d(TAG, "URI!!!! : " + mImageCaptureUri.toString());

        //절대경로를 받아옴
        String path=getPathFromUri(mImageCaptureUri);
        Log.d(TAG, "절대경로 : " + path);
        UploadFile uploadFile=new UploadFile(openGallerySendToUnityObject);

        Log.d(TAG, "switch request code");
        //call 된 함수에 따라 uploadFile 클래스를 이용해 백그라운드에서 올려줌
        switch(requestCode) {
            case CUSTOM_TEXTURE:
                uploadFile.execute(path, "SetCustomTextureFromAndroid");
                break;
            case CUSTOM_BACKGROUND:
                uploadFile.execute(path, "SetCustomBackgroundFromAndroid");
                break;
            default:
                Log.d(TAG, "default");
                break;
        }
    }

    //Uri를 절대경로로 바꿔주는 함수
    public String getPathFromUri(Uri uri) {
        Cursor cursor = getContentResolver().query(uri, null, null, null, null);
        cursor.moveToNext();
        String path = cursor.getString(cursor.getColumnIndex("_data"));
        cursor.close();
        return path;
    }
}
