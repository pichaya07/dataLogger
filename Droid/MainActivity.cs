﻿using Android.App;
using Android.Widget;
using Android.OS;
using Java.IO;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using System.Collections.Generic;
using Android.Provider;
using Java.Lang;
using Android.Locations;
using Android.Util;
using Android.Net;
using System.Threading.Tasks;
using Java.Util;
using DataLogger.Droid.Services;
using System;

namespace DataLogger.Droid
{
	[Activity(Label = "DataLogger", MainLauncher = true, Icon = "@mipmap/icon")]

	public class MainActivity : Activity
	{
		ImageView _imageView;
		static readonly string TAG = "X:" + typeof(Activity).Name;

		readonly string logTag = "MainActivity";
		// make our labels
		TextView latText;
		TextView longText;
		TextView altText;
		TextView speedText;
		TextView accText;

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

		    Log.Debug(logTag, "OnCreate: Location app is becoming active");

			// Set our view from the "main" layout resource
			SetContentView(Resource.Layout.Main);


			// Set our view from the "Main" layout resource
			SetContentView(Resource.Layout.Main);

			Spinner spinner = FindViewById<Spinner>(Resource.Id.spinner_mode);

			spinner.ItemSelected += new System.EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelected);
			var adapter = ArrayAdapter.CreateFromResource(
				this, Resource.Array.choice_array, Android.Resource.Layout.SimpleSpinnerItem);

			adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
			spinner.Adapter = adapter;



			//report message
			var editText_msg = FindViewById<EditText>(Resource.Id.editText_msg);
			//var textView = FindViewById<TextView>(Resource.Id.textView_address);

			//if text is change, do
			/*
			editText_msg.TextChanged += (object sender, Android.Text.TextChangedEventArgs e) =>
			{

				textView.Text = e.Text.ToString();

			};
			*/


			FindViewById<Button>(Resource.Id.bt_getPos).Click += getCurrentPos;


			//Picture
			if (IsThereAnAppToTakePictures())
			{
				
				CreateDirectoryForPictures();


				Button button = FindViewById<Button>(Resource.Id.bnt_takePic);
				_imageView = FindViewById<ImageView>(Resource.Id.imageView1);
				button.Click += TakeAPicture;
			}


			//LOCATION 

			//InitializeLocationManager();

			App.Current.LocationServiceConnected += (object sender, ServiceConnectedEventArgs e) =>
			{
				Log.Debug(logTag, "ServiceConnected Event Raised");
				// notifies us of location changes from the system
				App.Current.LocationService.LocationChanged += HandleLocationChanged;
				//notifies us of user changes to the location provider (ie the user disables or enables GPS)
				App.Current.LocationService.ProviderDisabled += HandleProviderDisabled;
				App.Current.LocationService.ProviderEnabled += HandleProviderEnabled;
				// notifies us of the changing status of a provider (ie GPS no longer available)
				App.Current.LocationService.StatusChanged += HandleStatusChanged;
			};

			latText = FindViewById<TextView>(Resource.Id.lat);
			longText = FindViewById<TextView>(Resource.Id.lon);
			altText = FindViewById<TextView>(Resource.Id.alt);
			speedText = FindViewById<TextView>(Resource.Id.speed);
			//bearText = FindViewById<TextView>(Resource.Id.bear);
			accText = FindViewById<TextView>(Resource.Id.acc);

			altText.Text = "altitude";
			speedText.Text = "speed";
			//bearText.Text = "bearing";
			accText.Text = "accuracy";

			// Start the location service:
			App.StartLocationService();


		}

		void getCurrentPos(object sender, EventArgs eventArgs)
		{
			string toast4 = string.Format("get current position");
			Toast.MakeText(this, toast4, ToastLength.Long).Show();
			// notifies us of location changes from the system
			App.Current.LocationService.LocationChanged += HandleLocationChanged;
			//notifies us of user changes to the location provider (ie the user disables or enables GPS)
			App.Current.LocationService.ProviderDisabled += HandleProviderDisabled;
			App.Current.LocationService.ProviderEnabled += HandleProviderEnabled;
			// notifies us of the changing status of a provider (ie GPS no longer available)
			App.Current.LocationService.StatusChanged += HandleStatusChanged;
		}


		bool IsThereAnAppToTakePictures()
		{
			
			Intent intent = new Intent(MediaStore.ActionImageCapture);
			IList<ResolveInfo> availableActivities =
				PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
			return availableActivities != null && availableActivities.Count > 0;
			

		}

		private void spinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
		{
			Spinner spinner = (Spinner)sender;

			string toast = string.Format("{0}", spinner.GetItemAtPosition(e.Position));
			Toast.MakeText(this, toast, ToastLength.Long).Show();
		}

		private void CreateDirectoryForPictures()
		{
			App._dir = new File(
				Android.OS.Environment.GetExternalStoragePublicDirectory(
					Android.OS.Environment.DirectoryPictures), "CameraAppDemo");
			if (!App._dir.Exists())
			{
				App._dir.Mkdirs();
			}
		}

		private void TakeAPicture(object sender, EventArgs eventArgs)
		{
			//Pause collect GPS points.
			OnPause();

			Intent intent = new Intent(MediaStore.ActionImageCapture);

			App._file = new File(App._dir, string.Format("myPhoto_{0}.jpg",UUID.RandomUUID()));

			//App._file = new File(App._dir, String.Format("myPhoto_{0}.jpg", guid.NewGuid()));
			intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
			StartActivityForResult(intent, 0);

			//Resume collect GPS
			OnResume();
		}

		protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			// Make it available in the gallery

			Intent mediaScanIntent = new Intent(Intent.ActionMediaScannerScanFile);
			Android.Net.Uri contentUri = Android.Net.Uri.FromFile(App._file);
			mediaScanIntent.SetData(contentUri);
			SendBroadcast(mediaScanIntent);

			// Display in ImageView. We will resize the bitmap to fit the display.
			// Loading the full sized image will consume to much memory
			// and cause the application to crash.

			int height = Resources.DisplayMetrics.HeightPixels;
			int width = _imageView.Height;
			App.bitmap = App._file.Path.LoadAndResizeBitmap(width, height);
			if (App.bitmap != null)
			{
				_imageView.SetImageBitmap(App.bitmap);
				App.bitmap = null;
			}

			// Dispose of the Java side bitmap.
			GC.Collect();
		}


		public void OnProviderDisabled(string provider)
		{

		}

		protected override void OnPause()
		{
			Log.Debug(logTag, "OnPause: Location app is moving to background");
			base.OnPause();
		}

		public void OnProviderEnabled(string provider) { }

		public void OnStatusChanged(string provider, Availability status, Bundle extras)
		{
			Log.Debug(TAG, "{0}, {1}", provider, status);
		}

		protected override void OnResume()
		{
			Log.Debug(logTag, "OnResume: Location app is moving into foreground");
			base.OnResume();
		}

		protected override void OnDestroy()
		{
			Log.Debug(logTag, "OnDestroy: Location app is becoming inactive");
			base.OnDestroy();

			// Stop the location service:
			App.StopLocationService();
		}


		#region Android Location Service methods

		///<summary>
		/// Updates UI with location data
		/// </summary>
		public void HandleLocationChanged(object sender, LocationChangedEventArgs e)
		{
			Location location = e.Location;
			Log.Debug(logTag, "Foreground updating");

			// these events are on a background thread, need to update on the UI thread
			RunOnUiThread(() =>
			{
				latText.Text = string.Format("Latitude: {0:f6}", location.Latitude);
				longText.Text = string.Format("Longitude: {0:f6}", location.Longitude);

				altText.Text = string.Format("Altitude: {0}", location.Altitude);
				/*
				speedText.Text = String.Format("Speed: {0}", location.Speed);
				accText.Text = String.Format("Accuracy: {0}", location.Accuracy);
				bearText.Text = String.Format("Bearing: {0}", location.Bearing);
				*/
			});

		}

		public void HandleProviderDisabled(object sender, ProviderDisabledEventArgs e)
		{
			Log.Debug(logTag, "Location provider disabled event raised");
		}

		public void HandleProviderEnabled(object sender, ProviderEnabledEventArgs e)
		{
			Log.Debug(logTag, "Location provider enabled event raised");
		}

		public void HandleStatusChanged(object sender, StatusChangedEventArgs e)
		{
			Log.Debug(logTag, "Location status changed, event raised");
		}

		#endregion

	}

	public static class BitmapHelpers
	{
		public static Bitmap LoadAndResizeBitmap(this string fileName, int width, int height)
		{
			// First we get the the dimensions of the file on disk
			BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
			BitmapFactory.DecodeFile(fileName, options);

			// Next we calculate the ratio that we need to resize the image by
			// in order to fit the requested dimensions.
			int outHeight = options.OutHeight;
			int outWidth = options.OutWidth;
			int inSampleSize = 1;

			if (outHeight > height || outWidth > width)
			{
				inSampleSize = outWidth > outHeight
								   ? outHeight / height
								   : outWidth / width;
			}

			// Now we will load the image and have BitmapFactory resize it for us.
			options.InSampleSize = inSampleSize;
			options.InJustDecodeBounds = false;
			Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

			return resizedBitmap;
		}
	}


	public class App
	{
		public static File _file;
		public static File _dir;
		public static Bitmap bitmap;

		// events

		public event EventHandler<ServiceConnectedEventArgs> LocationServiceConnected = delegate { };

		// declarations
		protected readonly string logTag = "App";
		protected static LocationServiceConnection locationServiceConnection;

		// properties

		public static App Current
		{
			get { return current; }
		}
		private static App current;

		public LocationService LocationService
		{
			
			get
			{
				if (locationServiceConnection.Binder == null)
					throw new Java.Lang.Exception("Service not bound yet");
				// note that we use the ServiceConnection to get the Binder, and the Binder to get the Service here
				return locationServiceConnection.Binder.Service;
			}
		}

		#region Application context

		static App()
		{
			current = new App();
		}

		protected App()
		{
			// create a new service connection so we can get a binder to the service
			locationServiceConnection = new LocationServiceConnection(null);

			// this event will fire when the Service connection in the OnServiceConnected call 
			locationServiceConnection.ServiceConnected += (object sender, ServiceConnectedEventArgs e) =>
			{

				Log.Debug(logTag, "Service Connected");
				// we will use this event to notify MainActivity when to start updating the UI
				LocationServiceConnected(this, e);

			};
		}


		public static void StartLocationService()
		{
			// Starting a service like this is blocking, so we want to do it on a background thread
			new Task(() =>
			{

				// Start our main service
				Log.Debug("App", "Calling StartService");
				Application.Context.StartService(new Intent(Application.Context, typeof(LocationService)));

				// bind our service (Android goes and finds the running service by type, and puts a reference
				// on the binder to that service)
				// The Intent tells the OS where to find our Service (the Context) and the Type of Service
				// we're looking for (LocationService)
				Intent locationServiceIntent = new Intent(Application.Context, typeof(LocationService));
				Log.Debug("App", "Calling service binding");

				// Finally, we can bind to the Service using our Intent and the ServiceConnection we
				// created in a previous step.
				Application.Context.BindService(locationServiceIntent, locationServiceConnection, Bind.AutoCreate);
			}).Start();
		}

		public static void StopLocationService()
		{
			// Check for nulls in case StartLocationService task has not yet completed.
			Log.Debug("App", "StopLocationService");

			// Unbind from the LocationService; otherwise, StopSelf (below) will not work:
			if (locationServiceConnection != null)
			{
				Log.Debug("App", "Unbinding from LocationService");
				Application.Context.UnbindService(locationServiceConnection);
			}

			// Stop the LocationService:
			if (Current.LocationService != null)
			{
				Log.Debug("App", "Stopping the LocationService");
				Current.LocationService.StopSelf();
			}
		}

		#endregion

	}

}




