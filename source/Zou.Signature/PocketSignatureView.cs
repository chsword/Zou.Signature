using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.IO;
using Java.Lang;
using Javax.Xml.Parsers;
using Org.Xml.Sax;
using Exception = System.Exception;
using Math = System.Math;
using Orientation = Android.Content.Res.Orientation;

namespace Zou.Signature
{
    public class PocketSignatureView : View
    {
        private bool _autoTouchtriggered;
        
        private bool _clearingCanvas;

        private float _lastTouchX;
        private float _lastTouchY;
        private Bitmap _bitmap;
        private float _newPositionOfX=0;
        private float newPositionOfY=0;
        private Paint _paint;

        private Path _path;
        private List<Path> _pathContainer;
        private bool _pathContainerInUse;

        private bool _pathContainerOpen;
        private float _previousWidth;
        private float _scalePointX;
        private float _scalePointY;
        private float _screenWidth;
        private RectF _signatureBoundRect;
     
      
      

        //View Properties
        

        private const string SvgEnd = "\" fill=\"none\" stroke=\"black\" stroke-width=\"1\"/></svg>";
        private bool _touchReleased;
        private string _vectorStringData;
        private float _widthRatio;
        public Color CanvasColor { get; set; }

        private Color _strokeColor;

        public Color StrokeColor
        {
            get { return _strokeColor; }
            set { _paint.Color = _strokeColor = value; }
        }

        private int _strokeWidth;

        public int StrokeWidth
        {
            get
            {
                return _strokeWidth;
            }
            set
            {
                _paint.StrokeWidth = _strokeWidth = value;
            }
        }

        Paint.Style _strokeStyle;

        public Paint.Style StrokeStyle
        {
            get { return _strokeStyle; }
            set { _paint.SetStyle(_strokeStyle = value); }
        }

        bool _strokeAntiAlias;

        public bool StrokeAntiAlias
        {
            get { return _strokeAntiAlias; }
            set { _paint.AntiAlias = _strokeAntiAlias = value; }
        }

      Paint.Join _strokeJoin;

        public Paint.Join StrokeJoin
        {
            get { return _strokeJoin; }
            set { _paint.StrokeJoin = _strokeJoin = value; }
        }
        

        #region Ctor

        public PocketSignatureView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            Init();
        }

        public PocketSignatureView(Context context) : base(context)
        {
            Init();
        }

        public PocketSignatureView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public PocketSignatureView(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            Init();
        }

        public PocketSignatureView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Init();
        }

        #endregion

        void Init()
        {
            InitializeVariables();
            CalculateRatioForOrientation();
            InitializelayoutProperties();
            InitializePaint();
        }
        private void InitializeVariables()
        {
            InitializeSignatureSettings();
            _pathContainer = new List<Path>();
            _path = new Path();
            _paint = new Paint();
            _vectorStringData = "";
            _screenWidth = Resources.DisplayMetrics.WidthPixels;
            _scalePointX = 0;
            _scalePointY = 0;
            _pathContainerOpen = true;
            _touchReleased = true;
            _autoTouchtriggered = false;
            _pathContainerInUse = false;
            _clearingCanvas = false;
        }

        private void InitializeSignatureSettings()
        {
            _strokeColor = Color.Black;
            CanvasColor = Color.ParseColor("#ffffff");
            _strokeWidth = (int) 10f;
            _strokeStyle = Paint.Style.Stroke;
            _strokeAntiAlias = true;
            _strokeJoin = Paint.Join.Round;
        }

        private void CalculateRatioForOrientation()
        {
            if (Resources.Configuration.Orientation == Orientation.Landscape)
            {
                LandscapeRatio();
            }
            else
            {
                _widthRatio = 1;
            }
        }

        private void InitializelayoutProperties()
        {
            _signatureBoundRect = new RectF(0, 0, _screenWidth, _screenWidth/2);
            SetBackgroundColor(CanvasColor);
        }

        private void InitializePaint()
        {
            _paint.AntiAlias = _strokeAntiAlias;
            _paint.Color = _strokeColor;
            _paint.SetStyle(_strokeStyle);
            _paint.StrokeJoin = _strokeJoin;
            _paint.StrokeWidth = _strokeWidth;
        }

        #region Public

        public string GetSVGString()
        {
            if (_vectorStringData != "")
            {
               return  CreateSvg(_vectorStringData);
            }
            else
            {
                Log.Verbose("PocketSignatureView_Log", "No Data to Draw");
            }
            return null;
        }

        private void LoadVectoreImage()
        {
            _pathContainerOpen = false;
            _pathContainerInUse = true;
            Invalidate();
        }

        public void LoadVectoreImage(string pathDataString)
        {
            if (pathDataString != null)
            {
                _vectorStringData = pathDataString;
                CreatePathFromVectorString();
                TriggerTouch();
            }
        }

        public string GetPathDataString()
        {
            return _vectorStringData;
        }

        public Bitmap GetBitmap()
        {
            View view = this;
            if (_bitmap == null)
            {
                _bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Rgb565);
            }

            var bitmapCanvas = new Canvas(_bitmap);
            try
            {
                view.Draw(bitmapCanvas);
                return _bitmap;
            }
            catch (Exception e)
            {
                Log.Verbose("PocketSignatureView_Log", e.ToString());
                return null;
            }
        }

        public void Clear()
        {
            _pathContainerOpen = false;
            _pathContainerInUse = true;
            _clearingCanvas = true;
            _vectorStringData = "";
            Invalidate();
        }

        #endregion

        private void LandscapeRatio()
        {
            var displayMetrics = Resources.DisplayMetrics;
            _screenWidth = displayMetrics.WidthPixels;
            _previousWidth = displayMetrics.HeightPixels;
        }
 

        // TODO : use  A xml parser -> much safer
        private void CreatePathFromVectorString()
        {
            _pathContainer = new List<Path>();

            //every path starts with 'M' so we split by them to get all the paths serparated
            //String[] pathArray = vectorStringData.split("M");

            var pathArray = ParsedPathList(CreateModifiedString(_vectorStringData));

            for (var x = 0; x < pathArray.Count; x++)
            {
                string tempStringStore;
                if (x == pathArray.Count - 1)
                {
                    tempStringStore = pathArray[x];
                }
                else
                {
                    tempStringStore = pathArray[x].Substring(0, pathArray[x].Length - 60);
                }
                //every corrdinates in Path starts with 'L' so we split by them to get all the coordinates serparated
                var arrayOfCoOrdinates = tempStringStore.Split(new[] {" L "}, StringSplitOptions.None);
                var newPath = new Path();

                for (var y = 0; y < arrayOfCoOrdinates.Length; y++)
                {
                    //each coordinate's X and Y points are separated by empty spaces
                    var xY = arrayOfCoOrdinates[y].Split(' ');
                    if (y == 0)
                    {
                        newPath.MoveTo(Float.ParseFloat(xY[1]), Float.ParseFloat(xY[2]));
                    }
                    else
                    {
                        try
                        {
                            newPath.LineTo(Float.ParseFloat(xY[0]), Float.ParseFloat(xY[1]));
                        }
                        catch (ArrayIndexOutOfBoundsException ex)
                        {
                            Log.Debug("PocketSignatureView_Log", ex.ToString());
                        }
                    }
                }
                _pathContainer.Add(newPath);
            }
            LoadVectoreImage();
        }

        private string CreateModifiedString(string toModifyString)
        {
            var stringValue = toModifyString;

            var svgStart = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"" + _screenWidth + "\" height=\"" +
                           _screenWidth/2 + "\" version=\"1.1\">\n" + "  <path d=\"";

            if (!stringValue.Contains("<svg"))
            {
                stringValue = svgStart + stringValue;
            }
            if (!stringValue.Contains("</svg>"))
            {
                stringValue = stringValue + SvgEnd;
            }
            else
            {
                stringValue = stringValue.Replace(SvgEnd, "");
                stringValue = stringValue + SvgEnd;
            }
            return stringValue;
        }

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas); //On Landscape the draw has to be magnified
            if (Resources.Configuration.Orientation == Orientation.Landscape)
            {
                var ratioWidth = _screenWidth/_previousWidth;
                canvas.Scale(ratioWidth, ratioWidth, _scalePointX, _scalePointY);
                canvas.Translate(_newPositionOfX, newPositionOfY);
            }

            if (_pathContainerInUse)
            {
                if (_clearingCanvas)
                {
                    _path.Reset();
                    _pathContainer.Clear();
                    _clearingCanvas = false;
                }
                else
                {
                    DrawAllPaths(canvas);
                }
                _pathContainerInUse = false;
            }

            if (_pathContainerOpen)
            {
                canvas.DrawPath(_path, _paint);
                _pathContainer.Add(_path);
                DrawAllPaths(canvas);
            }
        }

        private void DrawAllPaths(Canvas canvas)
        {
            if (_touchReleased)
            {
                for (var x = 0; x < _pathContainer.Count; x++)
                {
                    _path = _pathContainer[x];
                    canvas.DrawPath(_path, _paint);
                }
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            _touchReleased = false;
            float eventX;
            float eventY;
            if (_previousWidth != 0.0)
            {
                _widthRatio = _screenWidth/_previousWidth;
            }

            if (Resources.Configuration.Orientation == Orientation.Landscape)
            {
                var x = e.GetX()/_widthRatio;
                var y = e.GetY()/
                        _widthRatio;
                eventX = x - _newPositionOfX;
                eventY = y - newPositionOfY;
            }
            else
            {
                eventX = e.GetX();
                eventY = e.GetY();
            }

            _pathContainerOpen = true;

            if (!_autoTouchtriggered)
            {
                switch (e.Action)
                {
                    case MotionEventActions.Down:
                        _path.MoveTo(eventX, eventY);
                        if (!_autoTouchtriggered)
                        {
                            if (_vectorStringData.Contains("M"))
                            {
                                _vectorStringData +=
                                    " \" fill=\"none\" stroke=\"black\" stroke-width=\"1\"/>\n\"  <path d=\"";
                                _vectorStringData += "M " + eventX + " " + eventY;
                            }
                            else
                            {
                                if (_vectorStringData.Contains(SvgEnd))
                                {
                                    _vectorStringData = _vectorStringData.Replace(SvgEnd, "");
                                }
                                _vectorStringData = "M " + eventX + " " + eventY;
                            }
                        }
                        _lastTouchX = eventX;
                        _lastTouchY = eventY;
                        return true;

                    case MotionEventActions.Move:

                    case MotionEventActions.Up:
                        ResetSignatureBoundRect(eventX, eventY);
                        var historySize = e.HistorySize;

                        for (var i = 0; i < historySize; i++)
                        {
                            float historicalX;
                            float historicalY;
                            if (Resources.Configuration.Orientation == Orientation.Landscape)
                            {
                                historicalX = e.
                                    GetHistoricalX(i)/_widthRatio
                                              - _newPositionOfX;
                                historicalY = e.
                                    GetHistoricalY(i)/_widthRatio
                                              - newPositionOfY;
                                _vectorStringData += " L " + e.
                                    GetHistoricalX(i)/_widthRatio + " " + e.
                                        GetHistoricalY(i)/_widthRatio;
                            }
                            else
                            {
                                historicalX = e.
                                    GetHistoricalX(i)
                                              - _newPositionOfX;
                                historicalY = e.
                                    GetHistoricalY(i)
                                              - newPositionOfY;
                                _vectorStringData += " L " + e.
                                    GetHistoricalX(i) + " " + e.
                                        GetHistoricalY(i);
                            }
                            ExpandSignatureBoundRect(historicalX, historicalY);
                            _path.LineTo(historicalX, historicalY);
                        }

                        _path.LineTo(eventX, eventY);
                        _vectorStringData += " L " + eventX + " " + eventY;
                        break;

                    default:
                        return false;
                }
            }
            else
            {
                _autoTouchtriggered = false;
            }

            _touchReleased = true;

            Invalidate((int) (_signatureBoundRect.Left - _strokeWidth/2),
                (int) (_signatureBoundRect.Top - _strokeWidth/2),
                (int) (_signatureBoundRect.Right + _strokeWidth/2),
                (int) (_signatureBoundRect.Bottom + _strokeWidth/2));

            _lastTouchX = eventX;
            _lastTouchY = eventY;

            return true;
        }

        private string CreateSvg(string pathData)
        {
            var svgStart = 
                $"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{_screenWidth}\" height=\"{_screenWidth/2}\" version=\"1.1\">\n  <path d=\"";
            string resultSvg;
            if (pathData.Contains("<svg"))
            {
                resultSvg = pathData;
            }
            else
            {
                resultSvg = svgStart + pathData;
            }
            if (resultSvg.Contains(SvgEnd))
            {
                resultSvg = resultSvg.Replace(SvgEnd, "");
                if (!resultSvg.Contains(SvgEnd))
                {
                    resultSvg = resultSvg + SvgEnd;
                }
            }
            else
            {
                resultSvg = resultSvg + SvgEnd;
            }
            try
            {
                return resultSvg;
            }
            catch (IOException e)
            {
                e.PrintStackTrace();
            }
            return null;
        }

        private void ExpandSignatureBoundRect(float historicalX, float historicalY)
        {
            if (historicalX < _signatureBoundRect.Left)
            {
                _signatureBoundRect.Left = historicalX;
            }
            else if (historicalX > _signatureBoundRect.Right)
            {
                _signatureBoundRect.Right = historicalX;
            }
            if (historicalY < _signatureBoundRect.Top)
            {
                _signatureBoundRect.Top = historicalY;
            }
            else if (historicalY > _signatureBoundRect.Bottom)
            {
                _signatureBoundRect.Bottom = historicalY;
            }
        }

        private void ResetSignatureBoundRect(float eventX, float eventY)
        {
            _signatureBoundRect.Left = Math.Min(_lastTouchX, eventX);
            _signatureBoundRect.Right = Math.Max(_lastTouchX, eventX);
            _signatureBoundRect.Top = Math.Min(_lastTouchY, eventY);
            _signatureBoundRect.Bottom = Math.Max(_lastTouchY, eventY);
        }

        private void TriggerTouch()
        {
            _autoTouchtriggered = true;
            var downTime = SystemClock.UptimeMillis();
            var eventTime = SystemClock.UptimeMillis() + 100;
            var x = 0.0f;
            var y = 0.0f;
            var metaState = (MetaKeyStates) 0;

            var motionEvent = MotionEvent.Obtain(
                downTime,
                eventTime,
                (int) MotionEventActions.Up,
                x,
                y,
                metaState
                );
            DispatchTouchEvent(motionEvent);
        }

        protected override IParcelable OnSaveInstanceState()
        {
            var bundle = new Bundle();
            bundle.PutParcelable("superState", base.OnSaveInstanceState());
            bundle.PutFloat("previousWidth", _screenWidth);
            bundle.PutString("vectorStringData", _vectorStringData);
            return bundle;
        }

        protected override void OnRestoreInstanceState(IParcelable state)
        {
            if (state is Bundle)
            {
                var bundle = (Bundle) state;
                state = bundle.GetParcelable("superState") as IParcelable;
                _previousWidth = bundle.GetFloat("previousWidth", _screenWidth);
                if (Resources.Configuration.Orientation == Orientation.Landscape)
                {
                    LandscapeRatio();
                }
                _vectorStringData = bundle.GetString("vectorStringData");
                LoadVectoreImage(_vectorStringData);
            }
            base.OnRestoreInstanceState(state);
        }

        private List<string> ParsedPathList(string rawXml)
        {
            List<string> pathList = null;

            _vectorStringData = rawXml;
            var br = new BufferedReader(new StringReader(_vectorStringData));
            var is1 = new InputSource(br);

            try
            {
                var parser = new XmlParser();
                var factory = SAXParserFactory.NewInstance();
                var sp = factory.NewSAXParser();
                var reader = sp.XMLReader;
                reader.ContentHandler = parser;
                reader.Parse(is1);

                pathList = parser.List;
            }
            catch (Exception ex)
            {
                Log.Debug("XML Parser Exception", ex.ToString());
            }

            return pathList;
        }

        
    }
}