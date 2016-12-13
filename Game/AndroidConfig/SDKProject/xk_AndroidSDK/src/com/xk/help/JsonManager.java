package com.xk.help;

import java.util.HashMap;
import java.util.Hashtable;
import java.util.Iterator;
import java.util.List;
import java.util.Map;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.util.Log;

public class JsonManager {
	private static final String tag=JsonManager.class.getName();
	
	@SuppressWarnings("unchecked")
	public static  Map<String,Object> parseJsonFromStr(String mStr)
	{	
		Map<String,Object> mHash=new HashMap<String,Object>();
		try
		{
			JSONObject ja = new JSONObject(mStr);
			for(Iterator<String> key=ja.keys();key.hasNext();)
			{
				String key1= key.next();
				mHash.put(key1, ja.get(key1));
			}
			return mHash;
		}catch(Exception e)
		{
			Log.e(tag, e.getMessage());
			return null;
		}
	}
	
	public static String parseJsonFromHash(HashMap<String,Object> mMap)
	{	
		try
		{
			JSONObject  jo = new JSONObject(mMap);
			return jo.toString();
		}catch(Exception e)
		{
			Log.e(tag, e.getMessage());
			return null;
		}
	}

	
}
