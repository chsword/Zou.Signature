using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;

namespace Zou.Signature.Sample
{
    [Activity(Label = "Zou.Signature.Sample", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            var view = FindViewById<SignatureView>(Resource.Id.view); 
            Button clear = FindViewById<Button>(Resource.Id.clear);
            clear.Click += (s, e) =>
            {
                view.Clear();
            };
            Button getsvg = FindViewById<Button>(Resource.Id.getsvg);
            getsvg.Click += (s, e) =>
            {
                var svg = view.GetSVGString();
                var bitmap = view.GetBitmap();
            };
        }
    }
}

