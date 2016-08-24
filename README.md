## Zou.Signature

[![Build status](https://ci.appveyor.com/api/projects/status/g1bbe17lw4a4linl?svg=true)](https://ci.appveyor.com/project/chsword/zou-signature)

### Install

``` powershell
Install-Package Zou.Signature
```

### Sample
axml:

``` xml
<LinearLayout xmlns:android="http://schemas.android.com/apk/res/android"
    android:orientation="vertical"
    android:layout_width="match_parent"
    android:layout_height="match_parent">
    <Button
        android:id="@+id/clear"
        android:layout_width="match_parent"
        android:layout_height="wrap_content"
        android:text="Clear" />
    <Button
        android:id="@+id/getsvg"
        android:layout_width="match_parent"
        android:layout_height="wrap_content"
        android:text="Get Svg" />
    <Zou.Signature.PocketSignatureView
        android:id="@+id/view"
        android:layout_width="match_parent"
        android:layout_height="wrap_content" />
</LinearLayout>
```

activity:

``` csharp
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            var view = FindViewById<PocketSignatureView>(Resource.Id.view); 
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
```

 ### Reference

[https://github.com/Batzee/Pocket-Signature-View-Android](https://github.com/Batzee/Pocket-Signature-View-Android)